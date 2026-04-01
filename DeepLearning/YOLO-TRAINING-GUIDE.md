# YOLO Training Guide — Complete Professional Workflow
## Model: detector_v2.onnx — Trained April 2026, RTX 3070 Laptop GPU

---

## Training Results Summary

```
Dataset:        52 labeled images → 312 after augmentation (6x)
Classes:        bottle (class 0), soap (class 1), soap-cover (class 2)
Model:          YOLOv11n (2.6M parameters)
Training:       115 epochs (best at epoch 65), ~16 minutes on RTX 3070
Output:         detector_v2.onnx (10.1 MB)

Final Metrics (Best Epoch 65):
  Precision:    80.8%  — 81% of detections are correct
  Recall:       63.9%  — 64% of actual objects were found
  mAP50:        60.1%  — Overall accuracy
  mAP50-95:     40.8%  — Box placement precision

Inference Speed: ~8ms per image (ONNX Runtime + CUDA)
Detection Test:  55/56 images detected bottles at conf=0.25
```

---

## New Project Structure

```
DeepLearning/
├── dataset/                        ← Unified training dataset (generated)
│   ├── images/
│   │   ├── train/                  ← 246 augmented training images
│   │   └── val/                    ← 66 augmented validation images
│   ├── labels/
│   │   ├── train/                  ← 246 matching YOLO label files
│   │   └── val/                    ← 66 matching YOLO label files
│   └── data.yaml                   ← YOLO dataset config (3 classes)
│
├── raw-data/                       ← Consolidated raw images + labels
│   ├── images/                     ← 64 raw images (52 labeled + 12 unlabeled)
│   └── labels/                     ← 64 label files
│
├── scripts/                        ← Training pipeline scripts
│   ├── 1_prepare_dataset.py        ← Step 1: Consolidate raw data
│   ├── 2_augment_dataset.py        ← Step 2: Data augmentation (6x)
│   ├── 3_create_config.py          ← Step 3: Generate data.yaml
│   ├── 4_train.py                  ← Step 4: Train YOLO model
│   └── 5_export.py                 ← Step 5: Export to ONNX
│
├── models/                         ← Exported production models
│   └── detector_v2.onnx            ← ★ Latest trained ONNX model (10.1 MB)
│
├── runs/detect/                    ← YOLO training outputs
│   ├── train/                      ← Latest training run
│   │   ├── weights/
│   │   │   ├── best.pt             ← Best PyTorch model
│   │   │   └── best.onnx           ← Exported ONNX model
│   │   ├── results.png             ← Training metrics charts
│   │   └── results.csv             ← Raw training data
│   └── ...
│
├── bottles-images/                 ← Original bottle images (56 files)
├── soaps-images/                   ← Original soap images (56 files)
├── soaps-extra-images/             ← Extra unlabeled images (12 files)
├── yolo-battles-values/            ← Original bottle labels (52 files)
├── yolo-soaps-values/              ← Original soap labels (46 files)
├── bottle-dataset/                 ← Legacy v1 bottle-only dataset
│
├── detector_v2.onnx                ← ★ Latest model (copy in project root)
├── bottle_v1.onnx                  ← Previous v1 bottle-only model
├── yolo11n.pt                      ← Pre-trained base model
│
├── YOLO-TRAINING-GUIDE.md          ← This documentation
└── DeepLearning.sln / .csproj      ← .NET project files
```

---

## Complete Workflow: From Raw Images to Production ONNX Model

### Overview

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   STEP 1     │───▶│   STEP 2     │───▶│   STEP 3     │───▶│   STEP 4     │
│   Prepare    │    │   Augment    │    │   Config     │    │   Train      │
│   Raw Data   │    │   6x Data    │    │   data.yaml  │    │   GPU        │
└──────────────┘    └──────────────┘    └──────────────┘    └──────┬───────┘
                                                                   │
                    ┌──────────────┐    ┌──────────────┐          │
                    │   STEP 6     │◀───│   STEP 5     │◀─────────┘
                    │   .NET App   │    │   Export     │
                    │   Integrate  │    │   ONNX       │
                    └──────────────┘    └──────────────┘
