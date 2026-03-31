// =============================================================================
// IProjectPathProvider — Path Resolution Contract
// =============================================================================
//
// FILE:         IProjectPathProvider.cs
// LAYER:        Application (Abstractions)
// DEPENDENCIES: None (pure interface)
// DEPENDENTS:   ProjectPathProvider (Infrastructure), DetectImageFromFileUseCase,
//               RunDetectionApplication, ConsoleUserInterface
//
// PURPOSE:
//   Defines the contract for resolving file paths in a way that works
//   both during development and after deployment.
//
// THE PROBLEM IT SOLVES:
//   During development, the app runs from bin/Debug/net8.0-windows/ but files
//   like sample.jpg and soap_v7.onnx are in the project root. After publishing,
//   the exe and files are in the same folder. This abstraction handles both cases.
//
// METHODS:
//   GetProjectRoot()     — Returns the absolute path to the app root directory
//   GetProjectFilePath() — Combines root with a relative path → absolute path
//   GetAbsolutePath()    — If already absolute, normalize; else resolve against root
//   GetImageFiles()      — Enumerate all image files (.jpg, .png, etc.) in root
//
// =============================================================================

using DeepLearning.Application.Abstractions;

namespace DeepLearning.Application.Abstractions;

/// <summary>
/// Contract for resolving project-relative and absolute file paths,
/// and for enumerating available image files.
/// </summary>
public interface IProjectPathProvider
{
    /// <summary>
    /// Returns the absolute path to the application root directory.
    /// </summary>
    string GetProjectRoot();

    /// <summary>
    /// Combines the project root with a relative path and returns the full absolute path.
    /// </summary>
    string GetProjectFilePath(string relativePath);

    /// <summary>
    /// Converts any path to an absolute path.
    /// If rooted, returns as-is; otherwise resolves relative to the project root.
    /// </summary>
    string GetAbsolutePath(string path);

    /// <summary>
    /// Returns all image files found in the application root directory.
    /// </summary>
    string[] GetImageFiles();
}
