using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DPC_Server.Server;
using Gma.System.MouseKeyHook;

namespace DPC_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents m_GlobalHook;
        private Overlay ov = new Overlay();
        private Settings? settings = null;
        private DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            settings = Settings.load();
            if (settings == null)
            {
                settings = new Settings();
            }
            updateSettingsBoxes();
            if (settings.runOnStartUp)
            {
                runButton_Click(null, null);
            }

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;

        }

        private void updateSettingsBoxes()
        {
            overlayButtonBox.Text = settings.overlayKey.ToString();
            serverPortBox.Text = settings.port.ToString();
            startUpBox.IsChecked = settings.runOnStartUp;
        }


        private void OnKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == settings.overlayKey)
            {
                if (ov.IsVisible)
                {
                    ov.Hide();
                }
                else
                {
                    ov.Show();
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            logsBox.Text = string.Join('\n', SMain.logs);
        }

        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            if (!SMain.isLaunched)
            {
                m_GlobalHook = Hook.GlobalEvents();
                m_GlobalHook.KeyDown += OnKeyDown;
                
                timer.Start();


                if (int.TryParse(serverPortBox.Text, out int port))
                {
                    settings.port = port;
                }
                else
                {
                    System.Windows.MessageBox.Show($"Entered port is not valid number");
                    return;
                }
                settings.setStartUp();
                settings.save();
                SMain.Start(settings.port);

                runButton.Content = "Stop server";
            }
            else
            {
                SMain.Stop();
                runButton.Content = "Run server";
                m_GlobalHook.KeyDown -= OnKeyDown;
                m_GlobalHook.Dispose();
                timer.Stop();
            }
        }

        private void overlayButtonBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            settings.overlayKey = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            updateSettingsBoxes();
        }

        // блокируем введённый текст в бокс с выбором кнопки
        private void overlayButtonBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void startUpBox_Checked(object sender, RoutedEventArgs e)
        {
            settings.runOnStartUp = true;
        }

        private void startUpBox_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.runOnStartUp = false;
        }
    }
}
