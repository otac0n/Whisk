// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Static class containing utility methods for creating <see cref="IDependency{T}">dependencies</see>.
    /// </summary>
    public static class D
    {
        /// <summary>
        /// Creates a composite dependency from other dependencies that can be watched for changes.
        /// </summary>
        /// <param name="dependencies">The dependencies that will be combined into a single dependency.</param>
        /// <returns>The composite dependency.</returns>
        public static IDependency All(params IDependency[] dependencies) => new CompositeDependency(dependencies);

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
        /// <typeparam name="T">The static type of value that will be returned.</typeparam>
        /// <param name="dependency">The dependency or dependencies that will be involved in the compuatation of the value.</param>
        /// <param name="evaluate">A function that will be invoked to recompute the value of the dependency.</param>
        /// <returns>A dependency that will evaluate the specified function on-demand when its own dependency has changed.</returns>
        public static PureDependency<T> Pure<T>(IDependency dependency, Func<T> evaluate) => new PureDependency<T>(dependency, evaluate);

        /// <summary>
        /// Atomically processes the <see cref="ValueUpdate">value updates</see> obtained by calling <see cref="Value{T}(MutableDependency{T}, T)"/>.
        /// </summary>
        /// <param name="setters">The list of value updates to apply atomically.</param>
        public static void Set(params ValueUpdate[] setters)
        {
            if (setters == null)
            {
                throw new ArgumentNullException(nameof(setters));
            }
            else if (setters.Any(s => s == null))
            {
                throw new ArgumentOutOfRangeException(nameof(setters));
            }

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
                this.dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
                this.value = value;
            }

            internal override void SetValue(Action rest) => this.dependency.Set(this.value, rest);
        }
    }
}
