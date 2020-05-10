// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Event raised when a dependency is invalidated.
    /// </summary>
    /// <typeparam name="T">The type of object that the dependency is tracking.</typeparam>
    public class DependencyInvalidatedEventArgs<T> : EventArgs
    {
    }
}
