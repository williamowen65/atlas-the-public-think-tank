using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;

namespace atlas_the_public_think_tank.Models.ViewModel.AjaxVM
{
    public class Issue_CreateOrEdit_AjaxVM
    {
        public Issue_CreateVM? Issue { get; set; }
        public List<Scope> Scopes { get; set; }
    }
}
