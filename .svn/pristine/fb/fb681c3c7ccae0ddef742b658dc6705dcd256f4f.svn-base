using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace LateCat.PoseidonEngine.Utilities
{
    public static class SystemInfo
    {
        public static string GetGpuInfo()
        {
            try
            {
                using var videoObject = new ManagementObjectSearcher("select * from Win32_VideoController");
                var sb = new StringBuilder();

                foreach (ManagementObject obj in videoObject.Get())
                {
                    sb.AppendLine("GPU: " + obj["Name"]);
                }

                return sb.ToString().TrimEnd();
            }
            catch (Exception e)
            {
                return "GPU: " + e.Message;
            }
        }

        public static List<string> GetGpu()
        {
            var result = new List<string>();

            try
            {
                using var myVideoObject = new ManagementObjectSearcher("select * from Win32_VideoController");
                foreach (ManagementObject obj in myVideoObject.Get())
                {
                    result.Add(obj["Name"].ToString()!);
                }
            }
            catch { }
            return result;
        }

        public static string GetCpuInfo()
        {
            try
            {
                using var myProcessorObject = new ManagementObjectSearcher("select * from Win32_Processor");
                var sb = new StringBuilder();
                foreach (ManagementObject obj in myProcessorObject.Get())
                {
                    sb.AppendLine("CPU: " + obj["Name"]);
                }
                return sb.ToString().TrimEnd();
            }
            catch (Exception e)
            {
                return "CPU: " + e.Message;
            }
        }

        public static List<string> GetCpu()
        {
            var result = new List<string>();
            try
            {
                using var myProcessorObject = new ManagementObjectSearcher("select * from Win32_Processor");
                foreach (ManagementObject obj in myProcessorObject.Get())
                {
                    result.Add(obj["Name"].ToString()!);
                }
            }
            catch { }
            return result;
        }

        public static string GetOSInfo()
        {
            try
            {
                using var myOperativeSystemObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
                var sb = new StringBuilder();
                foreach (ManagementObject obj in myOperativeSystemObject.Get())
                {
                    sb.AppendLine("OS: " + obj["Caption"] + " " + obj["Version"]);
                }
                return sb.ToString().TrimEnd();
            }
            catch (Exception e)
            {
                return "OS: " + e.Message;
            }
        }

        public static bool CheckWindowsNorKN()
        {
            var result = false;
            try
            {
                var sku = 0;
                using var myOperativeSystemObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
                foreach (ManagementObject obj in myOperativeSystemObject.Get())
                {
                    sku = int.Parse(obj["OperatingSystemSKU"].ToString()!);
                    break;
                }

                result = (sku == 5 || sku == 16 || sku == 26 || sku == 27 || sku == 28 || sku == 47 || sku == 49 || sku == 84 || sku == 122 || sku == 162);
            }
            catch { }
            return result;
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        public static long GetTotalInstalledMemory()
        {
            GetPhysicallyInstalledSystemMemory(out long memKb);
            return memKb / 1024;
        }
    }
}
