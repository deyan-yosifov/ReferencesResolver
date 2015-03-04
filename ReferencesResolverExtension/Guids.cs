// Guids.cs
// MUST match guids.h
using System;

namespace Telerik.ReferencesResolverExtension
{
    static class GuidList
    {
        public const string guidReferencesResolverExtensionPkgString = "48e79884-e68f-41e3-8260-99660e5f76c9";
        public const string guidReferencesResolverExtensionCmdSetString = "98985fac-93dd-4c31-9cc0-5507bc65154b";
        public const string guidToolWindowPersistanceString = "7c182eab-a21a-464b-9ea7-be7c4bc97829";

        public static readonly Guid guidReferencesResolverExtensionCmdSet = new Guid(guidReferencesResolverExtensionCmdSetString);
    };
}