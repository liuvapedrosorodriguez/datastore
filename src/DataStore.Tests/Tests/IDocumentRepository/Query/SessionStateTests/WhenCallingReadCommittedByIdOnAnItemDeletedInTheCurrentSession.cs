namespace DataStore.Tests.Tests.IDocumentRepository.Query.SessionStateTests
{
    using System;
    using System.Threading.Tasks;
    using global::DataStore.Tests.Models;
    using global::DataStore.Tests.TestHarness;
    using Newtonsoft.Json;
    using Xunit;

    public class WhenCallingReadCommittedByIdOnAnItemDeletedInTheCurrentSession
    {
        private  Car carFromDatabase;

        private  Guid carId;

        async Task Setup()
        {
            // Given
            var testHarness = TestHarness.Create(nameof(WhenCallingReadCommittedByIdOnAnItemDeletedInTheCurrentSession));

            this.carId = Guid.NewGuid();
            var existingCar = new Car
            {
                id = this.carId,
                Active = false,
                Make = "Volvo"
            };
            testHarness.AddToDatabase(existingCar);

            await testHarness.DataStore.DeleteHardById<Car>(this.carId);

            // When
            var document = await testHarness.DataStore.WithoutEventReplay.ReadById<Car>(this.carId);
            try
            {
                this.carFromDatabase = document; //this approach is for Azure
            }
            catch (Exception)
            {
                this.carFromDatabase = JsonConvert.DeserializeObject<Car>(JsonConvert.SerializeObject(document));
            }
        }

        [Fact]
        public async void ItShouldReturnTheItem()
        {
            await Setup();
            Assert.Equal("Volvo", this.carFromDatabase.Make);
            Assert.Equal(this.carId, this.carFromDatabase.id);
        }
    }
}