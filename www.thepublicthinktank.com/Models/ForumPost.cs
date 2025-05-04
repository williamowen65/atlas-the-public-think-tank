using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Models
{
    public class ForumPost
    {
        [Key]
        public int PostID { get; set; }

        [Required]
        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public AppUser User { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public PostStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public Category Category { get; set; }

        public int? ParentPostID { get; set; }

        [ForeignKey("ParentPostID")]
        public ForumPost ParentPost { get; set; }
    }

    public class ForumPost_CreateVM
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryID { get; set; }

        public int? ParentPostID { get; set; }

        // Default to draft status when creating
        public PostStatus Status { get; set; } = PostStatus.Draft;
    }

    public class ForumPost_ReadVM
    {
        public int PostID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public PostStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Category_ReadVM Category { get; set; }
        public User_ReadVM User { get; set; }
    }
    public class Category_ReadVM
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
    }

    public class User_ReadVM
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    public enum PostStatus
    {
        Draft,
        Published,
        Archived
    }

    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]

        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

    }
}