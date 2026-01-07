
using System.Net;
using System.Net.Http.Json;
using dBanking.Core.DTOS;
using dBanking.Tests.TestUtils;
using FluentAssertions;
using Xunit;

namespace dBanking.Tests.Controllers
{
    public sealed class CustomersControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CustomersControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetCustomerById_returns_200_with_payload_when_exists()
        {
            // Arrange: use a seeded ID from your DbContext.Seed() or insert a test customer here.

            var customerId = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"); // sample from your seed

            // Act
            var resp = await _client.GetAsync($"/api/customers/{customerId}");

            // Assert
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await resp.Content.ReadFromJsonAsync<CustomerResponseDto>();
            dto.Should().NotBeNull();
            dto!.CustomerId.Should().Be(customerId);
        }
    }
}
