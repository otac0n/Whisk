// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Xunit;

    public partial class TransientDependencyTests
    {
        [Fact]
        public void Value_AfterMutation_ReturnsTheExpectedValue()
        {
            var mutable = D.Mutable("ok");
            var transient = new TransientDependency<string>(mutable, () => $"<<{mutable.Value.ToUpperInvariant()}>>");
            Assert.Equal("<<OK>>", transient.Value);
            mutable.Value = "changed";
            Assert.Equal("<<CHANGED>>", transient.Value);
        }

        [Fact]
        public void Value_WhenInvokedRepeatedly_InvokesTheEvaluationFunctionEachTime()
        {
            var invocations = 0;
            var evaluate = new Func<string>(() =>
            {
                Interlocked.Increment(ref invocations);
                return "OK";
            });
            var depenedency = new TransientDependency<string>(new StubDependency(), evaluate);

            var values = new HashSet<string>
            {
                depenedency.Value,
                depenedency.Value,
                depenedency.Value,
            };

            Assert.Equal(3, invocations);
            Assert.Equal("OK", values.Single());
        }

        [Fact]
        public void Constructor_WhenGivenANullDependency_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new TransientDependency<string>(null, () => "OK"));
            Assert.Equal("dependency", exception.ParamName);
        }

        [Fact]
        public void Constructor_WhenGivenANullEvaluateFunction_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new TransientDependency<string>(new StubDependency(), null));
            Assert.Equal("evaluate", exception.ParamName);
        }
    }
}
