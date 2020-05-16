// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds a reference to a value of a specific type, presented in the form of a <see cref="IDependency{T}">dependency</see>.
    /// </summary>
    /// <typeparam name="T">The static type of the value contained.</typeparam>
    public sealed class MutableDependency<T> : IDependency<T>
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
        public event EventHandler<EventArgs> MarkInvalidated;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated;

        /// <inheritdoc/>
        public T Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (!this.comparer.Equals(this.value, value))
                {
                    this.MarkInvalidated?.Invoke(this, EventArgs.Empty);
                    this.value = value;
                    this.SweepInvalidated?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        internal void Set(T value, Action rest = null)
        {
            if (!this.comparer.Equals(this.value, value))
            {
                this.MarkInvalidated?.Invoke(this, EventArgs.Empty);
                this.value = value;
                rest?.Invoke();
                this.SweepInvalidated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                rest?.Invoke();
            }
        }
    }
}
