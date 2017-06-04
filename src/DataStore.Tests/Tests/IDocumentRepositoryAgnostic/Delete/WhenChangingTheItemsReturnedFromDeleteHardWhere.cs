namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Delete
{
    using System;
    using System.Linq;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemsReturnedFromDeleteHardWhere
    {
        public WhenChangingTheItemsReturnedFromDeleteHardWhere()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(
                nameof(WhenChangingTheItemsReturnedFromDeleteHardWhere
                ));

            var carId = Guid.NewGuid();
            testHarness.AddToDatabase(new Car
            {
                id = carId,
                Make = "Volvo"
            });

            var result = testHarness.DataStore.DeleteHardWhere<Car>(car => car.id == carId).Result;

            //When
            result.Single().id = Guid.NewGuid(); //change in memory before commit
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;

        [Fact]
        public async void ItShouldNotAffectTheDeleteWhenCommittedBecauseItIsCloned()
        {
            Assert.Empty(testHarness.QueryDatabase<Car>());
            Assert.Empty(await testHarness.DataStore.Read<Car>(car => car));
        }
    }
}