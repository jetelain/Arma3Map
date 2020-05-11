using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace MakeTiles
{
    class Program
    {

        const int tileSize = 284;

        static void Main(string[] args)
        {
            var zoomLevel = 5;

            using (var img = SixLabors.ImageSharp.Image.Load(@"D:\Julien\Pictures\taunus.png"))
            {
                while (img.Width >= tileSize)
                {
                    Split(img, zoomLevel);

                    img.Mutate(i => i.Resize(img.Width / 2, img.Height / 2));

                    zoomLevel--;
                }
            }
        }

        private static void Split(Image img, int zoomLevel)
        {
            var bounds = img.Bounds();


            for (int x = 0; x < bounds.Width; x += tileSize)
            {
                for (int y = 0; y < bounds.Height; y += tileSize)
                {
                    var tile = img.Clone(i => i.Crop(new Rectangle(x, y, tileSize, tileSize)));

                    var file = $"{zoomLevel}/{x / tileSize}/{y / tileSize}.png";

                    var dir = System.IO.Path.GetDirectoryName(file);
                    Directory.CreateDirectory(dir);

                    tile.Save(file);

                }
            }
        }
    }
}
