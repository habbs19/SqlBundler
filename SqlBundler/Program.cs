using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

class SqlBundler
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: SqlBundler <inputDirectory> <outputFile.sql> [--ignore=folder1,folder2]");
            return 1;
        }

        var inputDirectory = args[0];
        var outputFilePath = args[1];
        var ignoredFolders = ParseIgnoredFolders(args);

        if (!Directory.Exists(inputDirectory))
        {
            Console.WriteLine($"Error: Input directory '{inputDirectory}' does not exist.");
            return 1;
        }

        var sqlFiles = Directory.GetFiles(inputDirectory, "*.sql", SearchOption.AllDirectories)
                                .Where(f => !IsUnderIgnoredFolder(f, ignoredFolders))
                                .OrderBy(f => f)
                                .ToList();

        if (!sqlFiles.Any())
        {
            Console.WriteLine("No .sql files found (after applying ignore filters).");
            return 1;
        }

        try
        {
            var outputDir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            using var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8);

            int total = sqlFiles.Count;
            int processed = 0;

            foreach (var file in sqlFiles)
            {
                var fileName = Path.GetFileName(file);

                writer.WriteLine($"-- =======================================");
                writer.WriteLine($"-- Begin: {fileName}");
                writer.WriteLine($"-- =======================================");
                writer.WriteLine();

                writer.WriteLine(File.ReadAllText(file));
                writer.WriteLine();

                writer.WriteLine($"-- =======================================");
                writer.WriteLine($"-- End: {fileName}");
                writer.WriteLine($"-- =======================================");
                writer.WriteLine();

                processed++;
                DrawProgressBar(processed, total);
            }

            Console.WriteLine();
            Console.WriteLine($"Success! Combined {sqlFiles.Count} files into: {outputFilePath}");

            if (ignoredFolders.Any())
                Console.WriteLine($"Ignored folders: {string.Join(", ", ignoredFolders)}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while writing output: {ex.Message}");
            return 1;
        }
    }

    // --------------------------------------------
    // Progress Bar
    // --------------------------------------------
    private static void DrawProgressBar(int progress, int total, int barSize = 40)
    {
        double percent = (double)progress / total;
        int filled = (int)(percent * barSize);

        Console.CursorVisible = false;
        Console.Write("\r[");
        Console.Write(new string('#', filled));
        Console.Write(new string('-', barSize - filled));
        Console.Write($"] {percent:0.0%}");
    }

    // --------------------------------------------
    // Parse ignore list
    // --------------------------------------------
    private static List<string> ParseIgnoredFolders(string[] args)
    {
        var ignored = new List<string>();

        var ignoreArg = args.FirstOrDefault(a => a.StartsWith("--ignore=", StringComparison.OrdinalIgnoreCase));
        if (ignoreArg == null)
            return ignored;

        var folders = ignoreArg.Substring("--ignore=".Length)
                               .Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(f => f.Trim())
                               .Where(f => !string.IsNullOrWhiteSpace(f));

        ignored.AddRange(folders);
        return ignored;
    }

    // --------------------------------------------
    // Check if file path contains an ignored folder
    // --------------------------------------------
    private static bool IsUnderIgnoredFolder(string filePath, List<string> ignoredFolders)
    {
        if (!ignoredFolders.Any())
            return false;

        foreach (var folder in ignoredFolders)
        {
            if (filePath.Contains(Path.DirectorySeparatorChar + folder + Path.DirectorySeparatorChar,
                                  StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
