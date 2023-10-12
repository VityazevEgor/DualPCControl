using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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


        private static async Task woker()
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                log($"Client with ip = {client.Client.RemoteEndPoint} was connected");

                if (packets.First is not null)
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        await stream.WriteAsync(packets.First(), 0, packets.First().Length);
                        log($"I sent packet with length = {packets.First().Length} to {client.Client.RemoteEndPoint}");
                        packets.RemoveFirst();
                    }
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
            logs.Add($"[{DateTime.Now}] {msg}");
        }

    }
}
