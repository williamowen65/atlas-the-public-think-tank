using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common
{

    public class Scope_CreateOrEditVM
    {
        public Guid? ScopeID { get; set; }

        [MinLength(1, ErrorMessage = "At least one scale must be selected.")]
        public ICollection<Scale> Scales { get; set; } = new List<Scale>();

        public ICollection<Domain> Domains { get; set; } = new List<Domain>();
        public ICollection<EntityType> EntityTypes { get; set; } = new List<EntityType>();
        public ICollection<Timeframe> Timeframes { get; set; } = new List<Timeframe>();
        public ICollection<BoundaryType> Boundaries { get; set; } = new List<BoundaryType>();
    }
}
