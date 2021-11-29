using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel;

namespace LateCat.Helpers
{
    public static class WindowsStartup
    {
        public static void SetStartupRegistry(bool setStartup = false)
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            Assembly curAssembly = Assembly.GetExecutingAssembly();
            if (setStartup)
            {
                try
                {
                    key.SetValue(curAssembly.GetName().Name, "\"" + Path.ChangeExtension(curAssembly.Location, ".exe") + "\"");
                }
                catch
                {

                }
            }
            else
            {
                try
                {
                    key.DeleteValue(curAssembly.GetName().Name, false);
                }
                catch
                {

                }
            }

            key.Close();
        }


        public static string GetStartupRegistry()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly curAssembly = Assembly.GetExecutingAssembly();
            string result = null;
            try
            {
                result = (string)key.GetValue(curAssembly.GetName().Name);
            }
            catch
            {

            }
            finally
            {
                key.Close();
            }
            return result;
        }

        public static int CheckStartupRegistry()
        {
            int status;
            var startupKey = GetStartupRegistry();
            if (string.IsNullOrEmpty(startupKey))
            {
                status = 0;
            }
            else if (string.Equals(startupKey, "\"" + Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe") + "\"", StringComparison.Ordinal))
            {
                status = 1;
            }
            else
            {
                status = -1;
            }
            return status;
        }

        public async static Task StartupWin10(bool setStartup = false)
        {
            StartupTask startupTask = await StartupTask.GetAsync("AppStartup");
            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    if (setStartup)
                    {
                        StartupTaskState newState = await startupTask.RequestEnableAsync();
                    }
                    break;
                case StartupTaskState.DisabledByUser:
                    if (setStartup)
                    {
                        await Task.Run(() => MessageBox.Show("您已禁用Late Cat的开机启动项，但如果您改变主意，可以在任务管理器的 “启动” 选项卡中启用此功能。",
                            Properties.Resources.TextError,
                            MessageBoxButton.OK));
                    }
                    break;
                case StartupTaskState.DisabledByPolicy:
                    break;
                case StartupTaskState.Enabled:
                    if (!setStartup)
                    {
                        startupTask.Disable();
                    }
                    break;
            }
        }

        public async static Task<int> StartupCheck()
        {
            var result = 0;
            StartupTask startupTask = await StartupTask.GetAsync("AppStartup");
            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    result = 0;
                    break;
                case StartupTaskState.DisabledByUser:
                    result = -1;
                    break;
                case StartupTaskState.DisabledByPolicy:
                    result = -1;
                    break;
                case StartupTaskState.Enabled:
                    result = 1;
                    break;
            }
            return result;
        }
    }
}
