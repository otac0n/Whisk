// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.Collections.Generic;

    public class StubDependency : IDependency
    {
        private List<EventHandler<EventArgs>> marks = new List<EventHandler<EventArgs>>();
        private List<EventHandler<EventArgs>> sweeps = new List<EventHandler<EventArgs>>();

        public StubDependency()
        {
            this.Marks = this.marks.AsReadOnly();
            this.Sweeps = this.sweeps.AsReadOnly();
        }

        public event EventHandler<EventArgs> MarkInvalidated
        {
            add
            {
                this.marks.Add(value);
            }

            remove
            {
                this.marks.Remove(value);
            }
        }

        public event EventHandler<EventArgs> SweepInvalidated
        {
            add
            {
                this.sweeps.Add(value);
            }

            remove
            {
                this.sweeps.Remove(value);
            }
        }

        public IList<EventHandler<EventArgs>> Marks { get; }

        public IList<EventHandler<EventArgs>> Sweeps { get; }
    }
}
