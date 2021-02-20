using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ConsoleApp1
{
    class Program
    {
        static Rectangle rect = new Rectangle(214, 190, 1491, 746);

        static Rectangle rect2 = new Rectangle(214, 555, 1491, 223);

        static Rectangle rect3 = new Rectangle(745, 190, 223, 746);

        static Rectangle rect4 = new Rectangle(745, 555, 223, 223);

        static void Main(string[] args)
        {
            // Recherche les captures d'écran différentes sur la zone utile
            var images = new List<Image<Rgba32>>();
            for(int i =0;i<400;++i)
            {
                var filename = @$"C:\Users\Julien\source\repos\jetelain\Arma3Map\MapExporter\MakeScreenShots\bin\Debug\{i}.png";
                if (File.Exists(filename))
                {
                    var img = (Image<Rgba32>)SixLabors.ImageSharp.Image.Load(filename);
                    /*if (i == 0 || IsDistinct(img, images[images.Count - 1]))
                    {*/
                        images.Add(img);
                    /*}*/
                }
            }
            // Assemble le tout
            using (var image = new Image<Rgba32>(null, 10000, 10000))
            {
                int num = 0;
                Point point = new Point(0, 0); // Léger décalage à cause de la zone utile retenue
                for (int x = 0; x < 7; ++x)
                {
                    point.Y = image.Height; // Léger décalage à cause de la zone utile retenue
                    for (int y = 0; y < 13; ++y)
                    {
                        var crop = rect; 
                        if ( y==12 && x==6)
                        {
                            crop = rect4;
                        }
                        else if (y == 12)
                        {
                            crop = rect2;
                        }
                        else if(x == 6)
                        {
                            crop = rect3;
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
