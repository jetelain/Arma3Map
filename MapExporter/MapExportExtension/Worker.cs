using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MapExportExtension
{
    internal static class Worker
    {
        private static MapInfos currentMap;
        private static string workingPath;

        private static double SafeZoneX;
        private static double SafeZoneY;
        private static double SafeZoneW;
        private static double SafeZoneH;

        private static int ScreenW;
        private static int ScreenH;

        private static int OneW;
        private static int OneH; 
        private static int OneWPixels;
        private static int OneHPixels;

        public static Image<Rgba32> FullImage { get; private set; }

        internal static void Message(string function, string[] args)
        {
            switch (function)
            {
                case "start":
                    Start(ArmaSerializer.ParseString(args[0]), 
                        int.Parse(args[1]), 
                        ArmaSerializer.ParseMixedArray(args[2]), 
                        ArmaSerializer.ParseDoubleArray(args[3]),
                        ArmaSerializer.ParseString(args[4]));
                    return;
                case "calibrate":
                    Calibrate(ArmaSerializer.ParseDoubleArray(args[0]),
                        ArmaSerializer.ParseDoubleArray(args[1]),
                        ArmaSerializer.ParseDoubleArray(args[2]),
                        int.Parse(args[3]), 
                        int.Parse(args[4]));
                    return;
                case "screenshot":
                    ScreenShot(int.Parse(args[0]), 
                        int.Parse(args[1]),
                        ArmaSerializer.ParseDoubleArray(args[2]),
                        ArmaSerializer.ParseDoubleArray(args[3]));
                    return;
                case "stop":
                    Stop();
                    return;
            }
        }

        private static void Stop()
        {
            if (FullImage != null)
            {
                SplitFullImage();
                FullImage = null;
            }
            workingPath = null;
            currentMap = null;
        }

        private static void SplitFullImage()
        {
            FullImage.SaveAsPng(Path.Combine(workingPath, $"{currentMap.worldName}.png"));

            var zoomLevel = currentMap.maxZoom;
            using (FullImage)
            {
                while (FullImage.Width >= currentMap.tileSize)
                {
                    Split(FullImage, zoomLevel);
                    FullImage.Mutate(i => i.Resize(FullImage.Width / 2, FullImage.Height / 2));
                    zoomLevel--;
                }
            }
        }

        private static void Start(string worldName, int worldSize, object[] cities, double[] center, string title)
        {
            currentMap = new MapInfos()
            {
                worldSize = worldSize,
                worldName = worldName,
                center = center.Select(i => (int)i).ToList(),
                title = title,
                cities = cities.Cast<object[]>().Select(c => new CityInfos()
                {
                    name =(string)c[0],
                    x = (double)((object[])c[1])[0],
                    y = (double)((object[])c[1])[1]
                }).ToList()
            };
            workingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Arma3MapExporter", worldName);
            if (!Directory.Exists(workingPath))
            {
                Directory.CreateDirectory(workingPath);
            }
            SaveJson();
        }

        private static void Calibrate(double[] safeZone, double[] pA, double[] pB, int w, int h)
        {
            SafeZoneX = safeZone[0];
            SafeZoneY = safeZone[1];
            SafeZoneW = safeZone[2];
            SafeZoneH = safeZone[3];
            ScreenH = Screen.PrimaryScreen.Bounds.Height;
            ScreenW = Screen.PrimaryScreen.Bounds.Width;

            var pxA = ArmaToScreen(pA);
            var pxB = ArmaToScreen(pB);

            OneW = w;
            OneH = h;

            OneWPixels = (pxB.X - pxA.X);
            OneHPixels = (pxA.Y - pxB.Y);

            var fullWidth = currentMap.worldSize * OneWPixels / w;

            var fullHeight = currentMap.worldSize * OneHPixels / h;

            var fullSizeInitial = Math.Max(fullWidth, fullHeight);

            var tileSize = (int)Math.Ceiling(fullSizeInitial);

            int maxZoom = 0;
            while (tileSize > 400)
            {
                tileSize = tileSize / 2;
                maxZoom++;
            }
            tileSize++;

            var fullSize = tileSize * (1 << maxZoom);

            currentMap.tileSize = tileSize;
            currentMap.maxZoom = maxZoom;
            currentMap.minZoom = 0;

            SaveJson();

            //Trace.TraceInformation("OneW={0} OneH={1} OneWPixels={2} OneHPixels={3} fullSize={4} tileSize={5} maxZoom={6}", OneW, OneH, OneWPixels, OneHPixels, fullSize, tileSize, maxZoom);

            FullImage = new Image<Rgba32>(null, fullSize, fullSize, new Rgba32(221, 221, 221));
        }

        private static void SaveJson()
        {
            File.WriteAllText(Path.Combine(workingPath, $"{currentMap.worldName}.json"), JsonConvert.SerializeObject(currentMap, Formatting.Indented));
        }

        private static void ScreenShot(int x, int y, double[] pA, double[] pB)
        {
            if (workingPath == null || currentMap == null)
            {
                return;
            }
            var pxA = ArmaToScreen(pA);
            var pxB = ArmaToScreen(pB);

            //Trace.TraceInformation("X={0} Y={1} W={2} (not used) H={3} (not used)", pxA.X, pxB.Y, (pxB.X - pxA.X), (pxA.Y - pxB.Y));

            var crop = new Rectangle(pxA.X, pxB.Y, OneWPixels, OneHPixels);
            var point = new Point((x / OneW) * OneWPixels, FullImage.Height - ((y / OneH) * OneHPixels) - OneHPixels);
            using (var data = TakeScreenShot($"{x}-{y}"))
            {
                data.Mutate(i => i.Crop(crop));
                FullImage.Mutate(i => i.DrawImage(data, point, 1f));
            }
        }


        private static Point ArmaToScreen(double[] point)
        {
            var p = new Point(
                (int)Math.Floor((point[0] - SafeZoneX) * ScreenW / SafeZoneW),
                (int)Math.Floor((point[1] - SafeZoneY) * ScreenH / SafeZoneH));
            //Trace.WriteLine($"[{point[0]},{point[1]}] => [{p.X},{p.Y}]");
            return p;
        }

        private static SixLabors.ImageSharp.Image TakeScreenShot(string name)
        {
            using (var bitmap = new System.Drawing.Bitmap(ScreenW, ScreenH))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, new System.Drawing.Size(ScreenW, ScreenH));
                }
                using(var ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    var bytes = ms.ToArray();
                    //File.WriteAllBytes(Path.Combine(workingPath, $"screen-{name}.png"), bytes);
                    return Image.Load(bytes, new PngDecoder());
                }
            }
        }

        private static void Split(Image img, int zoomLevel)
        {
            var tileSize = currentMap.tileSize;
            var bounds = img.Bounds();
            for (int x = 0; x < bounds.Width; x += tileSize)
            {
                for (int y = 0; y < bounds.Height; y += tileSize)
                {
                    var tile = img.Clone(i => i.Crop(new Rectangle(x, y, tileSize, tileSize)));
                    var file = Path.Combine(workingPath, $"{zoomLevel}/{x / tileSize}/{y / tileSize}.png");
                    var dir = System.IO.Path.GetDirectoryName(file);
                    Directory.CreateDirectory(dir);
                    tile.Save(file);
                }
            }
        }
    }
}
