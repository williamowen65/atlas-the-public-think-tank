namespace atlas_the_public_think_tank.Email.Models
{
    public class ConfirmationEmailModel
    {
        public string UserName { get; set; } = "";
        public string ConfirmationLink { get; set; } = "";
    }
}
