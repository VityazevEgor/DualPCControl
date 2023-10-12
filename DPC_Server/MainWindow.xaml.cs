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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DPC_Server.Server;

namespace DPC_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SMain.Start(1189);

            

            Task.Run(logsWriter);

            var ov = new Overlay();
            ov.Show();
        }

        private async Task logsWriter()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        logsBox.Document.Blocks.Clear();
                        foreach (string log in SMain.logs)
                        {
                            logsBox.Document.Blocks.Add(new Paragraph(new Run(log)));
                        }
                    });
                }
            });
        }

    }
}
