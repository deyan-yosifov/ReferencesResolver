using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Telerik.ReferencesResolverExtension.Common;
using Telerik.ReferencesResolverExtension.ViewModels;

namespace Telerik.ReferencesResolverExtension
{
    /// <summary>
    /// Interaction logic for ResolveReferencesCommandDialog.xaml
    /// </summary>
    public partial class ResolveReferencesCommandDialog : Window
    {
        private readonly ReferenceResolverControlViewModel viewModel;
        private readonly ICommand defaultExecuteCommand;
        private readonly ICommand defaultOpenSettingsToolWindowCommand;

        public ResolveReferencesCommandDialog()
        {
            InitializeComponent();
            this.viewModel = new ReferenceResolverControlViewModel();
            this.defaultExecuteCommand = this.viewModel.ExecuteCommand;
            this.viewModel.ExecuteCommand = new DelegateCommand((parameter) => { this.ExecuteCommandAction(); });
            this.defaultOpenSettingsToolWindowCommand = this.viewModel.OpenSettingToolWindowCommand;
            this.viewModel.OpenSettingToolWindowCommand = new DelegateCommand((parameter) => { this.OpenSettingToolWindowAction(); });
            this.DataContext = this.viewModel;            
        }

        public void Initialize(ReferencesResolverExtensionPackage package)
        {
            this.viewModel.Initialize(package);
        }

        private void ExecuteCommandAction()
        {
            this.defaultExecuteCommand.Execute(null);
            this.DialogResult = true;
        }
        
        private void OpenSettingToolWindowAction()
        {
            this.defaultOpenSettingsToolWindowCommand.Execute(null);
            this.DialogResult = true;
        }
    }
}
