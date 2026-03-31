// =============================================================================
// ImageDetectionReport — Detection Result DTO
// =============================================================================
//
// FILE:         ImageDetectionReport.cs
// LAYER:        Application (Models)
// DEPENDENCIES: Domain (DetectionResult)
// DEPENDENTS:   DetectImageFromFileUseCase, ConsoleUserInterface
//
// PURPOSE:
//   Encapsulates the complete result of an image detection run. This is a
//   Data Transfer Object (DTO) — it carries data between layers without
//   containing any behavior.
//
// PROPERTIES (all required init-only):
//   InputPath  — Full path to the input image that was processed
//   OutputPath — Full path to the annotated output image that was saved
//   Detections — List of DetectionResult objects found in the image
//
// DESIGN NOTES:
//   - 'required' keyword: compiler enforces that ALL three properties are set.
//     You cannot create a partially-populated report — this is a compile-time
//     safety net that prevents null reference bugs.
//   - 'init' keyword: properties can only be set during object initialization.
//     After construction, the report is immutable.
//   - 'sealed' keyword: cannot be inherited — this class is a complete type.
//
// =============================================================================

using System.Collections.Generic;
using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Application.Models;
using DeepLearning.Domain.Entities;

namespace DeepLearning.Application.Models;

/// <summary>
/// Encapsulates the complete result of an image detection run.
/// Produced by <see cref="UseCases.DetectImageFromFileUseCase"/> and consumed by the user interface.
/// </summary>
public sealed class ImageDetectionReport
{
    /// <summary>
    /// The full path to the input image file that was processed.
    /// </summary>
    public required string InputPath { get; init; }

    /// <summary>
    /// The full path to the output image file where annotations were saved.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// The list of detected objects found in the image.
    /// </summary>
    public required IReadOnlyList<DetectionResult> Detections { get; init; }
}
