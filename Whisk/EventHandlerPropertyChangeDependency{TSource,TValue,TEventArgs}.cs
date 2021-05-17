// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Holds a reference to an object with a mutable property, presented in the form of a <see cref="IDependency{TValue}">dependency</see>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TValue">The type of the property being presented.</typeparam>
    /// <typeparam name="TEventArgs">The type of <see cref="EventArgs"/> that the notification uses.</typeparam>
    public class EventHandlerPropertyChangeDependency<TSource, TValue, TEventArgs> : BasePropertyChangeDependency<TSource, TValue>
        where TSource : class
        where TEventArgs : EventArgs
    {
        private readonly Action<TSource, EventHandler<TEventArgs>> subscribe;
        private readonly Action<TSource, EventHandler<TEventArgs>> unsubscribe;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlerPropertyChangeDependency{TSource, TValue, TEventArgs}"/> class.
        /// </summary>
        /// <param name="source">The initial value to store.</param>
        /// <param name="get">Gets the current value of the property.</param>
        /// <param name="subscribe">Subscribes to the OnChange notifications for the property.</param>
        /// <param name="unsubscribe">Unsubscribes from the OnChange notifications for the property.</param>
        public EventHandlerPropertyChangeDependency(TSource source, Func<TSource, TValue> get, Action<TSource, EventHandler<TEventArgs>> subscribe, Action<TSource, EventHandler<TEventArgs>> unsubscribe)
            : base(source, get)
        {
            this.subscribe = subscribe ?? throw new ArgumentNullException(nameof(subscribe));
            this.unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
        }

        /// <summary>
        /// Invokes the <see cref="BasePropertyChangeDependency{TSource, TValue}.NotifyChanged"/> function as the property changes.
        /// </summary>
        /// <param name="sender">The sender of the original event.</param>
        /// <param name="args">The <see cref="EventArgs"/> of the original event.</param>
        protected virtual void OnChanged(object sender, TEventArgs args) => this.NotifyChanged();

        /// <inheritdoc/>
        protected override void Subscribe() => this.subscribe(this.Source, this.OnChanged);

        /// <inheritdoc/>
        protected override void Unsubscribe() => this.unsubscribe(this.Source, this.OnChanged);
    }
}
