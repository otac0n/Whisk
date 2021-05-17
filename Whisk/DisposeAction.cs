// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;
    using System.Threading;

    internal class DisposeAction : IDisposable
    {
        private Action action;

        public DisposeAction(Action action)
        {
            this.action = action;
        }

        public void Dispose() => Interlocked.Exchange(ref this.action, null)?.Invoke();
    }
}
