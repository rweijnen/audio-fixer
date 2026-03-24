// dotnet-script: generates app.ico with 16x16, 32x32, 48x48, 256x256 sizes
// Run: dotnet script tools/GenerateIcon.csx
#r "nuget: System.Drawing.Common, 9.0.0"

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

var outputPath = Path.Combine(
    Path.GetDirectoryName(Environment.GetCommandLineArgs()[1]) ?? ".",
    "..", "src", "AudioFixer", "Resources", "app.ico");

outputPath = Path.GetFullPath(outputPath);
Console.WriteLine($"Generating icon at: {outputPath}");

int[] sizes = [16, 32, 48, 256];
var images = new List<(Bitmap bmp, int size)>();

foreach (var size in sizes)
{
    var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.Clear(Color.Transparent);

    float s = size;

    // Background circle
    using (var bgBrush = new SolidBrush(Color.FromArgb(30, 100, 200)))
        g.FillEllipse(bgBrush, s * 0.04f, s * 0.04f, s * 0.92f, s * 0.92f);

    // Speaker body (white)
    using var spkBrush = new SolidBrush(Color.White);
    float bx = s * 0.18f, by = s * 0.33f, bw = s * 0.15f, bh = s * 0.34f;
    g.FillRectangle(spkBrush, bx, by, bw, bh);

    // Speaker cone
    var cone = new PointF[]
    {
        new(bx + bw, by),
        new(s * 0.48f, s * 0.18f),
        new(s * 0.48f, s * 0.82f),
        new(bx + bw, by + bh)
    };
    g.FillPolygon(spkBrush, cone);

    // Sound waves
    using var wavePen = new Pen(Color.White, Math.Max(1f, s * 0.045f));
    g.DrawArc(wavePen, s * 0.50f, s * 0.28f, s * 0.18f, s * 0.44f, -60, 120);
    g.DrawArc(wavePen, s * 0.58f, s * 0.18f, s * 0.26f, s * 0.64f, -60, 120);

    // Wrench overlay (bottom-right)
    float wx = s * 0.52f, wy = s * 0.52f, ws = s * 0.44f;
    using (var wrenchBg = new SolidBrush(Color.FromArgb(220, 160, 40)))
    {
        g.FillEllipse(wrenchBg, wx, wy, ws, ws);
    }
    // Simple wrench shape
    using var wrenchPen = new Pen(Color.White, Math.Max(1.5f, s * 0.055f))
    {
        StartCap = LineCap.Round,
        EndCap = LineCap.Round
    };
    float wcx = wx + ws / 2, wcy = wy + ws / 2;
    float wr = ws * 0.3f;
    // Wrench handle
    g.DrawLine(wrenchPen, wcx - wr * 0.7f, wcy + wr * 0.7f, wcx + wr * 0.5f, wcy - wr * 0.5f);
    // Wrench head
    g.DrawArc(wrenchPen, wcx + wr * 0.1f, wcy - wr * 1.1f, wr * 0.9f, wr * 0.9f, 45, 270);

    images.Add((bmp, size));
}

// Write ICO file
var fs = File.Create(outputPath);
var bw = new BinaryWriter(fs);

// ICO header
bw.Write((short)0);      // reserved
bw.Write((short)1);      // type: icon
bw.Write((short)images.Count);

// Calculate offsets
int dataOffset = 6 + images.Count * 16; // header + directory entries
var pngDataList = new List<byte[]>();

foreach (var (bmp, size) in images)
{
    using var ms = new MemoryStream();
    bmp.Save(ms, ImageFormat.Png);
    var pngData = ms.ToArray();
    pngDataList.Add(pngData);

    // Directory entry
    bw.Write((byte)(size < 256 ? size : 0)); // width
    bw.Write((byte)(size < 256 ? size : 0)); // height
    bw.Write((byte)0);    // color palette
    bw.Write((byte)0);    // reserved
    bw.Write((short)1);   // color planes
    bw.Write((short)32);  // bits per pixel
    bw.Write(pngData.Length);
    bw.Write(dataOffset);
    dataOffset += pngData.Length;
}

// Write image data
foreach (var data in pngDataList)
    bw.Write(data);

foreach (var (bmp, _) in images)
    bmp.Dispose();

bw.Dispose();
fs.Dispose();

Console.WriteLine($"Icon generated with {images.Count} sizes: {string.Join(", ", sizes.Select(s => $"{s}x{s}"))}");
