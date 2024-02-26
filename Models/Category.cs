using System.ComponentModel.DataAnnotations;

namespace SlackApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name required")]
        public string CategoryName { get; set; }

        public virtual ICollection<Channel>? Channels { get; set; }
    }
}
