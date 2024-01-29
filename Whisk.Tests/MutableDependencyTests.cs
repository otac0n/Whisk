// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.Threading;
    using Xunit;

    public class MutableDependencyTests
    {
        public static readonly object[][] TestValues =
        [
            [0],
            [1],
            [2],
            [-1],
            [10],
            [1024],
            [int.MaxValue],
            [int.MinValue],
        ];

        [Theory]
        [MemberData(nameof(TestValues))]
        public void Constructor_WhenGivenASpecificValue_ReturnsAMutableDependencyWithTheSpecifiedValue(int value)
        {
            var d = new MutableDependency<int>(value);
            Assert.Equal(value, d.Value);
        }

        [Fact]
        public void Constructor_WhenGivenDefaults_ReturnsAMutableDependencyWithThisDefaultValue()
        {
            var d = new MutableDependency<int>();
            Assert.Equal(default, d.Value);
        }

        [Theory]
        [MemberData(nameof(TestValues))]
        public void Set_WhenGivenADistinctValue_RaisesMarkBeforeSweep(int value)
        {
            var d = new MutableDependency<int>(value);
            var order = 1;
            var marked = 0;
            var swept = 0;
            d.MarkInvalidated += (sender, args) => marked = order++;
            d.SweepInvalidated += (sender, args) => swept = order++;

            d.Value = unchecked(value + 1);

            Assert.True(swept > marked);
        }

        [Theory]
        [MemberData(nameof(TestValues))]
        public void Set_WhenGivenADistinctValue_RaisesTheMarkInvalidatedEventOnce(int value)
        {
            var d = new MutableDependency<int>(value);
            var raised = 0;
            d.MarkInvalidated += (sender, args) => Interlocked.Increment(ref raised);

            d.Value = unchecked(value + 1);

            Assert.Equal(1, raised);
        }

        [Theory]
        [MemberData(nameof(TestValues))]
        public void Set_WhenGivenADistinctValue_RaisesTheSweepInvalidatedEventOnce(int value)
        {
            var d = new MutableDependency<int>(value);
            var raised = 0;
            d.SweepInvalidated += (sender, args) => Interlocked.Increment(ref raised);

            d.Value = unchecked(value + 1);

            Assert.Equal(1, raised);
        }

        [Fact]
        public void Set_WhenGivenAnEqualValue_DoesNotRaiseTheMarkInvalidatedEvent()
        {
            var d = new MutableDependency<string>("OK", StringComparer.OrdinalIgnoreCase);
            var raised = 0;
            d.MarkInvalidated += (sender, args) => Interlocked.Increment(ref raised);

            d.Value = "ok";

            Assert.Equal(0, raised);
            Assert.Equal("OK", d.Value);
        }

        [Fact]
        public void Set_WhenGivenAnEqualValue_DoesNotRaiseTheSweepInvalidatedEvent()
        {
            var d = new MutableDependency<string>("OK", StringComparer.OrdinalIgnoreCase);
            var raised = 0;
            d.SweepInvalidated += (sender, args) => Interlocked.Increment(ref raised);

            d.Value = "ok";

            Assert.Equal(0, raised);
            Assert.Equal("OK", d.Value);
        }
    }
}
