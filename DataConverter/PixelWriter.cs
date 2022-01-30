using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aijkl.VRChat.MutualCommunication
{
    public class PixelWriter
    {
        private readonly SKBitmap skBitmap;
        private readonly SKColor BIT_TRUE = SKColors.Black;
        private readonly SKColor BIT_FALSE = SKColors.White;
        public PixelWriter(SKBitmap skBitmap)
        {
            this.skBitmap = skBitmap;
            Width = skBitmap.Width;
            Height = skBitmap.Height;

            SKCanvas skCanvas = new SKCanvas(skBitmap);
            skCanvas.Clear(BIT_FALSE);
        }
        public int Width { private set; get; }
        public int Height { private set; get; }
        public int X { private set; get; }
        public int Y { private set; get; }
        public void WriteBytes(byte[] bytes)
        {            
            foreach (var bytevalue in bytes)
            {
                WriteByte(bytevalue);
            }            
        }        
        public void WriteUint16(ushort value)
        {
            WriteBitList(ConvertToBitList(value));
        }        
        public void WriteByte(byte value)
        {
            List<bool> bitList = ConvertToBitList(value);
            //Console.WriteLine(string.Join(string.Empty, bitList.Select(x => x == true ? "1" : "0")));
            WriteBitList(bitList);
        }
        public void LowerLine()
        {
            X = 0;
            Y++;
        }
        public void Seek(int x, int y)
        {
            if (x >= Width || y >= Height)
            {
                throw new ArgumentException();
            }

            X = x;
            Y = y;
        }
        public void WritePixel(bool bit)
        {
            skBitmap.SetPixel(X, Y, bit == true ? BIT_TRUE : BIT_FALSE);
        }
        public void WriteBitList(List<bool> bitList)
        {
            foreach (var bit in bitList)
            {
                if (X == Width)
                {
                    LowerLine();
                }
                WritePixel(bit);
                X++;
            }
        }
        public List<bool> ConvertToBitList(byte value)
        {
            return Convert.ToString(value, 2).PadLeft(8, '0').Select(x => x == '1').ToList();
        }
        public List<bool> ConvertToBitList(ushort value)
        {
            return Convert.ToString(value, 2).PadLeft(16, '0').Select(x => x == '1').ToList();
        }
    }
}
