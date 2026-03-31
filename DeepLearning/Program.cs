using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Application.UseCases;
using DeepLearning.Infrastructure.Capture;
using DeepLearning.Infrastructure.Detection;
using DeepLearning.Infrastructure.Pathing;
using DeepLearning.Infrastructure.Rendering;
using DeepLearning.Presentation.UI;

namespace DeepLearning;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var options = new DetectionOptions();
        IUserInterface userInterface = new ConsoleUserInterface();

        if (PromptLoadCustomModel(userInterface, options))
        {
            LoadCustomModelFlow(userInterface, options);
        }

        using IObjectDetector detector = new OnnxObjectDetector(options);
        IProjectPathProvider pathProvider = new ProjectPathProvider();
        IImageRenderer imageRenderer = new DetectionOverlayRenderer(options);
        IWebcamDetectionLoop webcamDetectionLoop = new WebcamDetectionLoop(options, detector, imageRenderer, userInterface);

        var detectImageFromFileUseCase = new DetectImageFromFileUseCase(
            options,
            detector,
            imageRenderer,
            pathProvider);

        var runDetectionApplication = new RunDetectionApplication(
            options,
            userInterface,
            webcamDetectionLoop,
            detectImageFromFileUseCase,
            pathProvider);

        runDetectionApplication.Execute();
    }

    private static bool PromptLoadCustomModel(IUserInterface userInterface, DetectionOptions options)
    {
        userInterface.ShowInfo("");
        userInterface.ShowInfo("  Would you like to load a custom ONNX model?");
        userInterface.ShowInfo("  Or use the default model configured in the app.");
        userInterface.ShowInfo("");

        while (true)
        {
            userInterface.ShowInfo("  [1] Browse my device for ONNX model");
            userInterface.ShowInfo("  [2] Use default model");
            userInterface.ShowInfo("");

            userInterface.ShowPrompt("Choice");
            string? input = Console.ReadLine()?.Trim();
            userInterface.ShowInfo("");

            if (input == "1") return true;
            if (input == "2") return false;

            userInterface.ShowError("Invalid. Enter 1 or 2.");
            userInterface.ShowInfo("");
        }
    }

    private static void LoadCustomModelFlow(IUserInterface userInterface, DetectionOptions options)
    {
        userInterface.ShowInfo("");
        userInterface.ShowInfo("  ═══ LOAD CUSTOM MODEL ═══");
        userInterface.ShowInfo("");

        string? onnxPath = userInterface.BrowseForOnnxFile();
        if (string.IsNullOrEmpty(onnxPath))
        {
            userInterface.ShowInfo("No model selected. Using default.");
            return;
        }

        userInterface.ShowInfo($"  Model selected: {Path.GetFileName(onnxPath)}");
        options.ModelPath = onnxPath;

        int expectedClasses = PromptForModelClassCount(userInterface);
        string[] classLabels = userInterface.PromptForClassLabels(expectedClasses);
        options.ClassLabels = classLabels;

        userInterface.ShowInfo("");
        userInterface.ShowSuccess($"  Custom model loaded successfully!");
        userInterface.ShowInfo("");
    }

    private static int PromptForModelClassCount(IUserInterface userInterface)
    {
        while (true)
        {
            userInterface.ShowInfo("  How many classes does your model detect?");
            userInterface.ShowInfo("  (Enter a number, e.g., 2 for binary classification)");
            userInterface.ShowInfo("");

            userInterface.ShowPrompt("Class count");
            string? input = Console.ReadLine()?.Trim();
            userInterface.ShowInfo("");

            if (int.TryParse(input, out int count) && count > 0)
            {
                return count;
            }

            userInterface.ShowError("Invalid. Enter a positive number.");
            userInterface.ShowInfo("");
        }
    }
}
