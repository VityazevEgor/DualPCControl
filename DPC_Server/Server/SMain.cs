using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DPC_Server.Server.Models;

namespace DPC_Server.Server
{
    internal class SMain
    {
        private static IPEndPoint? ipPoint = null;
        private static TcpListener? listener = null;

        public static List<string> logs = new List<string>();
        private static LinkedList<byte[]> packets = new LinkedList<byte[]>();


        private static TcpClient client = null;

        public static bool isLaunched = false;
        public static void Start(int port)
        {
            ipPoint = new IPEndPoint(IPAddress.Any, port);
            listener = new TcpListener(ipPoint);
            listener.Start();
            isLaunched=true;
            Task.Run(worker);

            log($"Server was started on port {port}");
        }

        public static void Stop()
        {
            // Остановить TcpListener
            listener?.Stop();

            // Закрыть все открытые соединения
            client?.Close();

            isLaunched = false;
            log("Server was stopped");
        }

        private static async Task worker()
        {
            try
            {
                while (isLaunched)
                {
                    log("Waiting for client");
                    client = await listener.AcceptTcpClientAsync();
                    NetworkStream stream = client.GetStream();
                    log($"Client with ip = {client.Client.RemoteEndPoint} was connected");
                    packets.Clear();

                    sendClipBoard(); // отправляем пакет с буфером обмена при подключении второго ПК
                    Task[] tasks = new Task[] { sender(stream), listenerT(stream), layoutGetter() };
                    await Task.WhenAll(tasks);
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                log($"Main worker was stopped: {ex.Message}");
            }
        }

        private static async Task sender(NetworkStream stream)
        {
            while (client.Connected)
            {
                if (packets.First is not null)
                {
                    try
                    {
                        byte[] packet = packets.First();
                        byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);

                        // отпрвляем инфу о длине пакета (4 байта)
                        await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                        //log("I sent info about len of packet");

                        await stream.WriteAsync(packet, 0, packet.Length);
                        //log($"I send packet with len = {packet.Length}");


                        packets.RemoveFirst();
                    }
                    catch (Exception ex)
                    {
                        log($"Got error: {ex.Message}");
                    }
                }
                await Task.Delay(10);
            }
        }

        private static async Task listenerT(NetworkStream stream)
        {
            while (client.Connected)
            {
                try
                {
                    byte[] rawBuffer = await readPacket(stream);

                    // получаем пакеты с инфой о буфере обмена
                    var cbPacket = JsonSerializer.Deserialize<ClipBoardPacket>(Encoding.UTF8.GetString(rawBuffer));
                    ClibBoardAPI.ClearClipboard();
                    ClibBoardAPI.processClipPacket(cbPacket);
                    log($"Got cb packet: {string.Join(',',cbPacket.clipBoardData)}");
                }
                catch (Exception ex)
                {
                    log($"Got error in listener: {ex.Message}");
                }
                await Task.Delay(10);
            }
        }

        private static async Task<byte[]> readPacket(NetworkStream stream)
        {
            // читаем длину пакета
            byte[] lengthBuffer = new byte[4];
            await stream.ReadAsync(lengthBuffer, 0, 4);
            int packetLength = BitConverter.ToInt32(lengthBuffer, 0);

            // читаем сам пакет
            byte[] packetBuffer = new byte[packetLength];
            int bytesRead = 0;
            while (bytesRead < packetLength)
            {
                bytesRead += await stream.ReadAsync(packetBuffer, bytesRead, packetLength - bytesRead);
            }
            //log($"I got this packet: {string.Join(',', packetBuffer)}");
            return packetBuffer;
        }


        public static void sendClipBoard()
        {
            try
            {
                if (ClibBoardAPI.ContainsText())
                {
                    var packet = new ClipBoardPacket { type = 1, clipBoardData = Encoding.UTF8.GetBytes(ClibBoardAPI.GetText()) };
                    sendPacket(packet);
                    log($"I added cp packet");
                }
            }
            catch (Exception ex)
            {
                log($"Got error in cpGetter: {ex.Message}");
            }
        }


        private static async Task layoutGetter()
        {
            string lastLayout = string.Empty;
            while (client.Connected)
            {
                try
                {
                    string currentLayout = LayoutAPI.GetLayout();//LayoutAPI.GetLayoutCode();
                    if (lastLayout != currentLayout)
                    {
                        sendPacket<LayoutPacket>(new LayoutPacket { layoutCode = currentLayout });
                        lastLayout = currentLayout;
                        log($"I added packet about layout with code = {lastLayout}");
                    }
                }
                catch (Exception ex)
                {
                    log($"Can't get layout cuz of it: {ex.Message}");
                }
                await Task.Delay(300);
            }
        }

        public static void sendPacket<T>(T packet)
        {
            string json = JsonSerializer.Serialize(packet);
            packets.AddLast(Encoding.UTF8.GetBytes(json));
        }


        private static void log(string msg)
        {
            if (logs.Count > 10) logs.Clear();
            logs.Add($"[{DateTime.Now}] {msg}");
        }

    }
}
