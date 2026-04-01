"""
Step 5: Export to ONNX
======================
Exports the trained PyTorch model to ONNX format for .NET integration.

Usage:
  python scripts\5_export.py
"""

import os
import shutil
from ultralytics import YOLO

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(SCRIPT_DIR)

BEST_PT  = os.path.join(ROOT, "runs", "detect", "train", "weights", "best.pt")
OUT_ONNX = os.path.join(ROOT, "models", "detector_v2.onnx")

print("="*60)
print("  EXPORTING MODEL TO ONNX")
print("="*60)

# Load trained model
model = YOLO(BEST_PT)

# Export to ONNX
model.export(format="onnx", opset=17, imgsz=640)

# The export creates best.onnx next to best.pt
exported = BEST_PT.replace(".pt", ".onnx")
if os.path.exists(exported):
    os.makedirs(os.path.dirname(OUT_ONNX), exist_ok=True)
    shutil.copy2(exported, OUT_ONNX)
    print(f"\n  ONNX model saved to: {OUT_ONNX}")
    print(f"  File size: {os.path.getsize(OUT_ONNX) / (1024*1024):.1f} MB")
else:
    print(f"\n  ERROR: Expected exported file not found at {exported}")

print("="*60)
