namespace Whisk
{
    using System;
    using System.Linq;

    internal class CompositeDependency : IDependency
    {
        private readonly IDependency[] dependencies;
        private bool subscribedMark = false;
        private bool subscribedSweep = false;

        public CompositeDependency(params IDependency[] dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }
            else if (dependencies.Any(s => s == null))
            {
                throw new ArgumentOutOfRangeException(nameof(dependencies));
            }

            this.dependencies = dependencies.ToArray();
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> MarkInvalidated
        {
            add
            {
                this.MarkInvalidatedInternal += value;
                if (!this.subscribedMark)
                {
                    for (var i = this.dependencies.Length - 1; i >= 0; i--)
                    {
                        this.dependencies[i].MarkInvalidated += this.Mark;
                    }

                    this.subscribedMark = true;
                }
            }

            remove
            {
                this.MarkInvalidatedInternal -= value;
                if (this.subscribedSweep && this.MarkInvalidatedInternal == null)
                {
                    for (var i = this.dependencies.Length - 1; i >= 0; i--)
                    {
                        this.dependencies[i].MarkInvalidated -= this.Mark;
                    }

                    this.subscribedMark = false;
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> SweepInvalidated
        {
            add
            {
                this.SweepInvalidatedInternal += value;
                if (!this.subscribedSweep)
                {
                    for (var i = this.dependencies.Length - 1; i >= 0; i--)
                    {
                        this.dependencies[i].SweepInvalidated += this.Sweep;
                    }

                    this.subscribedSweep = true;
                }
            }

            remove
            {
                this.SweepInvalidatedInternal -= value;
                if (this.subscribedSweep && this.SweepInvalidatedInternal == null)
                {
                    for (var i = this.dependencies.Length - 1; i >= 0; i--)
                    {
                        this.dependencies[i].SweepInvalidated -= this.Sweep;
                    }

                    this.subscribedSweep = false;
                }
            }
        }

        private event EventHandler<EventArgs> MarkInvalidatedInternal;

        private event EventHandler<EventArgs> SweepInvalidatedInternal;

        private void Mark(object sender, EventArgs e) => this.MarkInvalidatedInternal?.Invoke(sender, e);

        private void Sweep(object sender, EventArgs e) => this.SweepInvalidatedInternal?.Invoke(sender, e);
    }
}
