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
        public MainWindow()
        {
            InitializeComponent();
            //System.Windows.MessageBox.Show(LayoutAPI.GetLayout());
            SMain.Start(1189);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Insert)
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

    }
}
