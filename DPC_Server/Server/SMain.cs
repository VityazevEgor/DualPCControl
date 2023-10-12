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
        private static List<byte[]> packets = new List<byte[]>();

        public static void Start(int port)
        {
            ipPoint = new IPEndPoint(IPAddress.Any, port);
            listener = new TcpListener(ipPoint);
            listener.Start();
            Task.Run(woker);

            log($"Server was started on port {port}");
        }

        // скорее всего тут что-то кушает много ЦП
        private static async Task woker()
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            NetworkStream stream = client.GetStream();
            log($"Client with ip = {client.Client.RemoteEndPoint} was connected");
            while (true)
            {
                if (packets.Count > 0)
                {
                    log($"I sent packet with length = {packets[0].Length} to {client.Client.RemoteEndPoint}");
                    await stream.WriteAsync(packets[0], 0, packets[0].Length);
                    packets.RemoveAt(0);
                    await Task.Delay(10);
                }
            }
        }


        public static void sendKey(KeyPacket packet)
        {
            log("I trying to encode packet");
            string json = JsonSerializer.Serialize(packet);
            packets.Add(Encoding.UTF8.GetBytes(json));
            log("I encoded packet");
        }

        private static void log(string msg)
        {
            if (logs.Count > 10) logs.Clear();
            logs.Add($"[{DateTime.Now}] {msg}");
        }

    }
}
