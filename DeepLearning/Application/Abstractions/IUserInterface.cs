// =============================================================================
// IUserInterface — User Interaction Contract
// =============================================================================
//
// FILE:         IUserInterface.cs
// LAYER:        Application (Abstractions)
// DEPENDENCIES: Application (DetectionOptions, ImageDetectionReport),
//               Domain (DetectionResult, InputSource)
// DEPENDENTS:   ConsoleUserInterface (Presentation), RunDetectionApplication,
//               WebcamDetectionLoop, Program
//
// PURPOSE:
//   Defines the complete contract for all user interactions in the application.
//   15 methods covering: display output, collect input, file browsing, reports.
//
// METHOD GROUPS:
//   Display:    ShowWelcome, ShowInfo, ShowError, ShowSuccess, ShowPrompt,
//               ShowDetections, ShowImageDetectionReport, ShowWebcamInstructions,
//               ShowExitMessage
//   Input:      PromptForInputSource, PromptForImagePath, PromptForContinue,
//               PromptForClassLabels
//   Browsing:   BrowseForOnnxFile, BrowseForImageFile
//
// WHY THIS EXISTS:
//   Decouples the application from any specific UI technology. The current
//   implementation is a console UI, but you could create:
//   - WpfUserInterface (WPF windows)
//   - WebUserInterface (ASP.NET Core web UI)
//   - SilentUserInterface (headless/CI mode — no output)
//
// =============================================================================

using DeepLearning.Application.Configuration;
using DeepLearning.Application.Models;
using DeepLearning.Domain.Entities;
using DeepLearning.Domain.Enums;

namespace DeepLearning.Application.Abstractions;

public interface IUserInterface
{
    void ShowWelcome(DetectionOptions options);
    InputSource PromptForInputSource();
    string PromptForImagePath(string defaultImagePath, IProjectPathProvider pathProvider);
    void ShowImageDetectionReport(ImageDetectionReport report, string[] classLabels);
    void ShowWebcamInstructions(DetectionOptions options);
    void ShowDetections(IReadOnlyList<DetectionResult> detections, string[] classLabels);
    void ShowInfo(string message);
    void ShowError(string message);
    void ShowSuccess(string message);
    void ShowPrompt(string label);
    bool PromptForContinue();
    void ShowExitMessage();

    string? BrowseForOnnxFile();
    string[] PromptForClassLabels(int expectedCount);
    string? BrowseForImageFile();
}
