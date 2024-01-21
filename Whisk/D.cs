// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Static class containing utility methods for creating <see cref="IDependency{T}">dependencies</see>.
    /// </summary>
    public static class D
    {
        /// <summary>
        /// Creates a composite dependency from other dependencies that can be watched for changes.
        /// </summary>
        /// <param name="dependencies">The dependencies that will be combined into a single dependency.</param>
        /// <returns>The composite dependency.</returns>
        public static IDependency All(params IDependency[] dependencies) =>
            dependencies.Length == 1 ? dependencies[0] : new CompositeDependency(dependencies);

        /// <summary>
        /// Creates a constant dependency.
        /// </summary>
        /// <typeparam name="T">The static type of the value stored.</typeparam>
        /// <param name="value">The value to store as a dependency.</param>
        /// <returns>A <see cref="ConstantDependency{T}"/> containing the specified value.</returns>
        public static ConstantDependency<T> Constant<T>(T value) => new ConstantDependency<T>(value);

        /// <summary>
        /// Creates a mutable dependency.
        /// </summary>
        /// <typeparam name="T">The static type of the value stored.</typeparam>
        /// <param name="value">The initial value to store.</param>
        /// <param name="comparer">An optional equality comparer used to determine if a value has changed.</param>
        /// <returns>A <see cref="MutableDependency{T}"/> containing the specified value.</returns>
        public static MutableDependency<T> Mutable<T>(T value = default, IEqualityComparer<T> comparer = null) => new MutableDependency<T>(value, comparer);

        /// <summary>
        /// Creates a property change dependency using a notification event.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TValue">The type of the property being presented.</typeparam>
        /// <param name="source">The source object with mutable properties.</param>
        /// <param name="get">Gets the current value of the property.</param>
        /// <returns>A <see cref="PropertyBuilder{TSource, TValue}"/> object that can be used to select and configure the required property dependency.</returns>
        public static PropertyBuilder<TSource, TValue> Property<TSource, TValue>(TSource source, Func<TSource, TValue> get)
            where TSource : class
        {
            return new PropertyBuilder<TSource, TValue>(source, get);
        }

        /// <summary>
        /// Creates a property change dependency using <see cref="INotifyPropertyChanged"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TValue">The type of the property being presented.</typeparam>
        /// <param name="source">The source object with mutable properties.</param>
        /// <param name="get">Gets the current value of the property.</param>
        /// <returns>A <see cref="PropertyChangedEventHandlerDependency{TSource, TValue}"/> presenting the specified property from the source.</returns>
        /// <remarks>
        /// To avoid any performance penalty, consider directly invoking the desired object constructor with all of the type parameters specified.
        /// These overloads are provided for developer convenience.
        /// </remarks>
        public static IDependency<TValue> PropertyChanged<TSource, TValue>(this TSource source, Expression<Func<TSource, TValue>> get)
            where TSource : class, INotifyPropertyChanged
        {
            if (get == null)
            {
                throw new ArgumentNullException(nameof(get));
            }

            if (get.Body is MemberExpression getMember && getMember.Expression == get.Parameters[0] && (getMember.Member is FieldInfo || getMember.Member is PropertyInfo))
            {
                return PropertyDependencyFactory<TSource, TValue>.Factory(source, get.Compile(), getMember.Member.Name);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(get));
            }
        }

        /// <summary>
        /// Creates a pure computation dependency.
        /// </summary>
        /// <typeparam name="T">The static type of value that will be returned.</typeparam>
        /// <param name="dependency">The dependency or dependencies that will be involved in the compuatation of the value.</param>
        /// <param name="evaluate">A function that will be invoked to recompute the value of the dependency.</param>
        /// <returns>A dependency that will evaluate the specified function on-demand when its own dependency has changed.</returns>
        public static PureDependency<T> Pure<T>(IDependency dependency, Func<T> evaluate) => new PureDependency<T>(dependency, evaluate);

        /// <summary>
        /// Atomically processes the <see cref="ValueUpdate">value updates</see> obtained by calling <see cref="Value{T}(MutableDependency{T}, T)"/>.
        /// </summary>
        /// <param name="setters">The list of value updates to apply atomically.</param>
        public static void Set(params ValueUpdate[] setters)
        {
            if (setters == null)
            {
                throw new ArgumentNullException(nameof(setters));
            }
            else if (setters.Any(s => s == null))
            {
                throw new ArgumentOutOfRangeException(nameof(setters));
            }

            var i = 0;
            void SetNext()
            {
                if (i < setters.Length)
                {
                    var setter = setters[i++];
                    setter.SetValue(SetNext);
                }
            }

            SetNext();
        }

        /// <summary>
        /// Creates a <see cref="ValueUpdate"/> that will be processed during a later call to <see cref="Set(ValueUpdate[])"/>.
        /// </summary>
        /// <typeparam name="T">The type of values stored in the dependency.</typeparam>
        /// <param name="dependency">The dependency to update.</param>
        /// <param name="value">The new value to set the dependency to.</param>
        /// <returns>An object that can be provided along with other instances to the <see cref="Set(ValueUpdate[])"/> method to update multiple values atomically.</returns>
        public static ValueUpdate Value<T>(MutableDependency<T> dependency, T value) => new ValueUpdate<T>(dependency, value);

        /// <summary>
        /// Invokes an action for each value of the dependency.
        /// </summary>
        /// <typeparam name="T">The static type of the values provided by the dependency.</typeparam>
        /// <param name="dependency">The dependency to watch.</param>
        /// <param name="action">The action to invoke for each value of the dependency.</param>
        /// <returns>An <see cref="IDisposable"/> object that can be disposed to unsubscribe from the invocations.</returns>
        public static IDisposable Watch<T>(this IDependency<T> dependency, Action<T> action)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException(nameof(dependency));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            void Handler(object sender, EventArgs args) => action(dependency.Value);
            action(dependency.Value);
            dependency.SweepInvalidated += Handler;
            return new DisposeAction(() => dependency.SweepInvalidated -= Handler);
        }

        /// <summary>
        /// A builder supporting simpler C# syntax.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TValue">The type of the property being presented.</typeparam>
        public struct PropertyBuilder<TSource, TValue>
            where TSource : class
        {
            private readonly Func<TSource, TValue> get;
            private readonly TSource source;

            internal PropertyBuilder(TSource source, Func<TSource, TValue> get)
            {
                this.source = source ?? throw new ArgumentNullException(nameof(source));
                this.get = get ?? throw new ArgumentNullException(nameof(get));
            }

            /// <summary>
            /// Creates a property change dependency.
            /// </summary>
            /// <typeparam name="TEventArgs">The type of <see cref="EventArgs"/> that the notification uses.</typeparam>
            /// <param name="subscribe">Subscribes to the OnChange notifications for the property.</param>
            /// <param name="unsubscribe">Unsubscribes from the OnChange notifications for the property.</param>
            /// <returns>An <see cref="EventHandlerPropertyChangeDependency{TSource, TValue, TEventArgs}"/> presenting the specified property from the source.</returns>
            public EventHandlerPropertyChangeDependency<TSource, TValue, TEventArgs> Changed<TEventArgs>(Action<TSource, EventHandler<TEventArgs>> subscribe, Action<TSource, EventHandler<TEventArgs>> unsubscribe)
                where TEventArgs : EventArgs
            {
                return new EventHandlerPropertyChangeDependency<TSource, TValue, TEventArgs>(this.source, this.get, subscribe, unsubscribe);
            }

            /// <summary>
            /// Creates a property change dependency.
            /// </summary>
            /// <param name="subscribe">Subscribes to the OnChange notifications for the property.</param>
            /// <param name="unsubscribe">Unsubscribes from the OnChange notifications for the property.</param>
            /// <returns>An <see cref="EventHandlerPropertyChangeDependency{TSource, TValue}"/> presenting the specified property from the source.</returns>
            public EventHandlerPropertyChangeDependency<TSource, TValue> Changed(Action<TSource, EventHandler> subscribe, Action<TSource, EventHandler> unsubscribe)
            {
                return new EventHandlerPropertyChangeDependency<TSource, TValue>(this.source, this.get, subscribe, unsubscribe);
            }
        }

        private static class PropertyDependencyFactory<TSource, TValue>
            where TSource : class, INotifyPropertyChanged
        {
            static PropertyDependencyFactory()
            {
                var sourceParam = Expression.Parameter(typeof(TSource), "source");
                var getParam = Expression.Parameter(typeof(Func<TSource, TValue>), "get");
                var propertyNameParam = Expression.Parameter(typeof(string), "propertyName");

                var targetType = typeof(INotifyPropertyChanging).IsAssignableFrom(typeof(TSource))
                    ? typeof(PropertyChangingChangedEventHandlerDependency<,>).MakeGenericType(typeof(TSource), typeof(TValue))
                    : typeof(PropertyChangedEventHandlerDependency<TSource, TValue>);

                var expr = Expression.Lambda<Func<TSource, Func<TSource, TValue>, string, IDependency<TValue>>>(
                    Expression.New(
                        targetType.GetConstructor(new Type[]
                        {
                            typeof(TSource),
                            typeof(Func<TSource, TValue>),
                            typeof(string),
                        }),
                        sourceParam,
                        getParam,
                        propertyNameParam),
                    sourceParam,
                    getParam,
                    propertyNameParam);
                Factory = expr.Compile();
            }

            public static Func<TSource, Func<TSource, TValue>, string, IDependency<TValue>> Factory { get; }
        }
    }
}
