using System.ComponentModel.DataAnnotations.Schema;

namespace SlackApp.Models
{
    // tabelul asociativ care face legatura intre Article si Bookmark
    // un articol are mai multe colectii din care face parte
    // iar o colectie contine mai multe articole in cadrul ei
    public class SubscribedChannel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // cheie primara compusa (Id, ArticleId, BookmarkId)
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? ChannelId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Channel? Channel { get; set; }
    }
}