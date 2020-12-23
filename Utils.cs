using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using Newtonsoft.Json;

namespace DevRef
{
    public static class Utils
    {
        private static string FindCsproj()
        {
            var candidates = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj");

            if (candidates.Length == 0)
            {
                throw new FileNotFoundException("could not find a .csproj file");
            }

            if (candidates.Length > 1)
            {
                throw new FileNotFoundException("there can be only one .csproj in the folder for DevRef");
            }

            return candidates[0];
        }

        public static IEnumerable<string> LoadCsproj()
        {
            return File.ReadAllLines(FindCsproj());
        }

        public static void WriteCsproj(IEnumerable<string> lines)
        {
            File.WriteAllLines(FindCsproj(), lines);
        }

        public static void AddManagedEntry(DevRefFileEntry entry)
        {
            DevRefFile refFile = new DevRefFile
            {
                Version = Assembly.GetEntryAssembly()
                                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                .InformationalVersion
                                .ToString()
            };

            if (File.Exists(".devref"))
            {
                refFile = JsonConvert.DeserializeObject<DevRefFile>(File.ReadAllText(".devref"));
            }

            var lcPackage = entry.Package.ToLowerInvariant();

            if (refFile.Packages.ContainsKey(lcPackage))
            {
                refFile.Packages[lcPackage].Package = entry.Package.ToLowerInvariant();
                refFile.Packages[lcPackage].LocalPath = entry.LocalPath;
            }
            else
            {
                refFile.Packages[lcPackage] = entry;
            }

            File.WriteAllText(".devref", JsonConvert.SerializeObject(refFile, formatting: Formatting.Indented));
        }

        public static async Task RunDotnetRestore()
        {
            var result = await Cli.Wrap("dotnet").WithArguments("restore").ExecuteAsync();

            if (result.ExitCode != 0)
            {
                throw new Exception($"dotnet restore failed with exit code {result.ExitCode}");
            }
        }

        public static void ChangePackageToProject(string package, string localPath)
        {
            var csproj = LoadCsproj();

            var modified = new List<string>();

            var regex = @"(\s+)\<PackageReference Include=""(.*?)"" Version=""(.*?)""";

            foreach (var line in csproj)
            {
                var match = Regex.Match(line, regex);
                if (match.Success)
                {
                    var ws = match.Groups[1].Value;
                    var refPackage = match.Groups[2].Value;
                    var refVersion = match.Groups[3].Value;

                    if (refPackage.ToLowerInvariant().Equals(package.ToLowerInvariant()))
                    {
                        modified.Add($"{ws}<ProjectReference Include=\"{localPath}\" />");
                        continue;
                    }
                }

                modified.Add(line);
            }

            WriteCsproj(modified);
        }

        public static void ChangeProjectToPackage(string localPath, string package, string version)
        {
            var csproj = LoadCsproj();

            var modified = new List<string>();

            var regex = @"(\s+)\<ProjectReference Include=""(.*?)""";

            foreach (var line in csproj)
            {
                var match = Regex.Match(line, regex);
                if (match.Success)
                {
                    var ws = match.Groups[1].Value;
                    var refPath = match.Groups[2].Value;

                    if (refPath.Equals(localPath))
                    {
                        modified.Add($"{ws}<PackageReference Include=\"{package}\" Version=\"{version}\" />");
                        continue;
                    }
                }

                modified.Add(line);
            }

            WriteCsproj(modified);
        }

        public static string GetVersionForCsprojPackage(string package)
        {
            var csproj = LoadCsproj();

            var regex = @"<PackageReference Include=""(.*?)"" Version=""(.*?)""";

            foreach (var line in csproj)
            {
                var match = Regex.Match(line, regex);
                if (match.Success)
                {
                    var refProject = match.Groups[1].Value.ToLowerInvariant();
                    var refVersion = match.Groups[2].Value.ToLowerInvariant();

                    if (refProject.Equals(package.ToLowerInvariant()))
                    {
                        return refVersion;
                    }
                }
            }

            throw new Exception($"could not find package {package} in the csproj");
        }

        public static DevRefFileEntry GetEntryForPackage(string package)
        {
            var lcPackage = package.ToLowerInvariant();

            var refFile = JsonConvert.DeserializeObject<DevRefFile>(File.ReadAllText(".devref"));

            if (refFile.Packages.ContainsKey(lcPackage))
            {
                return refFile.Packages[lcPackage];
            }

            throw new Exception($"package {package} is not under management");
        }

        public static void ShowManagedPackages()
        {
            if (File.Exists(".devref"))
            {
                var refFile = JsonConvert.DeserializeObject<DevRefFile>(File.ReadAllText(".devref"));

                Console.WriteLine("Packages under DevRef management");
                Console.WriteLine("=================================");

                foreach (var package in refFile.Packages.Values)
                {
                    Console.WriteLine($"{package.Package} ({package.Version}) <=> {package.LocalPath}");
                }
            }
        }

        public static List<string> GetAllPackagesUnderManagement()
        {
            if (File.Exists(".devref"))
            {
                var refFile = JsonConvert.DeserializeObject<DevRefFile>(File.ReadAllText(".devref"));
                return refFile.Packages.Values.Select(p => p.Package).ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}