// Static catalog of known ONNX models with their human-readable summaries.

using DeepLearning.Application.Models;

namespace DeepLearning.Infrastructure.ModelMetadata;

/// <summary>
/// Central registry of known detection models and their metadata.
/// When a user loads a model that matches a known entry, the app displays
/// a rich summary instead of just the filename.
/// </summary>
public static class ModelCatalog
{
    /// <summary>
    /// Detector v3 — Multi-object model trained on bottles, soaps, and soap covers.
    /// Trained with 10x augmented dataset (~970 images) for robust real-world detection.
    /// </summary>
    public static ModelSummary DetectorV3 => new()
    {
        ModelName = "Detector v3",
        ModelFile = "detector_v3.onnx",
        Description = "Multi-object detector trained on bottles, soaps, and soap covers. "
            + "Handles various orientations, lighting conditions, and camera angles. "
            + "Built with 10x data augmentation (flip, rotate, brightness, blur, noise) "
            + "for robust real-world performance.",
        Classes = ["bottle", "soap", "soap-cover"],
        InputSize = "640x640",
        TrainingDate = "April 2026",
        Metrics = "mAP50: 78.1% | Precision: 83.2% | Recall: 75.0% | mAP50-95: 58.7%"
    };

    /// <summary>
    /// Detector v2 — Previous model trained on bottle-only dataset (52 images, 6x augmentation).
    /// Retained for backward compatibility.
    /// </summary>
    public static ModelSummary DetectorV2 => new()
    {
        ModelName = "Detector v2",
        ModelFile = "detector_v2.onnx",
        Description = "Object detector trained primarily on bottles. "
            + "Earlier version with limited class coverage. "
            + "Upgraded to v3 for full bottle + soap + soap-cover detection.",
        Classes = ["bottle", "soap", "soap-cover"],
        InputSize = "640x640",
        TrainingDate = "April 2026",
        Metrics = "mAP50: 60.1% | Precision: 80.8% | Recall: 63.9%"
    };

    /// <summary>
    /// YOLOv8n base model — Pre-trained on COCO dataset.
    /// Generic 80-class detector, not fine-tuned for our use case.
    /// </summary>
    public static ModelSummary YoloV8n => new()
    {
        ModelName = "YOLOv8n (COCO)",
        ModelFile = "yolov8n.onnx",
        Description = "Pre-trained YOLOv8 nano model on COCO 80-class dataset. "
            + "Detects common objects (person, car, dog, etc.). "
            + "Not fine-tuned for bottle/soap detection.",
        Classes = ["person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light", "... and 70 more"],
        InputSize = "640x640",
        TrainingDate = "COCO 2017",
        Metrics = "COCO mAP50-95: 37.3%"
    };

    /// <summary>
    /// YOLOv11n base model — Pre-trained on COCO dataset.
    /// Generic 80-class detector, not fine-tuned for our use case.
    /// </summary>
    public static ModelSummary YoloV11n => new()
    {
        ModelName = "YOLOv11n (COCO)",
        ModelFile = "yolo11n.onnx",
        Description = "Pre-trained YOLOv11 nano model on COCO 80-class dataset. "
            + "Latest YOLO architecture with improved accuracy. "
            + "Not fine-tuned for bottle/soap detection.",
        Classes = ["person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light", "... and 70 more"],
        InputSize = "640x640",
        TrainingDate = "COCO 2017",
        Metrics = "COCO mAP50-95: 39.5%"
    };

    /// <summary>
    /// Bottle v1 — Legacy single-class bottle detector.
    /// Superseded by v2 and v3.
    /// </summary>
    public static ModelSummary BottleV1 => new()
    {
        ModelName = "Bottle Detector v1",
        ModelFile = "bottle_v1.onnx",
        Description = "Legacy single-class bottle detector. "
            + "Only detects bottles. Superseded by detector_v2.onnx and detector_v3.onnx "
            + "which also detect soap and soap-cover.",
        Classes = ["bottle"],
        InputSize = "640x640",
        TrainingDate = "March 2026",
        Metrics = "Bottle-only detection"
    };

    /// <summary>
    /// Tries to find a known model summary by filename.
    /// </summary>
    /// <param name="modelPath">Full or relative path to the ONNX model file.</param>
    /// <returns>The matching ModelSummary, or null if the model is not in the catalog.</returns>
    public static ModelSummary? TryGetSummary(string modelPath)
    {
        if (string.IsNullOrEmpty(modelPath))
            return null;

        string fileName = Path.GetFileName(modelPath).ToLowerInvariant();

        return fileName switch
        {
            "detector_v3.onnx" => DetectorV3,
            "detector_v2.onnx" => DetectorV2,
            "yolov8n.onnx" => YoloV8n,
            "yolo11n.onnx" => YoloV11n,
            "bottle_v1.onnx" => BottleV1,
            _ => null
        };
    }
}
