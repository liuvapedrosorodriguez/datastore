namespace DataStore.Tests.Tests.IDocumentRepository.Update
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromUpdateWhere
    {
        private Guid carId;

        private ITestHarness testHarness;

        async Task Setup()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenChangingTheItemsReturnedFromUpdateWhere));

            this.carId = Guid.NewGuid();
            this.testHarness.AddToDatabase(
                new Car
                {
                    id = this.carId,
                    Make = "Volvo"
                });

            var results = await this.testHarness.DataStore.UpdateWhere<Car>(car => car.id == this.carId, car => car.Make = "Ford");

            //When
            foreach (var car in results) car.id = Guid.NewGuid();
            await this.testHarness.DataStore.CommitChanges();
        }

        [Fact]
        public async void ItShouldNotAffectTheUpdateWhenCommittedBecauseItIsCloned()
        {
            await Setup();
            Assert.Equal("Ford", this.testHarness.QueryDatabase<Car>(cars => cars.Where(car => car.id == this.carId)).Single().Make);
            Assert.Equal("Ford", (await this.testHarness.DataStore.ReadActiveById<Car>(this.carId)).Make);
        }
    }
}