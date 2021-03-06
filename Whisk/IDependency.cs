// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// The interface for dependencies which allows for mark/sweep notification without type information.
    /// </summary>
    public interface IDependency
    {
        /// <summary>
        /// Event raised to mark consumers of a dependency as stale.
        /// </summary>
        /// <remarks>
        /// No action should be take to update the value, as other dependencies may still be stale.
        /// This function may be throttled if no sweep operation has happened since the last time the dependency raised this event.
        /// </remarks>
        event EventHandler<EventArgs> MarkInvalidated;

        /// <summary>
        /// Event raised to allow consumers to perform updates after all stale values have been marked.
        /// </summary>
        event EventHandler<EventArgs> SweepInvalidated;
    }
}
