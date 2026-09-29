namespace atlas_the_public_think_tank.Models.Cacheable
{
    public class Vote_Cacheable
    {
        public Guid VoteID { get; set; }
        public Guid UserID { get; set; }
        public int VoteValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
