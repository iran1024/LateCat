using System;
using System.IO;
using System.IO.Pipes;

namespace LateCat.Helpers
{
    class PipeServer
    {
        public event EventHandler<string[]>? MessageReceived;
        public static string MessageDelimiter { get; } = "_mm_";

        public PipeServer(string channelName)
        {
            CreateRemoteService(channelName);
        }

        private async void CreateRemoteService(string channelName)
        {
            using var pipeServer = new NamedPipeServerStream(channelName, PipeDirection.In);

            while (true)
            {
                await pipeServer.WaitForConnectionAsync().ConfigureAwait(false);

                var reader = new StreamReader(pipeServer);
                var rawArgs = await reader.ReadToEndAsync();

                MessageReceived?.Invoke(this, rawArgs.Split(MessageDelimiter));

                pipeServer.Disconnect();
            }
        }
    }
}
