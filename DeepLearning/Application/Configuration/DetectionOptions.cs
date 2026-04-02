// Central configuration POCO — model path, thresholds, camera index, and output settings.

namespace DeepLearning.Application.Configuration;

public sealed class DetectionOptions
{
    public string ModelPath { get; set; } = "detector_v3.onnx";
    public string[] ClassLabels { get; set; } = ["bottle", "soap", "soap-cover"];
    public int ModelWidth { get; set; } = 640;
    public int ModelHeight { get; set; } = 640;
    public float ConfidenceThreshold { get; set; } = 0.45f;
    public float IouThreshold { get; set; } = 0.45f;
    public int CameraIndex { get; set; } = 0;
    public string WindowTitle { get; set; } = "Object Detection (ESC to exit)";
    public string DefaultImagePath { get; set; } = "sample.jpg";
    public string OutputFileName { get; set; } = "output.jpg";
    public bool AutoOpenOutput { get; set; } = true;
}
