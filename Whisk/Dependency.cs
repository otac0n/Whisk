// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The base clas for dependencies.
    /// </summary>
    /// <typeparam name="T">The type of object the dependency is tracking.</typeparam>
    public abstract class Dependency<T>
    {
        /// <summary>
        /// Event raised to mark consumers of a dependency as stale.
        /// </summary>
        /// <remarks>
        /// No action should be take to update the value, as other dependencies may still be stale.
        /// This function may be throttled if no sweep operation has happened since the last time the dependency raised this event.
        /// </remarks>
        public abstract event EventHandler<DependencyInvalidatedEventArgs<T>> MarkInvalidated;

        /// <summary>
        /// Event raised to allow consumers to perform updates after all stale values have been marked.
        /// </summary>
        public abstract event EventHandler<DependencyInvalidatedEventArgs<T>> SweepInvalidated;

        /// <summary>
        /// Gets the value of the dependency.
        /// </summary>
        public abstract T Value { get; }

        /// <summary>
        /// Creates a constant dependency for the specified value.
        /// </summary>
        /// <param name="value">The constant value.</param>
        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Provided via the `D` static class' `Constant` method.")]
        public static implicit operator Dependency<T>(T value) => new ConstantDependency<T>(value);
    }
}
