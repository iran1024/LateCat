using System.IO;
using System.IO.Pipes;
using System.Text;

namespace LateCat.Helpers
{
    class PipeClient
    {
        public static void SendMessage(string channelName, string[] msg)
        {
            using var pipeClient = new NamedPipeClientStream(".", channelName, PipeDirection.Out);

            pipeClient.Connect(0);

            var sb = new StringBuilder();

            foreach (var item in msg)
            {
                sb.Append(item);
                sb.Append(PipeServer.MessageDelimiter);
            }

            sb.Remove(sb.Length - PipeServer.MessageDelimiter.Length, PipeServer.MessageDelimiter.Length);

            using var writer = new StreamWriter(pipeClient) { AutoFlush = true };

            writer.Write(sb.ToString());

            writer.Flush();
            writer.Close();

            pipeClient.Dispose();
        }

        public static void SendMessage(string channelName, string msg)
        {
            using var pipeClient = new NamedPipeClientStream(".", channelName, PipeDirection.Out);

            pipeClient.Connect(0);

            using var writer = new StreamWriter(pipeClient) { AutoFlush = true };

            writer.Write(msg);

            writer.Flush();
            writer.Close();

            pipeClient.Dispose();
        }
    }
}