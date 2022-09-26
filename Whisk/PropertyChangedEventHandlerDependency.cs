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
        where TSource : class, INotifyPropertyChanged
    {
        private readonly string propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedEventHandlerDependency{TSource, TValue}"/> class.
        /// </summary>
        /// <param name="source">The source object with mutable properties.</param>
        /// <param name="get">Gets the current value of the property.</param>
        /// <param name="propertyName">The name of the property, used to filter the property change notifications.</param>
        public PropertyChangedEventHandlerDependency(TSource source, Func<TSource, TValue> get, string propertyName)
            : base(source, get)
        {
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
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
        protected override void Subscribe() => this.Source.PropertyChanged += this.PropertyChanged;

        /// <inheritdoc/>
        protected override void Unsubscribe() => this.Source.PropertyChanged -= this.PropertyChanged;
    }
}
