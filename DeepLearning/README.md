## Soap Detector (DeepLearning project)

**Soap Detector** is a .NET 8 console application that uses a YOLO ONNX model to detect objects (e.g., `soap`, `soap-cover`, or your own classes) in webcam video or static images.

- **Platform**: Windows (`net8.0-windows`, uses WinForms dialogs + OpenCV)
- **Architecture**: Clean Architecture (Domain / Application / Infrastructure / Presentation)
- **Model**: YOLO ONNX (`soap_v7.onnx` by default, or a custom `.onnx` you select at startup)

---

## Project structure

```text
DeepLearning/
├─ Application/        # Use cases, configuration, and interfaces
├─ Domain/             # Core entities and enums (no external deps)
├─ Infrastructure/     # ONNX Runtime, OpenCV capture, rendering, pathing
├─ Presentation/       # Console UI
├─ project-structure/  # Detailed docs and diagrams
├─ Program.cs          # Composition root
├─ DeepLearning.csproj # Project file (net8.0-windows exe)
└─ publish/            # Optional published build (self-contained exe)
```

### Layer responsibilities (high level)

- **Domain**: `DetectionResult`, `InputSource` – pure business concepts.
- **Application**:
  - Use cases: `RunDetectionApplication`, `DetectImageFromFileUseCase`.
  - Configuration: `DetectionOptions`.
  - Abstractions: `IObjectDetector`, `IImageRenderer`, `IUserInterface`, `IWebcamDetectionLoop`, `IProjectPathProvider`.
- **Infrastructure**:
  - Detection: `OnnxObjectDetector`, `ImagePreprocessor`, `NmsProcessor`.
  - Capture: `WebcamDetectionLoop`.
  - Rendering: `DetectionOverlayRenderer`.
  - Pathing: `ProjectPathProvider`.
- **Presentation**:
  - Console UI: `ConsoleUserInterface` (banner, prompts, detection summaries).

---

## How the app runs (runtime flow)

1. **Startup (`Program.Main`)**
   - Creates `DetectionOptions` and `ConsoleUserInterface`.
   - Asks if you want to browse for a **custom ONNX model** or use the default model path.
   - If you choose a custom model:
     - Opens a file dialog to pick a `.onnx`.
     - **Infers the model’s class count** from the ONNX output tensor.
     - Prompts you for exactly that many class labels and shows their index mapping.
   - Wires up:
     - `OnnxObjectDetector` (ONNX Runtime wrapper)
     - `DetectionOverlayRenderer` (GDI+ drawing)
     - `WebcamDetectionLoop` (OpenCV capture)
     - `DetectImageFromFileUseCase`
     - `RunDetectionApplication`

2. **Main loop (`RunDetectionApplication.Execute`)**
   - Prints a welcome banner showing active model, classes, and thresholds.
   - Asks for **input source**:
     - `[1] Webcam` – live detection.
     - `[2] Image File` – detect objects in a single image.
   - For **Webcam**:
     - Prints instructions.
     - Calls `WebcamDetectionLoop.Run`:
       - Captures frames from `CameraIndex`.
       - Runs `OnnxObjectDetector.Detect` on each frame.
       - Draws boxes/labels with `DetectionOverlayRenderer`.
       - Displays the result in an OpenCV window (`WindowTitle`).
       - Stops when you press `ESC` inside the window.
   - For **Image File**:
     - Uses `ConsoleUserInterface.PromptForImagePath` to let you:
       - Browse from disk or project folder, type a path, or use the default image.
     - Runs `DetectImageFromFileUseCase`:
       - Loads the image.
       - Detects objects.
       - Draws boxes/labels.
       - Saves `output.jpg` (configurable) and optionally opens it.
       - Returns an `ImageDetectionReport` which the UI prints.
   - Asks whether to run another detection or exit cleanly.

---

## Detection pipeline (ONNX + YOLO)

At the core, `OnnxObjectDetector` wraps ONNX Runtime and converts model outputs into friendly `DetectionResult` objects:

1. **Preprocessing**
   - `ImagePreprocessor.ToChwArray` resizes/normalizes the input `Bitmap` to the model’s `ModelWidth` × `ModelHeight` and converts it to a CHW (`[1, 3, H, W]`) float array.

2. **Inference**
   - Builds a `DenseTensor<float>` and a `NamedOnnxValue` with the correct input name.
   - Runs the ONNX `InferenceSession`.

