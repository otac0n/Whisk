// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using System;
    using System.ComponentModel;
    using Xunit;

    public class PropertyChangeDependencyTests
    {
        [Fact]
        public void NameChangedClassic_WhenMarkInvalidatedWhileUnsubscribed_RemovesDownstreamSubscriptions()
        {
            var stub = new PropertyChangeStub("OK");
            void MarkInvalidated(object sender, EventArgs e)
            {
            }

            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.NameChangedClassic += b, (a, b) => a.NameChangedClassic -= b);

            dependecy.MarkInvalidated += MarkInvalidated;
            Assert.True(stub.HasSubscribers);
            dependecy.MarkInvalidated -= MarkInvalidated;
            Assert.False(stub.HasSubscribers);
        }

        [Fact]
        public void NameChangedClassic_WhenNoSubscribersHaveSubscribed_ReturnsExpectedValue()
        {
            var stub = new PropertyChangeStub("OK");

            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.NameChangedClassic += b, (a, b) => a.NameChangedClassic -= b);

            Assert.Equal("OK", dependecy.Value);
        }

        [Fact]
        public void NameChangedClassic_WhenSweepInvalidatedWhileUnsubscribed_RemovesDownstreamSubscriptions()
        {
            var stub = new PropertyChangeStub("OK");
            void SweepInvalidated(object sender, EventArgs e)
            {
            }

            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.NameChangedClassic += b, (a, b) => a.NameChangedClassic -= b);

            dependecy.SweepInvalidated += SweepInvalidated;
            Assert.True(stub.HasSubscribers);
            dependecy.SweepInvalidated -= SweepInvalidated;
            Assert.False(stub.HasSubscribers);
        }

        [Fact]
        public void NameChangedClassic_WithDifferentValueWhenSubscribed_NotifiesSubscribersOfUpdatedValue()
        {
            var stub = new PropertyChangeStub("Clark Kent");
            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.NameChangedClassic += b, (a, b) => a.NameChangedClassic -= b);
            string updated = null;
            using (var subscription = D.Watch(dependecy, name => updated = name))
            {
                stub.Name = "Superman";
                Assert.Equal(stub.Name, updated);
            }
        }

        [Fact]
        public void OnNameChanged_WhenMarkInvalidatedWhileUnsubscribed_RemovesDownstreamSubscriptions()
        {
            var stub = new PropertyChangeStub("OK");
            void MarkInvalidated(object sender, EventArgs e)
            {
            }

            var dependecy = D.Property(stub, a => a.Name).Change<EventArgs>((a, b) => a.OnNameChanged += b, (a, b) => a.OnNameChanged -= b);

            dependecy.MarkInvalidated += MarkInvalidated;
            Assert.True(stub.HasSubscribers);
            dependecy.MarkInvalidated -= MarkInvalidated;
            Assert.False(stub.HasSubscribers);
        }

        [Fact]
        public void OnNameChanged_WhenNoSubscribersHaveSubscribed_ReturnsExpectedValue()
        {
            var stub = new PropertyChangeStub("OK");

            var dependecy = D.Property(stub, a => a.Name).Change<EventArgs>((a, b) => a.OnNameChanged += b, (a, b) => a.OnNameChanged -= b);

            Assert.Equal("OK", dependecy.Value);
        }

        [Fact]
        public void OnNameChanged_WhenSweepInvalidatedWhileUnsubscribed_RemovesDownstreamSubscriptions()
        {
            var stub = new PropertyChangeStub("OK");
            void SweepInvalidated(object sender, EventArgs e)
            {
            }

            var dependecy = D.Property(stub, a => a.Name).Change<EventArgs>((a, b) => a.OnNameChanged += b, (a, b) => a.OnNameChanged -= b);

            dependecy.SweepInvalidated += SweepInvalidated;
            Assert.True(stub.HasSubscribers);
            dependecy.SweepInvalidated -= SweepInvalidated;
            Assert.False(stub.HasSubscribers);
        }

        [Fact]
        public void OnNameChanged_WithDifferentValueWhenSubscribed_NotifiesSubscribersOfUpdatedValue()
        {
            var stub = new PropertyChangeStub("Clark Kent");
            var dependecy = D.Property(stub, a => a.Name).Change<EventArgs>((a, b) => a.OnNameChanged += b, (a, b) => a.OnNameChanged -= b);
            string updated = null;
            using (var subscription = D.Watch(dependecy, name => updated = name))
            {
                stub.Name = "Superman";
                Assert.Equal(stub.Name, updated);
            }
        }

        [Fact]
        public void PropertyChanged_WhenMarkInvalidatedWhileUnsubscribed_RemovesDownstreamSubscriptions()
        {
            var stub = new PropertyChangeStub("OK");
            void MarkInvalidated(object sender, EventArgs e)
            {
            }

            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.PropertyChanged += b, (a, b) => a.PropertyChanged -= b);

            dependecy.MarkInvalidated += MarkInvalidated;
            Assert.True(stub.HasSubscribers);
            dependecy.MarkInvalidated -= MarkInvalidated;
            Assert.False(stub.HasSubscribers);
        }

        [Fact]
        public void PropertyChanged_WhenNoSubscribersHaveSubscribed_ReturnsExpectedValue()
        {
            var stub = new PropertyChangeStub("OK");

            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.PropertyChanged += b, (a, b) => a.PropertyChanged -= b);

            Assert.Equal("OK", dependecy.Value);
        }

        [Fact]
        public void PropertyChanged_WhenSweepInvalidatedWhileUnsubscribed_RemovesDownstreamSubscriptions()
        {
            var stub = new PropertyChangeStub("OK");
            void SweepInvalidated(object sender, EventArgs e)
            {
            }

            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.PropertyChanged += b, (a, b) => a.PropertyChanged -= b);

            dependecy.SweepInvalidated += SweepInvalidated;
            Assert.True(stub.HasSubscribers);
            dependecy.SweepInvalidated -= SweepInvalidated;
            Assert.False(stub.HasSubscribers);
        }

        [Fact]
        public void PropertyChanged_WithDifferentValueWhenSubscribed_NotifiesSubscribersOfUpdatedValue()
        {
            var stub = new PropertyChangeStub("Clark Kent");
            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.PropertyChanged += b, (a, b) => a.PropertyChanged -= b);
            string updated = null;
            using (var subscription = D.Watch(dependecy, name => updated = name))
            {
                stub.Name = "Superman";
                Assert.Equal(stub.Name, updated);
            }
        }

        [Fact]
        public void PropertyChanged_WithFromOtherPropertyWhenSubscribed_DoesNotNotifySubscribersOfChange()
        {
            var stub = new PropertyChangeStub("Clark Kent");
            var dependecy = D.Property(stub, a => a.Name).Change((a, b) => a.PropertyChanged += b, (a, b) => a.PropertyChanged -= b);

            dependecy.SweepInvalidated += SweepInvalidated;
            void SweepInvalidated(object sender, EventArgs e)
            {
                throw new Exception("Notification unexpected.");
            }

            stub.InvokeOtherPropertyChanged();
        }

        internal class PropertyChangeStub : INotifyPropertyChanged
        {
            private string name;

            public PropertyChangeStub(string name)
            {
                this.name = name;
            }

            public event EventHandler NameChangedClassic;

            public event EventHandler<EventArgs> OnNameChanged;

            public event PropertyChangedEventHandler PropertyChanged;

            public bool HasSubscribers => this.OnNameChanged != null || this.NameChangedClassic != null || this.PropertyChanged != null;

            public string Name
            {
                get => this.name;

                set
                {
                    this.name = value;
                    this.OnNameChanged?.Invoke(this, new EventArgs());
                    this.NameChangedClassic?.Invoke(this, new EventArgs());
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Name)));
                }
            }

            public void InvokeOtherPropertyChanged()
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OtherProperty"));
            }
        }
    }
}
