using System.Text.Json;
using SharedTypes;

namespace BookPrompter.BusinessLayer
{
    public class BusinessLogic
    {
        private readonly HttpClient _httpClient;
        private int _failureCount = 0;
        private bool _isCircuitOpen = false;
        private DateTime _lastFailureTime;

        private readonly int _failureThreshold = 2;
        private readonly TimeSpan _breakDuration = TimeSpan.FromSeconds(10);

        public BusinessLogic(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BookSuggestion> GetBookSuggestion(string bookPrompt)
        {
            var apiUrl = $"https://localhost:7278/BookSuggestion/suggest/{bookPrompt}";

            return await ExecuteWithRetryAndCircuitBreaker(async () =>
            {
                var response = await _httpClient.GetAsync(apiUrl);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<BookSuggestion>(json) ?? throw new InvalidOperationException("Invalid JSON response.");
            }, maxRetries: 3, retryDelay: TimeSpan.FromSeconds(0.5), timeout:TimeSpan.FromSeconds(1));
        }

        public static async Task<T> TimeoutAfter<T>(Func<Task<T>> operation, TimeSpan timeout)
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            var task = operation();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token));

            if (completedTask == task)
            {
                cancellationTokenSource.Cancel();
                return await task;
            }

            throw new TimeoutException("The operation has timed out.");
        }


        private async Task<T> ExecuteWithRetryAndCircuitBreaker<T>(Func<Task<T>> operation, int maxRetries, TimeSpan retryDelay, TimeSpan timeout)
        {
            if (_isCircuitOpen && DateTime.Now - _lastFailureTime < _breakDuration)
            {
                throw new Exception("Circuit is open. Rejecting request.");
            }

            if (_isCircuitOpen && DateTime.Now - _lastFailureTime >= _breakDuration)
            {
                Console.WriteLine("Circuit is half-open. Testing recovery...");
                _isCircuitOpen = false;
            }

            int retryCount = 0;
            var delay = retryDelay;

            while (retryCount <= maxRetries)
            {
                
                try
                {
                    var result = await TimeoutAfter(operation, timeout);
                    _failureCount = 0;
                    return result;
                }
                catch (TimeoutException tex)
                {
                    Console.WriteLine($"Timeout occurred: {tex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    retryCount++;

                    Console.WriteLine($"Attempt {retryCount} failed: {ex.Message}");

                    if (retryCount > maxRetries)
                    {
                        _failureCount++;
                        Console.WriteLine($"Failure count incremented to {_failureCount}.");

                        if (_failureCount >= _failureThreshold)
                        {
                            Console.WriteLine("Failure threshold reached. Opening circuit.");
                            _isCircuitOpen = true;
                            _lastFailureTime = DateTime.Now;

                            throw new Exception("Failure threshold reached. Opening circuit.");
                        }

                        throw new Exception("Retries exceeded without success.");
                    }

                    await Task.Delay(delay);
                    delay *= 2;
                }
            }

            throw new Exception("Retries exceeded without success.");
        }

    }

}