3. **Parsing**
   - Finds the YOLO detection head tensor. Supports both `[1, C, N]` and `[1, N, C]` layouts.
   - Treats the first 4 channels as bounding box data; the rest are class scores.
   - For each candidate box:
     - Picks the best class index (`ClassId`) and score (`Confidence`).
     - Applies `ConfidenceThreshold`.
     - Scales the box back from model space into original image coordinates.

4. **Non-Maximum Suppression**
   - `NmsProcessor.Apply` groups detections by class, sorts by confidence, and removes overlapping boxes based on `IouThreshold`.

5. **Rendering & Reporting**
   - `DetectionOverlayRenderer` draws rectangles and labels (`ClassLabels[ClassId]` when in range) on a copy of the image.
   - `ConsoleUserInterface.ShowDetections` prints grouped detections, including confidence bars and coordinates.

> If a `ClassId` falls outside the `ClassLabels` array, the overlay would normally fall back to `class {id}`. The custom-model setup flow avoids this by inferring the class count and requiring exactly that many labels.

---

## Configuration (`DetectionOptions`)

All runtime settings live in `Application/Configuration/DetectionOptions.cs`:

| Property | Purpose |
|---|---|
| `ModelPath` | Path to the ONNX model. Defaults to `soap_v7.onnx`, but is overridden when you choose a custom model at startup. |
| `ClassLabels` | Ordered list of class names. Index must match the model’s training index. |
| `ModelWidth`, `ModelHeight` | Expected input size for the model. Must match the ONNX model configuration. |
| `ConfidenceThreshold` | Minimum confidence required for a detection to be kept. |
| `IouThreshold` | IoU threshold used when removing overlapping boxes in NMS. |
| `CameraIndex` | Which webcam to open (0 = default camera). |
| `WindowTitle` | Title for the OpenCV webcam window. |
| `DefaultImagePath` | Path of the image used when you choose image mode and press Enter. |
| `OutputFileName` | Name of the annotated image saved after detection (e.g., `output.jpg`). |
| `AutoOpenOutput` | Whether to automatically open the annotated image after saving. |

---

## How to run

### From source (development)

```bash
dotnet build
dotnet run --project DeepLearning/DeepLearning.csproj
```

You can also use the solution file (`ML.Net.sln`) and start `DeepLearning` from Visual Studio.

### From published executable

After publishing (e.g., via `dotnet publish` or the existing `publish/` folder):

1. Copy the entire `DeepLearning/publish/` directory to a Windows x64 machine.
2. Ensure the ONNX model file (e.g., `soap_v7.onnx`) and `sample.jpg` are present alongside `SoapDetector.exe`.
3. Double-click `SoapDetector.exe` to launch.

For more detailed operator instructions, see `project-structure/PROFESSIONAL-GUIDE.md`.

---

## Branches and models

This repository uses separate branches for different trained models:

- `feature/clean-architecture-yolo-detector` – SOAP model (`soap_v7.onnx`) with `soap`, `soap-cover`.
- `feature/bottle-detector` – Bottle model (`best_bottle.onnx`) with `bottle`, `bottles`, `capped-bottle`.

When switching branches:

- Use the matching ONNX model file for that branch.
- Ensure `DetectionOptions.ModelPath` and the entered `ClassLabels` match the model you are using.

> Model files (`*.onnx`) are not committed to git; keep them locally or in separate storage.

---

## Extending the project

Here are some safe, common extensions:

- **New input mode** (e.g., folder of images):  
  Add a new use case in `Application/UseCases`, expose it via `RunDetectionApplication`, and add options in `ConsoleUserInterface`.

- **New UI** (WinForms, WPF, Web API):  
  Implement `IUserInterface` (and potentially a new renderer) and change `Program.Main` to create your implementation instead of `ConsoleUserInterface`.

- **Alternative detection backend** (TensorRT, OpenVINO, etc.):  
  Implement `IObjectDetector` and wire it in from `Program.Main` without modifying use-case logic.

- **Logging / telemetry**:  
  Introduce an `ILogger` abstraction and inject it into use cases to report detections, errors, and performance metrics.

For a deeper, junior-friendly walkthrough of every layer and class, see:

- `project-structure/JUNIOR-DEVELOPER-CODE-WALKTHROUGH.md`

That document explains the architecture, runtime flow, and extension patterns in much more detail.

