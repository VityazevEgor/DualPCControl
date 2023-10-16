using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using DPC_Client.Client.Models;

namespace DPC_Client.Client
{
    internal class KeyboardEmulation
    {
        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        // исправление залипания клавиш
        private static List<int> currentlyDownKeys = new List<int>();
        public static void SimulateKey(KeyPacket packet)
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(packet.key);
            if (packet.type == 1 && !currentlyDownKeys.Contains(virtualKeyCode))
            {
                keybd_event((byte)virtualKeyCode, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
                currentlyDownKeys.Add(virtualKeyCode);
            }
            if (packet.type == 2)
            {
                keybd_event((byte)virtualKeyCode, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                currentlyDownKeys.Remove(virtualKeyCode);
            }
        }

        public static void clearPressedKeys()
        {
            currentlyDownKeys.ForEach(key =>
            {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            });
        }
    }
}
