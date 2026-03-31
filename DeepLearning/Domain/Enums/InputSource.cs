// Input mode enum — Webcam (live camera) or ExistingImage (static file).

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
