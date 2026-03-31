// =============================================================================
// IObjectDetector — Detection Engine Contract
// =============================================================================
//
// FILE:         IObjectDetector.cs
// LAYER:        Application (Abstractions)
// DEPENDENCIES: Domain (DetectionResult)
// DEPENDENTS:   OnnxObjectDetector (Infrastructure), DetectImageFromFileUseCase,
//               WebcamDetectionLoop
//
// PURPOSE:
//   Defines the contract that ANY object detection engine must fulfill.
//   It says "what" detection means without specifying "how" it's done.
//
// CONTRACT:
//   - Input:  A System.Drawing.Bitmap of any size
//   - Output: IReadOnlyList<DetectionResult> (empty list = nothing found, never null)
//   - Side effect: None (pure function — same input always produces same output)
//   - Lifecycle: Implements IDisposable — implementations must clean up resources
//                (e.g., ONNX InferenceSession, native handles)
//
// WHY THIS EXISTS:
//   This interface decouples the application from any specific ML runtime.
//   The current implementation uses ONNX Runtime, but you could create:
//   - TensorRtObjectDetector (NVIDIA TensorRT)
//   - OpenVinoObjectDetector (Intel OpenVINO)
//   - MockObjectDetector (for unit testing)
//   ...and swap them by changing ONE line in Program.cs.
//
// =============================================================================

using System.Drawing;
using DeepLearning.Domain.Entities;

namespace DeepLearning.Application.Abstractions;

/// <summary>
/// Contract for any object detection engine.
/// Implementations load a model file and return a list of detected objects for a given image.
/// This abstraction allows the application to stay decoupled from any specific ML runtime (ONNX, TensorRT, etc.).
/// </summary>
public interface IObjectDetector : IDisposable
{
    /// <summary>
    /// Runs object detection on a single image.
    /// </summary>
    /// <param name="image">The source image to analyze. Any size is supported; the detector resizes internally.</param>
    /// <returns>A read-only list of detected objects with positions and confidence scores. Returns an empty list if no objects meet the confidence threshold.</returns>
    IReadOnlyList<DetectionResult> Detect(Bitmap image);
}
