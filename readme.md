# Book Prompter
## Application Idea
The application is a single page website, which allows readers to generate theme prompts for choosing their next book to read. 
This is usefull for finding inspiration if one has alot of unread books or is looking for new books.
Additionally, the website provides a book suggestion fitting the prompt, which allows the user to directly pic their next book.

## Project Structure
The project is structured in two main parts. One is the blazor web application and the other is a web api.

### Blazor Web App
- UI for the Book Prompter
- One button for generating a random book prompt
- Prompt generation in the app
- Queries Book Suggestions for a prompt from an external API
- Displays Resilience Patterns Errors

### Web API
- Web API with a single controller
- Accepts a query for a book suggestion: HostAddress/BookSuggestion/suggest/{query}
- Return a book suggestion containing book title and author
- Simulates error to test resilience patterns

## How to run
- Open the BookPrompter Solution in Visual Studio
- Requires .NET 9.0
- Use the "Launch Project" configuration to start both the blazor app and the web api
- Accept SSL Certificate
- Web App can be accessed at: https://localhost:7027; http://localhost:5236
- Web API can be reached at: https://localhost:7278; http://localhost:5141
- Web API Swagger can be accessed at: https://localhost:7278/swagger/index.html

## Resilience Patterns
The resilience patterns were implemented in the BusinessLogic of the Blazor Web App (BusinessLogic.cs) in context of querying the API for book suggestions. The following patterns were implemented:

### Timeout Pattern
This pattern cancels an operation that takes more time then a set threshold.

```C#
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
```

The function takes an operation and timespan, which defines how long the operation is to run at maximum. The code then essentially runs the operation and a delay task at the same time. Setting the delay task to the timout time. If the first completed task is the operation the result is returned and the delay task is cancelled, if however the delay task finishes before the operation a timeout exception is thrown.

### Retry and Circuit Breaker Pattern
In this implementation both the Retry Pattern and the Circuit Breaker Pattern are combined, but act as seperate patterns.

```C#
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
```

#### Retry Pattern
The retry pattern has a maximum of retries, it triggers on server errors or other exception except TimeOuts. TimeOuts are not counted as errors in that sense and are directly communicated to the frontend. This is a design choice. However, by not further throwing a timeout exception another try could be triggered (assuming further changes to the try catch setup).

The retry pattern works by attempting the same operation until a maximum of retries has been reached. Each next retry waits the double amount of time as the previous before retrying this is done by multiplying the current delay time by two.

If all tries have been exhausted an exception is thrown.

#### Circuit Breaker Pattern
The circuit breaker pattern blocks out any further requests for a time, after a threshold of failures has been registered. A failure is only counted once all retries have failed. 

Once the circuit has been opened any request is blocked. Once enough time has past since the blocking, a request is accepted. If that request succeeds then the failure count is reset and the circuit is closed. However if after the retries no successfull request was completed, then the block time is reset prolonging the open circuit.

Exceptions are thrown to be displayed in the frontend.

### Request Logic Wrapping
The request logic is wrapped by the previously mentioned code block, this is the configuration used:

```C#
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
```

The current settings are the following for demo purposes:
```
MaxRetries: 3
Delay: 0.5 (increases to 1 then to 2) seconds
Timeout: 1 second
FailureThreshold: 2
BreakDuration: 10 seconds
```

### API Error Simulation

The API simulates different errors. It can simulate a timeout error by sleeping for 3 seconds and a server error, returning 500. The occurance of this errors is defined by the random generator. 1 out of 9 times a timeout error is thrown. 7 out of 9 times an internal server error and 1 out of 9 times a query is processed successfully.

```C#
[HttpGet("suggest/{query}")]
public IActionResult GetBookSuggestion(string query)
{
    var random = new Random();
    int issue = random.Next(1, 10);

    if (issue < 2)
    {
        Console.WriteLine("Experiencing Time Out Issue");
        System.Threading.Thread.Sleep(3000);
    }
    else if (issue < 9)
    {
        Console.WriteLine("Experiencing Internal Server Issue");
        return StatusCode(500, new { error = "Internal Server Error" });
    }

    _bookSuggestion.TryGetValue(HttpUtility.UrlDecode(query), out BookSuggestion? suggestion);
    Console.WriteLine("Query successful");
    return Ok(suggestion);
}
```
