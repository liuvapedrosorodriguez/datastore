namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Models.Messages;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession
    {
        private Car carFromSession;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveOnAnItemSoftDeletedInTheCurrentSession));

            var carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = carId,
                Active = false,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(existingCar);

            await this.testHarness.DataStore.DeleteSoftById<Car>(carId);

            // When
            this.carFromSession = (await this.testHarness.DataStore.ReadActive<Car>(car => car.id == carId)).SingleOrDefault();
        }

        [Fact]
        public async void ItShouldNotReturnThatItem()
        {
            await Setup();
            Assert.NotNull(this.testHarness.DataStore.ExecutedOperations.SingleOrDefault(e => e is AggregatesQueriedOperation<Car>));
            Assert.Single(this.testHarness.QueryDatabase<Car>());
            Assert.Null(this.carFromSession);
        }
    }
}