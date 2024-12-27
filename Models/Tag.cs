using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuickNote.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Tag zorunludur")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "En az 3 en fazla 150 karakter olabilir.")]
        public string Name { get; set; }

        public ICollection<NoteTag> NoteTags { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
