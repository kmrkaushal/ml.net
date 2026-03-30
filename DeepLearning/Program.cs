using DeepLearning.Application.Abstractions;
using DeepLearning.Application.Configuration;
using DeepLearning.Application.UseCases;
using DeepLearning.Infrastructure.Capture;
using DeepLearning.Infrastructure.Detection;
using DeepLearning.Infrastructure.Pathing;
using DeepLearning.Infrastructure.Rendering;
using DeepLearning.Presentation.Console;

var options = new DetectionOptions();

using IObjectDetector detector = new OnnxObjectDetector(options);
IProjectPathProvider pathProvider = new ProjectPathProvider();
IImageRenderer imageRenderer = new DetectionOverlayRenderer(options);
IUserInterface userInterface = new ConsoleUserInterface();
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
