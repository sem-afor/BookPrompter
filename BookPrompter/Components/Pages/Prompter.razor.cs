using BookPrompter.BusinessLayer;
using SharedTypes;

namespace BookPrompter.Components.Pages
{
    public partial class Prompter
    {
        private BusinessLogic _businessLogic;

        public string? CurrentPrompt;
        public BookSuggestion? BookSuggestion;

        private List<string> _prompts = 
        [
            "Stranger in a Strange Land",
            "Pirates",
            "Elves and/or Dwarves",
            "Biopunk",
            "Small Press or Self Published",
            "Published in 2025",
            "Epistolary",
            "Parent Protagonist",
            "Impossible Places",
            "Down With The System"
        ];

        public Prompter(BusinessLogic businessLogic)
        {
            _businessLogic = businessLogic;
        }

        private async void GeneratePrompt()
        {
            var random = new Random();
            int index = random.Next(_prompts.Count);
            CurrentPrompt = _prompts[index];

            try
            {
                BookSuggestion = await _businessLogic.GetBookSuggestion(CurrentPrompt);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
