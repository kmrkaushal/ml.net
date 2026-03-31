// =============================================================================
// InputSource — Input Mode Enumeration
// =============================================================================
//
// FILE:         InputSource.cs
// LAYER:        Domain (Enums)
// DEPENDENCIES: None — pure enum, zero external dependencies
// DEPENDENTS:   RunDetectionApplication, ConsoleUserInterface
//
// PURPOSE:
//   Represents the two available input modes for the detection application.
//   Using an enum instead of a boolean makes the code self-documenting.
//
// VALUES:
//   Webcam = 1        — Live camera mode: capture frames from webcam in real-time
//   ExistingImage = 2 — Static image mode: load a single image file from disk
//
// DESIGN NOTES:
//   - Explicit values (1, 2): prevents accidental default(0) bugs
//   - Why not a boolean? 'bool isWebcam' is ambiguous at call sites.
//     'InputSource.Webcam' is immediately clear and self-documenting.
//   - This enum lives in Domain because it represents a fundamental
//     business concept (how does the user want to provide input?),
//     not a technical implementation detail.
//
// =============================================================================

namespace DeepLearning.Domain.Enums;

/// <summary>
/// Represents the two available input modes for the detection application.
/// </summary>
public enum InputSource
{
    /// <summary>
    /// Use the webcam as a live video source for real-time object detection.
    /// </summary>
    Webcam = 1,

    /// <summary>
    /// Use an existing image file stored on disk.
    /// </summary>
    ExistingImage = 2
}
