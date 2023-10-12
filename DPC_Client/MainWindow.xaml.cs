using DPC_Client.Client;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DPC_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private async Task logsWriter()
        {
            int prevLen = 0;
            while (true)
            {
                if (prevLen != CMain.logs.Count())
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        logsBox.Text = string.Join('\n', CMain.logs);
                    });
                    prevLen = CMain.logs.Count();
                }
                await Task.Delay(100);
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            CMain.start(ipBox.Text.Trim(), int.Parse(portBox.Text.Trim()));
            Task.Run(logsWriter);
        }
    }
}
