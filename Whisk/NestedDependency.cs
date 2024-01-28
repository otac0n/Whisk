// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Holds a reference to a nested dependency.
    /// </summary>
    /// <typeparam name="T">The static type of the nested value contained.</typeparam>
    public sealed class NestedDependency<T> : IDependency<T>
    {
        private readonly IDependency<IDependency<T>> nested;
        private IDependency<T> value;
        private bool subscribed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedDependency{T}"/> class.
        /// </summary>
        /// <param name="nested">The nested dependency to track.</param>
        public NestedDependency(IDependency<IDependency<T>> nested)
        {
            this.nested = nested ?? throw new ArgumentNullException(nameof(nested));
            this.nested.MarkInvalidated += this.Nested_MarkInvalidated;
            this.nested.SweepInvalidated += this.Nested_SweepInvalidated;
            this.Nested_SweepInvalidated(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> MarkInvalidated;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated;

        /// <inheritdoc/>
        public T Value => this.value is IDependency<T> dependency
            ? dependency.Value
            : default;

        private void Nested_MarkInvalidated(object sender, EventArgs e)
        {
            if (this.subscribed)
            {
                this.value.MarkInvalidated -= this.Value_MarkInvalidated;
                this.value.SweepInvalidated -= this.Value_SweepInvalidated;
                this.subscribed = false;
            }

            this.MarkInvalidated?.Invoke(sender, e);
        }

        private void Nested_SweepInvalidated(object sender, EventArgs e)
        {
            this.value = this.nested.Value;

            if (this.value is IDependency<T> dependency)
            {
                this.value.MarkInvalidated += this.Value_MarkInvalidated;
                this.value.SweepInvalidated += this.Value_SweepInvalidated;
                this.subscribed = true;
            }

            this.SweepInvalidated?.Invoke(sender, e);
        }

        private void Value_MarkInvalidated(object sender, EventArgs e)
        {
            this.MarkInvalidated?.Invoke(sender, e);
        }

        private void Value_SweepInvalidated(object sender, EventArgs e)
        {
            this.SweepInvalidated?.Invoke(sender, e);
        }
    }
}
