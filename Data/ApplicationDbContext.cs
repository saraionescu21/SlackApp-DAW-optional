using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SlackApp.Models;

namespace SlackApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<SubscribedChannel> UserChannels { get; set; }
        public DbSet<Moderator> Moderators { get; set; }
        public DbSet<Request> Requests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<SubscribedChannel>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.ChannelId });


            // definire relatii cu modelele User si channel (FK)

            modelBuilder.Entity<SubscribedChannel>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.UserChannels)
                .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<SubscribedChannel>()
                .HasOne(ab => ab.Channel)
                .WithMany(ab => ab.SubscribedChannels)
                .HasForeignKey(ab => ab.ChannelId);





            modelBuilder.Entity<Moderator>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.ChannelId });


            modelBuilder.Entity<Moderator>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.Moderators)
                .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<Moderator>()
                .HasOne(ab => ab.Channel)
                .WithMany(ab => ab.Moderators)
                .HasForeignKey(ab => ab.ChannelId);





            modelBuilder.Entity<Request>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.ChannelId });



            modelBuilder.Entity<Request>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.Requests)
                .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<Request>()
                .HasOne(ab => ab.Channel)
                .WithMany(ab => ab.Requests)
                .HasForeignKey(ab => ab.ChannelId);
        }
    }
}
