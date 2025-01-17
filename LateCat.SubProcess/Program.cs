﻿using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

    private const uint SPI_SETDESKTOPWALLPAPER = 20;
    private const uint SPIF_UPDATEINIFILE = 0x1;

    private readonly static List<int> _wpItems = new();

    private static void Main(string[] args)
    {
        int pid;
        Process lateCat;

        if (args.Length == 1)
        {
            try
            {
                pid = Convert.ToInt32(args[0], 10);
            }
            catch
            {
                return;
            }
        }
        else
        {
            return;
        }

        try
        {
            lateCat = Process.GetProcessById(pid);
        }
        catch
        {
            return;
        }

        ListenToParent();

        lateCat.WaitForExit();

        foreach (var item in _wpItems)
        {
            try
            {
                Process.GetProcessById(item).Kill();
            }
            catch
            {

            }
        }


        _ = SystemParametersInfo(SPI_SETDESKTOPWALLPAPER, 0, null, SPIF_UPDATEINIFILE);
    }

    private static async void ListenToParent()
    {
        try
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    var text = await Console.In.ReadLineAsync();

                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (text.Equals("LateCat:clear", StringComparison.OrdinalIgnoreCase))
                    {
                        _wpItems.Clear();
                    }
                    else if (text.Contains("LateCat:add-pgm", StringComparison.OrdinalIgnoreCase))
                    {
                        var msg = text.Split(' ');
                        if (int.TryParse(msg[1], out var value))
                        {
                            _wpItems.Add(value);
                        }
                    }
                    else if (text.Contains("LateCat:rm-pgm", StringComparison.OrdinalIgnoreCase))
                    {
                        var msg = text.Split(' ');
                        if (int.TryParse(msg[1], out var value))
                        {
                            _wpItems.Remove(value);
                        }
                    }
                }
            });
        }
        catch
        {

        }
    }
}