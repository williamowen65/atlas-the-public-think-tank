using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common
{



    /// <summary>
    /// ViewModel for reading the scope of an issue or solution
    /// </summary>
    public class Scope_ReadVM
    {
        public Guid ScopeID { get; set; }
        public ICollection<Scale> Scales { get; set; } = new List<Scale>();
        public ICollection<Domain> Domains { get; set; } = new List<Domain>();
        public ICollection<EntityType> EntityTypes { get; set; } = new List<EntityType>();
        public ICollection<Timeframe> Timeframes { get; set; } = new List<Timeframe>();
        public ICollection<BoundaryType> Boundaries { get; set; } = new List<BoundaryType>();

    }

}
