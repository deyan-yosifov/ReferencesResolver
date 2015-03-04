using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Telerik.ReferencesResolverExtension.Common;

namespace Telerik.ReferencesResolverExtension.ViewModels
{
    public class CommandResultDialogViewModel : ViewModelBase
    {
        private readonly ObservableCollection<string> resultLines;
        private bool isFinishButtonEnabled;
        private string headerText;
        private int selectedResultLineIndex;
        private ICommand finishCommand;

        public CommandResultDialogViewModel()
        {
            this.isFinishButtonEnabled = false;
            this.headerText = "Resolving references, please wait ...";
            this.selectedResultLineIndex = -1;
            this.resultLines = new ObservableCollection<string>();
        }

        public ObservableCollection<string> ResultLines
        {
            get
            {
                return this.resultLines;
            }
        }

        public int SelectedResultLineIndex
        {
            get
            {
                return this.selectedResultLineIndex;
            }
            set
            {
                this.SetProperty(ref this.selectedResultLineIndex, value);
            }
        }

        public bool IsFinishButtonEnabled
        {
            get
            {
                return this.isFinishButtonEnabled;
            }
            set
            {
                if (this.SetProperty(ref this.isFinishButtonEnabled, value))
                {
                    if (this.isFinishButtonEnabled)
                    {
                        this.HeaderText = "Finished resolving!";
                    }
                }
            }
        }

        public ICommand FinishCommand
        {
            get
            {
                return this.finishCommand;
            }
            set
            {
                this.SetProperty(ref this.finishCommand, value);
            }
        }

        public string HeaderText
        {
            get
            {
                return this.headerText;
            }
            set
            {
                this.SetProperty(ref this.headerText, value);
            }
        }
    }
}
