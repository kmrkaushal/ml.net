// =============================================================================
// ConsoleUserInterface — Full Console UI Implementation (514 lines)
// =============================================================================
//
// FILE:         ConsoleUserInterface.cs
// LAYER:        Presentation (UI)
// DEPENDENCIES: System.Windows.Forms (OpenFileDialog), Application (interfaces,
//               DetectionOptions, ImageDetectionReport), Domain (DetectionResult,
//               InputSource)
// DEPENDENTS:   Program.cs, RunDetectionApplication, WebcamDetectionLoop
//
// PURPOSE:
//   Complete console-based user interface for the application. Handles all user
//   interaction: ASCII art banner, colored output, numbered menus, file dialogs,
//   confidence bar visualization, detection reports, and graceful exit.
//
// FEATURE BREAKDOWN:
//
//   VISUAL ELEMENTS:
//   - ASCII art banner (lines 22-34): "DEEPLEARNING" in block letters
//   - Box-drawing characters (lines 15-20): Unicode chars for clean borders
//   - Color coding: Cyan (prompts), Yellow (options), Green (success),
//                   Red (errors), DarkGray (info), White (headers)
//   - Confidence bars: [████████░░] 64% — visual representation of confidence
//
//   MENU SYSTEM:
//   - Input source selection: Webcam (1) or Image File (2)
//   - Image path options: Browse (1), App folder (2), Type path (3), Default (4)
//   - Continue/Exit: Run again (1) or Quit (2)
//   - All menus have input validation with retry loops
//
//   FILE DIALOGS:
//   - BrowseForOnnxFile(): WinForms OpenFileDialog for .onnx files
//   - BrowseForImageFile(): WinForms OpenFileDialog for image files
//   - BrowseForImage(): Lists available images in app folder with numbered menu
//
//   DETECTION DISPLAY:
//   - Groups detections by class name
//   - Shows count per class and top confidence
//   - Displays top 3 detections per class with confidence bars
//   - Shows "... and N more" for additional detections
//
//   PRINTING UTILITIES (private methods):
//   - PrintBanner()       — ASCII art in cyan
//   - PrintBoxedContent() — Draws a box around content lines
//   - PrintHeader()       — Section header with decorative line
//   - PrintOption()       — Numbered menu item with color coding
//   - PrintLine()         — Blank line or custom text
//   - PrintPrompt()       — Colored input prompt
//   - PrintInfo/Success/Error/Warning() — Colored message output
//   - PrintDetectionItem() — Confidence bar + coordinates
//   - BuildConfidenceBar() — Creates visual bar from float (0.0-1.0)
//
// DESIGN NOTES:
//   - This is the largest file (514 lines) because it handles ALL user interaction
//   - Every output method follows the same pattern: set color → write → reset color
//   - Console.ForegroundColor/ResetColor() pattern is critical — forgetting to
//     reset causes all subsequent output to inherit the color
//   - WinForms OpenFileDialog requires [STAThread] on Main() (set in Program.cs)
//   - The confidence bar uses Unicode block characters: █ (filled) and ░ (empty)
//   - Input validation: all menus use while(true) loops that only break on valid input
//
// =============================================================================

using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Application.Models;
using DeepLearning.Domain.Entities;
using DeepLearning.Domain.Enums;

namespace DeepLearning.Presentation.UI;

public sealed class ConsoleUserInterface : IUserInterface
{
    private const char BoxHoriz = '─';
    private const char BoxVert = '│';
    private const char BoxTopLeft = '┌';
    private const char BoxTopRight = '┐';
    private const char BoxBotLeft = '└';
    private const char BoxBotRight = '┘';

