using Microsoft.AspNetCore.Mvc;
using SharedTypes;

namespace BookSuggestionAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookSuggestionController : ControllerBase
    {
        private readonly Dictionary<string, BookSuggestion> _bookSuggestion = new Dictionary<string, BookSuggestion>()
        {
            { "Stranger in a Strange Land", new BookSuggestion() { BookTitle = "Spinning Silver", Author = "Naomi Novik" } },
            { "Pirates", new BookSuggestion() { BookTitle = "The Bone Ships", Author = " RJ Barker" } },
            { "Elves and/or Dwarves", new BookSuggestion() { BookTitle = "Eragon", Author = "Christopher Paolini" } },
            { "Biopunk", new BookSuggestion() { BookTitle = "The Tainted Cup", Author = "Robert Jackson Bennett" } },
            { "Small Press or Self Published", new BookSuggestion() { BookTitle = "The Sword of Kaigen", Author = "M. L. Wang" } },
            { "Published in 2025", new BookSuggestion() { BookTitle = "Sunrise on the Reaping", Author = "Suzanne Collins" } },
            { "Epistolary", new BookSuggestion() { BookTitle = " A Natural History of Dragons", Author = "Marie Brennan" } },
            { "Parent Protagonist", new BookSuggestion() { BookTitle = "The Book Eaters", Author = " Sunyi Dean" } },
            { "Impossible Places", new BookSuggestion() { BookTitle = "The Raven Boys", Author = "Maggie Stiefvater" } },
            { "Down With The System", new BookSuggestion() { BookTitle = "Red Rising", Author = "Pierce Brown" } }
        };

        [HttpGet("suggest/{query}")]
        public IActionResult GetBookSuggestion(string query)
        {
            _bookSuggestion.TryGetValue(query, out BookSuggestion? suggestion);

            return Ok(suggestion);
        }
    }
}
