using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Comment
{

    /// <summary>
    /// Defines a comment made on issues or solutions
    /// </summary>
    public class Comment : ContentBase
    {
        public Guid CommentID { get; set; }
        public Guid? IssueID { get; set; }
        public Guid? SolutionID { get; set; }
        public required string Content { get; set; }

        public Guid? ParentCommentID { get; set; }

        // Navigation properties
        public virtual Issue.Issue? Issue { get; set; }
        public virtual Solution.Solution? Solution { get; set; }
        public virtual Comment? ParentComment { get; set; }
        public virtual required ICollection<Comment> ChildComments { get; set; }

        //This might be better as a dictionary for easy look ups. 
        public virtual required ICollection<CommentVote> CommentVotes { get; set; }
    }


    /// <summary>
    /// FluentAPI for Comment
    /// </summary>
    public class CommentModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>().ToTable("Comments", "comments");
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentID);
                entity.Property(e => e.Content).HasMaxLength(3000).IsRequired();

                // Self-referencing relationship
                entity.HasOne(e => e.ParentComment)
                    .WithMany(e => e.ChildComments)
                    .HasForeignKey(e => e.ParentCommentID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationships
                entity.HasOne(e => e.Issue)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.IssueID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Solution)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.SolutionID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Author)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.AuthorID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BlockedContent)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.BlockedContentID)
                    .OnDelete(DeleteBehavior.Restrict);
            });


        }

    }
}
