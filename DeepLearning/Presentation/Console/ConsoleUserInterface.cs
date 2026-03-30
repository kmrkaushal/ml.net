using System;
using System.Linq;
using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Application.Models;
using DeepLearning.Domain.Entities;
using DeepLearning.Domain.Enums;

namespace DeepLearning.Presentation.Console;

/// <summary>
/// Handles all console input and output for the detection application.
/// Produces a clean, colorful, production-quality text interface.
///
/// <para>
/// Color scheme:
/// <list>
///   <item>Green  — success, selected items, positive outcomes</item>
///   <item>Red    — errors, failures, invalid input</item>
///   <item>Yellow — warnings, detection bars, confidence</item>
///   <item>Cyan   — headers, paths, system info</item>
///   <item>White  — labels, class names, menu text</item>
///   <item>Gray   — normal body text, prompts</item>
/// </list>
/// </para>
/// </summary>
public sealed class ConsoleUserInterface : IUserInterface
{
    private const char DividerChar = '=';

    private static readonly string[] BannerLines =
    [
        "  _____      _            _____                    ",
        " |  __ \\    | |          / ____|                   ",
        " | |  | |___| |__   ___ | |  __  __ _ _ __ ___  __| |",
        " | |  | / _ \\ '_ \\ / _ \\| | |_ |/ _` | '__/ _ \\/ _` |",
        " | |__| |  __/ |_) | (_) | |__| | (_| | | |  __/ (_| |",
        " |_____/ \\___|_.__/ \\___/ \\_____|\\__,_|_|  \\___|\\__,_|",
        "",
        "  Object Detection Console  |  Powered by ONNX Runtime"
    ];

    public void ShowWelcome(DetectionOptions options)
    {
        PrintBanner();
        PrintDivider();
        PrintInfo("Application started successfully.");
        PrintLine($"  Model file    : {options.ModelPath}");
        PrintLine($"  Classes      : {string.Join(", ", options.ClassLabels)}");
        PrintLine($"  Confidence   : {options.ConfidenceThreshold:P0} (minimum threshold)");
        PrintLine($"  NMS IoU     : {options.IouThreshold:P0} (overlap filter)");
        PrintLine($"  Camera index : {options.CameraIndex}");
        PrintDivider();
        PrintLine();
    }

    public InputSource PromptForInputSource()
    {
        PrintHeader("Select an input source");
        PrintLine();
        PrintLine("  [1]  Webcam");
        PrintLine("  [2]  Image File");
        PrintLine();
        PrintDivider();

        while (true)
        {
            PrintPrompt("Your choice");
            string? input = System.Console.ReadLine()?.Trim();
            PrintLine();

            if (input == "1")
            {
                PrintSuccess("Selected: Webcam mode.");
                return InputSource.Webcam;
            }

            if (input == "2")
            {
                PrintSuccess("Selected: Image File mode.");
                return InputSource.ExistingImage;
            }

            PrintError("Invalid choice. Please enter 1 or 2.");
            PrintLine();
        }
    }

    public string PromptForImagePath(string defaultImagePath, IProjectPathProvider pathProvider)
    {
        PrintHeader("Image File Mode");
        PrintLine();
        PrintLine("  Choose how to provide the image:");
        PrintLine();
        PrintLine("  [1]  Browse available images in the app folder");
        PrintLine("  [2]  Type a custom path manually");
        PrintLine("  [3]  Press Enter to use the default sample image");
        PrintLine();
        PrintDivider();

        while (true)
        {
            PrintPrompt("Option (1, 2 or 3)");
            string? input = System.Console.ReadLine()?.Trim();
            PrintLine();

            if (input == "1")
            {
                return BrowseForImage(pathProvider, defaultImagePath);
            }

            if (input == "2")
            {
                return PromptCustomPath();
            }

            if (string.IsNullOrWhiteSpace(input) || input == "3")
            {
                PrintSuccess($"Using default sample: {defaultImagePath}");
                return defaultImagePath;
            }

            PrintError("Invalid option. Please enter 1, 2, or 3.");
            PrintLine();
        }
    }

