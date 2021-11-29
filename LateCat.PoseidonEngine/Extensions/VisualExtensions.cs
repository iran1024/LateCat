using System;
using System.Windows.Media;

namespace LateCat.PoseidonEngine.Extensions
{
    public static class VisualExtensions
    {
        public static void CrossThreadAccess(this Visual visual, Action action)
            => visual.Dispatcher.Invoke(action);

        public static TResult CrossThreadAccess<TResult>(this Visual visual, Func<TResult> callback)
            => visual.Dispatcher.Invoke(callback);
    }
}