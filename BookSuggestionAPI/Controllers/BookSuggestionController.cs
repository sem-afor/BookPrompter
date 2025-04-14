using System.Web;
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

    }
}
