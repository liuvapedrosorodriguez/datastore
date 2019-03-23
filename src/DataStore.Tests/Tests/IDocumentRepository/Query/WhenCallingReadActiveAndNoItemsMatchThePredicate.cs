namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Collections.Generic;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveAndNoItemsMatchThePredicate
    {
        private readonly IEnumerable<Car> carsFromDatabase;

        public WhenCallingReadActiveAndNoItemsMatchThePredicate()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadActiveAndNoItemsMatchThePredicate));

            // When
            this.carsFromDatabase = testHarness.DataStore.ReadActive<Car>(car => car.id == Guid.NewGuid()).Result;
        }

        [Fact]
        public void ItShouldReturnAnEmptyResultset()
        {
            Assert.Empty(this.carsFromDatabase);
        }
    }
}