using System.Drawing;
using DeepLearning;

//using var yolo = new OnnxYoloRunner("yolov8n.onnx");
//Bitmap bmp = new Bitmap("traffic.jpg");
//var dets = yolo.Detect(bmp, 0.25f);

//foreach (var d in dets)
//    Console.WriteLine(d);

//using var result = yolo.DetectAndOverlay(bmp, 0.25f);
//result.Save("output.jpg");

// RealTime Object Detection 
new YoloRealTime().detect();