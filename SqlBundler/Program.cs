// See https://aka.ms/new-console-template for more information
using System.Text;

Console.WriteLine("Hello, World!");

// Validate arguments
if (args.Length < 2)
{
    Console.WriteLine("Usage: SqlBundler <inputDirectory> <outputFile.sql>");
    return;
}

string inputDirectory = args[0];
string outputFilePath = args[1];

if (!Directory.Exists(inputDirectory))
{
    Console.WriteLine($"Error: Input directory '{inputDirectory}' does not exist.");
    return;
}

// Find all .sql files recursively
var sqlFiles = Directory.GetFiles(inputDirectory, "*.sql", SearchOption.AllDirectories)
                        .OrderBy(f => f) // Optional: consistent order
                        .ToList();

if (sqlFiles.Count == 0)
{
    Console.WriteLine("No .sql files found.");
    return;
}

var sb = new StringBuilder();

foreach (var file in sqlFiles)
{
    sb.AppendLine($"-- Begin: {Path.GetFileName(file)}");
    sb.AppendLine(File.ReadAllText(file));
    sb.AppendLine($"-- End: {Path.GetFileName(file)}");
    sb.AppendLine(); // Extra space between files
}

// Ensure output directory exists
var outputDir = Path.GetDirectoryName(outputFilePath);
if (!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir))
{
    Directory.CreateDirectory(outputDir);
}

// Write to output file
File.WriteAllText(outputFilePath, sb.ToString());
Console.WriteLine($"✔ Combined {sqlFiles.Count} files into: {outputFilePath}");