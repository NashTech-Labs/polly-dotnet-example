using Microsoft.AspNetCore.Components.Server.Circuits;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace PollyExample.Services
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<APIService> _logger;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy; 
        private readonly AsyncRetryPolicy _retryPolicy;
        // Define the API endpoint
        string apiUrl = "https://api.example.com/test";

        public APIService(HttpClient httpClient, ILogger<APIService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
            // Define a circuit breaker policy
            _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 2, // Number of consecutive exceptions before breaking the circuit
                    durationOfBreak: TimeSpan.FromSeconds(30) // Duration the circuit remains open before allowing retries
                );
            var _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timespan, retrycount, context) =>
                {
                    _logger.LogInformation($"Retry #{retrycount} after {timespan.TotalSeconds} due to {exception.GetType().Name}: {exception.Message}");
                });

        }
        public async Task<string> GetDataFromTestApi()
        {
            // Use Polly's retry mechanism to handle transient errors
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                // Make the HTTP request to the test API
                var response = await _httpClient.GetAsync(apiUrl);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            });
        }

        public async Task<string> GetDataForCercuitBreaker()
        {

            if(_circuitBreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new Exception("Service unavailable");
            }
            var response = await _circuitBreakerPolicy.ExecuteAsync(() =>
            {
                return _retryPolicy.ExecuteAsync(() =>
                 {
                     // Make the HTTP request to the test API
                     return _httpClient.GetAsync(apiUrl);
                 });
            });
            _logger.LogInformation(response.IsSuccessStatusCode.ToString());
            if (!response.IsSuccessStatusCode)
            {
                return "Not available";
            }
            return await response.Content.ReadAsStringAsync();
        }
    }
}

