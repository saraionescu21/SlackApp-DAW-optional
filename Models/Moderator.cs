using System.ComponentModel.DataAnnotations.Schema;

namespace SlackApp.Models
{
    public class Moderator
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? ChannelId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Channel? Channel { get; set; }
    }
}
