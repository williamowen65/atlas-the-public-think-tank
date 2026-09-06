using System.Text.Json;

namespace atlas_the_public_think_tank.Models.ViewModel.UI_VM.FormComponentVM
{
    public class FormSelectVM
    {

        public required dynamic Model { get; set; }
        public required string PropertyName { get; set; }
        public List<object> ListItems { get; set; } = new List<object>();

        public required string ValueField { get; set; }
        public required string TextField { get; set; }
        public required string FieldPlaceholder { get; set; }
        public required string FieldLabel { get; set; }

        public bool AllowMultiple { get; set; }

        public string? SelectedValue { get; set; }
        public string[]? SelectedValues { get; set; }

        public bool DisabledField { get; set; } = false;

        public string? Select2ConfigurationCallback { get; set; }

        public string? Select2ListenerCallback { get; set; }

        /// <summary>
        /// If custom html is being used to render the items
        /// and you have an item selected by default, you may need to fetch
        /// the related html from the server
        /// </summary>
        public string? Select2TemplateCallback { get; set; }
    }


}

