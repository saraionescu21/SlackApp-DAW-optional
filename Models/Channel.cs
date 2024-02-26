using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SlackApp.Models
{
    public class Channel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name required")]
        [StringLength(100, ErrorMessage = "Too long")]
        [MinLength(1, ErrorMessage = "Too short")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description required")]
        public string Description { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Category required")]
      
        public int? CategoryId { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ICollection<SubscribedChannel>? SubscribedChannels { get; set; }

        public virtual ICollection<Moderator>? Moderators { get; set; }

        public virtual ICollection<Request>? Requests { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }
 
        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
    }
}
