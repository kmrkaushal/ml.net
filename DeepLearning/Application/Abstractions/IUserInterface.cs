using DeepLearning.Application.Configuration;
using DeepLearning.Application.Models;
using DeepLearning.Domain.Entities;
using DeepLearning.Domain.Enums;

namespace DeepLearning.Application.Abstractions;

/// <summary>
/// Contract for all user-facing input and output operations.
/// Implementations handle the specific UI technology (console, GUI, web, etc.)
/// while the application layer remains UI-agnostic.
/// </summary>
public interface IUserInterface
{
    /// <summary>
    /// Displays the welcome banner and model information.
    /// </summary>
    void ShowWelcome(DetectionOptions options);

    /// <summary>
    /// Prompts the user to choose between webcam and existing image mode.
    /// Blocks until a valid choice is made.
    /// </summary>
    InputSource PromptForInputSource();

    /// <summary>
    /// Prompts the user for an image path, optionally offering a file browser.
    /// </summary>
    /// <param name="defaultImagePath">The path shown as the default when the user presses Enter.</param>
    /// <param name="pathProvider">Used to enumerate available images for browsing.</param>
    string PromptForImagePath(string defaultImagePath, IProjectPathProvider pathProvider);

    /// <summary>
    /// Displays the final report for an image detection run.
    /// </summary>
    void ShowImageDetectionReport(ImageDetectionReport report, string[] classLabels);

    /// <summary>
    /// Displays instructions before starting webcam mode.
    /// </summary>
    void ShowWebcamInstructions(DetectionOptions options);

    /// <summary>
    /// Prints a list of detection results in a readable format.
    /// </summary>
    void ShowDetections(IReadOnlyList<DetectionResult> detections, string[] classLabels);

    /// <summary>
    /// Prints an informational message.
    /// </summary>
    void ShowInfo(string message);

    /// <summary>
    /// Prints an error message in a clear, user-friendly way.
    /// </summary>
    void ShowError(string message);

    /// <summary>
    /// Asks the user whether to return to the main menu or exit the app.
    /// Returns true to continue (back to menu), false to exit.
    /// </summary>
    bool PromptForContinue();

    /// <summary>
    /// Shows a goodbye message when the user chooses to exit.
    /// </summary>
    void ShowExitMessage();
}
