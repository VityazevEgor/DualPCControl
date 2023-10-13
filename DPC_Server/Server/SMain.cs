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
            Task.Run(worker);

            log($"Server was started on port {port}");
        }

        private static async Task worker()
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
                    if (packets.First is not null)
                    {
                        try
                        {
                            byte[] packet = packets.First();
                            byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);

                            // отпрвляем инфу о длине пакета (4 байта)
                            await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                            log("I sent info about len of packet");

                            await stream.WriteAsync(packet, 0, packet.Length);
                            log($"I send packet with len = {packet.Length}");


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
