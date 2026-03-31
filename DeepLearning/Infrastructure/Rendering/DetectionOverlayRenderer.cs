// =============================================================================
// DetectionOverlayRenderer — Bounding Box & Label Drawer (GDI+)
// =============================================================================
//
// FILE:         DetectionOverlayRenderer.cs
// LAYER:        Infrastructure (Rendering)
// DEPENDENCIES: System.Drawing (GDI+), Application (DetectionOptions)
// DEPENDENTS:   DetectImageFromFileUseCase, WebcamDetectionLoop
//
// PURPOSE:
//   Draws bounding boxes and class labels on images for detected objects.
//   Uses GDI+ (System.Drawing) to render overlays without modifying the source.
//
// RENDERING PIPELINE (per detection):
//   1. Create a copy of the source image (don't modify original)
//   2. Create GDI+ drawing objects: Pen, Font, Brushes
//   3. For each detection:
//      a. Draw DeepSkyBlue rectangle (2px) at bounding box coordinates
//      b. Format label text: "className confidence" (e.g., "soap 0.72")
//      c. Measure text size to calculate label background dimensions
//      d. Position label above the box (clamped to image top edge)
//      e. Fill semi-transparent dark background (ARGB: 200, 15, 23, 42)
//      f. Draw white text with 4px padding
//   4. Return the annotated image
//
// DESIGN NOTES:
//   - All GDI+ objects (Graphics, Pen, Font, Brush) are wrapped in 'using'
//     statements — they wrap unmanaged resources and MUST be disposed to
//     prevent memory leaks
//   - The source image is NEVER modified — a new Bitmap is created (line 28)
//   - Label positioning: placed above the box, but clamped to Y=0 so it
//     never goes off the top edge of the image
//   - Class label lookup is safe: if ClassId is out of range, falls back
//     to "class N" instead of throwing an exception
//
// =============================================================================

using System.Drawing;
using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Domain.Entities;

namespace DeepLearning.Infrastructure.Rendering;

/// <summary>
/// Draws bounding boxes and class labels on images for detected objects.
/// Uses GDI+ via <see cref="System.Drawing"/> to render overlays without modifying the source image.
/// </summary>
public sealed class DetectionOverlayRenderer : IImageRenderer
{
    private readonly string[] _classLabels;

    /// <summary>
    /// Creates a new renderer configured with the given options.
    /// </summary>
    /// <param name="options">Must supply the <see cref="DetectionOptions.ClassLabels"/> array.</param>
    public DetectionOverlayRenderer(DetectionOptions options)
    {
        _classLabels = options.ClassLabels;
    }

    /// <inheritdoc />
    public Bitmap DrawDetections(Bitmap image, IReadOnlyCollection<DetectionResult> detections)
    {
        Bitmap canvas = new(image);

        using Graphics graphics = Graphics.FromImage(canvas);
        using Pen boxPen = new(Color.DeepSkyBlue, 2f);
        using Font labelFont = new("Segoe UI", 9, FontStyle.Bold);
        using SolidBrush labelBackground = new(Color.FromArgb(200, 15, 23, 42));
        using SolidBrush labelForeground = new(Color.White);

        foreach (DetectionResult detection in detections)
        {
            Rectangle rectangle = Rectangle.FromLTRB(
                (int)detection.X1,
                (int)detection.Y1,
                (int)detection.X2,
                (int)detection.Y2);

            graphics.DrawRectangle(boxPen, rectangle);

            string label = FormatLabel(detection);
            SizeF textSize = graphics.MeasureString(label, labelFont);
            float labelY = Math.Max(0, detection.Y1 - textSize.Height - 4);
            RectangleF labelRectangle = new(detection.X1, labelY, textSize.Width + 8, textSize.Height + 4);

            graphics.FillRectangle(labelBackground, labelRectangle);
            graphics.DrawString(label, labelFont, labelForeground, labelRectangle.X + 4, labelRectangle.Y + 2);
        }

        return canvas;
    }

    private string FormatLabel(DetectionResult detection)
    {
        string label = detection.ClassId >= 0 && detection.ClassId < _classLabels.Length
            ? _classLabels[detection.ClassId]
            : $"class {detection.ClassId}";

        return $"{label} {detection.Confidence:0.00}";
    }
}