    public void ShowImageDetectionReport(ImageDetectionReport report, string[] classLabels)
    {
        PrintDivider();
        PrintHeader("Detection Complete");
        PrintLine();
        PrintCyan($"  Input  : {report.InputPath}");
        PrintGreen($"  Output : {report.OutputPath}");
        PrintLine();
        ShowDetections(report.Detections, classLabels);
        PrintDivider();
    }

    public void ShowWebcamInstructions(DetectionOptions options)
    {
        PrintDivider();
        PrintHeader("Webcam Mode");
        PrintLine();
        PrintInfo($"Opening webcam (index {options.CameraIndex})...");
        PrintInfo($"Press ESC inside the '{options.WindowTitle}' window to stop.");
        PrintDivider();
        PrintLine();
    }

    public void ShowDetections(IReadOnlyList<DetectionResult> detections, string[] classLabels)
    {
        if (detections.Count == 0)
        {
            PrintWarning("No objects met the confidence threshold.");
            PrintLine("  Try lowering ConfidenceThreshold or check the image content.");
            PrintLine();
            return;
        }

        var grouped = detections
            .GroupBy(d => classLabels.ElementAtOrDefault(d.ClassId) ?? $"unknown({d.ClassId})")
            .OrderByDescending(g => g.Max(d => d.Confidence))
            .ToList();

        PrintSuccess($"Total detections: {detections.Count}");
        PrintLine();

        foreach (var group in grouped)
        {
            string className = group.Key;
            int count = group.Count();
            float topConf = group.Max(d => d.Confidence);

            PrintLine($"  {className}  ({count} found, top conf: {topConf:P0})");

            foreach (DetectionResult detection in group.OrderByDescending(d => d.Confidence))
            {
                PrintDetectionItem(detection);
            }

            PrintLine();
        }
    }

    public void ShowInfo(string message) => PrintInfo(message);

