// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    /// <summary>
    /// The common interface for value dependencies.
    /// </summary>
    /// <typeparam name="T">The type of object the dependency is tracking.</typeparam>
    public interface IDependency<out T> : IDependency
    {
        /// <summary>
        /// Gets the value of the dependency.
        /// </summary>
        T Value { get; }
    }
}
