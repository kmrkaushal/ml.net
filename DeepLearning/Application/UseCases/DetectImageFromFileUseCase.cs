// =============================================================================
// DetectImageFromFileUseCase — Single Image Detection Workflow
// =============================================================================
//
// FILE:         DetectImageFromFileUseCase.cs
// LAYER:        Application (Use Cases)
// DEPENDENCIES: Application (IObjectDetector, IImageRenderer, IProjectPathProvider,
//               DetectionOptions), Domain (DetectionResult), System.Diagnostics
// DEPENDENTS:   RunDetectionApplication, Program.cs
//
// PURPOSE:
//   Encapsulates the complete workflow for detecting objects in a single image
//   file. This is a pure pipeline: input → transform → output.
//
// PIPELINE STEPS:
//   1. ResolveImagePath() — convert user input (empty/relative/absolute) to absolute path
//   2. Validate — throw FileNotFoundException if file doesn't exist
//   3. Load — create Bitmap from file (using statement ensures disposal)
//   4. Detect — run IObjectDetector.Detect() → list of DetectionResult
//   5. Render — run IImageRenderer.DrawDetections() → annotated Bitmap
//   6. Save — write annotated Bitmap to output.jpg
//   7. Open — optionally launch the output image in the default viewer
//   8. Report — return ImageDetectionReport with input/output paths and detections
//
// DESIGN NOTES:
//   - The 'using' statements on lines 73 and 75 ensure Bitmaps are disposed
//     even if an exception occurs (critical for GDI+ resource management)
//   - ResolveImagePath() handles three cases:
//     * Empty/whitespace → use default image from config
//     * Absolute path → normalize and return
//     * Relative path → resolve against project root
//   - This use case is side-effect free from the perspective of its return value
//     (all I/O is encapsulated in the report), making it easy to test
//
// =============================================================================

using System.Diagnostics;
using System.Drawing;
using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Application.Models;

namespace DeepLearning.Application.UseCases;

/// <summary>
/// Encapsulates the complete workflow for detecting objects in a single image file.
///
/// Steps:
/// <list type="number">
///   <item>Resolve the image path (relative or absolute).</item>
///   <item>Load the image from disk.</item>
///   <item>Run the object detector to find objects.</item>
///   <item>Render the detection overlays onto a copy of the image.</item>
///   <item>Save the annotated image to disk.</item>
///   <item>Optionally open the result in the default image viewer.</item>
///   <item>Return a report with input, output paths and detection results.</item>
/// </list>
///
/// This use case is side-effect free from the perspective of its return value
/// (all I/O is encapsulated in the report), making it easy to test.
/// </summary>
public sealed class DetectImageFromFileUseCase
{
    private readonly DetectionOptions _options;
    private readonly IObjectDetector _detector;
    private readonly IImageRenderer _imageRenderer;
    private readonly IProjectPathProvider _pathProvider;

    /// <summary>
    /// Creates a new instance configured with the given dependencies.
    /// </summary>
    /// <param name="options">Detection configuration such as thresholds and output settings.</param>
    /// <param name="detector">The object detection engine to use.</param>
    /// <param name="imageRenderer">The overlay renderer for drawing boxes and labels.</param>
    /// <param name="pathProvider">Resolves relative and absolute file paths.</param>
    public DetectImageFromFileUseCase(
        DetectionOptions options,
        IObjectDetector detector,
        IImageRenderer imageRenderer,
        IProjectPathProvider pathProvider)
    {
        _options = options;
        _detector = detector;
        _imageRenderer = imageRenderer;
        _pathProvider = pathProvider;
    }

    /// <summary>
    /// Executes the full image detection workflow for the given path.
    /// </summary>
    /// <param name="requestedPath">
    /// An absolute path, a path relative to the project root, or an empty string to use the default image.
    /// </param>
    /// <returns>An <see cref="ImageDetectionReport"/> containing input/output paths and all detections.</returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the resolved image file does not exist on disk.
    /// </exception>
    public ImageDetectionReport Execute(string requestedPath)
    {
        string imagePath = ResolveImagePath(requestedPath);

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException(
                $"Image file was not found: {imagePath}",
                imagePath);
        }

        using Bitmap sourceImage = new(imagePath);
        IReadOnlyList<Domain.Entities.DetectionResult> detections = _detector.Detect(sourceImage);
        using Bitmap renderedImage = _imageRenderer.DrawDetections(sourceImage, detections);

        string outputPath = _pathProvider.GetProjectFilePath(_options.OutputFileName);
        renderedImage.Save(outputPath);

        if (_options.AutoOpenOutput)
        {
            try { Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true }); }
            catch { }
        }

        return new ImageDetectionReport
        {
            InputPath = imagePath,
            OutputPath = outputPath,
            Detections = detections
        };
    }

    /// <summary>
    /// Resolves a user-provided path string to an absolute filesystem path.
    /// If the path is empty or whitespace, the default image from settings is used.
    /// If the path is already absolute, it is returned as-is.
    /// If it is relative, it is resolved against the project root directory.
    /// </summary>
    private string ResolveImagePath(string requestedPath)
    {
        if (string.IsNullOrWhiteSpace(requestedPath))
        {
            return _pathProvider.GetProjectFilePath(_options.DefaultImagePath);
        }

        return _pathProvider.GetAbsolutePath(requestedPath.Trim());
    }
}
