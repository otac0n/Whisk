// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk
{
    using System;

    internal class ValueUpdate<T> : ValueUpdate
    {
        private readonly MutableDependency<T> dependency;
        private readonly T value;

        public ValueUpdate(MutableDependency<T> dependency, T value)
        {
            this.dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
            this.value = value;
        }

        internal override void SetValue(Action rest) => this.dependency.Set(this.value, rest);
    }
}