```

---

### STEP 1: Prepare Raw Dataset

**Command:**
```bash
python scripts/1_prepare_dataset.py
```

**What it does:**
- Copies bottle images from `bottles-images/` with labels from `yolo-battles-values/`
- Remaps class IDs: bottle=0, soap=1, soap-cover=2
- Copies extra unlabeled images for future labeling
- Outputs consolidated data to `raw-data/`

**Expected output:**
```
[1/2] Processing bottle images...
  Bottles: 52 images copied, 0 skipped
[2/2] Processing extra soap images (no labels yet)...
  Extra images: 12 copied (need manual labeling)

Raw data prepared:
  Total images:  64
  Total labels:  64
  Labeled:       52
  Unlabeled:     12
```

---

### STEP 2: Data Augmentation (6x Multiplier)

**Command:**
```bash
python scripts/2_augment_dataset.py
```

**What it does — Augmentation types per image:**
| Augmentation | Image Transform | Label Transform |
|---|---|---|
| Original | No change | No change |
| Horizontal Flip | `cv2.flip(img, 1)` | `cx = 1.0 - cx` |
| Vertical Flip | `cv2.flip(img, 0)` | `cy = 1.0 - cy` |
| 90° CW Rotation | `cv2.rotate(img, ROTATE_90_CLOCKWISE)` | `new_cx=cy, new_cy=1-cx, swap w↔h` |
| 180° Rotation | `cv2.rotate(img, ROTATE_180)` | `new_cx=1-cx, new_cy=1-cy` |
| 270° CW Rotation | `cv2.rotate(img, ROTATE_90_COUNTERCLOCKWISE)` | `new_cx=1-cy, new_cy=cx, swap w↔h` |

**Why augmentation helps:**
- Teaches the model to recognize objects from different angles
- Prevents overfitting by showing varied versions of the same images
- Effectively multiplies your small dataset by 6x
- Rotations are critical for real-world scenarios where objects appear at various orientations

**Expected output:**
```
  DATA AUGMENTATION COMPLETE
  Original pairs:     52
  Augmentations:      original, hflip, vflip, rot90, rot180, rot270 (6x multiplier)
  Train images:       246
  Val images:         66
  Total images:       312
```

---

### STEP 3: Create data.yaml

**Command:**
```bash
python scripts/3_create_config.py
```

**Generated `dataset/data.yaml`:**
```yaml
path: D:/Ammar/YOLO/ml.net/DeepLearning/dataset
train: images/train
val: images/val

nc: 3
names:
  0: bottle
  1: soap
  2: soap-cover
```

---

### STEP 4: Train the Model

**Command:**
```bash
yolo train model=yolo11n.pt data="D:/Ammar/YOLO/ml.net/DeepLearning/dataset/data.yaml" epochs=200 imgsz=640 batch=16 workers=0 patience=50
```

**Parameters explained:**
| Parameter | Value | Purpose |
|---|---|---|
| `model` | `yolo11n.pt` | Pre-trained YOLOv11 nano (transfer learning) |
| `data` | path to data.yaml | Dataset configuration |
| `epochs` | `200` | Max training iterations (early stopping may stop sooner) |
| `imgsz` | `640` | Input image size (standard for YOLO) |
| `batch` | `16` | Images processed per step (fits in 8GB VRAM) |
| `workers` | `0` | DataLoader workers (0 avoids Windows multiprocessing issues) |
| `patience` | `50` | Stop if no improvement for 50 epochs |

**Training output location:**
```
runs/detect/train/
├── weights/
│   ├── best.pt          ← Best model checkpoint
│   └── last.pt          ← Final epoch model
├── results.png          ← Training charts
├── results.csv          ← Raw metrics
├── confusion_matrix.png
├── labels.jpg
└── val_batch0_pred.jpg  ← Validation predictions
```

---

### STEP 5: Export to ONNX

**Command:**
```bash
yolo export model=runs/detect/train/weights/best.pt format=onnx opset=17 imgsz=640
```

**Then copy to project:**
```bash
copy runs\detect\train\weights\best.onnx models\detector_v2.onnx
copy runs\detect\train\weights\best.onnx detector_v2.onnx
```

**What this does:**
- Converts PyTorch `.pt` → ONNX `.onnx` (universal format)
- ONNX Runtime works in .NET, Python, C++, etc.
- opset=17 ensures compatibility with latest ONNX Runtime
- Output: ~10.1 MB model file

---

### STEP 6: Integrate with .NET

Update `Program.cs` detection options:
```csharp
var options = new DetectionOptions
{
    ModelPath = "detector_v2.onnx",
    ClassLabels = ["bottle", "soap", "soap-cover"],
    ModelWidth = 640,
    ModelHeight = 640,
    ConfidenceThreshold = 0.25f,
    IouThreshold = 0.45f,
};
```

---

## Understanding Data Augmentation

### Why Rotation Helps

```
Original Image:          90° CW:              180°:              270° CW:
┌─────────────┐        ┌─────────────┐      ┌─────────────┐    ┌─────────────┐
│  ┌───┐      │        │      ┌───┐  │      │      ┌───┐  │    │  ┌───┐      │
│  │   │      │   ──▶  │      │   │  │  ──▶ │      │   │  │──▶ │  │   │      │
│  │ B │      │        │      │ B │  │      │      │ B │  │    │  │ B │      │
│  └───┘      │        │      └───┘  │      │      └───┘  │    │  └───┘      │
└─────────────┘        └─────────────┘      └─────────────┘    └─────────────┘

