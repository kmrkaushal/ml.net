// =============================================================================
// DetectionOptions — Central Configuration POCO
// =============================================================================
//
// FILE:         DetectionOptions.cs
// LAYER:        Application (Configuration)
// DEPENDENCIES: None (pure data class)
// DEPENDENTS:   Every layer — flows through the entire application
//
// PURPOSE:
//   Single source of truth for all application configuration. This object
//   is created in Program.cs and passed to every component that needs it.
//
// PROPERTIES (11 total):
//   ModelPath           — Path to the ONNX model file (default: soap_v7.onnx)
//   ClassLabels         — Human-readable names for model classes
//   ModelWidth/Height   — Input dimensions the model expects (640×640 for YOLO)
//   ConfidenceThreshold — Minimum confidence to keep a detection (0.20 = 20%)
//   IouThreshold        — Max overlap before NMS removes a box (0.45 = 45%)
//   CameraIndex         — Which webcam to use (0 = first camera)
//   WindowTitle         — Title of the OpenCV display window
//   DefaultImagePath    — Fallback image when user doesn't specify one
//   OutputFileName      — Name of the annotated output image
//   AutoOpenOutput      — Whether to auto-open output image after detection
//
// DESIGN NOTES:
//   - Not sealed: could be extended, but typically used as-is
//   - Mutable by design: properties can be changed at runtime (needed for
//     custom model loading flow in Program.cs)
//   - No validation: callers are responsible for providing valid values
//
// =============================================================================

namespace DeepLearning.Application.Configuration;

public sealed class DetectionOptions
{
    public string ModelPath { get; set; } = "soap_v7.onnx";
    public string[] ClassLabels { get; set; } = ["soap", "soap-cover"];
    public int ModelWidth { get; set; } = 640;
    public int ModelHeight { get; set; } = 640;
    public float ConfidenceThreshold { get; set; } = 0.20f;
    public float IouThreshold { get; set; } = 0.45f;
    public int CameraIndex { get; set; } = 0;
    public string WindowTitle { get; set; } = "Object Detection (ESC to exit)";
    public string DefaultImagePath { get; set; } = "sample.jpg";
    public string OutputFileName { get; set; } = "output.jpg";
    public bool AutoOpenOutput { get; set; } = true;
}
