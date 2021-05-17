namespace Whisk
{
    using System;

    /// <summary>
    /// Holds a reference to an object with a mutable property, presented in the form of a <see cref="IDependency{TValue}">dependency</see>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TValue">The type of the property being presented.</typeparam>
    public class EventHandlerPropertyChangeDependency<TSource, TValue> : BasePropertyChangeDependency<TSource, TValue>
        where TSource : class
    {
        private readonly Action<TSource, EventHandler> subscribe;
        private readonly Action<TSource, EventHandler> unsubscribe;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandlerPropertyChangeDependency{TSource, TValue}"/> class.
        /// </summary>
        /// <param name="source">The initial value to store.</param>
        /// <param name="get">Gets the current value of the property.</param>
        /// <param name="subscribe">Subscribes to the OnChange notifications for the property.</param>
        /// <param name="unsubscribe">Unsubscribes from the OnChange notifications for the property.</param>
        public EventHandlerPropertyChangeDependency(TSource source, Func<TSource, TValue> get, Action<TSource, EventHandler> subscribe, Action<TSource, EventHandler> unsubscribe)
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
        protected virtual void OnChanged(object sender, EventArgs args) => this.NotifyChanged();

        /// <inheritdoc/>
        protected override void Subscribe() => this.subscribe(this.Source, this.OnChanged);

        /// <inheritdoc/>
        protected override void Unsubscribe() => this.unsubscribe(this.Source, this.OnChanged);
    }
}
