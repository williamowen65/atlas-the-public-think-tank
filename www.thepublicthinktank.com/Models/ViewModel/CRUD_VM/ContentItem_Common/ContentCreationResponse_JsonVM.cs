namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common
{

    public class ContentCreationResponse_JsonVM
    {
        public bool Success { get; set; }

        /// <summary>
        /// Content would be the newly created content 
        /// </summary>
        public string Content { get; set; }

        public Guid ContentId { get; set; }

        /// <summary>
        /// Collection of validation errors or other error messages that occurred during content creation
        /// </summary>
        public List<List<string>> Errors { get; set; } = new List<List<string>>();
    }


}
