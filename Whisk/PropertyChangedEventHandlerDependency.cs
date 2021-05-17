namespace Whisk
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Holds a reference to an object with a mutable property, presented in the form of a <see cref="IDependency{TValue}">dependency</see>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TValue">The type of the property being presented.</typeparam>
    public class PropertyChangedEventHandlerDependency<TSource, TValue> : BasePropertyChangeDependency<TSource, TValue>
        where TSource : class
    {
        private readonly string propertyName;
        private readonly Action<TSource, PropertyChangedEventHandler> subscribe;
        private readonly Action<TSource, PropertyChangedEventHandler> unsubscribe;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedEventHandlerDependency{TSource, TValue}"/> class.
        /// </summary>
        /// <param name="source">The initial value to store.</param>
        /// <param name="get">Gets the current value of the property.</param>
        /// <param name="propertyName">The name of the property, used to filter the property change notifications.</param>
        /// <param name="subscribe">Subscribes to the OnChange notifications for the property.</param>
        /// <param name="unsubscribe">Unsubscribes from the OnChange notifications for the property.</param>
        public PropertyChangedEventHandlerDependency(TSource source, Func<TSource, TValue> get, string propertyName, Action<TSource, PropertyChangedEventHandler> subscribe, Action<TSource, PropertyChangedEventHandler> unsubscribe)
            : base(source, get)
        {
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.subscribe = subscribe ?? throw new ArgumentNullException(nameof(subscribe));
            this.unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
        }

        /// <summary>
        /// Invokes the <see cref="BasePropertyChangeDependency{TSource, TValue}.NotifyChanged"/> function as the property changes.
        /// </summary>
        /// <param name="sender">The sender of the original event.</param>
        /// <param name="args">The <see cref="EventArgs"/> of the original event.</param>
        protected virtual void PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args?.PropertyName == this.propertyName)
            {
                this.NotifyChanged();
            }
        }

        /// <inheritdoc/>
        protected override void Subscribe() => this.subscribe(this.Source, this.PropertyChanged);

        /// <inheritdoc/>
        protected override void Unsubscribe() => this.unsubscribe(this.Source, this.PropertyChanged);
    }
}
