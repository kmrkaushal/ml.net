// =============================================================================
// IImageRenderer — Rendering Contract
// =============================================================================
//
// FILE:         IImageRenderer.cs
// LAYER:        Application (Abstractions)
// DEPENDENCIES: Domain (DetectionResult)
// DEPENDENTS:   DetectionOverlayRenderer (Infrastructure), DetectImageFromFileUseCase,
//               WebcamDetectionLoop
//
// PURPOSE:
//   Defines the contract for drawing detection results (bounding boxes, labels)
//   onto an image.
//
// CONTRACT:
//   - Input:  Original Bitmap (MUST NOT be modified) + detection results
//   - Output: NEW Bitmap with overlays drawn (caller is responsible for disposal)
//   - Rule:   The source image is NEVER modified — a copy is always created
//
// WHY THIS EXISTS:
//   Decouples rendering from detection. You could implement:
//   - DetectionOverlayRenderer (current: GDI+ with blue boxes)
//   - ColoredOverlayRenderer (different color per class)
//   - SvgOverlayRenderer (vector overlay for web)
//   - NoOpRenderer (for benchmarking — skips rendering)
//
// =============================================================================

using System.Drawing;
using DeepLearning.Domain.Entities;

namespace DeepLearning.Application.Abstractions;

/// <summary>
/// Contract for rendering detection results onto an image.
/// Implementations draw bounding boxes and labels on a copy of the source image without modifying the original.
/// </summary>
public interface IImageRenderer
{
    /// <summary>
    /// Draws bounding boxes and class labels on a copy of the provided image.
    /// </summary>
    /// <param name="image">The original image to annotate. It is not modified.</param>
    /// <param name="detections">The detection results to render.</param>
    /// <returns>A new <see cref="Bitmap"/> with the overlays drawn on it. The caller is responsible for disposing it.</returns>
    Bitmap DrawDetections(Bitmap image, IReadOnlyCollection<DetectionResult> detections);
}
