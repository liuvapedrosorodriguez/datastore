namespace DataStore.Tests.Tests.IDocumentRepository.Delete
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteSoftById
    {
        private Guid carId;

        private ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            await Setup();
            Assert.False(this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Active);
            Assert.Empty(await this.testHarness.DataStore.ReadActive<Car>());
            Assert.NotEmpty(await this.testHarness.DataStore.Read<Car>());
        }

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromDeleteSoftById));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId, Make = "Volvo"
                });

            var result = await this.testHarness.DataStore.DeleteSoftById<Car>(this.carId);

            //When
            result.id = Guid.NewGuid(); //change in memory before commit
            await this.testHarness.DataStore.CommitChanges();
        }
    }
}