    private static readonly string[] BannerLines =
    [
        "  ██████╗ ██╗██████╗ ███████╗ ██████╗     ███████╗██╗ ██████╗ ███╗   ██╗ █████╗ ██╗     ",
        "  ██╔══██╗██║██╔══██╗██╔════╝██╔═══██╗    ██╔════╝██║██╔════╝ ████╗  ██║██╔══██╗██║     ",
        "  ██████╔╝██║██████╔╝█████╗  ██║   ██║    ███████╗██║██║  ███╗██╔██╗ ██║███████║██║     ",
        "  ██╔═══╝ ██║██╔══██╗██╔══╝  ██║   ██║    ╚════██║██║██║   ██║██║╚██╗██║██╔══██║██║     ",
        "  ██║     ██║██║  ██║███████╗╚██████╔╝    ███████║██║╚██████╔╝██║ ╚████║██║  ██║███████╗",
        "  ╚═╝     ╚═╝╚═╝  ╚═╝╚══════╝ ╚═════╝     ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝",
        "",
        "         ╔══════════════════════════════════════════════════════════════╗",
        "         ║        YOLO Object Detection Console  │  ONNX Runtime        ║",
        "         ╚══════════════════════════════════════════════════════════════╝"
    ];

    public void ShowWelcome(DetectionOptions options)
    {
        Console.Clear();
        PrintBanner();
        PrintBoxedContent([
            "✓ Application started successfully",
            $"  Model     : {options.ModelPath}",
            $"  Classes   : {string.Join(", ", options.ClassLabels)}",
            $"  Threshold : {options.ConfidenceThreshold:P0}",
            $"  IoU       : {options.IouThreshold:P0}"
        ]);
    }

    public InputSource PromptForInputSource()
    {
        PrintHeader("SELECT INPUT SOURCE");
        PrintLine();
        PrintOption(1, "Webcam", "Use your camera for real-time detection");
        PrintOption(2, "Image File", "Detect objects in a static image");
        PrintLine();

        while (true)
        {
            PrintPrompt("Choice");
            string? input = Console.ReadLine()?.Trim();
            PrintLine();

            if (input == "1")
            {
                PrintSuccess("▶ Webcam mode selected");
                return InputSource.Webcam;
            }

            if (input == "2")
            {
                PrintSuccess("▶ Image File mode selected");
                return InputSource.ExistingImage;
            }

            PrintError("✗ Invalid choice. Please enter 1 or 2.");
            PrintLine();
        }
    }

    public string PromptForImagePath(string defaultImagePath, IProjectPathProvider pathProvider)
    {
        PrintHeader("IMAGE FILE MODE");
        PrintLine();
        PrintOption(1, "Browse my device", "Open file dialog to select an image");
        PrintOption(2, "Browse app folder", "Select from images in the app folder");
        PrintOption(3, "Type path manually", "Enter a full path to an image file");
        PrintOption(4, "Use default", $"Press Enter to use: {defaultImagePath}");
        PrintLine();

        while (true)
        {
            PrintPrompt("Option");
            string? input = Console.ReadLine()?.Trim();
            PrintLine();

            if (input == "1")
            {
                string? browsed = BrowseForImageFile();
                if (!string.IsNullOrEmpty(browsed))
                {
                    PrintSuccess($"✓ Selected: {Path.GetFileName(browsed)}");
                    return browsed;
                }
                PrintInfo("No file selected. Try another option.");
                PrintLine();
                continue;
            }

            if (input == "2")
            {
                return BrowseForImage(pathProvider, defaultImagePath);
            }

            if (input == "3")
            {
                return PromptCustomPath();
            }

            if (string.IsNullOrWhiteSpace(input) || input == "4")
            {
                PrintSuccess($"✓ Using default: {defaultImagePath}");
                return defaultImagePath;
            }

            PrintError("✗ Invalid option. Enter 1, 2, 3, or 4.");
            PrintLine();
        }
    }

    public void ShowImageDetectionReport(ImageDetectionReport report, string[] classLabels)
    {
        PrintHeader("DETECTION COMPLETE");
        PrintBoxedContent([
            $"  Input  : {report.InputPath}",
            $"  Output : {report.OutputPath}"
        ]);
        ShowDetections(report.Detections, classLabels);
    }

    public void ShowWebcamInstructions(DetectionOptions options)
    {
        PrintHeader("WEBCAM MODE");
        PrintBoxedContent([
            $"Opening webcam (index {options.CameraIndex})...",
            $"Press ESC in the window to stop"
        ]);
    }

