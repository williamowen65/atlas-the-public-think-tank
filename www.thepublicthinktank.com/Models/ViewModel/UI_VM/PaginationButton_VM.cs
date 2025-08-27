using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

namespace atlas_the_public_think_tank.Models.ViewModel.UI_VM
{
    /// <summary>
    /// Represents a view model for a pagination button, inheriting pagination statistics.
    /// </summary>
    /// <remarks>This class is used to encapsulate the data and behavior associated with a pagination button
    /// in a user interface, providing additional context or functionality related to pagination.</remarks>
    
    public class PaginationButton_VM : PaginationStats_VM
    {
        public string ElementId { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        //public ContentType ContentType { get; set; }
        public string ContentType { get; set; }
    }
}
