using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Telerik.ReferencesResolverExtension.Common;
using Telerik.ReferencesResolverExtension.Models;

namespace Telerik.ReferencesResolverExtension.ViewModels
{
    public sealed class ReferenceResolverControlViewModel : ViewModelBase
    {
        private bool isBusy;
        private string busyMessage;
        private int selectedCommandIndex;
        private readonly ResolveReferencesCommandViewModel selectedCommandViewModel;
        private readonly ObservableCollection<ResolveReferencesCommandModel> resolveReferencesCommands;
        private ICommand executeCommand;
        private ICommand openSettingToolWindowCommand;
        private readonly ICommand createCommand;
        private readonly ICommand deleteCommand;
        private readonly ICommand saveChangesCommand;

        public ReferenceResolverControlViewModel()
        {
            this.resolveReferencesCommands = new ObservableCollection<ResolveReferencesCommandModel>();
            this.selectedCommandViewModel = new ResolveReferencesCommandViewModel();
            this.selectedCommandViewModel.OnSetBusy = this.SetBusy;
            this.selectedCommandViewModel.OnSetNotBusy = this.SetNotBusy;
            this.executeCommand = new DelegateCommand((parameter) => { this.ExecuteCommandAction(); });
            this.createCommand = new DelegateCommand((parameter) => { this.CreateCommandAction(); });
            this.deleteCommand = new DelegateCommand((parameter) => { this.DeleteCommandAction(); });
            this.saveChangesCommand = new DelegateCommand((parameter) => { this.SaveChanges(); });
            this.openSettingToolWindowCommand = new DelegateCommand((parameter) => { this.OpenSettingsToolWindow(); });
        }

        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }
            set
            {
                this.SetProperty(ref this.isBusy, value);
            }
        }

        public string BusyMessage
        {
            get
            {
                return this.busyMessage;
            }
            set
            {
                this.SetProperty(ref this.busyMessage, value);
            }
        }

        public int SelectedCommandIndex
        {
            get
            {
                return this.selectedCommandIndex;
            }
            set
            {
                if (this.SetProperty(ref this.selectedCommandIndex, value))
                {
                    if (this.SelectedCommandIndex >= 0 && this.SelectedCommandIndex < this.ResolveReferencesCommands.Count)
                    {
                        this.SelectedCommandViewModel.LoadModel(this.ResolveReferencesCommands[this.SelectedCommandIndex]);
                    }
                    else
                    {
                        this.SelectedCommandViewModel.Clear();
                    }
                }
            }
        }

        public ResolveReferencesCommandViewModel SelectedCommandViewModel
        {
            get
            {
                return this.selectedCommandViewModel;
            }
        }

        public ObservableCollection<ResolveReferencesCommandModel> ResolveReferencesCommands
        {
            get
            {
                return this.resolveReferencesCommands;
            }
        }

        public ICommand ExecuteCommand
        {
            get
            {
                return this.executeCommand;
            }
            set
            {
                this.SetProperty(ref this.executeCommand, value);
            }
        }

        public ICommand CreateCommand
        {
            get
            {
                return this.createCommand;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return this.deleteCommand;
            }
        }

        public ICommand SaveChangesCommand
        {
            get
            {
                return this.saveChangesCommand;
            }
        }

        public ICommand OpenSettingToolWindowCommand
        {
            get
            {
                return openSettingToolWindowCommand;
            }
            set
            {
                this.SetProperty(ref this.openSettingToolWindowCommand, value);
            }
        }

        private ReferencesResolverExtensionPackage Package { get; set; }

        public void Initialize(ReferencesResolverExtensionPackage package, int? selectedCommandIndex = null)
        {
            this.Package = package;

            UserSettingsModel userSettings = this.Package.GetUserSettings();

            this.ResolveReferencesCommands.Clear();
            foreach (ResolveReferencesCommandModel command in userSettings.CommandModels)
            {
                this.ResolveReferencesCommands.Add(command);
            }

            this.SelectedCommandIndex = -1;
            this.SelectedCommandIndex = selectedCommandIndex.HasValue ? selectedCommandIndex.Value : userSettings.SelectedIndex;
        }

        private void ExecuteCommandAction()
        {
            int index;

            if (this.TryGetSelectedCommandIndex(out index))
            {
                ResolveReferencesCommandModel command = this.SelectedCommandViewModel.ToModel();
                this.Package.ResolveActiveProjectsReferences(command);
            }
        }

        private void CreateCommandAction()
        {            
            ResolveReferencesCommandModel model = new ResolveReferencesCommandModel("Unnamed command", Enumerable.Empty<ReplaceReferencesOptionModel>(), Enumerable.Empty<ReferenceModel>());
            this.ResolveReferencesCommands.Add(model);
            this.SelectedCommandIndex = this.ResolveReferencesCommands.Count - 1;
        }

        private void DeleteCommandAction()
        {
            int index;

            if (this.TryGetSelectedCommandIndex(out index))
            {
                this.ResolveReferencesCommands.RemoveAt(index);
                this.SelectedCommandIndex = this.ResolveReferencesCommands.Count > 0 ? 0 : -1;
            }
        }

        private void SaveChanges()
        {
            string text = "Settings successfully saved!";

            try
            {
                int index;

                if (this.TryGetSelectedCommandIndex(out index))
                {
                    ResolveReferencesCommandModel model = this.SelectedCommandViewModel.ToModel();
                    this.ResolveReferencesCommands.RemoveAt(index);
                    this.ResolveReferencesCommands.Insert(index, model);
                    this.SelectedCommandIndex = index;

                    this.Package.SaveUserSettings(new UserSettingsModel()
                    {
                        SelectedIndex = index,
                        CommandModels = this.ResolveReferencesCommands.ToArray()
                    });
                }
            }
            catch (Exception ex)
            {
                text = string.Format("Some error occured: {0} \n StackTrace: {1}", ex.Message, ex.StackTrace);
            }
            
            System.Windows.MessageBox.Show(text, "Settings save result");
        }

        private void OpenSettingsToolWindow()
        {
            int? selectedIndex = this.SelectedCommandIndex > -1 ? (int?)this.SelectedCommandIndex : null;
            this.Package.ShowSettingsToolWindow(selectedIndex);
        }

        private bool TryGetSelectedCommandIndex(out int index)
        {
            index = this.SelectedCommandIndex;

            return index >= 0 && index < this.ResolveReferencesCommands.Count;
        }
        
        private void SetBusy(string message)
        {
            this.BusyMessage = message;
            this.IsBusy = true;
        }

        private void SetNotBusy()
        {
            this.IsBusy = false;
        }
    }
}
