using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace HollowDuck
{
    internal static class DuckGameInstallLocator
    {
        private const string AppId = "312530";

        public static string FindContentFolder()
        {
            foreach (string library in GetSteamLibraries())
            {
                string manifestPath = Path.Combine(library, "steamapps", "appmanifest_" + AppId + ".acf");
                if (!File.Exists(manifestPath))
                {
                    continue;
                }

                string manifest = File.ReadAllText(manifestPath);
                Match installDirectory = Regex.Match(manifest, "\\\"installdir\\\"\\s+\\\"([^\\\"]+)\\\"");
                string directoryName = installDirectory.Success ? installDirectory.Groups[1].Value : "Duck Game";
                string contentPath = Path.Combine(library, "steamapps", "common", directoryName, "Content");
                if (Directory.Exists(contentPath))
                {
                    return contentPath;
                }
            }

            return null;
        }

        private static IEnumerable<string> GetSteamLibraries()
        {
            HashSet<string> roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string steamPath = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Valve\Steam",
                "SteamPath",
                null) as string;

            if (!string.IsNullOrWhiteSpace(steamPath))
            {
                roots.Add(steamPath.Replace('/', Path.DirectorySeparatorChar));
            }

            string programFilesSteam = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Steam");
            if (Directory.Exists(programFilesSteam))
            {
                roots.Add(programFilesSteam);
            }

            List<string> initialRoots = new List<string>(roots);
            foreach (string root in initialRoots)
            {
                string libraryFolders = Path.Combine(root, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(libraryFolders))
                {
                    continue;
                }

                foreach (Match match in Regex.Matches(
                    File.ReadAllText(libraryFolders),
                    "\\\"path\\\"\\s+\\\"([^\\\"]+)\\\""))
                {
                    string path = match.Groups[1].Value.Replace("\\\\", "\\");
                    if (Directory.Exists(path))
                    {
                        roots.Add(path);
                    }
                }
            }

            return roots;
        }
    }
}