    public void ShowDetections(IReadOnlyList<DetectionResult> detections, string[] classLabels)
    {
        if (detections.Count == 0)
        {
            PrintWarning("⚠ No objects detected.");
            PrintInfo("  Lower the confidence threshold or try a different image.");
            PrintLine();
            return;
        }

        var grouped = detections
            .GroupBy(d => classLabels.ElementAtOrDefault(d.ClassId) ?? $"unknown({d.ClassId})")
            .OrderByDescending(g => g.Max(d => d.Confidence))
            .ToList();

        PrintSuccess($"✓ Total: {detections.Count} detection(s)");
        PrintLine();

        foreach (var group in grouped)
        {
            string className = group.Key;
            int count = group.Count();
            float topConf = group.Max(d => d.Confidence);

            PrintLine($"  [{className}] ×{count}  (top: {topConf:P0})");

            foreach (DetectionResult detection in group.OrderByDescending(d => d.Confidence).Take(3))
            {
                PrintDetectionItem(detection);
            }

            if (group.Count() > 3)
            {
                PrintInfo($"  ... and {group.Count() - 3} more");
            }
            PrintLine();
        }
    }

    public void ShowInfo(string message) => PrintInfo(message);

    public void ShowError(string message)
    {
        PrintHeader("ERROR");
        PrintBoxedContent([$"✗ {message}"]);
    }

    public void ShowSuccess(string message) => PrintSuccess(message);

    public void ShowPrompt(string label) => PrintPrompt(label);

    public bool PromptForContinue()
    {
        PrintHeader("WHAT'S NEXT?");
        PrintLine();
        PrintOption(1, "Back to main menu", "Run another detection");
        PrintOption(2, "Exit", "Close the application");
        PrintLine();

        while (true)
        {
            PrintPrompt("Choice");
            string? input = Console.ReadLine()?.Trim();
            PrintLine();

            if (input == "1") return true;
            if (input == "2") return false;

            PrintError("✗ Invalid. Enter 1 or 2.");
            PrintLine();
        }
    }

