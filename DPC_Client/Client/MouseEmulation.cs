using System.Runtime.InteropServices;
using DPC_Client.Client.Models;

namespace DPC_Client.Client
{
    internal class MouseEmulation
    {
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);


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

    }
}
