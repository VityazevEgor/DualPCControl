using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;

namespace DPC_Server
{
    internal class Settings
    {
        public Keys overlayKey { get; set; } = System.Windows.Forms.Keys.Insert;
        public bool runOnStartUp { get; set; } = false;
        public int port { get; set; } = 1189;

        public bool hideWindowAfterStart { get; set; } = false;

        private static string pathToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DPC_Server");
        private const string fileName = "DPC_Server_Settings.txt";

        // добовляет .dll файл а не exe
        public void setStartUp()
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                RegistryKey? rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk != null)
                {
                    if (runOnStartUp == true)
                    {
                        rk.SetValue("DPC_Server", exePath);
                    }
                    else
                    {
                        rk.DeleteValue("DPC_Server", false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"I can't add my self to start up: {ex.Message}");
            }
        }


        public void save()
        {
            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }

            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(Path.Combine(pathToSave, fileName), json);
        }

        public static Settings load()
        {
            if (!Directory.Exists(pathToSave) && !File.Exists(Path.Combine(pathToSave, fileName)))
            {
                return null;
            }

            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(pathToSave, fileName)));
        }
    }
}
