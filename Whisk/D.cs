// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System.Collections.Generic;

    /// <summary>
    /// Static class containing utility methods for creating <see cref="Dependency{T}">dependencies</see>.
    /// </summary>
    public static class D
    {
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
    }
}
