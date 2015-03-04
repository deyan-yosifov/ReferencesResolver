using System;

namespace Telerik.ReferencesResolverExtension.Models
{
    public class ReferenceModel
    {
        public ReferenceModel(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }
    }
}
