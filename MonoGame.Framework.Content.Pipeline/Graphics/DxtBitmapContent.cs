// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class DxtBitmapContent : BitmapContent
    {
        private byte[] _bitmapData;
        private int _blockSize;
        private SurfaceFormat _format;

        protected DxtBitmapContent(int blockSize)
        {
            if (!((blockSize == 8) || (blockSize == 16)))
                throw new ArgumentException("Invalid block size");
            _blockSize = blockSize;
            TryGetFormat(out _format);
        }

        protected DxtBitmapContent(int blockSize, int width, int height)
            : this(blockSize)
        {
            Width = width;
            Height = height;
        }

        public override byte[] GetPixelData()
        {
            return _bitmapData;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            _bitmapData = sourceData;
        }

        private static bool CheckTransparency(byte[] data)
        {
            bool hasTransparency = false;

            for (var x = 0; x < data.Length; x += 4)
            {
                // Look for non-opaque pixels.
                var alpha = data[x + 3];
                if (alpha < 255)
                    hasTransparency = true;
            }

            return hasTransparency;
        }

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            SurfaceFormat sourceFormat;
            if (!sourceBitmap.TryGetFormat(out sourceFormat))
                return false;

            SurfaceFormat format;
            TryGetFormat(out format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (format == sourceFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
            {
                SetPixelData(sourceBitmap.GetPixelData());
                return true;
            }

            // TODO: Add a XNA unit test to see what it does
            // my guess is that this is invalid for DXT.
            //
            // Destination region copy is not yet supported
            if (destinationRegion != new Rectangle(0, 0, Width, Height))
                return false;

            // If the source is not Vector4 or requires resizing, send it through BitmapContent.Copy
            if (!(sourceBitmap is PixelBitmapContent<Vector4>) || sourceRegion.Width != destinationRegion.Width || sourceRegion.Height != destinationRegion.Height)
            {
                try
                {
                    BitmapContent.Copy(sourceBitmap, sourceRegion, this, destinationRegion);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }


            /*var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
            BitmapContent.Copy(sourceBitmap, colorBitmap);
            var sourceData = colorBitmap.GetPixelData();

            CompressionFormat outputFormat;
            var alphaDither = false;
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1SRgb:
                    {
                        bool hasTransparency = CheckTransparency(sourceData);
                        outputFormat = hasTransparency ?
                            CompressionFormat.Bc1WithAlpha : //DXT1a aka Bc1WithAlpha
                            CompressionFormat.Bc1; //DXT1 aka BC1
                        alphaDither = true;
                        break;
                    }
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                    {
                        outputFormat = CompressionFormat.Bc2; //DXT3 aka BC2
                        break;
                    }
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                    {
                        outputFormat = CompressionFormat.Bc3; //DXT5 aka BC3
                        break;
                    }
                default:
                    throw new InvalidOperationException("Invalid DXT surface format!");
            }


            BcEncoder encoder = new(outputFormat);

            EncoderOutputOptions outputOptions = encoder.OutputOptions;
            outputOptions.Format = outputFormat;
            outputOptions.GenerateMipMaps = false;
            outputOptions.Quality = CompressionQuality.Balanced;

            _bitmapData = encoder.EncodeToRawBytes(
                sourceData,
                colorBitmap.Width,
                colorBitmap.Height,
                PixelFormat.Rgba32)[0]; //We use only the first mip's data*/

            Console.WriteLine(@$"Compressing texture completed:
input:
    bytes = {sourceData.Length},
    size = {sourceBitmap.Width}x{sourceBitmap.Height},
    bytes/pixel = {(sourceData.Length * 1f) / (sourceBitmap.Width * sourceBitmap.Height)},
    format = {sourceFormat}
output:
    bytes = {_bitmapData.Length},
    size = {Width}x{Height},
    bytes/pixel = {(_bitmapData.Length * 1f) / (Width * Height)},
    format = {outputFormat}
");

            return true;
        }

        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            SurfaceFormat destinationFormat;
            if (!destinationBitmap.TryGetFormat(out destinationFormat))
                return false;

            SurfaceFormat format;
            TryGetFormat(out format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            var fullRegion = new Rectangle(0, 0, Width, Height);
            if ((format == destinationFormat) && (sourceRegion == fullRegion) && (sourceRegion == destinationRegion))
            {
                destinationBitmap.SetPixelData(GetPixelData());
                return true;
            }

            // No other support for copying from a DXT texture yet
            return false;
        }
    }
}
