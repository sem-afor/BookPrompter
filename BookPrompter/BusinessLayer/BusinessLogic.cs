using System.Text.Json;
using SharedTypes;

namespace BookPrompter.BusinessLayer
{
    public class BusinessLogic
    {
        private readonly HttpClient _httpClient;

        public BusinessLogic(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BookSuggestion> GetBookSuggestion(string bookPrompt)
        {
            var apiUrl = $"https://localhost:7278/BookSuggestion/suggest/{bookPrompt}";

            var response = await _httpClient.GetAsync(apiUrl);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BookSuggestion>(json) ?? throw new InvalidOperationException();
        }
    }
}
