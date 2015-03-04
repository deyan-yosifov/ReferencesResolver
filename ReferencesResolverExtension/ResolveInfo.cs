using System;
using Telerik.ReferencesResolverExtension.Models;

namespace Telerik.ReferencesResolverExtension
{
    public class ResolveInfo
    {
        public Action<VSLangProj.VSProject> OnProjectModifyingStart { get; set; }
        public Action<VSLangProj.VSProject> OnProjectModifyingEnd { get; set; }
        public Action<ReferenceModel> OnReferenceRemoved { get; set; }
        public Action<ReferenceModel> OnReferenceAdded { get; set; }
        public Action<Exception> OnException { get; set; }
        public Action OnStartResolving { get; set; }
        public Action OnFinishedResolving { get; set; }

        public void DoOnProjectModifyingStart(VSLangProj.VSProject project)
        {
            if (this.OnProjectModifyingStart != null)
            {
                this.OnProjectModifyingStart(project);
            }
        }

        public void DoOnProjectModifyingEnd(VSLangProj.VSProject project)
        {
            if (this.OnProjectModifyingEnd != null)
            {
                this.OnProjectModifyingEnd(project);
            }
        }

        public void DoOnReferenceRemoved(ReferenceModel reference)
        {
            if (this.OnReferenceRemoved != null)
            {
                this.OnReferenceRemoved(reference);
            }
        }

        public void DoOnReferenceAdded(ReferenceModel reference)
        {
            if (this.OnReferenceAdded != null)
            {
                this.OnReferenceAdded(reference);
            }
        }

        public void DoOnException(Exception exception)
        {
            if (this.OnException != null)
            {
                this.OnException(exception);
            }
        }

        public void DoOnStartResolving()
        {
            if (this.OnStartResolving != null)
            {
                this.OnStartResolving();
            }
        }

        public void DoOnFinishedResolving()
        {
            if (this.OnFinishedResolving != null)
            {
                this.OnFinishedResolving();
            }
        }
    }
}
