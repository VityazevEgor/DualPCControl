using DPC_Client.Client;
using System;
using System.Windows;
using System.Windows.Threading;

namespace DPC_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private Settings? settings = null;
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();

            // загрузка настроек
            settings = Settings.load();
            if (settings == null)
            {
                settings = new Settings();
            }
            updateSettingsBoxes();

            if (settings.runOnStartUp)
            {
                startButton_Click(null, null);
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            logsBox.Text = string.Join('\n', CMain.logs);
        }
        private void updateSettingsBoxes()
        {
            ipBox.Text = settings?.mainPCIP;
            portBox.Text = settings?.mainPCport.ToString();
            startUpBox.IsChecked = settings?.runOnStartUp;
        }

        private void startUpBox_Checked(object sender, RoutedEventArgs e)
        {
            settings.runOnStartUp = true;
        }

        private void startUpBox_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.runOnStartUp = false;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CMain.isLaunched)
            {
                if (int.TryParse(portBox.Text, out int port))
                {
                    settings.mainPCport = port;
                }
                else
                {
                    MessageBox.Show($"Entered port is not valid number");
                    return;
                }
                settings.mainPCIP = ipBox.Text.Trim();
                settings.setStartUp();
                settings.save();
                CMain.start(settings.mainPCIP, settings.mainPCport);

                startButton.Content = "Disconect";
            }
            else
            {
                CMain.stop();
                startButton.Content = "Connect";
            }
        }
    }
}
