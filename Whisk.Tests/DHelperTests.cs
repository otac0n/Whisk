// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Xunit;

    public class DHelperTests
    {
        public static readonly object[][] TestValues =
        {
            new object[] { 0 },
            new object[] { 1 },
            new object[] { 2 },
            new object[] { -1 },
            new object[] { 10 },
            new object[] { 1024 },
            new object[] { int.MaxValue },
            new object[] { int.MinValue },
        };

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

            D.Set(D.Value(d, unchecked(value + 1)));

            Assert.True(swept > marked);
        }

        [Theory]
        [MemberData(nameof(TestValues))]
        public void Set_WhenGivenADistinctValue_RaisesTheMarkInvalidatedEventOnce(int value)
        {
            var d = new MutableDependency<int>(value);
            var raised = 0;
            d.MarkInvalidated += (sender, args) => Interlocked.Increment(ref raised);

            D.Set(D.Value(d, unchecked(value + 1)));

            Assert.Equal(1, raised);
        }

        [Theory]
        [MemberData(nameof(TestValues))]
        public void Set_WhenGivenADistinctValue_RaisesTheSweepInvalidatedEventOnce(int value)
        {
            var d = new MutableDependency<int>(value);
            var raised = 0;
            d.SweepInvalidated += (sender, args) => Interlocked.Increment(ref raised);

            D.Set(D.Value(d, unchecked(value + 1)));

            Assert.Equal(1, raised);
        }

        [Fact]
        public void Set_WhenGivenAnEqualValue_DoesNotRaiseTheMarkInvalidatedEvent()
        {
            var d = new MutableDependency<string>("OK", StringComparer.OrdinalIgnoreCase);
            var raised = 0;
            d.MarkInvalidated += (sender, args) => Interlocked.Increment(ref raised);

            D.Set(D.Value(d, "ok"));

            Assert.Equal(0, raised);
            Assert.Equal("OK", d.Value);
        }

        [Fact]
        public void Set_WhenGivenAnEqualValue_DoesNotRaiseTheSweepInvalidatedEvent()
        {
            var d = new MutableDependency<string>("OK", StringComparer.OrdinalIgnoreCase);
            var raised = 0;
            d.SweepInvalidated += (sender, args) => Interlocked.Increment(ref raised);

            D.Set(D.Value(d, "ok"));

            Assert.Equal(0, raised);
            Assert.Equal("OK", d.Value);
        }

        [Fact]
        public void Set_WhenGivenMultipleValues_UpdatesTheValuesAtomically()
        {
            var first = D.Mutable("Reginald");
            var last = D.Mutable("Dwight");
            var full = D.Pure(new[] { first, last }, () => $"{first.Value} {last.Value}");

            var distinct = new HashSet<string>();
            using (full.Watch(v => distinct.Add(v)))
            {
                D.Set(D.Value(first, "Elton"), D.Value(last, "John"));
            }

            Assert.Equal(distinct, new HashSet<string> { "Reginald Dwight", "Elton John" });
        }

        [Fact]
        public void Watch_WhenGivenAConstantDependency_InvokesTheActionOnce()
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
        public void Watch_WhenGivenAMutableDependency_InvokesTheActionForEachUpdate()
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
                dependency.Value = "OK1"; // No change, no update.
                dependency.Value = "OK2";
                dependency.Value = "OK3";
            }

            Assert.Equal(3, invocations);
            Assert.Equal("OK3", lastValue);
        }
    }
}
