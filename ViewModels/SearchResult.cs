using QuickNote.Models;

namespace QuickNote.ViewModels
{
    public class SearchResult
    {
        public string SearchString { get; set; }
        public List<Note> Notes { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
