using System.Collections.Generic;
using CommandLine;

namespace DevRef
{
    [Verb("manage")]
    public class ManageOptions
    {
        [Option('p', "package", Required = true, HelpText = "Name of package to put under DevRef management")]
        public string Package { get; set; }

        [Option('l', "local-path", Required = true, HelpText = "Path of local package to use in lieu of --package")]
        public string LocalPath { get; set; }
    }

    [Verb("local")]
    public class LocalOptions
    {
        [Value(0)]
        public string Package { get; set; }
    }

    [Verb("remote")]
    public class RemoteOptions
    {
        [Value(0)]
        public string Package { get; set; }
    }
}