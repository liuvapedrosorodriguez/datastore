namespace DataStore.Tests.Tests.IDocumentRepositoryAgnostic.Create
{
    using System;
    using System.Linq;
    using Models;
    using TestHarness;
    using Xunit;

    public class WhenChangingTheItemReturnedFromCreate
    {
        public WhenChangingTheItemReturnedFromCreate()
        {
            // Given
            testHarness = TestHarnessFunctions.GetTestHarness(nameof(WhenChangingTheItemReturnedFromCreate));

            newCarId = Guid.NewGuid();

            var newCar = new Car
            {
                id = newCarId,
                Make = "Volvo"
            };

            var result = testHarness.DataStore.Create(newCar).Result;

            //change the id before committing, if not cloned this would cause the item to be created with a different id
            result.id = Guid.NewGuid();

            //When
            testHarness.DataStore.CommitChanges().Wait();
        }

        private readonly ITestHarness testHarness;
        private readonly Guid newCarId;

        [Fact]
        public void ItShouldNotAffectTheCreateWhenCommittedBecauseItIsCloned()
        {
            Assert.True(testHarness.QueryDatabase<Car>().Single().id == newCarId);
            Assert.NotNull(testHarness.DataStore.ReadActiveById<Car>(newCarId).Result);
        }
    }
}