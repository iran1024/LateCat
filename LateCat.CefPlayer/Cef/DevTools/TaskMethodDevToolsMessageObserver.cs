using CefSharp;
using CefSharp.Callback;
using System.IO;

namespace LateCat.CefPlayer
{
    public class TaskMethodDevToolsMessageObserver : IDevToolsMessageObserver
    {
        private readonly TaskCompletionSource<Tuple<bool, byte[]>> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly int matchMessageId;

        public TaskMethodDevToolsMessageObserver(int messageId)
        {
            matchMessageId = messageId;
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
        }

        void IDevToolsMessageObserver.OnDevToolsAgentAttached(IBrowser browser)
        {

        }

        void IDevToolsMessageObserver.OnDevToolsAgentDetached(IBrowser browser)
        {

        }

        void IDevToolsMessageObserver.OnDevToolsEvent(IBrowser browser, string method, Stream parameters)
        {

        }

        bool IDevToolsMessageObserver.OnDevToolsMessage(IBrowser browser, Stream message)
        {
            return false;
        }

        void IDevToolsMessageObserver.OnDevToolsMethodResult(IBrowser browser, int messageId, bool success, Stream result)
        {
            if (matchMessageId == messageId)
            {
                var memoryStream = new MemoryStream((int)result.Length);

                result.CopyTo(memoryStream);

                var response = Tuple.Create(success, memoryStream.ToArray());

                taskCompletionSource.TrySetResult(response);
            }
        }

        public Task<Tuple<bool, byte[]>> Task => taskCompletionSource.Task;
    }
}
