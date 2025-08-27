using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Comment;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.History
{


    /// <summary>
    /// Defines a a table for  tracking user history
    /// History can include everything from creating account, casting votes, creating post, etc
    /// </summary>
    /// <remarks>
    /// TODO: Override OnSaveChanges to automate tracking this history
    /// </remarks>
    public class UserHistory
    {
        public required Guid UserHistoryID { get; set; }
        public required Guid UserID { get; set; }
        public required string Action { get; set; }
        public string? Link { get; set; }
        public DateTime Timestamp { get; set; }

        public Guid? IssueID { get; set; }
        public Guid? SolutionID { get; set; }
        public Guid? CommentID { get; set; }


        //public Guid? UserVoteID { get; set; }

        // Navigation properties
        public virtual required AppUser User { get; set; }
        public virtual Issue? Issue { get; set; }
        public virtual Solution? Solution { get; set; }
        public virtual Comment? Comment { get; set; }
    }


    public class UserHistoryModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserHistory>().ToTable("UserHistory", "users");
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserHistory>(entity =>
            {
                entity.HasKey(e => e.UserHistoryID);
                entity.Property(e => e.Action).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Link).HasMaxLength(200).IsRequired(false);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETDATE()");

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserHistory)
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Issue)
                    .WithMany()
                    .HasForeignKey(e => e.IssueID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Solution)
                    .WithMany()
                    .HasForeignKey(e => e.SolutionID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Comment)
                    .WithMany()
                    .HasForeignKey(e => e.CommentID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }




}
