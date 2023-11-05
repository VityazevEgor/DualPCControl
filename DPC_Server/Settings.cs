using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private const string taskName = "DPC Server";

        // добовляет .dll файл а не exe
        public void setStartUp()
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                System.Diagnostics.Debug.WriteLine(exePath);
                using (TaskService ts = new TaskService())
                {
                    if (runOnStartUp)
                    {
                        if (ts.RootFolder.AllTasks.FirstOrDefault(t => t.Name == taskName) is null)
                        {
                            TaskDefinition td = ts.NewTask();
                            td.RegistrationInfo.Description = "DPC Server autorun";
                            td.Principal.RunLevel = TaskRunLevel.Highest; // Админ права на запуск

                            LogonTrigger trigger = new LogonTrigger();
                            td.Triggers.Add(trigger);

                            td.Actions.Add(new ExecAction(exePath, null, null));

                            ts.RootFolder.RegisterTaskDefinition(taskName, td);
                        }
                    }
                    else
                    {
                        if (ts.RootFolder.AllTasks.FirstOrDefault(t => t.Name == taskName) is not null)
                        {
                            ts.RootFolder.DeleteTask("DPC Server");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"I can't add my self to start up: {ex.Message}\nSource:{ex.Source}");
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
            if (!Directory.Exists(pathToSave) || !File.Exists(Path.Combine(pathToSave, fileName)))
            {
                return null;
            }

            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(pathToSave, fileName)));
        }
    }
}