    public void ShowExitMessage()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("  ╔══════════════════════════════════════════╗");
        Console.WriteLine("  ║  Thanks for using Object Detector!      ║");
        Console.WriteLine("  ║  Goodbye! 👋                              ║");
        Console.WriteLine("  ╚══════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    public string? BrowseForOnnxFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select ONNX Model File",
            Filter = "ONNX Models (*.onnx)|*.onnx|All Files (*.*)|*.*",
            FilterIndex = 1,
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return dialog.FileName;
        }
        return null;
    }

    public string[] PromptForClassLabels(int expectedCount)
    {
        PrintHeader("CONFIGURE CLASS LABELS");
        PrintLine();
        PrintInfo($"The model has {expectedCount} class(es).");
        PrintInfo("Enter class names separated by commas:");
        PrintLine();
        PrintInfo("  Example: bottle, bottles, capped-bottle");
        PrintLine();

        while (true)
        {
            PrintPrompt("Classes");
            string? input = Console.ReadLine()?.Trim();
            PrintLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                PrintError("✗ No input. Please enter class names.");
                PrintLine();
                continue;
            }

            string[] labels = input.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            if (labels.Length == 0)
            {
                PrintError("✗ No valid class names entered.");
                PrintLine();
                continue;
            }

            if (labels.Length != expectedCount)
            {
                PrintError($"✗ Expected exactly {expectedCount} class name(s), but got {labels.Length}.");
                PrintInfo("Tip: if your model detects more objects than you think, add the missing class labels.");
                PrintLine();
                continue;
            }

            string mapping = string.Join(", ", labels.Select((label, index) => $"[{index}] {label}"));
            PrintSuccess($"✓ {labels.Length} class(es) configured: {mapping}");
            return labels;
        }
    }

    public string? BrowseForImageFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Image File",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files (*.*)|*.*",
            FilterIndex = 1,
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return dialog.FileName;
        }
        return null;
    }

    private string BrowseForImage(IProjectPathProvider pathProvider, string defaultImagePath)
    {
        string[] images = pathProvider.GetImageFiles();

        if (images.Length == 0)
        {
            PrintWarning("No images in app folder.");
            return PromptCustomPath();
        }

        PrintHeader("AVAILABLE IMAGES");
        PrintLine();
        PrintOption(0, "Use default", defaultImagePath);
        for (int i = 0; i < images.Length; i++)
        {
            PrintOption(i + 1, images[i], "");
        }
        PrintLine();

        while (true)
        {
            PrintPrompt("Select");
            string? input = Console.ReadLine()?.Trim();
            PrintLine();

            if (!int.TryParse(input, out int selection))
            {
                PrintError("✗ Enter a number.");
                PrintLine();
                continue;
            }

            if (selection == 0)
            {
                PrintSuccess($"✓ Default: {defaultImagePath}");
                return defaultImagePath;
            }

            if (selection < 1 || selection > images.Length)
            {
                PrintError($"✗ Out of range. Enter 0-{images.Length}.");
                PrintLine();
                continue;
            }

            string chosen = images[selection - 1];
            string fullPath = pathProvider.GetProjectFilePath(chosen);

            if (!File.Exists(fullPath))
            {
                PrintError($"✗ File not found: {chosen}");
                PrintLine();
                continue;
            }

            PrintSuccess($"✓ Selected: {chosen}");
            return fullPath;
        }
    }

    private string PromptCustomPath()
    {
        while (true)
        {
            PrintHeader("TYPE IMAGE PATH");
            PrintLine();
            PrintInfo("Enter full path or [B] to go back");
            PrintLine();
            PrintPrompt("Path");
            string? input = Console.ReadLine();
            PrintLine();

            if (string.IsNullOrWhiteSpace(input)) continue;

            string trimmed = input.Trim();

            if (trimmed.Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            if (!File.Exists(trimmed))
            {
                PrintError($"✗ File not found: {trimmed}");
                PrintLine();
                continue;
            }

            PrintSuccess($"✓ Using: {Path.GetFileName(trimmed)}");
            return trimmed;
        }
    }

    private void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        foreach (string line in BannerLines)
        {
            Console.WriteLine(line);
        }
        Console.ResetColor();
    }

    private void PrintBoxedContent(string[] lines)
    {
        int maxLen = lines.Max(l => l.Length) + 4;
        string top = BoxTopLeft + new string(BoxHoriz, maxLen) + BoxTopRight;
        string bot = BoxBotLeft + new string(BoxHoriz, maxLen) + BoxBotRight;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(top);
        foreach (string line in lines)
        {
            Console.WriteLine(BoxVert + " " + line.PadRight(maxLen - 1) + BoxVert);
        }
        Console.WriteLine(bot);
        Console.ResetColor();
    }

    private void PrintHeader(string text)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine($"  ╭─ {text} ─");
        Console.ResetColor();
    }

    private void PrintOption(int num, string title, string desc)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"  [{num}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(title);
        if (!string.IsNullOrEmpty(desc))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" - {desc}");
        }
        Console.WriteLine();
        Console.ResetColor();
    }

    private void PrintLine(string? text = null)
    {
        Console.WriteLine(text ?? string.Empty);
    }

    private void PrintPrompt(string label)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  {label}: ");
        Console.ResetColor();
    }

    private void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    private void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    private void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    private void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    private void PrintDetectionItem(DetectionResult detection)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"    {BuildConfidenceBar(detection.Confidence)}");
        Console.ResetColor();
        Console.WriteLine($"  @ ({detection.X1:F0}, {detection.Y1:F0})");
    }

    private string BuildConfidenceBar(float confidence)
    {
        const int barWidth = 8;
        int filled = (int)Math.Round(confidence * barWidth);
        string bar = new string('█', filled) + new string('░', barWidth - filled);
        return $"[{bar}] {confidence:P0}";
    }
}
