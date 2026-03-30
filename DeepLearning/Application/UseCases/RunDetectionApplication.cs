using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Domain.Enums;

namespace DeepLearning.Application.UseCases;

/// <summary>
/// Orchestrates the top-level detection application workflow.
/// Presents the user with a choice between webcam and existing-image modes,
/// then delegates to the appropriate use case while handling errors uniformly.
///
/// <para>
/// This class runs in a loop: after each detection run (webcam or image),
/// the user is asked whether to return to the menu or exit the app.
/// </para>
///
/// <para>
/// This class is the single entry point for running the application.
/// It is stateless and depends only on abstractions, making it easy to test.
/// </para>
/// </summary>
public sealed class RunDetectionApplication
{
    private readonly DetectionOptions _options;
    private readonly IUserInterface _userInterface;
    private readonly IWebcamDetectionLoop _webcamDetectionLoop;
    private readonly DetectImageFromFileUseCase _detectImageFromFileUseCase;
    private readonly IProjectPathProvider _pathProvider;

    /// <summary>
    /// Creates a new application runner with all its dependencies.
    /// </summary>
    /// <param name="options">Detection configuration such as thresholds and paths.</param>
    /// <param name="userInterface">The UI implementation for console interaction.</param>
    /// <param name="webcamDetectionLoop">The webcam capture and display loop.</param>
    /// <param name="detectImageFromFileUseCase">The use case for processing a single image file.</param>
    /// <param name="pathProvider">Resolves relative and absolute file paths.</param>
    public RunDetectionApplication(
        DetectionOptions options,
        IUserInterface userInterface,
        IWebcamDetectionLoop webcamDetectionLoop,
        DetectImageFromFileUseCase detectImageFromFileUseCase,
        IProjectPathProvider pathProvider)
    {
        _options = options;
        _userInterface = userInterface;
        _webcamDetectionLoop = webcamDetectionLoop;
        _detectImageFromFileUseCase = detectImageFromFileUseCase;
        _pathProvider = pathProvider;
    }

    /// <summary>
    /// Starts the application. Displays the welcome screen, then enters a loop where
    /// the user picks a mode, runs detection, and is asked whether to continue or exit.
    /// </summary>
    public void Execute()
    {
        _userInterface.ShowWelcome(_options);

        while (true)
        {
            try
            {
                InputSource inputSource = _userInterface.PromptForInputSource();

                if (inputSource == InputSource.Webcam)
                {
                    _userInterface.ShowWebcamInstructions(_options);
                    _webcamDetectionLoop.Run();
                }
                else
                {
                    string defaultImagePath = _pathProvider.GetProjectFilePath(_options.DefaultImagePath);
                    string selectedImagePath = _userInterface.PromptForImagePath(defaultImagePath, _pathProvider);

                    var report = _detectImageFromFileUseCase.Execute(selectedImagePath);
                    _userInterface.ShowImageDetectionReport(report, _options.ClassLabels);
                }
            }
            catch (FileNotFoundException ex)
            {
                _userInterface.ShowError($"Image file not found: \"{ex.FileName}\"");
            }
            catch (UnauthorizedAccessException ex)
            {
                _userInterface.ShowError($"Access denied: {ex.Message}");
            }
            catch (Exception ex)
            {
                _userInterface.ShowError($"An unexpected error occurred: {ex.Message}");
            }

            bool shouldContinue = _userInterface.PromptForContinue();

            if (!shouldContinue)
            {
                _userInterface.ShowExitMessage();
                return;
            }
        }
    }
}
