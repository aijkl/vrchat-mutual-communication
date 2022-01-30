using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aijkl.VRChat.MutualCommunication
{
    public static class PixelConverter
    {
        public enum ContentType
        {
            PlainText,
            Base64
        }
        public static SKBitmap ConvertToBitmap(byte[] bytes, int width, int height, ContentType contentType = ContentType.PlainText)
        {
            if (bytes.Length > ushort.MaxValue && bytes.Length < (width * height) - width)
            {
                throw new ArgumentException();
            }            

            SKBitmap skBitmap = new SKBitmap(width, height);            

            PixelWriter pixelWriter = new PixelWriter(skBitmap);
            pixelWriter.WriteByte((byte)contentType);
            pixelWriter.WriteUint16((ushort)bytes.Length);            
            pixelWriter.WriteBytes(bytes);            

            return skBitmap;
        }
    }
}
