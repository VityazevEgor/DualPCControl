using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace DPC_Client
{
    internal class Settings
    {
        public string? mainPCIP {  get; set; }
        public int mainPCport { get; set; } = 1189;
        public bool runOnStartUp { get; set; } = false;
        private static string pathToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DPC_Server");
        private const string fileName = "DPC_Client_Settings.txt";
        private const string taskName = "DPC Client";

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
                            td.RegistrationInfo.Description = "DPC Client autorun";
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
                            ts.RootFolder.DeleteTask(taskName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"I can't add my self to start up: {ex.Message}\nSource:{ex.Source}");
            }
        }
    }
}
