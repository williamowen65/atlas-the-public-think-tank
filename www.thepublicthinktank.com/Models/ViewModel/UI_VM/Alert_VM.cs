using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Models.ViewModel.UI_VM
{

    /// <summary>
    /// ViewModel for reading an alert
    /// </summary>
    public class Alert_VM
    {
        public string Message { get; set; }

        public bool Dismissible { get; set; } = true;

        public int Timeout { get; set; }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public AlertType Type { get; set; } = AlertType.info;

        public bool IsFullWidth { get; set; } = false;
    }
}