The model learns: "A bottle is a bottle regardless of orientation"
```

### Label Transformation for 90° CW Rotation

```
For YOLO format (cx, cy, w, h) normalized to [0, 1]:

Original:   cx=0.5, cy=0.3, w=0.2, h=0.6
After 90°:  new_cx = old_cy = 0.3
            new_cy = 1.0 - old_cx = 0.5
            new_w  = old_h = 0.6
            new_h  = old_w = 0.2
```

---

## Quick Command Reference

```bash
# ─── Full Pipeline ──────────────────────────────────────────
python scripts/1_prepare_dataset.py
python scripts/2_augment_dataset.py
python scripts/3_create_config.py
yolo train model=yolo11n.pt data="D:/Ammar/YOLO/ml.net/DeepLearning/dataset/data.yaml" epochs=200 imgsz=640 batch=16 workers=0 patience=50
yolo export model=runs/detect/train/weights/best.pt format=onnx opset=17 imgsz=640
copy runs\detect\train\weights\best.onnx models\detector_v2.onnx

# ─── Test the Model ────────────────────────────────────────
yolo predict model=models/detector_v2.onnx source=test.jpg imgsz=640 conf=0.25 save=True

# ─── Validate ──────────────────────────────────────────────
yolo val model=runs/detect/train/weights/best.pt data="D:/Ammar/YOLO/ml.net/DeepLearning/dataset/data.yaml"

# ─── Verify GPU ────────────────────────────────────────────
python -c "import torch; print('CUDA:', torch.cuda.is_available())"
```

---

## Training Metrics Explanation

### The Three Losses
| Loss | What it measures | Good target |
|---|---|---|
| `box_loss` | Bounding box position accuracy | < 0.5 |
| `cls_loss` | Class prediction accuracy | < 1.0 |
| `dfl_loss` | Box boundary precision | < 1.5 |

### Key Metrics
| Metric | Meaning | Our Result |
|---|---|---|
| **Precision** | % of detections that are correct | 80.8% |
| **Recall** | % of actual objects found | 63.9% |
| **mAP50** | Overall accuracy at IoU=0.5 | 60.1% |
| **mAP50-95** | Strict accuracy (IoU 0.5-0.95) | 40.8% |

---

## Important Notes

### Current Status
- **Bottle class**: Fully trained with 52 labeled images + augmentation
- **Soap class**: No labeled training images yet (needs manual labeling)
- **Soap-cover class**: No labeled training images yet (needs manual labeling)

### To Add Soap/Soap-Cover Detection
1. Take photos containing soaps and soap-covers
2. Label them using a tool like LabelImg or Roboflow
3. Place labels in `raw-data/labels/` with matching filenames
4. Use class 1 for soap and class 2 for soap-cover
5. Re-run the full pipeline (Steps 1-5)

### Recommended Label Format
```
# soap-cover (small wrapper) = class 2
2 0.587045 0.584132 0.591054 0.572950

# soap (main bar) = class 1  
1 0.461917 0.506890 0.729659 0.753937

# bottle = class 0
0 0.561486 0.515974 0.214058 0.807242
```

---

*Documentation generated April 2026*
