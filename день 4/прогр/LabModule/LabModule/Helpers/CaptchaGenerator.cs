using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LabModule.Helpers
{
    public static class CaptchaGenerator
    {
        private static readonly Random Rnd = new Random();
        private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static (string Text, BitmapImage Image) Generate()
        {
            const int width = 120, height = 40;
            string text = new string(Enumerable.Repeat(Chars, 5).Select(s => s[Rnd.Next(s.Length)]).ToArray());

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, width, height));

                for (int i = 0; i < 4; i++)
                    dc.DrawLine(new Pen(Brushes.Gray, 1.5),
                        new Point(Rnd.Next(width), Rnd.Next(height)),
                        new Point(Rnd.Next(width), Rnd.Next(height)));

                var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Arial"), 18, Brushes.Black, 1.0);
                double x = (width - ft.Width) / 2;
                double y = (height - ft.Height) / 2 + ft.Baseline / 2;
                dc.DrawText(ft, new Point(x, y));
            }

            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using var ms = new MemoryStream();
            encoder.Save(ms);
            ms.Position = 0;

            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = ms;
            img.EndInit();
            img.Freeze();

            return (text, img);
        }
    }
}