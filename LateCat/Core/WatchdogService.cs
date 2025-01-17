﻿using LateCat.PoseidonEngine.Abstractions;
using System;
using System.Diagnostics;
using System.IO;

namespace LateCat.Core
{
    public class WatchdogService : IWatchdogService
    {
        private Process _subProcess;

        public WatchdogService()
        {

        }

        public void Add(int pid)
        {
            SendMessage("LateCat:add-pgm " + pid);
        }

        public void Clear()
        {
            SendMessage("LateCat:clear");
        }

        public void Remove(int pid)
        {
            SendMessage("LateCat:rm-pgm " + pid);
        }

        public void Start()
        {
            if (_subProcess != null)
                return;

            var start = new ProcessStartInfo()
            {
                Arguments = Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "subproc", "LateCat.SubProcess.exe"),
                RedirectStandardInput = true,
                UseShellExecute = false,
            };

            _subProcess = new Process
            {
                StartInfo = start,
            };

            try
            {
                _subProcess.Start();
            }
            catch
            {

            }
        }

        private void SendMessage(string text)
        {
            try
            {
                _subProcess.StandardInput.WriteLine(text);
            }
            catch
            {

            }
        }
    }
}
