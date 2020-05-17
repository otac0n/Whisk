// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Xunit;

    public partial class PureDependencyTests
    {
        [Fact]
        public void Mark_WhenTheValueHasntBeenObserved_UnsubscribesFromDependencies()
        {
            var stubDependency = new StubDependency();

            var markedDirty = 0;
            var sweptDirty = 0;
            var pure = new PureDependency<string>(stubDependency, () => "OK");
            pure.MarkInvalidated += (sender, args) => markedDirty++;
            pure.SweepInvalidated += (sender, args) => sweptDirty++;

            // Before observing the value, there should be no subscription.
            Assert.Empty(stubDependency.Marks);
            Assert.Empty(stubDependency.Sweeps);
            var fresh = pure.TryPeek(out var peek);
            Assert.False(fresh);

            // Observing the value should return the expected string.
            Assert.Equal("OK", pure.Value);
            Assert.Equal(0, markedDirty);
            Assert.Equal(0, sweptDirty);

            // After observing the value, the subscription should be estiablished, and the value should be peek-able.
            Assert.Single(stubDependency.Marks);
            Assert.Single(stubDependency.Sweeps);
            fresh = pure.TryPeek(out peek);
            Assert.True(fresh);
            Assert.Equal("OK", peek);

            // Mark the value dirty and sweep.
            stubDependency.Marks.Single().Invoke(this, EventArgs.Empty);
            stubDependency.Sweeps.Single().Invoke(this, EventArgs.Empty);
            Assert.Equal(1, markedDirty);
            Assert.Equal(1, sweptDirty);

            // After the first dirty, the subscription should be maintianed, but it should not be fresh.
            Assert.Single(stubDependency.Marks);
            Assert.Single(stubDependency.Sweeps);
            fresh = pure.TryPeek(out peek);
            Assert.False(fresh);
            Assert.Equal("OK", peek);

            // Mark the value dirty and sweep again.
            stubDependency.Marks.Single().Invoke(this, EventArgs.Empty);
            stubDependency.Sweeps.SingleOrDefault()?.Invoke(this, EventArgs.Empty);
            Assert.Equal(1, markedDirty);
            Assert.Equal(1, sweptDirty);

            // After the second dirty, the subscription should be disposed and the references should be destroyed.
            Assert.Empty(stubDependency.Marks);
            Assert.Empty(stubDependency.Sweeps);
            fresh = pure.TryPeek(out peek);
            Assert.False(fresh);
            Assert.Null(peek);
        }

        [Fact]
        public void Value_AfterMutation_ReturnsTheExpectedValue()
        {
            var mutable = D.Mutable("ok");
            var pure = new PureDependency<string>(mutable, () => $"<<{mutable.Value.ToUpperInvariant()}>>");
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
            var depenedency = new PureDependency<string>(new StubDependency(), evaluate);

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
        public void When_GivenANullDependency_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new PureDependency<string>(null, () => "OK"));
            Assert.Equal("dependency", exception.ParamName);
        }

        [Fact]
        public void When_GivenANullEvaluateFunction_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new PureDependency<string>(new StubDependency(), null));
            Assert.Equal("evaluate", exception.ParamName);
        }
    }
}
