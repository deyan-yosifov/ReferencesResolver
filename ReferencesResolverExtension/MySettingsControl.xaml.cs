using System;
using System.Windows.Controls;
using Telerik.ReferencesResolverExtension.ViewModels;

namespace Telerik.ReferencesResolverExtension
{
    /// <summary>
    /// Interaction logic for MySettingsControl.xaml
    /// </summary>
    public partial class MySettingsControl : UserControl
    {
        private readonly ReferenceResolverControlViewModel viewModel;

        public MySettingsControl()
        {
            InitializeComponent();
            this.viewModel = new ReferenceResolverControlViewModel();
            this.DataContext = this.viewModel;
        }

        public void Initialize(ReferencesResolverExtensionPackage package, int? selectedCommandIndex = null)
        {
            this.viewModel.Initialize(package, selectedCommandIndex);
        }
    }
}