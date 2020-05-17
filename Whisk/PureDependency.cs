// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    /// <summary>
    /// Represents the outcome of a pure computation over <see cref="IDependency{T}">dependencies</see> as a dependency.
    /// </summary>
    /// <typeparam name="T">The static type of the result of the pure computation.</typeparam>
    public class PureDependency<T> : IDependency<T>
    {
        private const int UnsubscribeThreshold = 1;
        private readonly Action<EventHandler<EventArgs>> addMark;
        private readonly Action<EventHandler<EventArgs>> addSweep;
        private readonly Func<T> evaluate;
        private readonly Action<EventHandler<EventArgs>> removeMark;
        private readonly Action<EventHandler<EventArgs>> removeSweep;
        private int redundantDirtyCount;
        private State state;
        private bool subscribed;
        private T value;

        internal PureDependency(Func<T> evaluate, Action<EventHandler<EventArgs>> addMark, Action<EventHandler<EventArgs>> removeMark, Action<EventHandler<EventArgs>> addSweep, Action<EventHandler<EventArgs>> removeSweep)
        {
            this.state = State.Swept;
            this.evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
            this.addMark = addMark ?? throw new ArgumentNullException(nameof(addMark));
            this.removeMark = removeMark ?? throw new ArgumentNullException(nameof(removeMark));
            this.addSweep = addSweep ?? throw new ArgumentNullException(nameof(addSweep));
            this.removeSweep = removeSweep ?? throw new ArgumentNullException(nameof(removeSweep));
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> MarkInvalidated;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated;

        private enum State : byte
        {
            Swept = 0,
            Fresh,
            Dirty,
        }

        /// <inheritdoc/>
        public T Value
        {
            get
            {
                if (this.state != State.Fresh)
                {
                    this.EnsureSubscribed();
                    this.value = this.evaluate();
                    this.state = State.Fresh;
                    this.redundantDirtyCount = 0;
                }

                return this.value;
            }
        }

        /// <summary>
        /// Attempts to retrieve the value stored, returning a value indicating whether or not the attempt succeeded.
        /// </summary>
        /// <param name="value">This parameter will be set to the observed value.</param>
        /// <returns>A value indicating whether or not the observed value is fresh.</returns>
        /// <remarks>
        /// Internally, the <see cref="PureDependency{T}"/> may reset the value of the stored value to allow efficient garbage collection.
        /// As such, the value must be validated whenever the return value is <c>false</c>.
        /// </remarks>
        public bool TryPeek(out T value)
        {
            value = this.value;
            return this.state == State.Fresh;
        }

        private void EnsureSubscribed()
        {
            if (!this.subscribed)
            {
                this.addMark(this.Mark);
                this.addSweep(this.Sweep);
                this.subscribed = true;
                this.redundantDirtyCount = 0;
            }
        }

        private void EnsureUnsubscribed()
        {
            if (this.subscribed)
            {
                this.removeMark(this.Mark);
                this.removeSweep(this.Sweep);
                this.subscribed = false;
                this.redundantDirtyCount = 0;
                this.value = default;
                this.state = State.Swept;
            }
        }

        private void Mark(object sender, EventArgs args)
        {
            switch (this.state)
            {
                case State.Swept:
                    this.redundantDirtyCount++;
                    if (this.redundantDirtyCount >= UnsubscribeThreshold)
                    {
                        this.EnsureUnsubscribed();
                    }

                    break;

                case State.Fresh:
                    this.state = State.Dirty;
                    this.MarkInvalidated?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private void Sweep(object sender, EventArgs args)
        {
            switch (this.state)
            {
                case State.Dirty:
                    this.state = State.Swept;
                    this.SweepInvalidated?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
