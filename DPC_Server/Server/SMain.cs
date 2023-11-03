using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DPC_Server.Server.Models;
using TextCopy;

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
        private static CancellationTokenSource stopMainW = null;
        public static void Start(int port)
        {
            ipPoint = new IPEndPoint(IPAddress.Any, port);
            listener = new TcpListener(ipPoint);
            listener.Start();
            isLaunched=true;
            stopMainW = new CancellationTokenSource();
            Task.Run(worker);

            log($"Server was started on port {port}");
        }

        public static void Stop()
        {
            stopMainW.Cancel();
            client?.Close();
            listener?.Stop();
            isLaunched = false;
            log("Server was stopped");
        }

        private static async Task worker()
        {
            while (isLaunched)
            {
                log("Waiting for client");
                try
                {
                    client = await listener.AcceptTcpClientAsync(stopMainW.Token);
                }
                catch (OperationCanceledException ex)
                {
                    Debug.WriteLine($"Task canceled in worker: {ex.Message}");
                    // убираем токен и выходим из цикла
                    stopMainW.Dispose();
                    break;
                }
                catch (Exception ex)
                {
                    log($"Error in MainWoker: {ex.Message}");
                    // если произошла инная ошибка, то просто ждём немного и продолжаем ждать подключения
                    await Task.Delay(1000);
                    continue;
                }
                NetworkStream stream = client.GetStream();
                log($"Client with ip = {client.Client.RemoteEndPoint} was connected");
                packets.Clear();

                sendClipBoard(); // отправляем пакет с буфером обмена при подключении второго ПК
                Task[] tasks = new Task[] { sender(stream), listenerT(stream), layoutGetter() };
                await Task.WhenAll(tasks);
                await Task.Delay(10);
            }
        }

        private static async Task sender(NetworkStream stream)
        {
            while (client!=null && IsConnected(client.Client))
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
            log("Sender finished his work");
        }

        private static async Task listenerT(NetworkStream stream)
        {
            while (client != null && IsConnected(client.Client))
            {
                try
                {
                    byte[] rawBuffer = await readPacket(stream);

                    // получаем пакеты с инфой о буфере обмена
                    var cbPacket = JsonSerializer.Deserialize<ClipBoardPacket>(Encoding.UTF8.GetString(rawBuffer));
                    await ClipboardService.SetTextAsync(Encoding.UTF8.GetString(cbPacket.clipBoardData));
                    log($"Got cb packet: {string.Join(',',cbPacket.clipBoardData)}");
                }
                catch (Exception ex)
                {
                    log($"Got error in listener: {ex.Message}");
                }
                await Task.Delay(10);
            }
            log("Listener finished his work");
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


        // с использованием библиотеки TextCopy
        public static void sendClipBoard()
        {
            try
            {
                string? text = ClipboardService.GetText();
                if (text != null)
                {
                    var packet = new ClipBoardPacket { type = 1, clipBoardData = Encoding.UTF8.GetBytes(text) };
                    sendPacket(packet);
                    log("I sent clipBoard packet");
                }
                else
                {
                    log("No text in clipBoard");
                }
            }
            catch (Exception ex)
            {
                log("Error in sendClipBoard: " + ex.Message);
            }
        }


        private static async Task layoutGetter()
        {
            string lastLayout = string.Empty;
            while (client != null && IsConnected(client.Client))
            {
                try
                {
                    string currentLayout = LayoutAPI.GetLayout();
                    if (lastLayout != currentLayout)
                    {
                        sendPacket(new LayoutPacket { layoutCode = currentLayout });
                        lastLayout = currentLayout;
                        log($"I added packet about layout with code = {lastLayout}");
                    }
                }
                catch (Exception ex)
                {
                    log($"Can't get layout cuz of it: {ex.Message}");
                }
                await Task.Delay(200);
            }
        }

        public static void sendPacket<T>(T packet)
        {
            string json = JsonSerializer.Serialize(packet);
            packets.AddLast(Encoding.UTF8.GetBytes(json));
        }

        private static bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
        private static void log(string msg)
        {
            if (logs.Count > 12) logs.Clear();
            logs.Add($"[{DateTime.Now}] {msg}");
        }

    }
}
