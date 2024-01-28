// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Represents a constant, unchanging dependency with no type.
    /// </summary>
    public class NullDependency : IDependency
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="NullDependency"/>.
        /// </summary>
        public static readonly NullDependency Instance = new();

        private NullDependency()
        {
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> MarkInvalidated
        {
            add { }
            remove { }
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated
        {
            add { }
            remove { }
        }
    }
}
