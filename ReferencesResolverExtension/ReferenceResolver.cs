using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Telerik.ReferencesResolverExtension.Models;

namespace Telerik.ReferencesResolverExtension
{
    public sealed class ReferenceResolver : IDisposable
    {
        private readonly ReferencesResolverExtensionPackage package;
        private readonly ResolveInfo info;
        private readonly BackgroundWorker worker;

        public ReferenceResolver(ReferencesResolverExtensionPackage package)
        {
            this.package = package;
            this.info = new ResolveInfo();
            this.worker = new BackgroundWorker();
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = false;
        }

        public ResolveInfo Info
        {
            get
            {
                return this.info;
            }
        }

        private BackgroundWorker Worker
        {
            get
            {
                return this.worker;
            }
        }

        public void ResolveActiveProjectsReferences(ResolveReferencesCommandModel selectedCommandModel)
        {
            if (!this.Worker.IsBusy)
            {
                this.AttachToWorkerEventsWhenResolvingActiveProjects();
                this.Worker.RunWorkerAsync(selectedCommandModel);
            }
        }

        public void ResolveSingleProjectReferences(VSLangProj.VSProject project, ResolveReferencesCommandModel selectedCommandModel)
        {
            if (!this.Worker.IsBusy)
            {
                this.AttachToWorkerEventsWhenResolvingSingleProject();
                this.Worker.RunWorkerAsync(new Tuple<VSLangProj.VSProject, ResolveReferencesCommandModel>(project, selectedCommandModel));
            }
        }

        private void AttachToWorkerEventsWhenResolvingActiveProjects()
        {
            this.Worker.DoWork += this.Worker_DoWorkWhenResolvingActiveProjects;
            this.Worker.RunWorkerCompleted += this.Worker_RunWorkerCompletedWhenResolvingActiveProjects;
            this.Worker.ProgressChanged += this.Worker_ProgressChanged;
        }

        private void DetachFromWorkerEventsWhenResolvingActiveProjects()
        {
            this.Worker.DoWork -= this.Worker_DoWorkWhenResolvingActiveProjects;
            this.Worker.RunWorkerCompleted -= this.Worker_RunWorkerCompletedWhenResolvingActiveProjects;
            this.Worker.ProgressChanged -= this.Worker_ProgressChanged;
        }

        private void Worker_DoWorkWhenResolvingActiveProjects(object sender, DoWorkEventArgs e)
        {
            ResolveReferencesCommandModel selectedCommandModel = (ResolveReferencesCommandModel)e.Argument;
            this.ResolveActiveProjectsReferencesInternal(selectedCommandModel);
        }

        private void Worker_RunWorkerCompletedWhenResolvingActiveProjects(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DetachFromWorkerEventsWhenResolvingActiveProjects();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Action action = (Action)e.UserState;
            action();
        }

        private void AttachToWorkerEventsWhenResolvingSingleProject()
        {
            this.Worker.DoWork += this.Worker_DoWorkWhenResolvingSingleProject;
            this.Worker.RunWorkerCompleted += this.Worker_RunWorkerCompletedWhenResolvingSingleProject;
            this.Worker.ProgressChanged += this.Worker_ProgressChanged;
        }

        private void DetachFromWorkerEventsWhenResolvingSingleProject()
        {
            this.Worker.DoWork -= this.Worker_DoWorkWhenResolvingSingleProject;
            this.Worker.RunWorkerCompleted -= this.Worker_RunWorkerCompletedWhenResolvingSingleProject;
            this.Worker.ProgressChanged -= this.Worker_ProgressChanged;
        }

        private void Worker_RunWorkerCompletedWhenResolvingSingleProject(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DetachFromWorkerEventsWhenResolvingSingleProject();
        }

        private void Worker_DoWorkWhenResolvingSingleProject(object sender, DoWorkEventArgs e)
        {
            var arguments = (Tuple<VSLangProj.VSProject, ResolveReferencesCommandModel>)e.Argument;
            VSLangProj.VSProject project = arguments.Item1;
            ResolveReferencesCommandModel selectedCommandModel = arguments.Item2;

            this.ResolveSingleProjectReferencesInternal(project, selectedCommandModel);
        }

