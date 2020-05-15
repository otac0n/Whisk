// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Holds an immutable reference to a specific value, presented in the form of a <see cref="IDependency{T}">dependency</see>.
    /// </summary>
    /// <typeparam name="T">The static type of the value contained.</typeparam>
    public class ConstantDependency<T> : IDependency<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantDependency{T}"/> class.
        /// </summary>
        /// <param name="value">The constant value.</param>
        public ConstantDependency(T value)
        {
            this.Value = value;
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> MarkInvalidated
        {
            add { }
            remove { }
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated
        {
            add { }
            remove { }
        }

        /// <inheritdoc/>
        public T Value { get; }
    }
}
