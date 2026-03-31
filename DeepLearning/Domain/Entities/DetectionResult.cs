// =============================================================================
// DetectionResult — Core Domain Entity
// =============================================================================
//
// FILE:         DetectionResult.cs
// LAYER:        Domain (Entities)
// DEPENDENCIES: None — pure domain object, zero external dependencies
// DEPENDENTS:   Every layer (shared data type across the entire application)
//
// PURPOSE:
//   Represents a single detected object returned by the detection engine.
//   This is the fundamental data type that every layer in the application
//   understands and uses.
//
// PROPERTIES:
//   ClassId    (required int)   — Zero-based index into the ClassLabels array
//   Confidence (required float) — Model confidence: 0.0 (not confident) to 1.0 (certain)
//   X1, Y1     (required float) — Top-left corner of bounding box (in original image pixels)
//   X2, Y2     (required float) — Bottom-right corner of bounding box (in original image pixels)
//
// COMPUTED PROPERTIES (derived, never stored):
//   Width  => X2 - X1         — Bounding box width in pixels
//   Height => Y2 - Y1         — Bounding box height in pixels
//   Area   => Width * Height  — Bounding box area in square pixels
//
// METHODS:
//   Format(classLabels) — Returns human-readable string like:
//                         "soap 44.2% at (206, 190, 727, 953)"
//
// DESIGN NOTES:
//   - 'sealed': cannot be inherited — this is a complete, final type
//   - 'required init': all properties MUST be set during construction.
//     The compiler prevents creating a DetectionResult with missing data.
//   - 'init' (not 'set'): properties are immutable after construction.
//     This makes DetectionResult a value object — its state never changes.
//   - Computed properties (Width, Height, Area) are calculated on demand.
//     They use no memory and are always consistent with X1/Y1/X2/Y2.
//   - Format() includes safe class label lookup: if ClassId is out of range,
//     it returns "unknown(N)" instead of throwing an exception.
//   - Coordinate format: corner-based (X1,Y1 = top-left, X2,Y2 = bottom-right).
//     This is different from YOLO's center-based format (centerX, centerY, w, h).
//     Conversion happens in OnnxObjectDetector.ParseDetections().
//
// =============================================================================

namespace DeepLearning.Domain.Entities;

/// <summary>
/// Represents a single detected object returned by the model.
/// Contains the class index, confidence score, and bounding box in corner-format coordinates.
/// </summary>
public sealed class DetectionResult
{
    /// <summary>
    /// Zero-based index of the detected class. Maps to the label at the same index in the class label array.
    /// </summary>
    public required int ClassId { get; init; }

    /// <summary>
    /// Model confidence for this detection, between 0.0 (not confident) and 1.0 (very confident).
    /// </summary>
    public required float Confidence { get; init; }

    /// <summary>
    /// Left edge of the bounding box, in original image pixels.
    /// </summary>
    public required float X1 { get; init; }

    /// <summary>
    /// Top edge of the bounding box, in original image pixels.
    /// </summary>
    public required float Y1 { get; init; }

    /// <summary>
    /// Right edge of the bounding box, in original image pixels.
    /// </summary>
    public required float X2 { get; init; }

    /// <summary>
    /// Bottom edge of the bounding box, in original image pixels.
    /// </summary>
    public required float Y2 { get; init; }

    /// <summary>Width of the bounding box in pixels (X2 minus X1).</summary>
    public float Width => X2 - X1;

    /// <summary>Height of the bounding box in pixels (Y2 minus Y1).</summary>
    public float Height => Y2 - Y1;

    /// <summary>Area of the bounding box in square pixels.</summary>
    public float Area => Width * Height;

    /// <summary>
    /// Builds a human-readable string describing this detection.
    /// </summary>
    /// <param name="classLabels">Array of class label strings, indexed by class ID.</param>
    /// <returns>A string like "soap 44.2% at (206, 190, 727, 953)".</returns>
    public string Format(string[] classLabels)
    {
        string label = ClassId >= 0 && ClassId < classLabels.Length
            ? classLabels[ClassId]
            : $"unknown({ClassId})";

        return $"{label} {Confidence:P1} at ({X1:F0}, {Y1:F0}, {X2:F0}, {Y2:F0})";
    }
}
