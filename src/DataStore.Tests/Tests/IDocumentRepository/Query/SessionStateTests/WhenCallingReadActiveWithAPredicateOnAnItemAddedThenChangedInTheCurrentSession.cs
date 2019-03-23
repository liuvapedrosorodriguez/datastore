namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
{
    using System;
    using System.Linq;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Xunit;

    public class WhenCallingReadActiveWithAPredicateOnAnItemAddedThenChangedInTheCurrentSession
    {
        private readonly ITestHarness testHarness;

        private readonly Guid fordId;

        public WhenCallingReadActiveWithAPredicateOnAnItemAddedThenChangedInTheCurrentSession()
        {
            // Given
            this.testHarness = TestHarness.Create(nameof(WhenCallingReadActiveByIdOnAnItemAddedInTheCurrentSession));

            this.testHarness.AddToDatabase(new Car
            {
                Id = Guid.NewGuid(),
                Active = true,
                Make = "Lambo"
            });

            this.testHarness.DataStore.Create(
                new Car
                {
                    Id = Guid.NewGuid(),
                    Active = true,
                    Make = "Volvo"
                }).Wait();

            this.testHarness.DataStore.Create(
                new Car
                {
                    Id = Guid.NewGuid(),
                    Active = true,
                    Make = "Mazda"
                }).Wait();

            this.fordId = Guid.NewGuid();

            this.testHarness.DataStore.Create(
                new Car
                {
                    Id = this.fordId,
                    Active = true,
                    Make = "Ford"
                }).Wait();

            this.testHarness.DataStore.UpdateById<Car>(this.fordId, c => c.Make = "Ford2").Wait();
        }

        [Fact]
        public void ItShouldNotHaveAddedTheFordToTheDatabaseYet()
        {
            var result = this.testHarness.QueryDatabase<Car>(cars => cars.Where(x => x.Id == this.fordId));
            Assert.Empty(result);
        }

        [Fact]
        public void ItShouldNotReturnThatItemIfThePredicateDoesntMatch()
        {
            var newCarFromSession = this.testHarness.DataStore.ReadActive<Car>(c => c.Make == "Ford").Result.SingleOrDefault();
            Assert.Null(newCarFromSession);
        }

        [Fact]
        public void ItShouldReturnThatItemIfThePredicateMatches()
        {
            var newCarFromSession = this.testHarness.DataStore.ReadActive<Car>(c => c.Make == "Ford2").Result.SingleOrDefault();
            Assert.NotNull(newCarFromSession);
            Assert.Equal(this.fordId, newCarFromSession.Id);            
        }
    }
}