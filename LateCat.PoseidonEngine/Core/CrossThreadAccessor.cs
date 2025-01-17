﻿using System;
using System.Windows.Threading;

namespace LateCat.PoseidonEngine.Core
{
    internal class CrossThreadAccessor
    {
        private static Action<Action, bool> _executor =
               (action, async) => action();

        internal static void Initialize()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;

            _executor = (action, async) =>
            {
                if (dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    if (async)
                    {
                        dispatcher.BeginInvoke(action);
                    }
                    else
                    {
                        dispatcher.Invoke(action);
                    }
                }
            };
        }

        public static void Run(Action action)
        {
            _executor(action, false);
        }

        public static void RunAsync(Action action)
        {
            _executor(action, true);
        }
    }
}
