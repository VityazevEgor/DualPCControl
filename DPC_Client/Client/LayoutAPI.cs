using System;
using System.Runtime.InteropServices;
using DPC_Client.Client.Models;

namespace DPC_Client.Client
{
    internal class LayoutAPI
    {
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;

        public static void setLayout(LayoutPacket packet)
        {
            IntPtr hwnd = GetForegroundWindow();
            IntPtr layoutId = LoadKeyboardLayout(packet.layoutCode, 0);
            PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, 0, layoutId.ToInt32());
        }
    }
}
