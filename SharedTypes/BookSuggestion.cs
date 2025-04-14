using System.Text.Json.Serialization;

namespace SharedTypes
{
    public class BookSuggestion
    {
        [JsonPropertyName("bookTitle")]
        public required string BookTitle { get; set; }
        [JsonPropertyName("author")]
        public required string Author { get; set; }

        public override string ToString()
        {
            return $"{BookTitle} by {Author}";
        }
    }
}
