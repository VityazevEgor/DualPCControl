using DPC_Server.Server.Models;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DPC_Server.Server
{
    internal class ClibBoardAPI
    {
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern bool CloseClipboard();

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EmptyClipboard();

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;


        public static void ClearClipboard()
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                EmptyClipboard();
                CloseClipboard();
            }
        }

        private static void SetText(string text)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                var chars = (text.Length + 1) * 2;
                var hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)chars);

                if (hGlobal != IntPtr.Zero)
                {
                    var lpwcstr = GlobalLock(hGlobal);
                    Marshal.Copy(text.ToCharArray(), 0, lpwcstr, text.Length);
                    GlobalUnlock(hGlobal);

                    if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
                    {
                        // Если SetClipboardData не удалось, освождаем
                        Marshal.FreeHGlobal(hGlobal);
                    }
                }

                CloseClipboard();
            }
        }

        public static void processClipPacket(ClipBoardPacket packet)
        {
            if (packet.type == 1)
            {
                SetText(Encoding.UTF8.GetString(packet.clipBoardData));
            }
        }

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern bool IsClipboardFormatAvailable(uint format);
        public static bool ContainsText()
        {
            return IsClipboardFormatAvailable(CF_UNICODETEXT);
        }

        public static string GetText()
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                var ptr = GetClipboardData(CF_UNICODETEXT);
                var text = Marshal.PtrToStringUni(ptr);
                CloseClipboard();
                return text;
            }

            return null;
        }
    }
}
