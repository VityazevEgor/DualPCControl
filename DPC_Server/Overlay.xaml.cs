using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DPC_Server.Server;
using DPC_Server.Server.Models;

namespace DPC_Server
{
    /// <summary>
    /// Логика взаимодействия для Overlay.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        public Overlay()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            SMain.sendPacket<KeyPacket>(new KeyPacket { key = e.Key, type = 1 });
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SMain.sendPacket<KeyPacket>(new KeyPacket { key = e.Key, type =2 });
        }


        private DateTime latMouseMove = DateTime.Now;
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (DateTime.Now - latMouseMove >= TimeSpan.FromMilliseconds(50))
            {
                Point pos = e.GetPosition(this);
                SMain.sendPacket<MouseMovePacket>(new MouseMovePacket { x = pos.X, y = pos.Y, formHeight = this.Height, formWidth = this.Width });
                latMouseMove = DateTime.Now;
            }
        }
    }
}
