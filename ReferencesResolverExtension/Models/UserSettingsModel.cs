using System;

namespace Telerik.ReferencesResolverExtension.Models
{
    public class UserSettingsModel
    {
        public int SelectedIndex { get; set; }
        public ResolveReferencesCommandModel[] CommandModels { get; set; }
    }
}
