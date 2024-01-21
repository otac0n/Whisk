namespace Whisk.Tests
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Xunit;

    public class NestedDependencyTests
    {
        [Fact]
        public void NestedDependency_WhenSubscribed_RespondsToNestedEvents()
        {
            var dependency = new MutableDependency<Helper>();
            NestedDependency<Helper> output = D.Nested(() => dependency.Value.Property);

            var recorded = new List<string>();
            using (output.Watch(recorded.Add))
            {
                var a = new Helper();

                dependency.Value = a;
                a.Property = "OK A";

                var b = new Helper
                {
                    Property = "OK B",
                };

                dependency.Value = b;

                a.Property = "NAK";

                dependency.Value = null;

                b.Property = "NAK";
            }

            Assert.Equal(recorded, new List<string> { null, "OK A", "OK B", null });
        }

        private class Helper : INotifyPropertyChanged
        {
            private string property;

            public event PropertyChangedEventHandler PropertyChanged;

            public string Property
            {
                get => this.property;
                set
                {
                    if (this.property != value)
                    {
                        this.property = value;
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Property)));
                    }
                }
            }
        }
    }
}
