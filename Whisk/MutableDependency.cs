// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds a reference to a value of a specific type, presented in the form of a <see cref="Dependency{T}"/>.
    /// </summary>
    /// <typeparam name="T">The static type of the value contained.</typeparam>
    public sealed class MutableDependency<T> : Dependency<T>
    {
        private readonly IEqualityComparer<T> comparer;
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutableDependency{T}"/> class.
        /// </summary>
        /// <param name="value">The initial value to store.</param>
        /// <param name="comparer">An optional equality comparer used to determine if a value has changed.</param>
        /// <remarks>
        /// If no <paramref name="comparer"/> is specified, the default <see cref="EqualityComparer{T}"/> will be used.
        /// </remarks>
        public MutableDependency(T value = default, IEqualityComparer<T> comparer = null)
        {
            this.value = value;
            this.comparer = comparer ?? EqualityComparer<T>.Default;
        }

        /// <inheritdoc/>
        public override event EventHandler<DependencyInvalidatedEventArgs<T>> MarkInvalidated;

        /// <inheritdoc/>
        public override event EventHandler<DependencyInvalidatedEventArgs<T>> SweepInvalidated;

        /// <inheritdoc/>
        public override T Value => this.value;

        /// <summary>
        /// Updates the value stored in this reference.
        /// </summary>
        /// <param name="value">The new value to store.</param>
        /// <returns>The value stored in the reference after the store operation.</returns>
        /// <remarks>
        /// If the value is determined to be equal according to the <see cref="IEqualityComparer{T}"/> specified at creation, then the original value will be maintianed and no notification will be sent.
        /// </remarks>
        public T Set(T value)
        {
            if (!this.comparer.Equals(this.value, value))
            {
                this.MarkInvalidated?.Invoke(this, new DependencyInvalidatedEventArgs<T>());
                this.value = value;
                this.SweepInvalidated?.Invoke(this, new DependencyInvalidatedEventArgs<T>());
            }

            return this.value;
        }
    }
}
