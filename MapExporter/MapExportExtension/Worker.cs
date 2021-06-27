using System;
using System.Diagnostics;
using System.Globalization;
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
        private static string mapDataPath;

        private static double SafeZoneX;
        private static double SafeZoneY;
        private static double SafeZoneW;
        private static double SafeZoneH;

        private static int ScreenW;
        private static int ScreenH;

        private static int OneW;
        private static int OneH; 
        private static int OneWPx;
        private static int OneHPx;

        private static bool IsHiRes;

        public static Image<Rgba32> FullImage { get; private set; }

        internal static void Message(string function, string[] args)
        {
            switch (function)
            {
                case "start":
                    Start(ArmaSerializer.ParseString(args[0]), 
                        double.Parse(args[1], CultureInfo.InvariantCulture), 
                        ArmaSerializer.ParseMixedArray(args[2]), 
                        ArmaSerializer.ParseDoubleArray(args[3]),
                        ArmaSerializer.ParseString(args[4]));
                    return;
                case "histart":
                    HiResStart();
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
                case "histop":
                    HiResStop();
                    return;
                case "dispose":
                    Dispose();
                    return;
            }
        }

        private static void Dispose()
        {
            if (FullImage != null)
            {
                FullImage.Dispose();
                FullImage = null;
            }
            mapDataPath = null;
            currentMap = null;
            IsHiRes = false;
        }

        private static void HiResStop()
        {
            if (FullImage != null)
            {
                FullImage.SaveAsPng(Path.Combine(mapDataPath, $"{currentMap.worldName}-hires.png"));
                using (FullImage)
                {
                    Split(FullImage, currentMap.maxZoom);
                }
            }
        }

        private static void HiResStart()
        {
            IsHiRes = true;
            if (FullImage != null)
            {
                FullImage.Dispose();
                FullImage = null;
            }
        }

        private static void Stop()
        {
            if (FullImage != null)
            {
                SplitFullImage();
                FullImage = null;
            }
        }

        private static void SplitFullImage()
        {
            FullImage.SaveAsPng(Path.Combine(mapDataPath, $"{currentMap.worldName}.png"));

            var zoomLevel = currentMap.maxZoom-1;
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

        private static void Start(string worldName, double worldSize, object[] cities, double[] center, string title)
        {
            IsHiRes = false;
            currentMap = new MapInfos()
            {
                worldSize = worldSize,
                worldName = worldName.ToLowerInvariant(),
                center = new System.Collections.Generic.List<int>() { (int)worldSize / 2, (int)worldSize / 2 }, //center.Select(i => (int)i).ToList(),
                title = title,
                cities = cities.Cast<object[]>().Select(c => new CityInfos()
                {
                    name =(string)c[0],
                    x = (double)((object[])c[1])[0],
                    y = (double)((object[])c[1])[1]
                }).ToList(),
                tilePattern = "/maps/" + worldName.ToLowerInvariant() + "/{z}/{x}/{y}.png",
                attribution = "&copy; Bohemia Interactive"
            };
            mapDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Arma3MapExporter", "maps", currentMap.worldName);
            if (!Directory.Exists(mapDataPath))
            {
                Directory.CreateDirectory(mapDataPath);
            }
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

            OneWPx = (pxB.X - pxA.X);
            OneHPx = (pxA.Y - pxB.Y);

            var fullWidthInitialPx = currentMap.worldSize * OneWPx / w;
            var fullHeightInitialPx = currentMap.worldSize * OneHPx / h;
            var fullSizeInitialPx = Math.Max(fullWidthInitialPx, fullHeightInitialPx);

            if (IsHiRes)
            {
                CalibrateHiRes(fullWidthInitialPx, fullHeightInitialPx, fullSizeInitialPx);
            }
            else
            {
                CalibrateInitial(fullWidthInitialPx, fullHeightInitialPx, fullSizeInitialPx);
            }
        }

        private static void CalibrateInitial(double fullWidthInitialPx, double fullHeightInitialPx, double fullSizeInitialPx)
        {
            var tileSizePx = (int)Math.Ceiling(fullSizeInitialPx);

            int maxZoom = 0;
            while (tileSizePx > 400)
            {
                tileSizePx = tileSizePx / 2;
                maxZoom++;
            }
            tileSizePx++;

            var fullSizePx = tileSizePx * (1 << maxZoom);

            currentMap.tileSize = tileSizePx;
            currentMap.maxZoom = maxZoom+1;
            currentMap.defaultZoom = Math.Max(2, maxZoom / 2);
            currentMap.minZoom = 0;

            var json = JsonConvert.SerializeObject(currentMap, Formatting.Indented);
            File.WriteAllText(Path.Combine(mapDataPath, $"{currentMap.worldName}.json"), json);

            var adjustedWorldWidth = fullSizePx * currentMap.worldSize / fullWidthInitialPx;
            var adjustedWorldHeight = fullSizePx * currentMap.worldSize / fullHeightInitialPx;

            var coefWidth = tileSizePx / adjustedWorldWidth;
            var coefHeight = tileSizePx / adjustedWorldHeight;

            WriteJavaScript(tileSizePx, json, coefWidth, coefHeight);
            WriteHtml();

            FullImage = new Image<Rgba32>(null, fullSizePx, fullSizePx, new Rgba32(221, 221, 221));
        }

        private static void CalibrateHiRes(double fullWidthInitialPx, double fullHeightInitialPx, double fullSizeInitialPx)
        {
            var fullSizePx = currentMap.tileSize * (1 << currentMap.maxZoom);
            FullImage = new Image<Rgba32>(null, fullSizePx, fullSizePx, new Rgba32(221, 221, 221));
        }

        private static void WriteJavaScript(int tileSizePx, string json, double coefWidth, double coefHeight)
        {
            var js = $@"Arma3Map.Maps.{currentMap.worldName} = {{
  CRS: MGRS_CRS({coefWidth.ToString(CultureInfo.InvariantCulture)}, {coefHeight.ToString(CultureInfo.InvariantCulture)}, {tileSizePx}),
{json.Substring(3)};";

            File.WriteAllText(Path.Combine(mapDataPath, "..", $"{currentMap.worldName}.js"), js);
        }

        private static void WriteHtml()
        {
            File.WriteAllText(Path.Combine(mapDataPath, "..", "..", $"{currentMap.worldName}.html"), $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{currentMap.title}</title>
    <link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.6.0/dist/leaflet.css"" />
    <link rel=""stylesheet"" href=""css/mapUtils.css"" />
</head>
<body style="" margin:0;padding:0;border:0;"">
    <div class=""map"" id=""map"" style=""width:100%; height:100vh; margin:0;padding:0;border:0;"">
    </div>
    <script src=""https://unpkg.com/leaflet@1.6.0/dist/leaflet.js"">
    </script>
	<script src=""https://unpkg.com/jquery@3.5.1/dist/jquery.min.js"">
    </script>
    <script src=""js/mapUtils.js"">
    </script>
    <script src=""js/defaultMap.js"">
    </script>
    <script src=""maps/{currentMap.worldName}.js"">
    </script>
    <script>
        InitMap(Arma3Map.Maps.{currentMap.worldName});
    </script>
</body>
</html>");
        }

        private static void ScreenShot(int x, int y, double[] pA, double[] pB)
        {
            if (mapDataPath == null || currentMap == null)
            {
                return;
            }
            var pxA = ArmaToScreen(pA);
            var pxB = ArmaToScreen(pB);

            //Trace.TraceInformation("X={0} Y={1} W={2} (not used) H={3} (not used)", pxA.X, pxB.Y, (pxB.X - pxA.X), (pxA.Y - pxB.Y));

            var crop = new Rectangle(pxA.X, pxB.Y, OneWPx, OneHPx);
            var point = new Point((x / OneW) * OneWPx, FullImage.Height - ((y / OneH) * OneHPx) - OneHPx);
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
                (int)Math.Ceiling((point[1] - SafeZoneY) * ScreenH / SafeZoneH));
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
                    var file = Path.Combine(mapDataPath, $"{zoomLevel}/{x / tileSize}/{y / tileSize}.png");
                    var dir = System.IO.Path.GetDirectoryName(file);
                    Directory.CreateDirectory(dir);
                    tile.Save(file);
                }
            }
        }
    }
}
