// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Xunit;

    public class PureDependencyTests
    {
        [Fact]
        public void Mark_WhenTheValueHasntBeenObserved_UnsubscribesFromDependencies()
        {
            var marks = new HandlerList();
            var sweeps = new HandlerList();

            var markedDirty = 0;
            var sweptDirty = 0;
            var pure = new PureDependency<string>(() => "OK", marks.Add, marks.Remove, sweeps.Add, sweeps.Remove);
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
            var pure = D.Pure(new[] { mutable }, () => $"<<{mutable.Value.ToUpperInvariant()}>>");
            Assert.Equal("<<OK>>", pure.Value);
            mutable.Value = "changed";
            Assert.Equal("<<CHANGED>>", pure.Value);
        }

        [Fact]
        public void Value_WhenInvokedRepeatedly_OnlyInvokesTheEvaluationFunctionOnce()
        {
            var invocations = 0;
            var evaluate = new Func<string>(() =>
            {
                Interlocked.Increment(ref invocations);
                return "OK";
            });
            var depenedency = new PureDependency<string>(evaluate, _ => { }, _ => { }, _ => { }, _ => { });

            var values = new HashSet<string>
            {
                depenedency.Value,
                depenedency.Value,
                depenedency.Value,
            };

            Assert.Equal(1, invocations);
            Assert.Equal("OK", values.Single());
        }

        [Fact]
        public void When_GivenANullAddMarkFunction_ThrowsArgumentNullException()
        {
            var marks = new HandlerList();
            var sweeps = new HandlerList();
            var exception = Assert.Throws<ArgumentNullException>(() => new PureDependency<string>(() => "OK", null, marks.Remove, sweeps.Add, sweeps.Remove));
            Assert.Equal("addMark", exception.ParamName);
        }

        [Fact]
        public void When_GivenANullAddSweepFunction_ThrowsArgumentNullException()
        {
            var marks = new HandlerList();
            var sweeps = new HandlerList();
            var exception = Assert.Throws<ArgumentNullException>(() => new PureDependency<string>(() => "OK", marks.Add, marks.Remove, null, sweeps.Remove));
            Assert.Equal("addSweep", exception.ParamName);
        }

        [Fact]
        public void When_GivenANullEvaluateFunction_ThrowsArgumentNullException()
        {
            var marks = new HandlerList();
            var sweeps = new HandlerList();
            var exception = Assert.Throws<ArgumentNullException>(() => new PureDependency<string>(null, marks.Add, marks.Remove, sweeps.Add, sweeps.Remove));
            Assert.Equal("evaluate", exception.ParamName);
        }

        [Fact]
        public void When_GivenANullRemoveMarkFunction_ThrowsArgumentNullException()
        {
            var marks = new HandlerList();
            var sweeps = new HandlerList();
            var exception = Assert.Throws<ArgumentNullException>(() => new PureDependency<string>(() => "OK", marks.Add, null, sweeps.Add, sweeps.Remove));
            Assert.Equal("removeMark", exception.ParamName);
        }

        [Fact]
        public void When_GivenANullRemoveSweepFunction_ThrowsArgumentNullException()
        {
            var marks = new HandlerList();
            var sweeps = new HandlerList();
            var exception = Assert.Throws<ArgumentNullException>(() => new PureDependency<string>(() => "OK", marks.Add, marks.Remove, sweeps.Add, null));
            Assert.Equal("removeSweep", exception.ParamName);
        }

        private class HandlerList : List<EventHandler<EventArgs>>
        {
            public new void Remove(EventHandler<EventArgs> item)
            {
                base.Remove(item);
            }
        }
    }
}
