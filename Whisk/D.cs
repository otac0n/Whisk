// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Static class containing utility methods for creating <see cref="IDependency{T}">dependencies</see>.
    /// </summary>
    public static class D
    {
        /// <summary>
        /// Creates a constant dependency.
        /// </summary>
        /// <typeparam name="T">The static type of the value stored.</typeparam>
        /// <param name="value">The value to store as a dependency.</param>
        /// <returns>A <see cref="ConstantDependency{T}"/> containing the specified value.</returns>
        public static ConstantDependency<T> Constant<T>(T value) => new ConstantDependency<T>(value);

        /// <summary>
        /// Creates a mutable dependency.
        /// </summary>
        /// <typeparam name="T">The static type of the value stored.</typeparam>
        /// <param name="value">The initial value to store.</param>
        /// <param name="comparer">An optional equality comparer used to determine if a value has changed.</param>
        /// <returns>A <see cref="MutableDependency{T}"/> containing the specified value.</returns>
        public static MutableDependency<T> Mutable<T>(T value = default, IEqualityComparer<T> comparer = null) => new MutableDependency<T>(value, comparer);

        /// <summary>
        /// Creates a pure computation dependency.
        /// </summary>
        /// <typeparam name="T1">The type of the values from the dependency.</typeparam>
        /// <typeparam name="TResult">The type of value that will be returned.</typeparam>
        /// <param name="d1">The dependency.</param>
        /// <param name="evaluate">A function that will be invoked to recompute the value of the dependency.</param>
        /// <returns>A dependency that will evaluate the specified function on-demand when its own dependency has changed.</returns>
        public static PureDependency<TResult> Pure<T1, TResult>(IDependency<T1> d1, Func<T1, TResult> evaluate)
        {
            if (d1 == null)
            {
                throw new ArgumentNullException(nameof(d1));
            }

            return new PureDependency<TResult>(
                () => evaluate(d1.Value),
                handler =>
                {
                    d1.MarkInvalidated += handler;
                },
                handler =>
                {
                    d1.MarkInvalidated -= handler;
                },
                handler =>
                {
                    d1.SweepInvalidated += handler;
                },
                handler =>
                {
                    d1.SweepInvalidated -= handler;
                });
        }

        /// <summary>
        /// Creates a pure computation dependency.
        /// </summary>
        /// <typeparam name="T1">The type of the values from the first dependency.</typeparam>
        /// <typeparam name="T2">The type of the values from the second dependency.</typeparam>
        /// <typeparam name="TResult">The type of value that will be returned.</typeparam>
        /// <param name="d1">The first dependency.</param>
        /// <param name="d2">The second dependency.</param>
        /// <param name="evaluate">A function that will be invoked to recompute the value of the dependency.</param>
        /// <returns>A dependency that will evaluate the specified function on-demand when any of its dependencies have changed.</returns>
        public static PureDependency<TResult> Pure<T1, T2, TResult>(IDependency<T1> d1, IDependency<T2> d2, Func<T1, T2, TResult> evaluate)
        {
            if (d1 == null)
            {
                throw new ArgumentNullException(nameof(d1));
            }

            if (d2 == null)
            {
                throw new ArgumentNullException(nameof(d2));
            }

            return new PureDependency<TResult>(
                () => evaluate(d1.Value, d2.Value),
                handler =>
                {
                    d1.MarkInvalidated += handler;
                    d2.MarkInvalidated += handler;
                },
                handler =>
                {
                    d1.MarkInvalidated -= handler;
                    d2.MarkInvalidated -= handler;
                },
                handler =>
                {
                    d1.SweepInvalidated += handler;
                    d2.SweepInvalidated += handler;
                },
                handler =>
                {
                    d1.SweepInvalidated -= handler;
                    d2.SweepInvalidated -= handler;
                });
        }

        /// <summary>
        /// Creates a pure computation dependency.
        /// </summary>
        /// <typeparam name="T1">The type of the values from the first dependency.</typeparam>
        /// <typeparam name="T2">The type of the values from the second dependency.</typeparam>
        /// <typeparam name="T3">The type of the values from the third dependency.</typeparam>
        /// <typeparam name="TResult">The type of value that will be returned.</typeparam>
        /// <param name="d1">The first dependency.</param>
        /// <param name="d2">The second dependency.</param>
        /// <param name="d3">The third dependency.</param>
        /// <param name="evaluate">A function that will be invoked to recompute the value of the dependency.</param>
        /// <returns>A dependency that will evaluate the specified function on-demand when any of its dependencies have changed.</returns>
        public static PureDependency<TResult> Pure<T1, T2, T3, TResult>(IDependency<T1> d1, IDependency<T2> d2, IDependency<T3> d3, Func<T1, T2, T3, TResult> evaluate)
        {
            if (d1 == null)
            {
                throw new ArgumentNullException(nameof(d1));
            }

            if (d2 == null)
            {
                throw new ArgumentNullException(nameof(d2));
            }

            if (d3 == null)
            {
                throw new ArgumentNullException(nameof(d3));
            }

            return new PureDependency<TResult>(
                () => evaluate(d1.Value, d2.Value, d3.Value),
                handler =>
                {
                    d1.MarkInvalidated += handler;
                    d2.MarkInvalidated += handler;
                    d3.MarkInvalidated += handler;
                },
                handler =>
                {
                    d1.MarkInvalidated -= handler;
                    d2.MarkInvalidated -= handler;
                    d3.MarkInvalidated -= handler;
                },
                handler =>
                {
                    d1.SweepInvalidated += handler;
                    d2.SweepInvalidated += handler;
                    d3.SweepInvalidated += handler;
                },
                handler =>
                {
                    d1.SweepInvalidated -= handler;
                    d2.SweepInvalidated -= handler;
                    d3.SweepInvalidated -= handler;
                });
        }

        /// <summary>
        /// Atomically processes the <see cref="ValueUpdate">value updates</see> obtained by calling <see cref="Value{T}(MutableDependency{T}, T)"/>.
        /// </summary>
        /// <param name="setters">The list of value updates to apply atomically.</param>
        public static void Set(params ValueUpdate[] setters)
        {
            var i = 0;
            void SetNext()
            {
                if (i < setters.Length)
                {
                    var setter = setters[i++];
                    setter.SetValue(SetNext);
                }
            }

            SetNext();
        }

        /// <summary>
        /// Creates a <see cref="ValueUpdate"/> that will be processed during a later call to <see cref="Set(ValueUpdate[])"/>.
        /// </summary>
        /// <typeparam name="T">The type of values stored in the dependency.</typeparam>
        /// <param name="dependency">The dependency to update.</param>
        /// <param name="value">The new value to set the dependency to.</param>
        /// <returns>An object that can be provided along with other instances to the <see cref="Set(ValueUpdate[])"/> method to update multiple values atomically.</returns>
        public static ValueUpdate Value<T>(MutableDependency<T> dependency, T value) => new ValueUpdate<T>(dependency, value);

        /// <summary>
        /// Invokes an action for each value of the dependency.
        /// </summary>
        /// <typeparam name="T">The static type of the values provided by the dependency.</typeparam>
        /// <param name="dependency">The dependency to watch.</param>
        /// <param name="action">The action to invoke for each value of the dependency.</param>
        /// <returns>An <see cref="IDisposable"/> object that can be disposed to unsubscribe from the invocations.</returns>
        public static IDisposable Watch<T>(this IDependency<T> dependency, Action<T> action)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            void Handler(object sender, EventArgs args) => action(dependency.Value);
            action(dependency.Value);
            dependency.SweepInvalidated += Handler;
            return new DisposeAction(() => dependency.SweepInvalidated -= Handler);
        }

        /// <summary>
        /// Contains a tuple of a <see cref="MutableDependency{T}"/> and a value, but erases the type information.
        /// This allows them to be managed as a collection.
        /// </summary>
        public abstract class ValueUpdate
        {
            internal ValueUpdate()
            {
            }

            internal abstract void SetValue(Action rest);
        }

        private class DisposeAction : IDisposable
        {
            private Action action;

            public DisposeAction(Action action)
            {
                this.action = action;
            }

            public void Dispose() => Interlocked.Exchange(ref this.action, null)?.Invoke();
        }

        private class ValueUpdate<T> : ValueUpdate
        {
            private readonly MutableDependency<T> dependency;
            private readonly T value;

            public ValueUpdate(MutableDependency<T> dependency, T value)
            {
                this.dependency = dependency;
                this.value = value;
            }

            internal override void SetValue(Action rest) => this.dependency.Set(this.value, rest);
        }
    }
}
