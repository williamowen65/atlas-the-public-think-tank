namespace atlas_the_public_think_tank.Models
{
 
        ///
        /// This file holds View models meant to be served to specific pages.
        ///


        /// <summary>
        /// ViewModel for the home page
        /// </summary>
        public class HomeIndexViewModel
        {
            public List<Issue_ReadVM> Issues { get; set; } = new List<Issue_ReadVM>();
            public List<Category_ReadVM> Categories { get; set; } = new List<Category_ReadVM>();

            public PaginatedIssuesResponse PaginatedPosts { get; set; } = new PaginatedIssuesResponse();

        }
}
