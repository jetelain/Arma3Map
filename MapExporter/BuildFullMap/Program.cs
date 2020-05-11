using System;
using System.Collections.Generic;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ConsoleApp1
{
    class Program
    {
        static Rectangle rect = new Rectangle(240, 175, 1328, 885);
        static Rectangle rect2 = new Rectangle(240, 547, 1328, 227);

        static void Main(string[] args)
        {
            // Recherche les captures d'écran différentes sur la zone utile
            var images = new List<Image<Rgba32>>();
            for(int i =0;i<190;++i)
            {
                var img = (Image<Rgba32>)SixLabors.ImageSharp.Image.Load(@$"C:\Users\Julien\source\repos\jetelain\Arma3Map\MapExporter\MakeScreenShots\bin\Debug\{i}.png");
                if (i == 0 || IsDistinct(img, images[images.Count-1]))
                {
                    images.Add(img);
                }
            }
            // Assemble le tout
            using (var image = new Image<Rgba32>(null, 10000, 10000))
            {
                int num = 0;
                Point point = new Point(-1, 0); // Léger décalage à cause de la zone utile retenue
                for (int x = 0; x < 7; ++x)
                {
                    point.Y = image.Height - 2; // Léger décalage à cause de la zone utile retenue
                    for (int y = 0; y < 11; ++y)
                    {
                        var crop = rect; 
                        if (y == 10)
                        {
                            crop = rect2;
                        }
                        point.Y -= crop.Height;
                        images[num].Mutate(x => x.Crop(crop));
                        image.Mutate(i => i.DrawImage(images[num], point, 1f));
                        num++;
                    }
                    point.X += rect.Width;
                }

                image.Save("map.png");
            }
        }

        private static bool IsDistinct(Image<Rgba32> a, Image<Rgba32> b)
        {
            var distinct = 0;

            for(int x = rect.X; x < rect.X + rect.Width; ++x)
            {
                for (int y = rect.Y; y < rect.Y + rect.Height; ++y)
                {
                    if (a[x,y] != b[x,y] && (a[x,y].ToVector4() - b[x, y].ToVector4()).LengthSquared() > 1)
                    {
                        distinct++;

                        if (distinct > 100)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
