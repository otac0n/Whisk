namespace Whisk.Tests
{
    using System.Collections.Generic;
    using Xunit;

    public class NestedDependencyTests
    {
        [Fact]
        public void NestedDependency_WhenSubscribed_RespondsToNestedEvents()
        {
            var dependency = D.Mutable<Helper>();

            var output = D.Unwrap(D.Transient(dependency, h => h?.Demo.Cast<int, int?>()));

            var recorded = new List<int?>();
            using (output.Watch(recorded.Add))
            {
                var a = new Helper();

                dependency.Value = a;
                a.Demo.Value = 1;

                var b = new Helper();
                b.Demo.Value = 2;

                dependency.Value = b;

                a.Demo.Value = -1;

                dependency.Value = null;

                b.Demo.Value = -1;
            }

            Assert.Equal([null, 0, 1, 2, null], recorded);
        }

        private class Helper
        {
            public MutableDependency<int> Demo { get; } = D.Mutable<int>();
        }
    }
}
