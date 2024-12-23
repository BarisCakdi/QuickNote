using System.ComponentModel.DataAnnotations;

namespace QuickNote.Models
{
    public enum Status
    {
        Published = 1,
        Archived = 2,
        Deleted = 3,
    }
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "En az 3 en fazla 150 karakter olabilir.")]
        public string Title { get; set; }

        [StringLength(100000, ErrorMessage = "Max 100000 karakter olabilir.")]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdateDate { get; set; }


        public Status Status { get; set; }

        public ICollection<NoteTag> NoteTags { get; set; }
    }
}
