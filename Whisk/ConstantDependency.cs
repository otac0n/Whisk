// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Holds an immutable reference to a specific object, presented in the form of a <see cref="Dependency{T}"/>.
    /// </summary>
    /// <typeparam name="T">The static type of the object contained.</typeparam>
    public class ConstantDependency<T> : Dependency<T>
    {
        private readonly T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantDependency{T}"/> class.
        /// </summary>
        /// <param name="value">The constant value.</param>
        public ConstantDependency(T value)
        {
            this.value = value;
        }

        /// <inheritdoc/>
        public override event EventHandler<DependencyInvalidatedEventArgs<T>> MarkInvalidated
        {
            add { }
            remove { }
        }

        /// <inheritdoc/>
        public override event EventHandler<DependencyInvalidatedEventArgs<T>> SweepInvalidated
        {
            add { }
            remove { }
        }

        /// <inheritdoc/>
        public override T Value => this.value;
    }
}
