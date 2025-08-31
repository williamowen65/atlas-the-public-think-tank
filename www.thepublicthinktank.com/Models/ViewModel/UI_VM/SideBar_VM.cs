namespace atlas_the_public_think_tank.Models.ViewModel.UI_VM
{
    public class SideBar_VM
    {
        public PageInfo? PageInfo { get; set; }

        public bool ShowPageDisplayOptions { get; set; } = true;
    }


    public class PageInfo
    {
        public string? PageContext { get; set; }
        public string? FilterAlert { get; set; }

    }
}
