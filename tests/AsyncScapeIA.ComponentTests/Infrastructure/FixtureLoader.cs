namespace AsyncScapeIA.ComponentTests.Infrastructure;

using System;
using System.IO;

public static class FixtureLoader
{
    public static string LoadText(string relativePath)
    {
        var fullPath = ResolvePath(relativePath);
        return File.ReadAllText(fullPath);
    }

    public static Stream LoadStream(string relativePath)
    {
        var fullPath = ResolvePath(relativePath);
        return File.OpenRead(fullPath);
    }

    private static string ResolvePath(string relativePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var candidate = Path.Combine(baseDirectory, "Fixtures", relativePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(candidate))
        {
            throw new FileNotFoundException($"Fixture file '{relativePath}' not found under '{candidate}'.", candidate);
        }

        return candidate;
    }
}
