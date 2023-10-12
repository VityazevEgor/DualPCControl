using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DPC_Client.Client.Models;
using System.Text.Json;

namespace DPC_Client.Client
{
    internal class CMain
    {
        private static TcpClient? client = null;
        public static List<string> logs = new List<string>();

        private static LinkedList<KeyPacket> packets = new LinkedList<KeyPacket>();

        public static void start(string ip, int port)
        {
            client = new TcpClient(ip, port);
            log($"I connected to {ip}:{port}");
            Task.Run(worker);
            Task.Run(packetsProcessor);
        }

        private static async Task worker()
        {
            byte[] buffer = new byte[1024];
            NetworkStream stream = client.GetStream();
            while (true)
            {
                try
                {
                    int countBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                    packets.AddLast(JsonSerializer.Deserialize<KeyPacket>(Encoding.UTF8.GetString(buffer, 0, countBytes)));
                    log($"I got packet with {countBytes} bytes for key {packets.Last().key}");
                }
                catch (Exception ex)
                {
                    log($"Got error: {ex.Message}");
                }
                await Task.Delay(10);
            }
        }

        private static async Task packetsProcessor()
        {
            while (true)
            {
                if (packets.Count > 0)
                {
                    try
                    {
                        KeyboardEmulation.SimulateKey(packets.First());
                        log($"I emulated key: {packets.First().key} with state {packets.First().type}");
                    }
                    catch (Exception ex)
                    {
                        log($"Got error in pProcessor: {ex.Message}");
                    }
                    packets.RemoveFirst();
                }
                await Task.Delay(10);
            }
        }


        private static void log(string message)
        {
            if (logs.Count > 10) logs.Clear();
            logs.Add($"[{DateTime.Now}] {message}");
        }
    }
}
