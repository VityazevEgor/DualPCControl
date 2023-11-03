using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DPC_Server.Server;
using DPC_Server.Server.Models;

namespace DPC_Server
{
    /// <summary>
    /// Логика взаимодействия для Overlay.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        private const bool debug = false;

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        // это нужно чтобы избавиться от ошибки залипания клавиш, когда мы выходим из оверлея (успевает отправиться только с инфой о том что кнопка зажата)
        private Key _ignoreKey;

        public Overlay(System.Windows.Forms.Keys ignoreKey)
        {
            InitializeComponent();
            _ignoreKey = KeyInterop.KeyFromVirtualKey((int)ignoreKey);
            if (!debug)
            {
                this.Height = GetSystemMetrics(1);
                this.Width = GetSystemMetrics(0);
                this.WindowStyle = WindowStyle.None;
                this.Topmost = true;
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != _ignoreKey)
            {
                SMain.sendPacket(new KeyPacket { key = e.Key, type = 1 });
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != _ignoreKey)
            {
                SMain.sendPacket(new KeyPacket { key = e.Key, type = 2 });
            }
        }


        private DateTime latMouseMove = DateTime.Now;
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            // это нужно чтобы не спамить пакетами с движеним
            if (DateTime.Now - latMouseMove >= TimeSpan.FromMilliseconds(50))
            {
                Point pos = e.GetPosition(this);
                SMain.sendPacket(new MouseMovePacket { x = pos.X, y = pos.Y, formHeight = this.Height, formWidth = this.Width });
                latMouseMove = DateTime.Now;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SMain.sendPacket(new MouseButtonPacket { mButton = e.ChangedButton, state = 1 });
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SMain.sendPacket(new MouseButtonPacket { mButton = e.ChangedButton, state = 2 });
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SMain.sendPacket(new MouseWheelPacket { delta = e.Delta });
        }

        
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                // отпрвляем буфер обмена каждый раз когда начинаем управлять вторым ПК
                SMain.sendClipBoard();
                // делаем скриншот
                int monitorH = GetSystemMetrics(1);
                int monitorW = GetSystemMetrics(0);
                string newFileName = System.IO.Path.GetTempFileName();
                using (var bmp = new System.Drawing.Bitmap(monitorW, monitorH))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(monitorW, monitorH));

                        // Создание шрифта и кисти
                        using (var font = new System.Drawing.Font("Arial", 16))
                        using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
                        using (var backgroundBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
                        {
                            var text = "You are controlling second pc";
                            var point = new System.Drawing.PointF(0, 0);

                            // Получение размеров текста
                            var textSize = g.MeasureString(text, font);

                            // Рисование фона
                            g.FillRectangle(backgroundBrush, point.X, point.Y, textSize.Width, textSize.Height);

                            // Добавление надписи
                            g.DrawString(text, font, brush, point);
                        }
                    }

                    // Сохранение скриншота в файл
                    bmp.Save(newFileName, System.Drawing.Imaging.ImageFormat.Png);
                }


                // Загрузка скриншота и установка его в качестве фона формы
                var image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(newFileName, UriKind.Relative);
                image.EndInit();
                this.Background = new ImageBrush(image);
            }
        }
    }
}
