using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace copy_pdb_to_nuget
{
    class Program
    {

        static readonly string[] separator = new string[] { ";" };

        static readonly Regex AssemblyNameRegex = new Regex(@"\<AssemblyName\>(.+)\</AssemblyName\>");
        static readonly Regex VersionRegex = new Regex(@"\<Version\>(\d+\.\d+\.\d+)\</Version\>");
        static readonly Regex TargetFrameworksRegex = new Regex(@"\<TargetFramework[s]?\>([a-zA-Z0-9;\.]+)</TargetFramework[s]?>");

        static void Main(string[] args)
        {
            var platform = args != null && args.Contains("debug") ? "Debug" : "Release";
            Console.WriteLine("platform = " + platform);

            var targetFolder = ConfigurationManager.AppSettings["TargetFolder"];

            //var root = new DirectoryInfo(@"D:\TheGitlabWorkspace\savory-net-foundation\savory-codedom");
            var root = new DirectoryInfo(Environment.CurrentDirectory);

            var files = root.GetFiles("*csproj", SearchOption.AllDirectories);

            foreach (var item in files)
            {
                var content = File.ReadAllText(item.FullName);

                var assemblyNameMatch = AssemblyNameRegex.Match(content);
                if (!assemblyNameMatch.Success)
                {
                    continue;
                }
                var assemblyName = assemblyNameMatch.Groups[1].Value;

                var versionMatch = VersionRegex.Match(content);
                if (!versionMatch.Success)
                {
                    continue;
                }
                var version = versionMatch.Groups[1].Value;

                var targetFrameworksMatch = TargetFrameworksRegex.Match(content);
                if (!targetFrameworksMatch.Success)
                {
                    continue;
                }
                var targetFrameworksText = targetFrameworksMatch.Groups[1].Value;
                var targetFrameworks = targetFrameworksText.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                var targetPdbFolder = Path.Combine(targetFolder, assemblyName, version);
                if (!Directory.Exists(targetPdbFolder))
                {
                    Console.Error.WriteLine($"{targetPdbFolder} NOT exists.");
                    continue;
                }

                Console.WriteLine("================");
                Console.WriteLine($"AssemblyName = {assemblyName}");
                Console.WriteLine($"version = {version}");
                Console.WriteLine($"targetFrameworks = {targetFrameworksText}");

                foreach (var targetFramework in targetFrameworks)
                {
                    {
                        var pdbFileName = assemblyName + ".pdb";
                        var pdbSourceFileName = Path.Combine(item.DirectoryName, "bin", platform, targetFramework, pdbFileName);
                        var pdbDestFileName = Path.Combine(targetPdbFolder, "lib", targetFramework, pdbFileName);
                        if (File.Exists(pdbSourceFileName))
                        {
                            Console.WriteLine(pdbSourceFileName);
                            Console.WriteLine(pdbDestFileName);
                            File.Copy(pdbSourceFileName, pdbDestFileName, true);
                        }
                        else
                        {
                            Console.Error.WriteLine($"{pdbSourceFileName} NOT exists.");
                        }
                    }

                    {
                        var dllFileName = assemblyName + ".dll";
                        var dllSourceFileName = Path.Combine(item.DirectoryName, "bin", platform, targetFramework, dllFileName);
                        var dllDestFileName = Path.Combine(targetPdbFolder, "lib", targetFramework, dllFileName);
                        if (File.Exists(dllSourceFileName))
                        {
                            Console.WriteLine(dllSourceFileName);
                            Console.WriteLine(dllDestFileName);
                            File.Copy(dllSourceFileName, dllDestFileName, true);
                        }
                        else
                        {
                            Console.Error.WriteLine($"{dllSourceFileName} NOT exists.");
                        }
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine("success.");
        }

        static void Help()
        {
            Console.WriteLine("example: CopyPdb 1.0.1");
        }
    }
}
