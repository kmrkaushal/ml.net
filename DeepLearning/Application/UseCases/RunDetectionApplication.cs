// =============================================================================
// RunDetectionApplication — Main Application Orchestrator
// =============================================================================
//
// FILE:         RunDetectionApplication.cs
// LAYER:        Application (Use Cases)
// DEPENDENCIES: Application (IUserInterface, IWebcamDetectionLoop,
//               IProjectPathProvider, DetectionOptions, DetectImageFromFileUseCase)
//               Domain (InputSource)
// DEPENDENTS:   Program.cs (composition root)
//
// PURPOSE:
//   This is the "conductor" of the application. It orchestrates the main
//   interaction loop: show welcome → ask user for input source → dispatch
//   to the appropriate handler → show results → ask to continue or exit.
//
// FLOW:
//   1. ShowWelcome() — display banner with current configuration
//   2. PromptForInputSource() — webcam (1) or image file (2)?
//   3a. Webcam: ShowWebcamInstructions() → WebcamDetectionLoop.Run() [blocks]
//   3b. Image:  PromptForImagePath() → DetectImageFromFileUseCase.Execute()
//               → ShowImageDetectionReport()
//   4. Catch and display errors (FileNotFoundException, etc.)
//   5. PromptForContinue() — run again or exit?
//
// DESIGN NOTES:
//   - This class knows NOTHING about how detection works, how rendering works,
//     or how the UI looks. It only knows about interfaces.
//   - The while(true) loop runs until the user chooses to exit (PromptForContinue returns false)
//   - Error handling is centralized here — all exceptions from use cases are caught
//     and displayed via the UI interface
//   - No return value — all output goes through IUserInterface
//
// =============================================================================

using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Domain.Enums;

namespace DeepLearning.Application.UseCases;

public sealed class RunDetectionApplication
{
    private readonly DetectionOptions _options;
    private readonly IUserInterface _userInterface;
    private readonly IWebcamDetectionLoop _webcamDetectionLoop;
    private readonly DetectImageFromFileUseCase _detectImageFromFileUseCase;
    private readonly IProjectPathProvider _pathProvider;

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
