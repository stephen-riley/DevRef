using System;
using System.IO;
using CommandLine;

namespace DevRef
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<ManageOptions, LocalOptions, RemoteOptions>(args)
                    .WithParsed<ManageOptions>(options => Manage(options))
                    .WithParsed<LocalOptions>(options => SwitchToLocal(options))
                    .WithParsed<RemoteOptions>(options => SwitchToRemote(options));
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                Environment.Exit(-1);
            }
        }

        static void Manage(ManageOptions options)
        {
            if (!File.Exists(options.LocalPath))
            {
                throw new FileNotFoundException($"csproj {options.LocalPath} does not exist");
            }
            var entry = new DevRefFileEntry
            {
                Package = options.Package,
                Version = Utils.GetVersionForCsprojPackage(options.Package),
                LocalPath = options.LocalPath,
            };

            Utils.AddManagedEntry(entry);

            // switch to local
            Utils.ChangePackageToProject(entry.Package, entry.LocalPath);

            // run dotnet restore
            Utils.RunDotnetRestore();
        }

        static void SwitchToLocal(LocalOptions options)
        {
            var entry = Utils.GetEntryForPackage(options.Package);
            Utils.ChangePackageToProject(entry.Package, entry.LocalPath);
            Utils.RunDotnetRestore();
        }

        static void SwitchToRemote(RemoteOptions options)
        {
            var entry = Utils.GetEntryForPackage(options.Package);
            Utils.ChangeProjectToPackage(entry.LocalPath, entry.Package, entry.Version);
            Utils.RunDotnetRestore();
        }
    }
}
