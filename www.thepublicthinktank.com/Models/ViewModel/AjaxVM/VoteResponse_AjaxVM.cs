namespace atlas_the_public_think_tank.Models.ViewModel.AjaxVM
{
    public class VoteResponse_AjaxVM
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public double? Average { get; set; }
        public int? Count { get; set; }
    }
}
