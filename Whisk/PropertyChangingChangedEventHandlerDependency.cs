namespace Whisk
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Holds a reference to an object with a mutable property, presented in the form of a <see cref="IDependency{TValue}">dependency</see>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TValue">The type of the property being presented.</typeparam>
    public class PropertyChangingChangedEventHandlerDependency<TSource, TValue> : IDependency<TValue>
        where TSource : class, INotifyPropertyChanging, INotifyPropertyChanged
    {
        private readonly Func<TSource, TValue> get;
        private readonly string propertyName;
        private readonly TSource source;
        private bool changedSubscribed;
        private bool changingSubscribed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangingChangedEventHandlerDependency{TSource, TValue}"/> class.
        /// </summary>
        /// <param name="source">The source object with mutable properties.</param>
        /// <param name="get">Gets the current value of the property.</param>
        /// <param name="propertyName">The name of the property, used to filter the property change notifications.</param>
        public PropertyChangingChangedEventHandlerDependency(TSource source, Func<TSource, TValue> get, string propertyName)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.get = get ?? throw new ArgumentNullException(nameof(get));
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> MarkInvalidated
        {
            add
            {
                this.MarkInvalidatedInternal += value;
                this.EnsureChangingSubscribed();
            }

            remove
            {
                this.MarkInvalidatedInternal -= value;
                if (this.MarkInvalidatedInternal == null)
                {
                    this.EnsureChangingUnsubscribed();
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated
        {
            add
            {
                this.SweepInvalidatedInternal += value;
                this.EnsureChangedSubscribed();
            }

            remove
            {
                this.SweepInvalidatedInternal -= value;
                if (this.MarkInvalidatedInternal == null)
                {
                    this.EnsureChangedUnsubscribed();
                }
            }
        }

        private event EventHandler<EventArgs> MarkInvalidatedInternal;

        private event EventHandler<EventArgs> SweepInvalidatedInternal;

        /// <inheritdoc/>
        public TValue Value => this.get(this.source);

        /// <summary>
        /// Invokes the <see cref="SweepInvalidated"/> event if the property being changed is the expected property.
        /// </summary>
        /// <param name="sender">The sender of the original event.</param>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> of the original event.</param>
        protected virtual void PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args?.PropertyName == this.propertyName)
            {
                this.SweepInvalidatedInternal?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Invokes the <see cref="MarkInvalidated"/> event if the property being changed is the expected property.
        /// </summary>
        /// <param name="sender">The sender of the original event.</param>
        /// <param name="args">The <see cref="PropertyChangingEventArgs"/> of the original event.</param>
        protected virtual void PropertyChanging(object sender, PropertyChangingEventArgs args)
        {
            if (args?.PropertyName == this.propertyName)
            {
                this.MarkInvalidatedInternal?.Invoke(this, EventArgs.Empty);
            }
        }

        private void EnsureChangedSubscribed()
        {
            if (!this.changedSubscribed)
            {
                this.source.PropertyChanged += this.PropertyChanged;
                this.changedSubscribed = true;
            }
        }

        private void EnsureChangedUnsubscribed()
        {
            if (this.changedSubscribed)
            {
                this.source.PropertyChanged -= this.PropertyChanged;
                this.changedSubscribed = false;
            }
        }

        private void EnsureChangingSubscribed()
        {
            if (!this.changingSubscribed)
            {
                this.source.PropertyChanging += this.PropertyChanging;
                this.changingSubscribed = true;
            }
        }

        private void EnsureChangingUnsubscribed()
        {
            if (this.changingSubscribed)
            {
                this.source.PropertyChanging -= this.PropertyChanging;
                this.changingSubscribed = false;
            }
        }
    }
}
