using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes
{

   class GTATexture
   {

      class UnsopportedRasterFormatException : ApplicationException
      {
         public UnsopportedRasterFormatException(string message)
            : base(message)
         {
         }
      }


      enum RasterFormat
      {
          Default = 0x0000,
          fmt1555 = 0x0100, // (1 bit alpha, RGB 5 bits each; also used for DXT1 with alpha)
          fmt565 = 0x0200, // (5 bits red, 6 bits green, 5 bits blue; also used for DXT1 without alpha)
          fmt4444 = 0x0300, // (RGBA 4 bits each; also used for DXT3)
          LUM8 = 0x0400, // (gray scale)
          fmt8888 = 0x0500, // (RGBA 8 bits each)
          fmt888 = 0x0600, // (RGB 8 bits each)
          fmt555 = 0x0A00, // (RGB 5 bits each - rare, use 565 instead)
          EXT_AUTO_MIPMAP = 0x1000, // (RW generates mipmaps)
          EXT_PAL8 = 0x2000, // (2^8 = 256 palette colors)
          EXT_PAL4 = 0x4000, // (2^4 = 16 palette colors)
          EXT_MIPMAP = 0x8000, // (mipmaps included)
      }


/*
 * Header structure:
 used | type |     field                  | offset |
-----------------------------------------------------
    -    u32      platformId                 0
    -    u16      filterFlags                4
    -    u8       textureWrapV               6
    -    u8       textureWrapU               7
    -    u8       diffuseTextureName[32]     8
    -    u8       alphaTextureName[32]       9
    +    u32      rasterFormat               10
    +    u32      alphaOrFourCC              14
    +    u16      imageWidth                 18
    +    u16      imageHeight                20
    -    u8       bitsPerPixel               22
    -    u8       mipMapCount                23
    -    u8       rasterType                 24
    +    u8       dxtCompressionType         25
 * 
 * 
 * Header size: 26 bytes
*/

      private static int HEADER_SIZE = 26;


      private static void UnsopportedRasterFormat(string fmt)
      {
         string msg = "Unsopported raster format: " + fmt;
         Log.Instance.Print(msg, MessageType.Error);
         throw new NotSupportedException(msg);
      }


      private static void LoadFromBytes(byte[] data)
      {
         // see table above for offset information
         int rasterFormatSource = BitConverter.ToInt32(data, 10);
         int[] alphaOrFourCC = { data[14], data[15], data[16], data[17] };
         
         short imageWidth = BitConverter.ToInt16(data, 18);
         short imageHeight = BitConverter.ToInt16(data, 20);
         byte dxtCompressionType = data[25];

         RasterFormat rasterFormat = (RasterFormat)(rasterFormatSource % 0x1000);
         RasterFormat rasterFormatExt = (RasterFormat)(rasterFormatSource - rasterFormatSource % 0x1000);

         // ECOLOR_FORMAT colorFormat = ECF_A8R8G8B8;
         byte bytesPerPixel = 0;
         ParseRasterFormat(rasterFormat, dxtCompressionType, out bytesPerPixel);

         if (alphaOrFourCC[0] == 'D' && alphaOrFourCC[1] == 'X' && alphaOrFourCC[2] == 'T')
         {
            if (alphaOrFourCC[3] == '1') 
               dxtCompressionType = 1;
            else if (alphaOrFourCC[3] == '3')
               dxtCompressionType = 3;
            else if (alphaOrFourCC[3] == '5')
               dxtCompressionType = 5;
         }

         byte[] image;
         if (rasterFormatExt == RasterFormat.EXT_PAL8 || rasterFormatExt == RasterFormat.EXT_PAL4)
            image = ProcessPaletted(data, rasterFormatExt, imageWidth, imageHeight, bytesPerPixel);
         else if (dxtCompressionType != 0)
            image = ProcessCompressed(data, dxtCompressionType, imageWidth, imageHeight, bytesPerPixel);
         else
            image = ProcessUncompressedNonPaletted(data, imageWidth, imageHeight, bytesPerPixel);

         // IImage *image = Device->getVideoDriver()->createImageFromData(colorFormat, core::dimension2d<u32>(header.imageWidth, header.imageHeight), data);
      }


      private static void ParseRasterFormat(RasterFormat rasterFormat, byte dxtCompressionType, out byte bytesPerPixel)
      {
         /*
          * Возможно, придётся нужно будет добавить параметры:
          * [in]  RasterFormatEx
          * [out] colorFormat
          */
         bytesPerPixel = 0;
         switch (rasterFormat)
         {
            case RasterFormat.Default:
               //colorFormat = ECF_A8R8G8B8;
               bytesPerPixel = 4;
               break;

            case RasterFormat.fmt1555:
               //colorFormat = ECF_A1R5G5B5;
               bytesPerPixel = 2;
               break;

            case RasterFormat.fmt565:
               //colorFormat = ECF_R5G6B5;
               bytesPerPixel = 2;
               break;

            case RasterFormat.fmt4444:
               UnsopportedRasterFormat("Format 4444");
               break;

            case RasterFormat.LUM8:
               UnsopportedRasterFormat("LUM8");
               break;

            case RasterFormat.fmt8888:
               //colorFormat = ECF_A8R8G8B8;
               bytesPerPixel = 4;
               break;

            case RasterFormat.fmt888:
               //colorFormat = ECF_R8G8B8;
               bytesPerPixel = 3;
               break;

            case RasterFormat.fmt555:
               //colorFormat = ECF_R5G6B5;
               bytesPerPixel = 2;
               break;

            default:
               UnsopportedRasterFormat("Unknown Format");
               break;
         }

         //     if (rasterFormatExt == FORMAT_EXT_AUTO_MIPMAP) // auto mipmaps
         //else if (rasterFormatExt == FORMAT_EXT_MIPMAP) // mipmaps included

         if (dxtCompressionType != 0)
         {
            //colorFormat = ECF_A8R8G8B8;
            bytesPerPixel = 4;
         }
      }


      private static byte[] ProcessPaletted(byte[] data, RasterFormat rasterFormatExt, short imgWidth, short imgHeight, byte bytesPerPixel)
      {
         byte[] image = new byte[imgWidth * imgHeight * bytesPerPixel];
         int paletteSize = rasterFormatExt == RasterFormat.EXT_PAL4 ? 16 : 256;
         byte[] palette = new byte[paletteSize * 4];
         int readoffset = 0, writeoffset = 0;

         for (readoffset = 0; readoffset < palette.Length; ++readoffset)
            palette[readoffset] = data[HEADER_SIZE + readoffset];

         readoffset += 4; // skip 4 bytes of unneeded raster size data

         for (; writeoffset < image.Length; ++readoffset)
         {
            for (int i = 0; i != bytesPerPixel; ++i)
               image[writeoffset++] = palette[(data[HEADER_SIZE + readoffset] * 4) + i];
         }
         return image;
      }

      private static byte[] ProcessCompressed(byte[] data, byte dxtCompressionType, short imgWidth, short imgHeight, byte bytesPerPixel)
      {
         byte[] image = new byte[imgWidth * imgHeight * bytesPerPixel];
         /*
            if(dxtCompressionType == 1)
               DecompressImage(data, imageWidth, imageHeight, data + HEADER_SIZE + 4, squish::kDxt1);
            else if(dxtCompressionType == 3)
               DecompressImage(data, header.imageWidth, header.imageHeight, data + HEADER_SIZE + 4, squish::kDxt3);
            else if(dxtCompressionType == 5)
               DecompressImage(data, header.imageWidth, header.imageHeight, data + HEADER_SIZE + 4, squish::kDxt5);
            */
         return image;
      }


      private static byte[] ProcessUncompressedNonPaletted(byte[] data, short imgWidth, short imgHeight, byte bytesPerPixel)
      {
         // don't even know if uncompressed non-palette data exists, but i support it just in case
         byte[] image = new byte[imgWidth * imgHeight * bytesPerPixel];
         int readoffset = 0, writeoffset = 0;
         readoffset += 4; // skip 4 bytes of unneeded raster size data
         for(; writeoffset < image.Length; ++writeoffset, ++readoffset)
            image[writeoffset++] = data[readoffset++];
         return image;
      }


      public static void LoadFromFile(string fileName, int offset, int size)
      {
         using (Log.Instance.EnterStage(String.Format("Loading texture from {0}, offset {1}, size {2}", fileName, offset, size)))
         {
            using (BinaryReader fin = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
               byte[] data = new byte[fin.BaseStream.Length];
               LoadFromBytes(data);
            }
         }
      }

   }
}
