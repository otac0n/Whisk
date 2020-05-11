// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class PureDependencyTests
    {
        [Fact]
        public void Mark_WhenTheValueHasntBeenObserved_UnsubscribesFromDependencies()
        {
            var marks = new HandlerList();
            var sweeps = new HandlerList();
            void AddMark(EventHandler<EventArgs> handler) => marks.Add(handler);
            void RemoveMark(EventHandler<EventArgs> handler) => marks.Remove(handler);
            void AddSweep(EventHandler<EventArgs> handler) => sweeps.Add(handler);
            void RemoveSweep(EventHandler<EventArgs> handler) => sweeps.Remove(handler);

            var markedDirty = 0;
            var sweptDirty = 0;
            var pure = new PureDependency<string>(() => "OK", AddMark, RemoveMark, AddSweep, RemoveSweep);
            pure.MarkInvalidated += (sender, args) => markedDirty++;
            pure.SweepInvalidated += (sender, args) => sweptDirty++;

            // Before observing the value, there should be no subscription.
            Assert.Empty(marks);
            Assert.Empty(sweeps);
            var fresh = pure.TryPeek(out var peek);
            Assert.False(fresh);

            // Observing the value should return the expected string.
            Assert.Equal("OK", pure.Value);
            Assert.Equal(0, markedDirty);
            Assert.Equal(0, sweptDirty);

            // After observing the value, the subscription should be estiablished, and the value should be peek-able.
            Assert.NotEmpty(marks);
            Assert.NotEmpty(sweeps);
            fresh = pure.TryPeek(out peek);
            Assert.True(fresh);
            Assert.Equal("OK", peek);

            // Mark the value dirty and sweep.
            marks.Single().Invoke(this, EventArgs.Empty);
            sweeps.Single().Invoke(this, EventArgs.Empty);
            Assert.Equal(1, markedDirty);
            Assert.Equal(1, sweptDirty);

            // After the first dirty, the subscription should be maintianed, but it should not be fresh.
            Assert.NotEmpty(marks);
            Assert.NotEmpty(sweeps);
            fresh = pure.TryPeek(out peek);
            Assert.False(fresh);
            Assert.Equal("OK", peek);

            // Mark the value dirty and sweep again.
            marks.Single().Invoke(this, EventArgs.Empty);
            sweeps.SingleOrDefault()?.Invoke(this, EventArgs.Empty);
            Assert.Equal(1, markedDirty);
            Assert.Equal(1, sweptDirty);

            // After the second dirty, the subscription should be disposed and the references should be destroyed.
            Assert.Empty(marks);
            Assert.Empty(sweeps);
            fresh = pure.TryPeek(out peek);
            Assert.False(fresh);
            Assert.Null(peek);
        }

        [Fact]
        public void Value_AfterMutation_ReturnsTheExpectedValue()
        {
            var mutable = D.Mutable("ok");
            var pure = D.Pure(mutable, value => $"<<{value.ToUpperInvariant()}>>");
            Assert.Equal("<<OK>>", pure.Value);
            mutable.Set("changed");
            Assert.Equal("<<CHANGED>>", pure.Value);
        }

        private class HandlerList : List<EventHandler<EventArgs>>
        {
        }
    }
}