    public void ShowError(string message)
    {
        PrintDivider();
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"  ERROR: {message}");
        System.Console.ResetColor();
        PrintDivider();
        PrintLine();
    }

    public bool PromptForContinue()
    {
        PrintLine();
        PrintLine("  What would you like to do next?");
        PrintLine();
        PrintLine("  [1]  Back to main menu");
        PrintLine("  [2]  Exit application");
        PrintLine();
        PrintDivider();

        while (true)
        {
            PrintPrompt("Choice (1 or 2)");
            string? input = System.Console.ReadLine()?.Trim();
            PrintLine();

            if (input == "1")
            {
                return true;
            }

            if (input == "2")
            {
                return false;
            }

            PrintError("Invalid choice. Please enter 1 or 2.");
            PrintLine();
        }
    }

    public void ShowExitMessage()
    {
        PrintDivider();
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("  Goodbye! Thanks for using Soap Detector.");
        System.Console.ResetColor();
        PrintDivider();
    }

    // ──────────────── Private helpers ────────────────

    private string BrowseForImage(IProjectPathProvider pathProvider, string defaultImagePath)
    {
        string[] images = pathProvider.GetImageFiles();

        if (images.Length == 0)
        {
            PrintWarning("No image files found in the app folder.");
            PrintInfo("Please type a custom path manually.");
            PrintLine();
            return PromptCustomPath();
        }

        PrintHeader("Available images in app folder");
        PrintLine();
        PrintCyan($"  [0]  Use default sample");
        for (int i = 0; i < images.Length; i++)
        {
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write($"  [{i + 1}]  ");
            System.Console.ResetColor();
            PrintLine(images[i]);
        }
        PrintLine();
        PrintDivider();

        while (true)
        {
            PrintPrompt("Select number");
            string? input = System.Console.ReadLine()?.Trim();
            PrintLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                PrintError("No selection made. Please enter a number.");
                PrintLine();
                continue;
            }

            if (!int.TryParse(input, out int selection))
            {
                PrintError($"\"{input}\" is not a valid number. Please enter 0-{images.Length}.");
                PrintLine();
                continue;
            }

            if (selection == 0)
            {
                PrintSuccess($"Using default sample: {Path.GetFileName(defaultImagePath)}");
                return defaultImagePath;
            }

            if (selection < 1 || selection > images.Length)
            {
                PrintError($"Number out of range. Please enter 0-{images.Length}.");
                PrintLine();
                continue;
            }

            string chosen = images[selection - 1];
            string fullPath = pathProvider.GetProjectFilePath(chosen);

            if (!File.Exists(fullPath))
            {
                PrintError($"File not found: \"{chosen}\"");
                PrintLine();
                continue;
            }

            PrintSuccess($"Selected: {chosen}");
            return fullPath;
        }
    }

    private string PromptCustomPath()
    {
        while (true)
        {
            PrintHeader("Type image path manually");
            PrintLine();
            PrintLine("  Enter the full path to an image file.");
            PrintLine("  Examples:");
            PrintLine("    D:\\Photos\\soap_batch_01.jpg");
            PrintLine("    D:/Photos/soap_batch_01.jpg");
            PrintLine("    C:\\Users\\John\\Desktop\\image.png");
            PrintLine();
            PrintLine("  Or enter [B] to go back to image source selection.");
            PrintLine();
            PrintPrompt("Image path");
            string? input = System.Console.ReadLine();
            PrintLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                PrintWarning("No path entered. Please enter a path or [B] to go back.");
                PrintLine();
                continue;
            }

            string trimmed = input.Trim();

            if (trimmed.Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                PrintInfo("Going back to image source selection.");
                PrintLine();
                return PromptForImagePath(
                    "sample.jpg",
                    null!);
            }

            string resolved = Path.IsPathRooted(trimmed)
                ? Path.GetFullPath(trimmed)
                : Path.GetFullPath(Path.Combine(PathProviderGetRoot(), trimmed));

            if (!File.Exists(resolved))
            {
                PrintError($"File not found: \"{trimmed}\"");
                PrintInfo("Please check the path and try again, or enter [B] to go back.");
                PrintLine();
                continue;
            }

            PrintSuccess($"Using: {Path.GetFileName(trimmed)}");
            return resolved;
        }
    }

    private static string PathProviderGetRoot()
    {
        string baseDir = AppContext.BaseDirectory;
        string devRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        return File.Exists(Path.Combine(devRoot, "DeepLearning.csproj"))
            ? devRoot
            : baseDir;
    }

    private static void PrintBanner()
    {
        System.Console.ForegroundColor = ConsoleColor.DarkCyan;
        foreach (string line in BannerLines)
        {
            System.Console.WriteLine(line);
        }
        System.Console.ResetColor();
    }

    private static void PrintDivider()
    {
        int width = 68;
        try { width = Math.Min(System.Console.WindowWidth, 68); } catch { }
        System.Console.WriteLine(new string(DividerChar, width));
    }

    private static void PrintLine(string? text = null)
    {
        if (text is not null)
            System.Console.WriteLine(text);
        else
            System.Console.WriteLine();
    }

    private static void PrintHeader(string text)
    {
        System.Console.ForegroundColor = ConsoleColor.White;
        System.Console.WriteLine($"  {text}");
        System.Console.ResetColor();
    }

    private static void PrintInfo(string message)
        => System.Console.WriteLine($"  {message}");

    private static void PrintSuccess(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"  {message}");
        System.Console.ResetColor();
    }

    private static void PrintError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"  ERROR: {message}");
        System.Console.ResetColor();
    }

    private static void PrintWarning(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"  {message}");
        System.Console.ResetColor();
    }

    private static void PrintCyan(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkCyan;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }

    private static void PrintGreen(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }

    private static void PrintPrompt(string label)
    {
        System.Console.Write($"  {label}: ");
    }

    private static void PrintDetectionItem(DetectionResult detection)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.Write($"    {BuildConfidenceBar(detection.Confidence)}");
        System.Console.ResetColor();
        System.Console.WriteLine($"  at ({detection.X1:F0}, {detection.Y1:F0}, {detection.X2:F0}, {detection.Y2:F0})");
    }

    private static string BuildConfidenceBar(float confidence)
    {
        const int barWidth = 10;
        int filled = (int)Math.Round(confidence * barWidth);
        string bar = new string('#', filled) + new string('-', barWidth - filled);
        return $"[{bar}] {confidence:P0}";
    }
}
