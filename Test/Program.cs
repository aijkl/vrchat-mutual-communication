using Aijkl.VRChat.MutualCommunication;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {           
        static void Main()
        {
            string data = Console.ReadLine();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SKEncodedImageFormat encodedImageFormat = SKEncodedImageFormat.Png;            

            int width = 170;
            int height = 170;
            //2100byteまで安定
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Console.WriteLine(string.Join(",", bytes));


            SKBitmap skBitmap = PixelConverter.ConvertToBitmap(bytes, width, height);

            using (FileStream fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), $"hoge.{encodedImageFormat.ToString().ToLower()}"), FileMode.Create))
            {
                skBitmap.Encode(encodedImageFormat, 100).SaveTo(fileStream);
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }        
    }
}
