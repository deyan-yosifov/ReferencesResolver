using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Telerik.ReferencesResolverExtension.Common;
using Telerik.ReferencesResolverExtension.Common.FolderBrowserDialogExtension;
using Telerik.ReferencesResolverExtension.Models;

namespace Telerik.ReferencesResolverExtension.ViewModels
{
    public sealed class ResolveReferencesCommandViewModel : ViewModelBase
    {
        private string commandName;
        private int selectedFolderIndex;
        private int selectedReferenceIndex;
        private readonly ObservableCollection<ReplaceReferencesOptionModel> referencesToReplace;
        private readonly ObservableCollection<ReferenceModel> referencesToResolve;
        private readonly ICommand addFolderCommand;
        private readonly ICommand addFolderFromArchiveCommand;
        private readonly ICommand removeFolderCommand;
        private readonly ICommand addReferenceCommand;
        private readonly ICommand removeReferenceCommand;

        public ResolveReferencesCommandViewModel()
        {
            this.referencesToReplace = new ObservableCollection<ReplaceReferencesOptionModel>();
            this.referencesToResolve = new ObservableCollection<ReferenceModel>();
            this.addFolderCommand = new DelegateCommand((parameter) => { this.AddFolder(ArchivesExtractor.ExtractorDocumentsFolder); });
            this.addFolderFromArchiveCommand = new DelegateCommand((parameter) => { this.AddFolderFromArchive(); });
            this.removeFolderCommand = new DelegateCommand((parameter) => { this.RemoveFolder(); });
            this.addReferenceCommand = new DelegateCommand((parameter) => { this.AddReference(); });
            this.removeReferenceCommand = new DelegateCommand((parameter) => { this.RemoveReference(); });
        }

        public string CommandName
        {
            get
            {
                return this.commandName;
            }
            set
            {
                this.SetProperty(ref this.commandName, value);
            }
        }

        public int SelectedFolderIndex
        {
            get
            {
                return this.selectedFolderIndex;
            }
            set
            {
                this.SetProperty(ref this.selectedFolderIndex, value);
            }
        }

        public int SelectedReferenceIndex
        {
            get
            {
                return this.selectedReferenceIndex;
            }
            set
            {
                this.SetProperty(ref this.selectedReferenceIndex, value);
            }
        }

        public ICommand AddFolderCommand
        {
            get
            {
                return this.addFolderCommand;
            }
        }

        public ICommand AddFolderFromArchiveCommand
        {
            get
            {
                return this.addFolderFromArchiveCommand;
            }
        }

        public ICommand RemoveFolderCommand
        {
            get
            {
                return this.removeFolderCommand;
            }
        }

        public ICommand AddReferenceCommand
        {
            get
            {
                return this.addReferenceCommand;
            }
        }

        public ICommand RemoveReferenceCommand
        {
            get
            {
                return this.removeReferenceCommand;
            }
        }

        public ObservableCollection<ReplaceReferencesOptionModel> ReferencesToReplace
        {
            get
            {
                return this.referencesToReplace;
            }
        }

        public ObservableCollection<ReferenceModel> ReferencesToResolve
        {
            get
            {
                return this.referencesToResolve;
            }
        }
        
        public Action<string> OnSetBusy { get; set; }

        public Action OnSetNotBusy { get; set; }

        public ResolveReferencesCommandModel ToModel()
        {
            return new ResolveReferencesCommandModel(this.CommandName, this.ReferencesToReplace.ToArray(), this.ReferencesToResolve.ToArray());
        }

        public void LoadModel(ResolveReferencesCommandModel model)
        {
            this.CommandName = model.CommandName;

            this.ReferencesToReplace.Clear();
            foreach (ReplaceReferencesOptionModel optionModel in model.ReferencesToReplace)
            {
                this.ReferencesToReplace.Add(optionModel);
            }

            this.ReferencesToResolve.Clear();
            foreach (ReferenceModel reference in model.ReferencesToResolve)
            {
                this.ReferencesToResolve.Add(reference);
            }
        }

        public void Clear()
        {
            this.CommandName = null;
            this.ReferencesToReplace.Clear();
            this.ReferencesToResolve.Clear();
        }

        private void AddFolder(string startFolder = null)
        {
            //FolderBrowserDialogEx dialog = new FolderBrowserDialogEx();
            //dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            var dialog = new Deyo.Controls.Dialogs.Explorer.FolderBrowserDialog();
            dialog.Title = "Select a folder containing the binaries you want to replace.";
            dialog.ShowEditbox = true;

            if (startFolder != null)
            {
                dialog.SelectedPath = startFolder;
            }

            //if (dialog.ShowDialog() == DialogResult.OK)
            if (dialog.ShowDialog() == true)
            {
                ReplaceReferencesOptionModel option = new ReplaceReferencesOptionModel() 
                {
                    CopyFolderPath = dialog.SelectedPath 
                };

                this.ReferencesToReplace.Add(option);
                this.SelectedFolderIndex = this.ReferencesToReplace.Count - 1;
            }
        }

        private void AddFolderFromArchive()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "ZIP files|*.zip";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string shortFileName = Path.GetFileName(dialog.FileName);
                this.SetIsBusy(string.Format("Extracting {0} ...", shortFileName));
                ArchivesExtractor extractor = new ArchivesExtractor(this.OnFolderExtractSuccess, this.OnExtractException);
                extractor.ExtractZip(dialog.FileName); 
            }
        }

        private void OnFolderExtractSuccess(string folder)
        {
            this.SetNotBusy();
            this.AddFolder(folder);
        }

        private void OnExtractException(Exception exception)
        {
            this.SetNotBusy();
            MessageBox.Show(string.Format("Some error happened: {0}\n{1}", exception.Message, exception.StackTrace));
        }

        private void RemoveFolder()
        {
            int index = this.SelectedFolderIndex;

            if (index >= 0 && index < this.ReferencesToReplace.Count)
            {
                this.ReferencesToReplace.RemoveAt(index);
            }
        }

        private void AddReference()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "DLL files|*.dll";
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string filePath in dialog.FileNames)
                {
                    string shortFileName = Path.GetFileNameWithoutExtension(filePath);
                    ReferenceModel model = new ReferenceModel(shortFileName, filePath);

                    this.ReferencesToResolve.Add(model);
                }

                this.SelectedReferenceIndex = this.ReferencesToResolve.Count - 1;
            }            
        }

        private void RemoveReference()
        {
            int index = this.SelectedReferenceIndex;

            if (index >= 0 && index < this.ReferencesToResolve.Count)
            {
                this.ReferencesToResolve.RemoveAt(index);
            }
        }

        private void SetIsBusy(string message)
        {
            if (this.OnSetBusy != null)
            {
                this.OnSetBusy(message);
            }
        }

        private void SetNotBusy()
        {
            if (this.OnSetNotBusy != null)
            {
                this.OnSetNotBusy();
            }
        }
    }
}
