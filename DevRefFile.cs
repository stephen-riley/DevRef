using System.Collections.Generic;

namespace DevRef
{
    public class DevRefFile
    {
        public string Version { get; set; }

        public IDictionary<string, DevRefFileEntry> Packages { get; set; } = new Dictionary<string, DevRefFileEntry>();
    }

    public class DevRefFileEntry
    {
        public string Package { get; set; }

        public string Version { get; set; }

        public string LocalPath { get; set; }
    }
}