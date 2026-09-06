namespace atlas_the_public_think_tank.Models.ViewModel.UI_VM
{
    public class ContentCount_VM
    {
        /// <summary>
        /// Represents a count of a content with the filters applied
        /// </summary>
        public int FilteredCount { get; set; }

        /// <summary>
        /// Represents a count of a content without any filters applied
        /// </summary>
        public int AbsoluteCount { get; set; }

    }
}
