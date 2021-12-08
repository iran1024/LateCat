using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace LateCat.Helpers
{
    class PipeServer
    {
        public event EventHandler<string[]>? MessageReceived;
        public static string MessageDelimiter { get; } = "_mm_";

        public PipeServer(string channelName)
        {
            CreateRemoteService(channelName).ConfigureAwait(false);
        }

        private async Task CreateRemoteService(string channelName)
        {
            await using var pipeServer = new NamedPipeServerStream(channelName, PipeDirection.In);

            while (true)
            {
                await pipeServer.WaitForConnectionAsync().ConfigureAwait(false);

                using var reader = new StreamReader(pipeServer);

                var rawArgs = await reader.ReadToEndAsync();

                MessageReceived?.Invoke(this, rawArgs.Split(MessageDelimiter));

                pipeServer.Disconnect();
            }
        }
    }
}
