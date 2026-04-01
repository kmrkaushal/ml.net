"""
Step 4: Train YOLO Model (ENHANCED — 200 epochs, AdamW, cosine LR)
===================================================================
Trains a YOLOv11n model on the augmented dataset.
Uses if __name__ == '__main__' guard for Windows multiprocessing.

Enhanced parameters for multi-class training:
  - epochs: 200 (up from 100, larger dataset can handle more training)
  - patience: 50 (up from 20, allow more epochs before early stopping)
  - optimizer: AdamW (better convergence on multi-class data)
  - cos_lr: True (cosine learning rate schedule for smoother convergence)

Usage:
  python scripts/4_train.py
"""

import os
import sys

# Add project root to path
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(SCRIPT_DIR)

DATA_YAML = os.path.join(ROOT, "dataset", "data.yaml")
PRETRAINED = os.path.join(ROOT, "yolo11n.pt")
OUTPUT_DIR = os.path.join(ROOT, "runs", "detect")


def main():
    from ultralytics import YOLO

    print("=" * 60)
    print("  YOLO TRAINING STARTED (ENHANCED)")
    print("=" * 60)
    print(f"  Dataset:  {DATA_YAML}")
    print(f"  Model:    yolo11n.pt (pre-trained)")
    print(f"  Epochs:   200")
    print(f"  Image:    640x640")
    print(f"  Batch:    16")
    print(f"  Optimizer: AdamW")
    print(f"  LR Sched: Cosine")
    print(f"  Patience: 50")
    print(f"  Output:   {OUTPUT_DIR}")
    print("=" * 60)

    # Load pre-trained model
    model = YOLO(PRETRAINED)

    # Train with enhanced parameters
    results = model.train(
        data=DATA_YAML,
        epochs=200,
        imgsz=640,
        batch=16,
        workers=0,  # Set to 0 to avoid Windows multiprocessing issues
        name="train",
        project=OUTPUT_DIR,
        exist_ok=True,
        patience=50,
        optimizer="AdamW",
        cos_lr=True,
        save=True,
        plots=True,
        verbose=True,
    )

    print("\n" + "=" * 60)
    print("  TRAINING COMPLETE")
    print("=" * 60)
    print(f"  Best model: {OUTPUT_DIR}/train/weights/best.pt")
    print(f"  Last model: {OUTPUT_DIR}/train/weights/last.pt")
    print("=" * 60)


if __name__ == "__main__":
    main()
