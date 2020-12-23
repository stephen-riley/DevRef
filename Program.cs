using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;

namespace DevRef
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<ManageOptions, LocalOptions, RemoteOptions, InfoOptions>(args)
                    .WithParsed<ManageOptions>(options => Task.Run(async () => await Manage(options)).Wait())
                    .WithParsed<InfoOptions>(options => ShowManagedPackages(options))
                    .WithParsed<LocalOptions>(options => Task.Run(async () => await SwitchToLocal(options)).Wait())
                    .WithParsed<RemoteOptions>(options => Task.Run(async () => await SwitchToRemote(options)).Wait());
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                Environment.Exit(-1);
            }
        }

        static async Task Manage(ManageOptions options)
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

            Console.WriteLine($"Package {entry.Package} now registered with DevRef.");

            await SwitchToLocal(new LocalOptions { Package = options.Package });
        }

        static void ShowManagedPackages(InfoOptions options)
        {
            Utils.ShowManagedPackages();
        }

        static async Task SwitchToLocal(LocalOptions options)
        {
            var packagesTbd =
                options.Package == null
                ? Utils.GetAllPackagesUnderManagement()
                : new List<string> { options.Package };

            foreach (var package in packagesTbd)
            {
                var entry = Utils.GetEntryForPackage(package);
                Utils.ChangePackageToProject(entry.Package, entry.LocalPath);
                Console.WriteLine($"Switched package \"{entry.Package}\" to use local path \"{entry.LocalPath}\".");
            }

            await Utils.RunDotnetRestore();
        }

        static async Task SwitchToRemote(RemoteOptions options)
        {
            var packagesTbd =
                options.Package == null
                ? Utils.GetAllPackagesUnderManagement()
                : new List<string> { options.Package };

            foreach (var package in packagesTbd)
            {
                var entry = Utils.GetEntryForPackage(package);
                Utils.ChangeProjectToPackage(entry.LocalPath, entry.Package, entry.Version);
                Console.WriteLine($"Switched package \"{entry.Package}\" to use remote version \"{entry.Version}\".");
            }

            await Utils.RunDotnetRestore();
        }
    }
}
