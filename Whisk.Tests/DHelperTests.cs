// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.Threading;
    using Xunit;

    public class DHelperTests
    {
        [Fact]
        public void Watch_GivenAConstantDependency_InvokesTheActionOnce()
        {
            var dependency = D.Constant("OK");
            var invocations = 0;
            var lastValue = default(string);
            var action = new Action<string>(value =>
            {
                Interlocked.Increment(ref invocations);
                lastValue = value;
            });

            using (dependency.Watch(action))
            {
            }

            Assert.Equal(1, invocations);
            Assert.Equal("OK", lastValue);
        }

        [Fact]
        public void Watch_GivenAMutableDependency_InvokesTheActionForEachUpdate()
        {
            var dependency = D.Mutable("OK1");
            var invocations = 0;
            var lastValue = default(string);
            var action = new Action<string>(value =>
            {
                Interlocked.Increment(ref invocations);
                lastValue = value;
            });

            using (dependency.Watch(action))
            {
                dependency.Set("OK1"); // No change, no update.
                dependency.Set("OK2");
                dependency.Set("OK3");
            }

            Assert.Equal(3, invocations);
            Assert.Equal("OK3", lastValue);
        }
    }
}
