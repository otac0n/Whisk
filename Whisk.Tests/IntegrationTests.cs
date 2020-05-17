// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Whisk.Tests
{
    using Xunit;

    public class IntegrationTests
    {
        [Fact]
        public void PureDependency_WithMultipleConsumers_SweepsConsumers()
        {
            var value1 = D.Mutable(1);
            var value2 = D.Mutable("OK");
            var combined = D.Pure(D.All(value1, value2), () => $"<<{value1.Value}: {value2.Value}>>");
            var derrivedA = D.Pure(D.All(value1, combined), () => $"DerivedA({value1.Value}, {combined.Value})");
            var derrivedB = D.Pure(D.All(combined), () => $"DerivedB({combined.Value})");

            string derrivedAWitness = null;
            string derrivedBWitness = null;
            using (derrivedA.Watch(value => derrivedAWitness = value))
            using (derrivedB.Watch(value => derrivedBWitness = value))
            {
                value1.Value = 2;
            }

            Assert.Equal(derrivedA.Value, derrivedAWitness);
            Assert.Equal(derrivedB.Value, derrivedBWitness);
        }
    }
}
