// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Represents the outcome of a pure computation over <see cref="IDependency{T}">dependencies</see> as a dependency.
    /// </summary>
    /// <typeparam name="T">The static type of the result of the pure computation.</typeparam>
    public class TransientDependency<T> : IDependency<T>
    {
        private readonly IDependency dependency;
        private readonly Func<T> evaluate;

        internal TransientDependency(IDependency dependency, Func<T> evaluate)
        {
            this.dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
            this.evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> MarkInvalidated
        {
            add => this.dependency.MarkInvalidated += value;
            remove => this.dependency.MarkInvalidated -= value;
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated
        {
            add => this.dependency.SweepInvalidated += value;
            remove => this.dependency.SweepInvalidated -= value;
        }

        /// <inheritdoc/>
        public T Value => this.evaluate();
    }
}
