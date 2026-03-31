// =============================================================================
// SoapDetector — Composition Root
// =============================================================================
//
// FILE:         Program.cs
// LAYER:        Composition Root (outside Clean Architecture layers)
// DEPENDENCIES: All layers (Application, Infrastructure, Presentation, Domain)
//
// PURPOSE:
//   This is the single entry point and composition root of the application.
//   It is the ONLY place where concrete implementations are instantiated and
//   wired together. Every other file in the project depends on abstractions
//   (interfaces), not concretions — this is the Dependency Inversion Principle.
//
// RESPONSIBILITIES:
//   1. Create DetectionOptions with default configuration
//   2. Ask user: custom ONNX model or default?
//   3. If custom: browse for .onnx, infer class count, prompt for labels
//   4. Instantiate all concrete implementations (detector, renderer, UI, etc.)
//   5. Wire dependencies into use case classes
//   6. Start the main application loop via RunDetectionApplication.Execute()
//
// RUNTIME FLOW:
//   Main() → PromptLoadCustomModel() → LoadCustomModelFlow() → Wire deps → Execute()
//
// KEY DESIGN DECISIONS:
//   - [STAThread] is required for WinForms OpenFileDialog (used in ConsoleUserInterface)
//   - OnnxObjectDetector is wrapped in 'using' because it implements IDisposable
//     (the ONNX InferenceSession holds unmanaged resources)
//   - All dependencies are passed via constructor injection — no DI container needed
//     for an application of this size
//
// TO EXTEND:
//   - Swap detection engine: change line 25 to new YourDetector(options)
//   - Swap UI: change line 18 to new YourUserInterface()
//   - Swap renderer: change line 27 to new YourRenderer(options)
//   - Zero changes needed in Application or Domain layers
//
// =============================================================================

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

        int inferredClassCount;
        try
        {
            using OnnxObjectDetector probeDetector = new(options);
            inferredClassCount = probeDetector.InferClassCount();
            userInterface.ShowInfo($"  Model inference: detected {inferredClassCount} class(es).");
        }
        catch (Exception ex)
        {
            userInterface.ShowError($"  Could not infer class count from the ONNX output: {ex.Message}");
            userInterface.ShowInfo("");
            userInterface.ShowInfo("  Falling back to manual class count entry.");
            inferredClassCount = PromptForModelClassCount(userInterface);
        }

        string[] classLabels = userInterface.PromptForClassLabels(inferredClassCount);
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
