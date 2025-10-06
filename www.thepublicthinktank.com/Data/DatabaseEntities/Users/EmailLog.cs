using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Users
{
    public class EmailLog
    {

        public Guid EmailLogID { get; set; }

        /// <summary>
        /// A foreign key
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// This EmailID points to an hard coded email in the codebase.
        /// The EmailQueue class manages and hold the IDs.
        /// </summary>
        public Guid EmailID { get; set; }

        public EmailStatus EmailStatus { get; set; }

        public DateTime SentAt { get; set; }

        public int Retries { get; set; }

        public virtual AppUser User { get; set; }

    }

    public enum EmailStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,

        [EnumMember(Value = "Sent")]
        Sent,

        [EnumMember(Value = "Failed")]
        Failed
    }


    /// <summary>
    /// Defined the SQL relationships of a the email log
    /// </summary>
    public class EmailLogModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailLog>().ToTable(
                "EmailLog",
                "users");
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailLog>(entity =>
            {
                entity.HasKey(e => e.EmailLogID);
                entity.Property(e => e.EmailStatus).IsRequired();
                entity.Property(e => e.EmailID).IsRequired();

                entity.HasOne(e => e.User)
                 .WithMany(e => e.EmailLog)
                 .HasForeignKey(e => e.UserID)
                 .OnDelete(DeleteBehavior.Cascade);

            });

        }

    }
}
