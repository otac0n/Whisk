namespace Whisk.Tests
{
    using System.Collections.Generic;
    using Xunit;

    public class NestedDependencyTests
    {
        [Fact]
        public void NestedDependency_WhenSubscribed_RespondsToNestedEvents()
        {
            var dependency = new MutableDependency<Helper>();

            var output = D.Unwrap(D.Pure(dependency, v => v?.Demo));

            var recorded = new List<string>();
            using (output.Watch(recorded.Add))
            {
                var a = new Helper();

                dependency.Value = a;
                a.Demo.Value = "OK A";

                var b = new Helper();
                b.Demo.Value = "OK B";

                dependency.Value = b;

                a.Demo.Value = "NAK";

                dependency.Value = null;

                b.Demo.Value = "NAK";
            }

            Assert.Equal(new List<string> { null, null, "OK A", "OK B", null }, recorded);
        }

        private class Helper
        {
            private MutableDependency<string> demo;

            public Helper()
            {
                this.demo = D.Mutable<string>();
            }

            public MutableDependency<string> Demo => this.demo;
        }
    }
}
