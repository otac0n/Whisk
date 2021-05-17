// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Contains a tuple of a <see cref="MutableDependency{T}"/> and a value, but erases the type information.
    /// This allows them to be managed as a collection.
    /// </summary>
    public abstract class ValueUpdate
    {
        internal ValueUpdate()
        {
        }

        internal abstract void SetValue(Action rest);
    }
}
