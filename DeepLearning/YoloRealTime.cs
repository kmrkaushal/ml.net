using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearning
{    
    internal class YoloRealTime
    {
        public void detect()
        {
            using var yolo = new OnnxYoloRunner("yolo11n.onnx");
            using var cap = new VideoCapture(0, VideoCaptureAPIs.DSHOW);

            if (!cap.IsOpened())
            {
                Console.WriteLine("Unable to open webcam.");
                return;
            }

            var frame = new Mat();

            while (true)
            {
                cap.Read(frame);
                if (frame.Empty()) continue;

                // Mat -> Bitmap
                using Bitmap bmp = BitmapConverter.ToBitmap(frame);

                using Bitmap overlay = yolo.DetectAndOverlay(bmp, 0.30f);

                // Bitmap -> Mat for display
                using Mat show = BitmapConverter.ToMat(overlay);

                Cv2.ImShow("YOLO Realtime (ESC to exit)", show);

                // ESC to exit
                if (Cv2.WaitKey(1) == 27)
                    break;
            }

            cap.Release();
            Cv2.DestroyAllWindows();
        }
    }
}
