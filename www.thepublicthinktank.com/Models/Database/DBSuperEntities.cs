using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;


/**
* This file defines database related classes to be inherited,
* and keep the codebase DRY.
*/
namespace atlas_the_public_think_tank.Models.Database
{

    /// <summary>
    /// Issues, Solutions, and Comments
    /// </summary>
    public abstract class ContentBase
    {
        public ContentStatus ContentStatus { get; set; }
        public Guid AuthorID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
      
        public Guid? BlockedContentID { get; set; }

        // Navigation properties common to both
        public virtual AppUser Author { get; set; }
        public virtual BlockedContent BlockedContent { get; set; }
        
    }

    /// <summary>
    /// Issues and Solutions, but not Comments
    /// </summary>
    public abstract class ContentItem : ContentBase
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public Guid ScopeID { get; set; }
        public virtual Scope Scope { get; set; }

        public Guid ParentIssueID { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Issue ParentIssue { get; set; }


        public virtual ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
    }


    public abstract class VoteBase
    {
        public Guid VoteID { get; set; }
        public Guid UserID { get; set; }
        public int VoteValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Navigation
        public virtual AppUser User { get; set; }
    }


}