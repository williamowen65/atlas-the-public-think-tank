namespace atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common
{
    /// <summary>
    /// This object defines info related to pagination
    /// </summary>
    public class PaginationStats_VM
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
