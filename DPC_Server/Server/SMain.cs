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

        public static void Start(int port)
        {
            ipPoint = new IPEndPoint(IPAddress.Any, port);
            listener = new TcpListener(ipPoint);
            listener.Start();
            Task.Run(woker);

            log($"Server was started on port {port}");
        }

        // надо добавить флаг, который бы показывал шо передача закончена
        private static async Task woker()
        {
            while (true)
            {
                log("Waiting for client");
                TcpClient client = await listener.AcceptTcpClientAsync();
                NetworkStream stream = client.GetStream();
                log($"Client with ip = {client.Client.RemoteEndPoint} was connected");
                packets.Clear();
                while (client.Connected)
                {
                    if (packets.Count > 0)
                    {
                        try
                        {
                            log($"I sent packet with length = {packets.First().Length} to {client.Client.RemoteEndPoint}");
                            await stream.WriteAsync(packets.First(), 0, packets.First().Length);
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
        }


        public static void sendKey(KeyPacket packet)
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
