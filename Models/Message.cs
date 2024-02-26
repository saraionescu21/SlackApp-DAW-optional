using System.ComponentModel.DataAnnotations;

namespace SlackApp.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Content required")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        // un comentariu apartine unui articol
        public int? ChannelId { get; set; }

        // un comentariu este postat de catre un user
        public string? UserId { get; set; }

        // PASUL 6 - useri si roluri
        public virtual ApplicationUser? User { get; set; }

        public virtual Channel? Channel { get; set; }
    }
}
