using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GTAWorldRenderer.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Scenes.Loaders
{

   class GTATextureLoader
   {

      enum RasterFormat
      {
         Default = 0x0000,
         R5_G5_B5_A1 = 0x0100,
         R5_G6_B5 = 0x0200,
         R4_G4_B4_A4 = 0x0300,
         LUM8 = 0x0400, // (gray scale)
         R8_G8_B8_A8 = 0x0500,
         R8_G8_B8 = 0x0600,
         fmt555 = 0x0A00, // (RGB 5 bits each - rare, use 565 instead)
      }

      enum RasterFormatEx
      {
         Default = 0,
         AutoMipMap = 0x1000, // (RW generates mipmaps)
         Pal8 = 0x2000, // (2^8 = 256 palette colors)
         Pal4 = 0x4000, // (2^4 = 16 palette colors)
         MipMap = 0x8000, // (mipmaps included)
      }


      private static void UnsopportedRasterFormat(string fmt)
      {
         string msg = "Unsopported raster format: " + fmt;
         Log.Instance.Print(msg, MessageType.Error);
         throw new NotSupportedException(msg);
      }

      
      /// <summary>
      /// Структура заголовка
      /// </summary>
      static class HeaderFieldOffset
      {
         public const int RasterFormat = 0; // uint32
         public const int AlphaOrFourCC = 4; // uint32
         public const int ImageWidth = 8; // uint16
         public const int ImageHeight = 10; // uint16
         public const int BitsPerPixel = 12; // byte
         public const int MipMapCount = 13; // byte
         public const int RasterType = 14; // byte
         public const int DxtCompressionType = 15; // byte
      }
      const int HEADER_SIZE = 16;

      class Header
      {
         public int RasterFormatSource { get; private set; }
         public RasterFormat RasterFormat { get; private set; }
         public RasterFormatEx RasterFormatEx { get; private set; }
         public byte BitsPerPixel { get; private set; }
         public byte DXTnumber { get; private set; }
         public int AlphaUsed { get; private set; }
         public short ImageWidth { get; private set; }
         public short ImageHeight { get; private set; }
         public byte MipMaps { get; private set; }

         public Header(BinaryReader reader)
         {
            RasterFormatSource = reader.ReadInt32();

            RasterFormat = (RasterFormat)(RasterFormatSource % 0x1000);
            RasterFormatEx = (RasterFormatEx)(RasterFormatSource - RasterFormatSource % 0x1000);

            AlphaUsed = reader.ReadInt32();
            ImageWidth = reader.ReadInt16();

            ImageHeight = reader.ReadInt16();
            BitsPerPixel = reader.ReadByte();
            MipMaps = reader.ReadByte();

            reader.BaseStream.Seek(HeaderFieldOffset.DxtCompressionType - HeaderFieldOffset.RasterType, SeekOrigin.Current);
            DXTnumber = reader.ReadByte();

            if (!Enum.IsDefined(typeof(RasterFormat), RasterFormat) || !Enum.IsDefined(typeof(RasterFormatEx), RasterFormatEx))
               UnsopportedRasterFormat(RasterFormatSource.ToString("x"));
         }
      }


      BinaryReader reader;
      Header header;
      SurfaceFormat format;
      Color[] palette;


      public GTATextureLoader(byte[] data)
      {
         this.reader = new BinaryReader(new MemoryStream(data));
      }


      public GTATextureLoader(BinaryReader reader)
      {
         this.reader = reader;
      }


      public Texture2D Load()
      {
         header = new Header(reader);

         bool shouldUsePalette = ((header.RasterFormat == RasterFormat.R8_G8_B8_A8 || header.RasterFormat == RasterFormat.R8_G8_B8)
                                    && header.RasterFormatEx == RasterFormatEx.Pal8) ||
                                    (header.BitsPerPixel == 8); // Последнее условие - WORKAROUND в GTAIII, без него не грузится \txd\mainsc1.txd
         
         if (shouldUsePalette)
            ReadPalette(reader, HEADER_SIZE);

         switch (header.RasterFormat)
         {
            case RasterFormat.R5_G5_B5_A1:
               format = header.DXTnumber == 0 ? SurfaceFormat.Bgra5551 : SurfaceFormat.Dxt1;
               break;

            case RasterFormat.R5_G6_B5:
               format = SurfaceFormat.Dxt1;
               break;

            case RasterFormat.R4_G4_B4_A4:
               format = SurfaceFormat.Dxt3;
               break;

            case RasterFormat.R8_G8_B8_A8:
               format = SurfaceFormat.Color;
               break;

            case RasterFormat.R8_G8_B8:
               format = SurfaceFormat.Color;
               break;
         }

         int dataSize = reader.ReadInt32();

         Texture2D texture = new Texture2D(GraphicsDeviceHolder.Device, header.ImageWidth, header.ImageHeight, header.MipMaps, TextureUsage.None, format);

         if (shouldUsePalette)
         {
            Color[] imageData = new Color[header.ImageWidth * header.ImageHeight];

            for (int i = 0; i < header.ImageHeight; ++i)
            {
               for (int j = 0; j < header.ImageWidth; ++j)
               {
                  int paletteIndex = reader.ReadByte();
                  imageData[i * header.ImageWidth + j] = palette[paletteIndex];
               }
            }
            texture.SetData(imageData);
         }
         else
         {
            byte[] chunk = reader.ReadBytes(dataSize);
            texture.SetData(chunk);
            for (int i = 1; i < header.MipMaps; ++i)
            {
               int size = reader.ReadInt32();
               if (size == 0)
                  continue;
               chunk = reader.ReadBytes(size);
               texture.SetData(i, null, chunk, 0, size, SetDataOptions.Discard);
            }
         }

         return texture;
      }


      private void ReadPalette(BinaryReader reader, int startIdx)
      {
         palette = new Color[256];
         for (int i = 0; i != 256; ++i)
         {
            var tmp = new byte[4];
            for (int j = 0; j != 4; ++j)
               tmp[j] = reader.ReadByte();
            palette[i] = new Color(tmp[0], tmp[1], tmp[2], tmp[3]);
         }
      }

   }

}
