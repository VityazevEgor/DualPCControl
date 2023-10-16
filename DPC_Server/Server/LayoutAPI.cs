using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DPC_Server.Server
{
    internal class LayoutAPI
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint thread);

        private static int GetCurrentKeyboardLayout()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                uint foregroundProcess = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
                int keyboardLayout = GetKeyboardLayout(foregroundProcess).ToInt32() & 0xFFFF;

                if (keyboardLayout == 0)
                {
                    // что-то пошло не так. тогда возвращаем инглиш
                    keyboardLayout = 1033;
                }
                return keyboardLayout;
            }
            catch (Exception ex)
            {
                // что-то пошло не так. тогда возвращаем инглиш
                return 1033;
            }
        }

        public static string GetLayout()
        {
            return GetCurrentKeyboardLayout().ToString("X8");
        }
    }
}
