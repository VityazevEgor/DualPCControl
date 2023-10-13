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

        private static LinkedList<KeyPacket> keyPackets = new LinkedList<KeyPacket>();
        private static LinkedList<MouseMovePacket> movePackets = new LinkedList<MouseMovePacket>();

        public static void start(string ip, int port)
        {
            client = new TcpClient(ip, port);
            log($"I connected to {ip}:{port}");
            Task.Run(worker);
            Task.Run(packetsProcessor);
        }


        private static async Task worker()
        {
            NetworkStream stream = client.GetStream();
            while (true)
            {
                try
                {
                    byte[] rawPacket = await readPacket(stream);
                    string rawJson = Encoding.UTF8.GetString(rawPacket, 0, rawPacket.Length);

                    Type packetType = detectPacketType(rawJson);

                    if (packetType == typeof(KeyPacket))
                    {
                        keyPackets.AddLast(JsonSerializer.Deserialize<KeyPacket>(rawJson));
                        log($"I got packet with {rawPacket.Length} bytes for key {keyPackets.Last().key}");
                    }
                    if (packetType == typeof (MouseMovePacket))
                    {
                        movePackets.AddLast(JsonSerializer.Deserialize<MouseMovePacket>(rawJson));
                        log($"I gout mouse move packet with x={movePackets.Last().x}, y={movePackets.Last().y}, formWidth = {movePackets.Last().formWidth}");
                    }

                    
                }
                catch (Exception ex)
                {
                    log($"Got error: {ex.Message}");
                }
                await Task.Delay(10);
            }
        }

        private static Type detectPacketType(string jsonString)
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("key", out _))
                {
                    return typeof(KeyPacket);
                }
                else if (root.TryGetProperty("formHeight", out _))
                {
                    return typeof(MouseMovePacket);
                }
            }

            throw new Exception("Unknown type");
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
            log($"I got this packet: {string.Join(',', packetBuffer)}");
            return packetBuffer;
        }


        private static async Task packetsProcessor()
        {
            while (true)
            {
                if (keyPackets.First is not null)
                {
                    try
                    {
                        KeyboardEmulation.SimulateKey(keyPackets.First());
                        log($"I emulated key: {keyPackets.First().key} with state {keyPackets.First().type}");
                    }
                    catch (Exception ex)
                    {
                        log($"Got error in pProcessor: {ex.Message}");
                    }
                    keyPackets.RemoveFirst();
                }

                if (movePackets.First is not null)
                {
                    try
                    {
                        MouseEmulation.moveMouse(movePackets.First());
                        //log($"I emulated mouse move");
                    }
                    catch (Exception ex)
                    {
                        log($"Got error in pProcessor: {ex.Message}");
                    }
                    movePackets.RemoveFirst();
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