        private void ResolveActiveProjectsReferencesInternal(ResolveReferencesCommandModel selectedCommandModel)
        {
            this.Worker.ReportProgress(0, this.Info.OnStartResolving);

            EnvDTE.DTE instanceService = this.package.GetDteService();
            Dictionary<string, ReferenceModel> availableReplaceReferences = this.GetAvailableReplaceReferences(selectedCommandModel);
            Dictionary<string, ReferenceModel> referencesThanMustBeAdded = this.GetReferencesThatMustBeAdded(selectedCommandModel);

            try
            {
                foreach (EnvDTE.Project projectItem in (IEnumerable)instanceService.DTE.ActiveSolutionProjects)
                {
                    VSLangProj.VSProject project = (projectItem.Object as VSLangProj.VSProject);

                    if (project != null)
                    {
                        this.ResolveProjectReferences(project, availableReplaceReferences, referencesThanMustBeAdded);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Worker.ReportProgress(50, GetAction(() => { this.Info.DoOnException(ex); }));
            }

            this.Worker.ReportProgress(100, this.Info.OnFinishedResolving);
        }

        private void ResolveSingleProjectReferencesInternal(VSLangProj.VSProject project, ResolveReferencesCommandModel selectedCommandModel)
        {
            this.Worker.ReportProgress(0, this.Info.OnStartResolving);

            Dictionary<string, ReferenceModel> availableReplaceReferences = this.GetAvailableReplaceReferences(selectedCommandModel);
            Dictionary<string, ReferenceModel> referencesThanMustBeAdded = this.GetReferencesThatMustBeAdded(selectedCommandModel);

            try
            {
                this.ResolveProjectReferences(project, availableReplaceReferences, referencesThanMustBeAdded);
            }
            catch (Exception ex)
            {
                this.Worker.ReportProgress(50, GetAction(() => { this.Info.DoOnException(ex); }));
            }

            this.Worker.ReportProgress(100, this.Info.OnFinishedResolving);
        }

        private void ResolveProjectReferences(VSLangProj.VSProject project,
            Dictionary<string, ReferenceModel> availableReplaceReferences, Dictionary<string, ReferenceModel> referencesThanMustBeAdded)
        {
            this.Worker.ReportProgress(50, GetAction(() => { this.Info.DoOnProjectModifyingStart(project); }));

            VSLangProj.References existingRefs = project.References;
            List<VSLangProj.Reference> refsToRemove;
            Dictionary<string, ReferenceModel> refsToAdd;
            this.GetReferencesToAddAndRemove(existingRefs, availableReplaceReferences, referencesThanMustBeAdded, out refsToRemove, out refsToAdd);

            for (int i = 0; i < refsToRemove.Count; i++)
            {
                VSLangProj.Reference refToRemove = refsToRemove[i];
                ReferenceModel refModelToRemove = new ReferenceModel(refToRemove.Name, refToRemove.Path);
                refToRemove.Remove();
                this.Worker.ReportProgress(50, GetAction(() => { this.Info.DoOnReferenceRemoved(refModelToRemove); }));
            }

            foreach (KeyValuePair<string, ReferenceModel> refToAdd in refsToAdd)
            {
                existingRefs.Add(refToAdd.Value.Path);
                this.Worker.ReportProgress(50, GetAction(() => { this.Info.DoOnReferenceAdded(refToAdd.Value); }));
            }

            this.Worker.ReportProgress(50, GetAction(() => { this.Info.DoOnProjectModifyingEnd(project); }));
        }

        private static Action GetAction(Action action)
        {
            return action;
        }

        private void GetReferencesToAddAndRemove(VSLangProj.References existingRefs, Dictionary<string, ReferenceModel> availableReplaceReferences,
            Dictionary<string, ReferenceModel> referencesThanMustBeAdded, out List<VSLangProj.Reference> refsToRemove, out Dictionary<string, ReferenceModel> refsToAdd)
        {
            refsToRemove = new List<VSLangProj.Reference>();
            refsToAdd = new Dictionary<string, ReferenceModel>();

            foreach (VSLangProj.Reference reference in existingRefs)
            {
                string name = reference.Name;

                ReferenceModel mustAddModel;
                ReferenceModel canReplaceModel;
                if (referencesThanMustBeAdded.TryGetValue(name, out mustAddModel))
                {
                    refsToRemove.Add(reference);
                    refsToAdd[name] = mustAddModel;
                }
                else if (availableReplaceReferences.TryGetValue(name, out canReplaceModel))
                {
                    refsToRemove.Add(reference);
                    refsToAdd[name] = canReplaceModel;
                }
            }

            foreach (var mustAddPair in referencesThanMustBeAdded)
            {
                if (!refsToAdd.ContainsKey(mustAddPair.Key))
                {
                    refsToAdd[mustAddPair.Key] = mustAddPair.Value;
                }
            }
        }

        private Dictionary<string, ReferenceModel> GetReferencesThatMustBeAdded(ResolveReferencesCommandModel selectedCommandModel)
        {
            Dictionary<string, ReferenceModel> referencesToAdd = new Dictionary<string, ReferenceModel>();

            foreach (ReferenceModel model in selectedCommandModel.ReferencesToResolve)
            {
                if (File.Exists(model.Path))
                {
                    referencesToAdd[model.Name] = model;
                }
            }

            return referencesToAdd;
        }

        private Dictionary<string, ReferenceModel> GetAvailableReplaceReferences(ResolveReferencesCommandModel selectedCommandModel)
        {
            Dictionary<string, ReferenceModel> availableReferences = new Dictionary<string, ReferenceModel>();

            foreach (ReplaceReferencesOptionModel replaceModel in selectedCommandModel.ReferencesToReplace)
            {
                if (Directory.Exists(replaceModel.CopyFolderPath))
                {
                    string[] files = Directory.GetFiles(replaceModel.CopyFolderPath, "*.dll", SearchOption.TopDirectoryOnly);

                    foreach (string filePath in files)
                    {
                        string shortFileName = Path.GetFileNameWithoutExtension(filePath);

                        //if (Regex.IsMatch(shortFileName, replaceModel.ReferenceToReplaceRegexPattern))
                        //{
                            availableReferences[shortFileName] = new ReferenceModel(shortFileName, filePath);
                        //}
                    }
                }
            }

            return availableReferences;
        }

        public void Dispose()
        {
            this.worker.Dispose();
        }
    }
}
