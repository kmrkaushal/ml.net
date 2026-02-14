using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public sealed class OnnxYoloRunner : IDisposable
{
    public sealed class Detection
    {
        public int ClassId { get; init; }
        public float Confidence { get; init; }

        public float X1 { get; init; }
        public float Y1 { get; init; }
        public float X2 { get; init; }
        public float Y2 { get; init; }

        public override string ToString()
            => $"cls={Coco[ClassId]}, conf={Confidence:0.00}, box=({X1:0},{Y1:0},{X2:0},{Y2:0})";
    }

    private readonly InferenceSession _session;
    private readonly string _inputName;
    private readonly int _modelW;
    private readonly int _modelH;
    static readonly string[] Coco = new[]
{
"person","bicycle","car","motorcycle","airplane","bus","train","truck","boat",
"traffic light","fire hydrant","stop sign","parking meter","bench","bird","cat",
"dog","horse","sheep","cow","elephant","bear","zebra","giraffe","backpack","umbrella",
"handbag","tie","suitcase","frisbee","skis","snowboard","sports ball","kite",
"baseball bat","baseball glove","skateboard","surfboard","tennis racket","bottle",
"wine glass","cup","fork","knife","spoon","bowl","banana","apple","sandwich",
"orange","broccoli","carrot","hot dog","pizza","donut","cake","chair","couch",
"potted plant","bed","dining table","toilet","tv","laptop","mouse","remote",
"keyboard","cell phone","microwave","oven","toaster","sink","refrigerator",
"book","clock","vase","scissors","teddy bear","hair drier","toothbrush"
};


    public OnnxYoloRunner(string onnxPath, int modelWidth = 640, int modelHeight = 640)
    {
        _modelW = modelWidth;
        _modelH = modelHeight;

        _session = new InferenceSession(onnxPath);
        _inputName = _session.InputMetadata.Keys.First();
    }

    public List<Detection> Detect(Bitmap image, float confThreshold = 0.25f, float iouThreshold = 0.45f)
    {
        int origW = image.Width;
        int origH = image.Height;

        var chw = ToCHW(image, _modelW, _modelH);
        var input = new DenseTensor<float>(chw, new[] { 1, 3, _modelH, _modelW });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, input)
        };

        using var results = _session.Run(inputs);

        var (tensor, dims) = PickDetectionHead(results);
        var data = tensor.ToArray();

        int a = dims[1], b = dims[2];
        bool channelsFirst = a < b;

        int C = channelsFirst ? a : b;
        int N = channelsFirst ? b : a;
        int classes = C - 4;

        float At(int c, int i) => channelsFirst ? data[c * N + i] : data[i * C + c];

        var detections = new List<Detection>(256);

        for (int i = 0; i < N; i++)
        {
            float cx = At(0, i);
            float cy = At(1, i);
            float w = At(2, i);
            float h = At(3, i);

            int bestId = -1;
            float best = 0f;

            for (int c = 0; c < classes; c++)
            {
                float p = At(4 + c, i);
                if (p > best)
                {
                    best = p;
                    bestId = c;
                }
            }

            if (best < confThreshold) continue;

            float x1 = (cx - w / 2f) * origW / _modelW;
            float y1 = (cy - h / 2f) * origH / _modelH;
            float x2 = (cx + w / 2f) * origW / _modelW;
            float y2 = (cy + h / 2f) * origH / _modelH;

            detections.Add(new Detection
            {
                ClassId = bestId,
                Confidence = best,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            });
        }

        return ApplyNms(detections, iouThreshold);
    }

    // ---------------- NMS ----------------

    private static List<Detection> ApplyNms(List<Detection> boxes, float iouThreshold)
    {
        var result = new List<Detection>();

        foreach (var group in boxes.GroupBy(b => b.ClassId))
        {
            var sorted = group.OrderByDescending(b => b.Confidence).ToList();

            while (sorted.Count > 0)
            {
                var best = sorted[0];
                result.Add(best);
                sorted.RemoveAt(0);

                sorted = sorted
                    .Where(b => IoU(best, b) < iouThreshold)
                    .ToList();
            }
        }

        return result;
    }

    private static float IoU(Detection a, Detection b)
    {
        float areaA = (a.X2 - a.X1) * (a.Y2 - a.Y1);
        float areaB = (b.X2 - b.X1) * (b.Y2 - b.Y1);

        float x1 = Math.Max(a.X1, b.X1);
        float y1 = Math.Max(a.Y1, b.Y1);
        float x2 = Math.Min(a.X2, b.X2);
        float y2 = Math.Min(a.Y2, b.Y2);

        float inter = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        return inter / (areaA + areaB - inter + 1e-6f);
    }

    // ---------------- Preprocess ----------------

    private static float[] ToCHW(Bitmap src, int w, int h)
    {
        using var resized = new Bitmap(src, new Size(w, h));
        var data = new float[3 * h * w];

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var c = resized.GetPixel(x, y);
                int idx = y * w + x;

                data[0 * h * w + idx] = c.R / 255f;
                data[1 * h * w + idx] = c.G / 255f;
                data[2 * h * w + idx] = c.B / 255f;
            }

        return data;
    }

    private static (Tensor<float>, int[]) PickDetectionHead(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
    {
        foreach (var r in results)
        {
            var t = r.AsTensor<float>();
            var d = t.Dimensions.ToArray();

            if (d.Length == 3 && d[0] == 1)
            {
                int channels = Math.Min(d[1], d[2]);
                int count = Math.Max(d[1], d[2]);

                if (channels >= 6 && count >= 1000)
                    return (t, d);
            }
        }

        var first = results.First().AsTensor<float>();
        return (first, first.Dimensions.ToArray());
    }

    public Bitmap DetectAndOverlay(Bitmap image, float confThreshold = 0.50f)
    {
        var detections = Detect(image, confThreshold);      
        var canvas = new Bitmap(image);

        using var g = Graphics.FromImage(canvas);
        using var pen = new Pen(Color.Blue, 2);
        using var font = new Font("Segoe UI", 9, FontStyle.Regular);
        using var bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
        using var fg = new SolidBrush(Color.White);

        foreach (var d in detections)
        {
            var rect = Rectangle.FromLTRB(
                (int)d.X1,
                (int)d.Y1,
                (int)d.X2,
                (int)d.Y2);

            // Draw box
            g.DrawRectangle(pen, rect);

            // Label text
            //string label = $"cls {d.ClassId} {d.Confidence:0.00}";
            string name = d.ClassId < Coco.Length ? Coco[d.ClassId] : $"cls {d.ClassId}";
            string label = $"{name} {d.Confidence:0.00}";


            var size = g.MeasureString(label, font);
            var textRect = new RectangleF(rect.Left, rect.Top - size.Height, size.Width + 4, size.Height);

            g.FillRectangle(bg, textRect);
            g.DrawString(label, font, fg, textRect.Location);
        }

        return canvas;
    }


    public void Dispose() => _session?.Dispose();
}
