using System;
using System.Collections.Generic;

namespace Telerik.ReferencesResolverExtension.Models
{
    public class ResolveReferencesCommandModel
    {
        public ResolveReferencesCommandModel(string commandName, IEnumerable<ReplaceReferencesOptionModel> referencesToReplace, IEnumerable<ReferenceModel> referencesToResolve)
        {
            this.CommandName = commandName;
            this.ReferencesToReplace = referencesToReplace;
            this.ReferencesToResolve = referencesToResolve;
        }

        public string CommandName { get; private set; }
        public IEnumerable<ReplaceReferencesOptionModel> ReferencesToReplace { get; private set; }
        public IEnumerable<ReferenceModel> ReferencesToResolve { get; private set; }
    }
}
