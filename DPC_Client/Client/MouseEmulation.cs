using System.Runtime.InteropServices;
using System.Windows.Input;
using DPC_Client.Client.Models;

namespace DPC_Client.Client
{
    internal class MouseEmulation
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        public static void moveMouse(MouseMovePacket packet)
        {
            int screenWidth = GetSystemMetrics(0); // SM_CXSCREEN = 0
            int screenHeight = GetSystemMetrics(1); // SM_CYSCREEN = 1

            // Рассчитайте координаты курсора на основе размеров экрана и пакета
            int x = (int)(packet.x / packet.formWidth * screenWidth);
            int y = (int)(packet.y / packet.formHeight * screenHeight);

            // Установите позицию курсора
            SetCursorPos(x, y);

        }



        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;
        public static void mouseButton(MouseButtonPacket packet)
        {
            uint buttonDown = 0;
            uint buttonUp = 0;


            switch (packet.mButton)
            {
                case MouseButton.Left:
                    buttonDown = MOUSEEVENTF_LEFTDOWN;
                    buttonUp = MOUSEEVENTF_LEFTUP;
                    break;
                case MouseButton.Right:
                    buttonDown = MOUSEEVENTF_RIGHTDOWN;
                    buttonUp = MOUSEEVENTF_RIGHTUP;
                    break;
                case MouseButton.Middle:
                    buttonDown = MOUSEEVENTF_MIDDLEDOWN;
                    buttonUp = MOUSEEVENTF_MIDDLEUP;
                    break;
            }

            if (packet.state == 1) // Если кнопка мыши нажата
            {
                mouse_event(buttonDown, 0, 0, 0, 0);
            }
            else if (packet.state == 2) // Если кнопка мыши отпущена
            {
                mouse_event(buttonUp, 0, 0, 0, 0);
            }
        }

        private const int MOUSEEVENTF_WHEEL = 0x0800;
        public static void wheel(MouseWheelPacket packet)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)packet.delta, 0);
        }

    }
}
