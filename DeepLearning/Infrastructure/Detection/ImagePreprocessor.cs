// =============================================================================
// ImagePreprocessor — Bitmap to CHW Tensor Converter
// =============================================================================
//
// FILE:         ImagePreprocessor.cs
// LAYER:        Infrastructure (Detection)
// DEPENDENCIES: System.Drawing (Bitmap, Color, Size)
// DEPENDENTS:   OnnxObjectDetector (internal use only)
//
// PURPOSE:
//   Converts a System.Drawing.Bitmap into a CHW (Channels × Height × Width)
//   normalized float array — the exact format that YOLO neural networks expect
//   as input.
//
// CONVERSION PIPELINE:
//   1. Resize: Scale image to target dimensions (typically 640×640)
//      - Uses bilinear interpolation (default Bitmap constructor behavior)
//   2. Channel separation: HWC → CHW format
//      - Before (interleaved): [R0,G0,B0, R1,G1,B1, ...] — each pixel has all 3 channels together
//      - After  (planar):     [R0,R1,..., G0,G1,..., B0,B1,...] — all R, then all G, then all B
//   3. Normalization: Divide each value by 255
//      - Before: 0-255 (byte range, as stored in the image)
//      - After:  0.0-1.0 (float range, as expected by the neural network)
//
// OUTPUT LAYOUT (for 640×640 image):
//   Array length: 3 × 640 × 640 = 1,228,800 floats
//   Indices 0 to 409,599:       Red channel (all pixels)
//   Indices 409,600 to 819,199: Green channel (all pixels)
//   Indices 819,200 to 1,228,799: Blue channel (all pixels)
//
// DESIGN NOTES:
//   - Static class: no state, no instances needed — pure function
//   - No external dependencies beyond System.Drawing
//   - GetPixel() is not the fastest approach (LockBits would be faster),
//     but it's simple and correct. For production with high FPS needs,
//     consider using Bitmap.LockBits() for direct memory access.
//   - The CHW format is critical: neural networks expect channels as separate
//     planes, not interleaved per-pixel. This is the #1 source of bugs when
//     implementing custom detection pipelines.
//
// =============================================================================

using System.Drawing;

namespace DeepLearning.Infrastructure.Detection;

/// <summary>
/// Converts a <see cref="Bitmap"/> into a CHW (Channels × Height × Width) float array
/// suitable for YOLO model input.
///
/// <para>
/// The conversion pipeline is:
/// <list type="number">
///   <item>Resize the image to the model's target width and height (typically 640×640).</item>
///   <item>Read each pixel and separate the R, G, B channels into three distinct planes.</item>
///   <item>Normalize each channel from 0–255 to 0.0–1.0 by dividing by 255.</item>
///   <item>Pack the planes into a flat float array: [all R, all G, all B].</item>
/// </list>
/// </para>
///
/// <para>
/// This static class has no state and no external dependencies.
/// It is a pure transformation utility used internally by <see cref="OnnxObjectDetector"/>.
/// </para>
/// </summary>
public static class ImagePreprocessor
{
    /// <summary>
    /// Converts a source bitmap to a CHW-normalized float array.
    /// </summary>
    /// <param name="source">The original image in any size.</param>
    /// <param name="targetWidth">Target width in pixels (must match the model's expected input width).</param>
    /// <param name="targetHeight">Target height in pixels (must match the model's expected input height).</param>
    /// <returns>
    /// A flat float array of length 3 × targetHeight × targetWidth,
    /// laid out as [R₀₀, R₀₁, ..., Rₕ₋₁,ᵥ₋₁, G₀₀, G₀₁, ..., B₀₀, B₀₁, ...].
    /// Each value is in the range 0.0 to 1.0.
    /// </returns>
    public static float[] ToChwArray(Bitmap source, int targetWidth, int targetHeight)
    {
        using Bitmap resized = new(source, new Size(targetWidth, targetHeight));

        int pixelCount = targetWidth * targetHeight;
        float[] data = new float[3 * pixelCount];

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                Color pixel = resized.GetPixel(x, y);
                int index = y * targetWidth + x;

                data[index] = pixel.R / 255f;
                data[pixelCount + index] = pixel.G / 255f;
                data[(2 * pixelCount) + index] = pixel.B / 255f;
            }
        }

        return data;
    }
}
