using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using PollyExample.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PollyExampleTest.Services
{
    public class APIServiceTests
    {
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<APIService>> _loggerMock;
        private readonly APIService _apiService;

        public APIServiceTests()
        {
            _httpClient = new HttpClient();
            _loggerMock = new Mock<ILogger<APIService>>();
            _apiService = new APIService(_httpClient, _loggerMock.Object);
        }

        [Fact]
        public async Task GetDataFromTestApi_Success()
        {
            // Arrange
            var expectedData = "Test data";
            var httpClientHandlerStub = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(httpClientHandlerStub.Object);

            var apiService = new APIService(httpClient, _loggerMock.Object);

            // Act
            // Assert
            Assert.ThrowsAsync<Exception>(() => apiService.GetDataFromTestApi());
        }

        [Fact]
        public async Task GetDataForCercuitBreaker_CircuitOpen_ReturnsServiceUnavailable()
        {
            // Arrange
            var circuitBreakerOpenMessage = "Service unavailable";
            var expectedException = new Exception(circuitBreakerOpenMessage);
            var apiService = new APIService(_httpClient, _loggerMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(() => apiService.GetDataForCercuitBreaker());
        }
    }
}
