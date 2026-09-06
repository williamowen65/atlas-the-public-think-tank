using Microsoft.AspNetCore.Authentication;

namespace atlas_the_public_think_tank.Models.ViewModel.UI_VM.ThirdPartyAuthButtonVMs
{
    public class GoogleAuthButtonModel
    {
        public bool? isLogin { get; set; }
        public bool? isSignin { get; set; }
        public AuthenticationScheme provider { get; set; }
    }
}
