using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DPC_Client.Client
{
    internal class CMain
    {
        private static TcpClient? client = null;
        public static List<string> logs = new List<string>();


        public static void start(string ip, int port)
        {
            client = new TcpClient(ip, port);
            log($"I connected to {ip}:{port}");
            Task.Run(worker);
        }

        private static async Task worker()
        {
            StringBuilder rawPacket = new StringBuilder();
            byte[] buffer = new byte[1024];
            NetworkStream stream = client.GetStream();
            while (true)
            {
                int countBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                rawPacket.Append(Encoding.UTF8.GetString(buffer));
                logs.Add($"I got packet with {countBytes} bytes:\n{rawPacket}");
                rawPacket.Clear();
            }
        }

        private static void log(string message)
        {
            logs.Add($"[{DateTime.Now}] {message}");
        }
    }
}
