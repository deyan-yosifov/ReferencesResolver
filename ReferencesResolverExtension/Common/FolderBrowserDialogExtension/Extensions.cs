using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Telerik.ReferencesResolverExtension.Common.FolderBrowserDialogExtension
{
    public static class Extensions
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        public static System.Windows.Forms.DialogResult ShowDialog(this FolderBrowserDialogEx dialog)
        {
            return dialog.ShowDialog(new WindowWrapper(GetActiveWindow()));
        }
    }
}
