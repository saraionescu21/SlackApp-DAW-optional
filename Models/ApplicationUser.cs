//pasul 1 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlackApp.Models
{
    public class ApplicationUser : IdentityUser
    {

            public virtual ICollection<Channel>? Channels { get; set; }
            public virtual ICollection<Message>? Messages { get; set; }
            public virtual ICollection<SubscribedChannel>? UserChannels { get; set; }
            public virtual ICollection<Moderator>? Moderators { get; set; }
            public virtual ICollection<Request>? Requests { get; set; }

            [NotMapped]
            public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
