using System;
using System.Linq;
using System.Windows;
using Telerik.ReferencesResolverExtension.Common;
using Telerik.ReferencesResolverExtension.ViewModels;

namespace Telerik.ReferencesResolverExtension
{
    /// <summary>
    /// Interaction logic for CommandResultDialog.xaml
    /// </summary>
    public partial class CommandResultDialog : Window
    {
        private readonly CommandResultDialogViewModel viewModel;

        public CommandResultDialog()
        {
            InitializeComponent();
            this.viewModel = new CommandResultDialogViewModel();
            this.viewModel.FinishCommand = new DelegateCommand((parameter) => { this.Finish(); });
            this.AttachToEvents();

            this.DataContext = this.viewModel;
        }

        private void AttachToEvents()
        {
            this.Closing += this.CommandResultDialogClosing;
            this.Deactivated += CommandResultDialogDeactivated;
            this.PreviewKeyDown += this.CommandResultDialogPreviewKeyDown;
        }

        private void DetachFromEvents()
        {
            this.Closing -= this.CommandResultDialogClosing;
            this.Deactivated -= CommandResultDialogDeactivated;
            this.PreviewKeyDown -= this.CommandResultDialogPreviewKeyDown;
        }

        private void CommandResultDialogDeactivated(object sender, EventArgs e)
        {
            this.Activate();
        }

        private void CommandResultDialogClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.viewModel.IsFinishButtonEnabled)
            {
                e.Cancel = true;
            }
            else
            {
                this.Finish();
            }
        }

        private void CommandResultDialogPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                this.Finish();
            }
        }

        public void AddResult(string resultLine)
        {
            this.viewModel.ResultLines.Add(resultLine);
            this.viewModel.SelectedResultLineIndex = this.viewModel.ResultLines.Count - 1;
        }

        public void ShowFinishButton()
        {
            this.viewModel.IsFinishButtonEnabled = true;
        }

        private void Finish()
        {
            this.DetachFromEvents();
            this.DialogResult = true;
        }

        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListBox listBox = (System.Windows.Controls.ListBox)sender;
            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }
}
