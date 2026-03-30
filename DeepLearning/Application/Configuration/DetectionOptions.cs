namespace DeepLearning.Application.Configuration;

/// <summary>
/// Holds all configurable settings for the detection pipeline.
/// Change these values to adapt the app to different models, thresholds, cameras, or output behavior.
///
/// <para>
/// To change settings permanently, edit the default property values below.
/// To override at runtime (e.g., from a config file or CLI argument), construct this class
/// with the desired values instead of relying on the defaults.
/// </para>
/// </summary>
public sealed class DetectionOptions
{
    /// <summary>
    /// Path to the ONNX model file. Must be in the same directory as the executable or accessible via a full path.
    /// </summary>
    public string ModelPath { get; init; } = "soap_v7.onnx";

    /// <summary>
    /// Class label names in the exact order used during model training.
    /// The index in this array corresponds to the ClassId in each <see cref="Domain.Entities.DetectionResult"/>.
    /// </summary>
    public string[] ClassLabels { get; init; } = ["soap", "soap-cover"];

    /// <summary>
    /// Model input width in pixels. Must match the model's expected input size.
    /// </summary>
    public int ModelWidth { get; init; } = 640;

    /// <summary>
    /// Model input height in pixels. Must match the model's expected input size.
    /// </summary>
    public int ModelHeight { get; init; } = 640;

    /// <summary>
    /// Minimum confidence score to accept a detection (0.0 to 1.0).
    /// Lower values show more detections but may include false positives.
    /// Higher values are stricter and show only confident detections.
    /// </summary>
    public float ConfidenceThreshold { get; init; } = 0.20f;

    /// <summary>
    /// IoU (Intersection over Union) threshold for Non-Maximum Suppression (0.0 to 1.0).
    /// Boxes with IoU above this value are considered duplicates of the same object.
    /// Lower values allow more overlapping boxes; higher values remove more boxes.
    /// </summary>
    public float IouThreshold { get; init; } = 0.45f;

    /// <summary>
    /// Camera device index passed to OpenCV's VideoCapture.
    /// 0 = default webcam, 1 = second camera, etc.
    /// </summary>
    public int CameraIndex { get; init; } = 0;

    /// <summary>
    /// Title of the webcam display window.
    /// </summary>
    public string WindowTitle { get; init; } = "Soap Detection (ESC to exit)";

    /// <summary>
    /// Default image file used when the user presses Enter without typing a path.
    /// Relative to the project root directory.
    /// </summary>
    public string DefaultImagePath { get; init; } = "sample.jpg";

    /// <summary>
    /// Filename for the annotated output image. Saved in the project root directory.
    /// </summary>
    public string OutputFileName { get; init; } = "output.jpg";

    /// <summary>
    /// When true, the annotated output image is opened automatically in the default image viewer after saving.
    /// Set to false for unattended or automated environments.
    /// </summary>
    public bool AutoOpenOutput { get; init; } = true;
}
