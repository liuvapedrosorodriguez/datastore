namespace DataStore.Tests.Tests.IDocumentRepository.Query
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession
    {
        private  Guid activeCarId;

        private  ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingExistsOnAnItemThatHasBeenDeletedInTheCurrentSession));

            this.activeCarId = Guid.NewGuid();
            var activeExistingCar = new Car
            {
                id = this.activeCarId,
                Make = "Volvo"
            };
            this.testHarness.AddToDatabase(activeExistingCar);

            // When
            await this.testHarness.DataStore.DeleteHardById<Car>(this.activeCarId);
        }

        [Fact]
        public async void ItShouldReturnNull()
        {
            await Setup();
            Assert.Null(await this.testHarness.DataStore.ReadActiveById<Car>(this.activeCarId));
        }
    }
}