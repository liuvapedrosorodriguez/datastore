namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadOnAnItemWhichHasBeenDeletedInTheCurrentSession
    {
        private readonly Car carFromSession;

        private readonly ITestHarness testHarness;

        public WhenCallingReadOnAnItemWhichHasBeenDeletedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadOnAnItemWhichHasBeenDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                Id = carId,
                Active = false,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            this.testHarness.DataStore.DeleteHardById<Car>(carId).Wait();

            // When
            this.carFromSession = this.testHarness.DataStore.Read<Car>(car => car.Id == carId).Result.SingleOrDefault();
        }

        [Fact]
        public void ItShouldNotReturnThatItem()
        {
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Single(this.testHarness.QueryDatabase<Car>());
            Assert.Null(this.carFromSession);
        }
    }
}