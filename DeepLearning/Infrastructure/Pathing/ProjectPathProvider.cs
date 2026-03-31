// =============================================================================
// ProjectPathProvider — Smart Path Resolver (Dev + Deployed)
// =============================================================================
//
// FILE:         ProjectPathProvider.cs
// LAYER:        Infrastructure (Pathing)
// DEPENDENCIES: System.IO, System.Linq
// DEPENDENTS:   DetectImageFromFileUseCase, RunDetectionApplication,
//               ConsoleUserInterface
//
// PURPOSE:
//   Resolves file paths in a way that works both during development and after
//   deployment. This solves the problem that files are in different locations
//   depending on how the app is run.
//
// THE PROBLEM:
//   During development:
//     - App runs from: bin/Debug/net8.0-windows/
//     - Files are in:  project root (DeepLearning/)
//     - Need to go UP 3 levels to find files
//
//   After publishing:
//     - App runs from: publish/
//     - Files are in:  publish/ (same folder as exe)
//     - No navigation needed
//
// DETECTION LOGIC (constructor):
//   1. Get AppContext.BaseDirectory (where the exe is running)
//   2. Calculate dev root: go up 3 levels (bin/Debug/net8.0-windows → project root)
//   3. Check if DeepLearning.csproj exists at dev root
//      - YES → development mode → use dev root
//      - NO  → deployed mode → use base directory
//
// DESIGN NOTES:
//   - The .csproj file check is the key: it only exists in the project root,
//     never in a published folder. This is a reliable dev/deploy detector.
//   - GetImageFiles() filters by extension (.jpg, .jpeg, .png, .bmp, .gif)
//     and returns just filenames (not full paths) for display in the UI
//   - GetAbsolutePath() handles both cases: if the path is already absolute,
//     it just normalizes it (removes .., resolves symlinks); if relative,
//     it resolves against the project root
//
// =============================================================================

using System;
using System.IO;
using System.Linq;
using DeepLearning.Application.Abstractions;

namespace DeepLearning.Infrastructure.Pathing;

/// <summary>
/// Resolves paths relative to the application base directory (where the .exe lives).
/// For development builds, this resolves to the project root so that relative paths
/// like "sample.jpg" and "soap_v7.onnx" are found correctly.
/// For published (deployed) builds, this resolves to the publish folder itself,
/// allowing the exe to be distributed as-is with its model and sample files.
/// </summary>
public sealed class ProjectPathProvider : IProjectPathProvider
{
    private readonly string _appRoot;

    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];

    /// <summary>
    /// Initializes a new <see cref="ProjectPathProvider"/> by determining
    /// the appropriate application root directory.
    /// </summary>
    public ProjectPathProvider()
    {
        string baseDir = AppContext.BaseDirectory;

        string devRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

        _appRoot = File.Exists(Path.Combine(devRoot, "DeepLearning.csproj"))
            ? devRoot
            : baseDir;
    }

    /// <inheritdoc />
    public string GetProjectRoot() => _appRoot;

    /// <inheritdoc />
    public string GetProjectFilePath(string relativePath)
        => Path.GetFullPath(Path.Combine(_appRoot, relativePath));

    /// <inheritdoc />
    public string GetAbsolutePath(string path)
        => Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : GetProjectFilePath(path);

    /// <inheritdoc />
    public string[] GetImageFiles()
    {
        if (!Directory.Exists(_appRoot))
            return [];

        return Directory.GetFiles(_appRoot)
            .Where(f => ImageExtensions.Contains(
                Path.GetExtension(f).TrimStart('.').ToLowerInvariant(),
                StringComparer.OrdinalIgnoreCase))
            .Select(f => Path.GetFileName(f))
            .OrderBy(f => f)
            .ToArray();
    }
}
