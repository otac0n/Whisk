// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Holds a reference to an object with a mutable property, presented in the form of a <see cref="IDependency{TValue}">dependency</see>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TValue">The type of the property being presented.</typeparam>
    public abstract class BasePropertyChangeDependency<TSource, TValue> : IDependency<TValue>
        where TSource : class
    {
        private readonly Func<TSource, TValue> get;
        private bool subscribed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePropertyChangeDependency{TSource, TValue}"/> class.
        /// </summary>
        /// <param name="source">The initial value to store.</param>
        /// <param name="get">Gets the current value of the property.</param>
        public BasePropertyChangeDependency(TSource source, Func<TSource, TValue> get)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.get = get ?? throw new ArgumentNullException(nameof(get));
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> MarkInvalidated
        {
            add
            {
                this.MarkInvalidatedInternal += value;
                this.EnsureSubscribed();
            }

            remove
            {
                this.MarkInvalidatedInternal -= value;
                this.UnsubscribeIfPossible();
            }
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated
        {
            add
            {
                this.SweepInvalidatedInternal += value;
                this.EnsureSubscribed();
            }

            remove
            {
                this.SweepInvalidatedInternal -= value;
                this.UnsubscribeIfPossible();
            }
        }

        private event EventHandler<EventArgs> MarkInvalidatedInternal;

        private event EventHandler<EventArgs> SweepInvalidatedInternal;

        /// <inheritdoc/>
        public TValue Value => this.get(this.Source);

        /// <summary>
        /// Gets the source object.
        /// </summary>
        protected TSource Source { get; private set; }

        /// <summary>
        /// Allows derived classes to inform subscribers that the value has changed.
        /// </summary>
        protected void NotifyChanged()
        {
            this.MarkInvalidatedInternal?.Invoke(this, EventArgs.Empty);
            this.SweepInvalidatedInternal?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// When implemented by a derived class, subscribes to the source object's notifications.
        /// </summary>
        protected abstract void Subscribe();

        /// <summary>
        /// When implemented by a derived class, unsubscribes from the source object's notifications.
        /// </summary>
        protected abstract void Unsubscribe();

        private void EnsureSubscribed()
        {
            if (!this.subscribed)
            {
                this.Subscribe();
                this.subscribed = true;
            }
        }

        private void EnsureUnsubscribed()
        {
            if (this.subscribed)
            {
                this.Unsubscribe();
                this.subscribed = false;
            }
        }

        private void UnsubscribeIfPossible()
        {
            if (this.MarkInvalidatedInternal == null && this.SweepInvalidatedInternal == null)
            {
                this.EnsureUnsubscribed();
            }
        }
    }
}
