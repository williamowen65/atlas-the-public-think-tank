using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;

namespace atlas_the_public_think_tank.Models.ViewModel.AjaxVM
{
    public class Solution_CreateOrEdit_AjaxVM
    {
        public Solution_CreateVM? Solution { get; set; }
        public List<Scope> Scopes { get; set; }
    }
}
