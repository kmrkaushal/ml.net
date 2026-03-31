// =============================================================================
// IWebcamDetectionLoop — Webcam Detection Contract
// =============================================================================
//
// FILE:         IWebcamDetectionLoop.cs
// LAYER:        Application (Abstractions)
// DEPENDENCIES: None (pure interface)
// DEPENDENTS:   WebcamDetectionLoop (Infrastructure), RunDetectionApplication
//
// PURPOSE:
//   Defines the contract for the real-time webcam detection loop.
//   Single method: Run() — blocks until user exits (typically via ESC key).
//
// CONTRACT:
//   - Input:  None (configuration is injected via constructor)
//   - Output: None (displays frames in a window, side-effect only)
//   - Blocking: YES — this method does not return until the user exits
//   - Cleanup: Implementations must release camera and destroy windows
//
// WHY THIS EXISTS:
//   Encapsulates all webcam complexity (capture, display, input handling)
//   behind a single method. The application just calls Run() and waits.
//
// =============================================================================

namespace DeepLearning.Application.Abstractions;

/// <summary>
/// Contract for the webcam capture and real-time detection loop.
/// Implementations manage camera access, frame acquisition, and the detection display pipeline.
/// </summary>
public interface IWebcamDetectionLoop
{
    /// <summary>
    /// Starts the real-time detection loop. Blocks until the user exits (e.g. by pressing ESC).
    /// </summary>
    void Run();
}
