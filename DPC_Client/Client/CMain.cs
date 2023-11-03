using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DPC_Client.Client.Models;
using System.Text.Json;
using DPC_Client.Client;

namespace DPC_Client.Client
{
    internal class CMain
    {
        private static TcpClient? client = null;
        public static List<string> logs = new List<string>();

        // пакеты, которые мы принимаем
        private static LinkedList<KeyPacket> keyPackets = new LinkedList<KeyPacket>();
        private static LinkedList<MouseMovePacket> movePackets = new LinkedList<MouseMovePacket>();
        private static LinkedList<MouseButtonPacket> mouseButtonPackets = new LinkedList<MouseButtonPacket>();
        private static LinkedList<MouseWheelPacket> mouseWheelPackets = new LinkedList<MouseWheelPacket>();
        private static LinkedList<ClipBoardPacket> clipBoardPackets = new LinkedList<ClipBoardPacket>();
        private static LinkedList<LayoutPacket> layoutPackets = new LinkedList<LayoutPacket>();

        //пакеты, которые мы отправляем
        private static LinkedList<byte[]> packets = new LinkedList<byte[]>();

        public static bool isLaunched = false;

        public static void start(string ip, int port)
        {
            Task.Run(()=>worker(ip, port));
            Task.Run(packetsProcessor);
            isLaunched=true;
        }

        public static void stop()
        {
            isLaunched=false;
            client?.Close();
            client?.Dispose();
        }

        private static async Task worker(string ip, int port)
        {
            while (isLaunched)
            {
                try
                {
                    // подготовка
                    client = new TcpClient(ip, port);
                    NetworkStream stream = client.GetStream();
                    packets.Clear();
                    await ClipBoardAPI.ClearClipboard();
                    KeyboardEmulation.clearPressedKeys();

                    // запуск всего
                    Task[] tasks = new Task[] { listener(stream), sender(stream), clipBoardGetter() };
                    log("Connected to the server!");

                    
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    log($"Can't connect to the server: {ex.Message}");
                    client?.Dispose();
                }
                await Task.Delay(10);
            }
            log("Main worker finished his work");
            
        }

        private static async Task listener(NetworkStream stream)
        {
            while (client!=null && IsConnected(client.Client))
            {
                try
                {
                    byte[] rawPacket = await readPacket(stream);
                    string rawJson = Encoding.UTF8.GetString(rawPacket, 0, rawPacket.Length);

                    Type packetType = detectPacketType(rawJson);

                    // знаю что это можно упростить, но пока оставлю так
                    if (packetType == typeof(KeyPacket))
                    {
                        keyPackets.AddLast(JsonSerializer.Deserialize<KeyPacket>(rawJson));
                        //log($"I got packet with {rawPacket.Length} bytes for key {keyPackets.Last().key}");
                    }
                    if (packetType == typeof(MouseMovePacket))
                    {
                        movePackets.AddLast(JsonSerializer.Deserialize<MouseMovePacket>(rawJson));
                        //log($"I gout mouse move packet with x={movePackets.Last().x}, y={movePackets.Last().y}, formWidth = {movePackets.Last().formWidth}");
                    }
                    if (packetType == typeof(MouseButtonPacket))
                    {
                        mouseButtonPackets.AddLast(JsonSerializer.Deserialize<MouseButtonPacket>(rawJson));
                        //log($"I gor packet for mouse button = {mouseButtonPackets.Last().mButton} with state = {mouseButtonPackets.Last().state}");
                    }
                    if (packetType == typeof(MouseWheelPacket))
                    {
                        mouseWheelPackets.AddLast(JsonSerializer.Deserialize<MouseWheelPacket>(rawJson));
                        //log($"Got mouse wheel packet with delta = {mouseWheelPackets.Last().delta}");
                    }
                    if (packetType == typeof(ClipBoardPacket))
                    {
                        clipBoardPackets.AddLast(JsonSerializer.Deserialize<ClipBoardPacket>(rawJson));
                        log($"Got clipBoard packet");
                    }
                    if (packetType == typeof(LayoutPacket))
                    {
                        layoutPackets.AddLast(JsonSerializer.Deserialize<LayoutPacket>(rawJson));
                        log($"Got layout packet with code - {layoutPackets.Last().layoutCode}");
                    }

                }
                catch (Exception ex)
                {
                    log($"Got error: {ex.Message}");
                }
                await Task.Delay(10);
            }
        }

        private static async Task sender(NetworkStream stream)
        {
            while (client != null && IsConnected(client.Client))
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
                        log($"I sent packet with len = {packet.Length}");


                        packets.RemoveFirst();
                    }
                    catch (Exception ex)
                    {
                        log($"Got error while sending packet: {ex.Message}");
                    }
                }

                await Task.Delay(10);
            }
        }


        private static async Task clipBoardGetter()
        {
            while (client != null && IsConnected(client.Client))
            {
                try
                {
                    var text = await ClipBoardAPI.GetTextN();
                    if (text != null)
                    {
                        var packet = new ClipBoardPacket { type = 1, clipBoardData = Encoding.UTF8.GetBytes(text) };
                        sendPacket(packet);
                        log($"I added cp packet");
                    }
                }
                catch (Exception ex)
                {
                    log($"Got error in cpGetter: {ex.Message}");
                }
                await Task.Delay(100);
            }
        }



        public static void sendPacket<T>(T packet)
        {
            string json = JsonSerializer.Serialize(packet);
            packets.AddLast(Encoding.UTF8.GetBytes(json));
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
                else if (root.TryGetProperty("mButton", out _))
                {
                    return typeof(MouseButtonPacket);
                }
                else if (root.TryGetProperty("delta", out _))
                {
                    return typeof(MouseWheelPacket);
                }
                else if (root.TryGetProperty("clipBoardData", out _) )
                {
                    return typeof(ClipBoardPacket);
                }
                else if (root.TryGetProperty("layoutCode", out _))
                {
                    return typeof(LayoutPacket);
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
            //log($"I got this packet: {string.Join(',', packetBuffer)}");
            return packetBuffer;
        }


        private static async Task packetsProcessor()
        {
            while (true)
            {
                ProcessPacket(keyPackets, KeyboardEmulation.SimulateKey);

                ProcessPacket(movePackets, MouseEmulation.moveMouse);

                ProcessPacket(mouseButtonPackets, MouseEmulation.mouseButton);

                ProcessPacket(mouseWheelPackets, MouseEmulation.wheel);

                ProcessPacket(clipBoardPackets, ClipBoardAPI.processClipPacketN);

                ProcessPacket(layoutPackets, LayoutAPI.setLayout);

                await Task.Delay(10);
            }
        }

        private static void ProcessPacket<T>(LinkedList<T> packets, Action<T> action)
        {
            if (packets.First is not null)
            {
                try
                {
                    action(packets.First());
                }
                catch (Exception ex)
                {
                    log($"Got error in pProcessor: {ex.Message}");
                }
                packets.RemoveFirst();
            }
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


        private static void log(string message)
        {
            if (logs.Count > 10) logs.Clear();
            logs.Add($"[{DateTime.Now}] {message}");
        }
    }
}
