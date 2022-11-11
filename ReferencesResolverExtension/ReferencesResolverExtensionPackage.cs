using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using System.Threading;
using System.Threading.Tasks;
using Telerik.ReferencesResolverExtension.Models;

namespace Telerik.ReferencesResolverExtension
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell. These attributes tell the pkgdef creation
	/// utility what data to put into .pkgdef file.
	/// </para>
	/// <para>
	/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
	/// </para>
	/// </remarks>
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(MyToolWindow))]
    [Guid(GuidList.guidReferencesResolverExtensionPkgString)]
    public sealed class ReferencesResolverExtensionPackage : AsyncPackage
	{
        private const string PathCollectionString = "ReferencesResolverExtension";
        private const string UserSettingsProperty = "UserSettings";
        private static readonly UserSettingsModel DefaultUserSettings;
        private static readonly string DefaultUserSettingsJson;

        static ReferencesResolverExtensionPackage()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { MaxDepth = 128 };
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            DefaultUserSettings = new UserSettingsModel()
            {
                CommandModels = new ResolveReferencesCommandModel[]
                {
                    new ResolveReferencesCommandModel("WPF Current Telerik Binaries", 
                        new ReplaceReferencesOptionModel[]
                        {
                            new ReplaceReferencesOptionModel()
                            {
                                CopyFolderPath = @"C:\Work\WPF_Scrum\Current\Binaries\WPF\",
                            },
                        },
                        new ReferenceModel[]
                        {
                            // No specific items to add
                        }),
                    new ResolveReferencesCommandModel("WPF Development Telerik Binaries", 
                        new ReplaceReferencesOptionModel[]
                        {
                            new ReplaceReferencesOptionModel()
                            {
                                CopyFolderPath = @"C:\Work\WPF_Scrum\Development\Binaries\WPF\",
                            },
                        },
                        new ReferenceModel[]
                        {
                            // No specific items to add
                        }),
                    new ResolveReferencesCommandModel("WPF Release Telerik Binaries", 
                        new ReplaceReferencesOptionModel[]
                        {
                            new ReplaceReferencesOptionModel()
                            {
                                CopyFolderPath = @"C:\Work\WPF_Scrum\Release\Binaries\WPF\",
                            },
                        },
                        new ReferenceModel[]
                        {
                            // No specific items to add
                        }),
                    new ResolveReferencesCommandModel("Silverlight Current Telerik Binaries", 
                        new ReplaceReferencesOptionModel[]
                        {
                            new ReplaceReferencesOptionModel()
                            {
                                CopyFolderPath = @"C:\Work\WPF_Scrum\Current\Binaries\Silverlight\",
                            },
                        },
                        new ReferenceModel[]
                        {
                            // No specific items to add
                        }),
                    new ResolveReferencesCommandModel("Silverlight Development Telerik Binaries", 
                        new ReplaceReferencesOptionModel[]
                        {
                            new ReplaceReferencesOptionModel()
                            {
                                CopyFolderPath = @"C:\Work\WPF_Scrum\Development\Binaries\Silverlight\",
                            },
                        },
                        new ReferenceModel[]
                        {
                            // No specific items to add
                        }),
                    new ResolveReferencesCommandModel("Silverlight Release Telerik Binaries", 
                        new ReplaceReferencesOptionModel[]
                        {
                            new ReplaceReferencesOptionModel()
                            {
                                CopyFolderPath = @"C:\Work\WPF_Scrum\Release\Binaries\Silverlight\",
                            },
                        },
                        new ReferenceModel[]
                        {
                            // No specific items to add
                        }),
                },

                SelectedIndex = 0
            };            

            DefaultUserSettingsJson = ConvertUserSettingToJson(DefaultUserSettings);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.ToUpper().StartsWith("XCEED.WPF"))
            {
                string executingAssembly = Assembly.GetExecutingAssembly().Location;
                string assemblyLocation = Path.GetDirectoryName(executingAssembly);
                string assemblyName = args.Name.Substring(0, args.Name.IndexOf(','));
                string assemblyFilePath = Path.Combine(assemblyLocation, assemblyName + ".dll");

                if (File.Exists(assemblyFilePath))
                {
                    return Assembly.LoadFrom(assemblyFilePath);
                }
            }

            return null;
        }

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ReferencesResolverExtensionPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            this.IsResolving = false;
        }

        private bool IsResolving { get; set; }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            this.ShowSettingsToolWindow();
        }

        public void ShowSettingsToolWindow(int? selectedCommandIndex = null)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            MyToolWindow window = (MyToolWindow)this.FindToolWindow(typeof(MyToolWindow), 0, true);
            window.Initialize(this, selectedCommandIndex);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

		/////////////////////////////////////////////////////////////////////////////
		// Overridden Package Implementation
		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
		/// <param name="progress">A provider for progress updates.</param>
		/// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidReferencesResolverExtensionCmdSet, (int)PkgCmdIDList.cmdidAddRemoveReferences);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidReferencesResolverExtensionCmdSet, (int)PkgCmdIDList.cmdidRefenencesCommandSettings);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            ResolveReferencesCommandDialog dialog = new ResolveReferencesCommandDialog();
            dialog.Initialize(this);
            dialog.ShowDialog();
        }

        public void ResolveActiveProjectsReferences(ResolveReferencesCommandModel selectedCommandModel)
        {
            if (this.EnsureNotResolving())
            {
                ReferenceResolver resolver = this.PrepareReferenceResolverWithDialog();
                resolver.ResolveActiveProjectsReferences(selectedCommandModel);
            }
        }

        // This method is never used and tested!
        public void ResolveSelectedProjectReferences(ResolveReferencesCommandModel selectedCommandModel)
        {
            if (this.EnsureNotResolving())
            {
                object selectedObject = this.GetSelectedContextMenuItem();

                VSLangProj.VSProject project = selectedObject as VSLangProj.VSProject;

                if (project != null)
                {                    
                    ReferenceResolver resolver = this.PrepareReferenceResolverWithDialog();
                    resolver.ResolveSingleProjectReferences(project, selectedCommandModel);
                }
            }
        }

        public EnvDTE.DTE GetDteService()
        {
            EnvDTE.DTE service = (EnvDTE.DTE)GetService(typeof(SDTE));

            return service;
        }

        private ReferenceResolver PrepareReferenceResolverWithDialog()
        {
            CommandResultDialog dialog = new CommandResultDialog();
            ReferenceResolver resolver = new ReferenceResolver(this);

            resolver.Info.OnStartResolving = () => { this.DoOnStartResolving(dialog); };

            int projectsCount = 0;
            int referencesAdded = 0;
            int referencesRemoved = 0;
            
            resolver.Info.OnProjectModifyingStart = (project) =>
            {
                projectsCount++;
                referencesAdded = 0;
                referencesRemoved = 0;
                this.DoOnProjectModifyingStart(dialog, project);
            };

            resolver.Info.OnReferenceAdded = (reference) =>
            {
                referencesAdded++;
                this.DoOnReferenceAdded(dialog, reference);
            };

            resolver.Info.OnReferenceRemoved = (reference) => 
            {
                referencesRemoved++;
                this.DoOnReferenceRemoved(dialog, reference); 
            };

            resolver.Info.OnProjectModifyingEnd = (project) => { this.DoOnProjectModifyingEnd(dialog, project, referencesAdded, referencesRemoved); }; 
            resolver.Info.OnFinishedResolving = () => { this.DoOnFinishedResolving(dialog, projectsCount); };
            resolver.Info.OnException = (ex) => { this.DoOnException(dialog, ex); };

            return resolver;
        }

        private bool EnsureNotResolving()
        {
            if (this.IsResolving)
            {
                System.Windows.MessageBox.Show("Please wait until the current reference resolve command execution ends!");
            }

            return !this.IsResolving;
        }

        private void DoOnException(CommandResultDialog dialog, Exception ex)
        {
            dialog.AddResult(string.Format("Exception occured: {0} \n StackTrace: {1}", ex.Message, ex.StackTrace));
        }

        private void DoOnProjectModifyingStart(CommandResultDialog dialog, VSLangProj.VSProject project)
        {
            dialog.AddResult(string.Format(" --------------\nModifying project: {0}", project.Project.Name));
        }

        private void DoOnProjectModifyingEnd(CommandResultDialog dialog, VSLangProj.VSProject project, int addedReferences, int removedReferences)
        {
            dialog.AddResult(string.Format("Finished modifying project: {0} \n {1} references added \n {2} references removed \n --------------",
                project.Project.Name, addedReferences, removedReferences));
        }

        private void DoOnReferenceAdded(CommandResultDialog dialog, ReferenceModel reference)
        {
            dialog.AddResult(string.Format("Added reference: {0} \n {1}", reference.Name, reference.Path));
        }

        private void DoOnReferenceRemoved(CommandResultDialog dialog, ReferenceModel reference)
        {
            dialog.AddResult(string.Format("Removed reference: {0} \n {1}", reference.Name, reference.Path));
        }

        private void DoOnStartResolving(CommandResultDialog dialog)
        {
            this.IsResolving = true;
            dialog.AddResult("Start resolving references ...");
            dialog.ShowDialog();
        }

        private void DoOnFinishedResolving(CommandResultDialog dialog, int projectsCount)
        {
            dialog.AddResult(string.Format("Finished! \n {0} projects resolved!", projectsCount));
            this.IsResolving = false;
            dialog.ShowFinishButton();
        }

        private object GetSelectedContextMenuItem()
        {
            IntPtr hierarchyPointer, selectionContainerPointer;
            Object selectedObject = null;
            IVsMultiItemSelect multiItemSelect;
            uint projectItemId;

            IVsMonitorSelection monitorSelection =
                    (IVsMonitorSelection)Package.GetGlobalService(
                    typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                 out projectItemId,
                                                 out multiItemSelect,
                                                 out selectionContainerPointer);

            IVsHierarchy selectedHierarchy = Marshal.GetTypedObjectForIUnknown(
                                                 hierarchyPointer,
                                                 typeof(IVsHierarchy)) as IVsHierarchy;

            if (selectedHierarchy != null)
            {
                ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(
                                                  projectItemId,
                                                  (int)__VSHPROPID.VSHPROPID_ExtObject,
                                                  out selectedObject));
            }

            return selectedObject;
        }
                
        public ResolveReferencesCommandModel GetSelectedCommandModel()
        {
            UserSettingsModel settings = this.GetUserSettings();
            ResolveReferencesCommandModel selectedModel = null;

            if (settings.SelectedIndex > -1)
            {
                selectedModel = settings.CommandModels[settings.SelectedIndex];
            }

            return selectedModel;
        }        

        public UserSettingsModel GetUserSettings()
        {
            SettingsManager settingsManager = new ShellSettingsManager(this);
            WritableSettingsStore userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            ReferencesResolverExtensionPackage.EnsureSettingsStoreCollectionExists(userSettingsStore);
            string settingsJson = userSettingsStore.GetString(PathCollectionString, UserSettingsProperty, DefaultUserSettingsJson);
            UserSettingsModel userSettings = GetUserSettingsFromJson(settingsJson);

            return userSettings;
        }

        public void SaveUserSettings(UserSettingsModel userSettings)
        {
            SettingsManager settingsManager = new ShellSettingsManager(this);
            WritableSettingsStore userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            ReferencesResolverExtensionPackage.EnsureSettingsStoreCollectionExists(userSettingsStore);
            string settingsJson = ConvertUserSettingToJson(userSettings);
            userSettingsStore.SetString(PathCollectionString, UserSettingsProperty, settingsJson);
        }

        private static void EnsureSettingsStoreCollectionExists(WritableSettingsStore userSettingsStore)
        {
            if (!userSettingsStore.CollectionExists(PathCollectionString))
            {
                userSettingsStore.CreateCollection(PathCollectionString);
            }
        }

        private static string ConvertUserSettingToJson(UserSettingsModel userSettings)
        {
            string json = JsonConvert.SerializeObject(userSettings);

            return json;
        }

        private static UserSettingsModel GetUserSettingsFromJson(string json)
        {
            UserSettingsModel userSettings = JsonConvert.DeserializeObject<UserSettingsModel>(json);

            return userSettings;
        }

        public void ShowMessage(string content)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "ReferencesResolverExtension",
                       content,
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }
    }
}
