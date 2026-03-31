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
