using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes
{

   partial class SceneLoader
   {

      /// <summary>
      /// Загрузчик файлов моделей формата DFF (RenderWare)
      /// Информация о формате:
      /// http://www.gtamodding.com/index.php?title=RenderWare_binary_stream_file
      /// http://www.chronetal.co.uk/gta/index.php?page=dff#GeometryList.MaterialSplit
      /// </summary>
      class DffLoader
      {

         enum SectionType
         {
            Undefined = 0,
            Data = 1,
            String = 2,
            Extension = 3,
            Texture = 6,
            Material = 7,
            MaterialList = 8,
            FrameList = 14,
            Geometry = 15,
            Clump = 16,
            Atomic = 20,
            GeometryList = 26,
            MaterialSplit = 1294,
            Frame = 39056126,
         }


         enum SectionFlags
         {
            HasTextureCoords = 4,
            HasColorInfo = 8,
            HasNormalsInfo = 16,
         }


         enum DffVersion
         {
            GTA3_1 = 0,
            GTA3_2 = 2048,
            GTA3_3 = 65400,
            GTA_VC_1 = 3074,
            GTA_VC_2 = 4099,
            GTA_SA = 6147,
         }


         class SectionHeader
         {
            public SectionType SectionType{ get; private set; }
            public int SectionSize { get; private set; }
            public DffVersion Version { get; private set; }

            public void Read(BinaryReader reader)
            {
               SectionType = (SectionType)reader.ReadInt32();
               SectionSize = reader.ReadInt32();
               reader.BaseStream.Seek(sizeof(short), SeekOrigin.Current); // пропускаем "unknown"
               Version = (DffVersion)reader.ReadInt16();
            }
         }


         private string fileName;
         BinaryReader input;

         private List<string> textures = new List<string>();


         public DffLoader(string fileName)
         {
            this.fileName = fileName;
         }


         public void Load()
         {
            using (Log.Instance.EnterStage("Loading model from DFF file: " + fileName))
            {
               using (input = new BinaryReader(new FileStream(fileName, FileMode.Open)))
               {
                  SectionHeader header = new SectionHeader();
                  header.Read(input);

                  if (!Enum.IsDefined(typeof(DffVersion), header.Version))
                     TerminateWithError("Unknown dff file version: " + header.Version);

                  Log.Instance.Print("Root version: " + header.Version);

                  ProcessRenderwareSection(header.SectionSize, 0, header.SectionType);
               }
            }
         }


         private void ProcessRenderwareSection(int size, int depthLevel, SectionType parentType) // depthLevel нужен для отладки, если мы вдруг захочется вывести структуру файла
         {
            int sectionEnd = (int)input.BaseStream.Position + size;

            while (input.BaseStream.Position < sectionEnd)
            {
               SectionHeader header = new SectionHeader();
               header.Read(input);

               switch(header.SectionType)
               {
                  case SectionType.Extension:
                  case SectionType.Texture:
                  case SectionType.Material:
                  case SectionType.MaterialList:
                  case SectionType.FrameList:
                  case SectionType.Geometry:
                  case SectionType.Clump:
                  case SectionType.Atomic:
                  case SectionType.GeometryList:
                     ProcessRenderwareSection(header.SectionSize, depthLevel + 1, header.SectionType);
                     break;

                  case SectionType.MaterialSplit:
                     // Пока MaterialSplit никак не обрабатывается
                     // Не очень понятно, нужно ли это для отображения статического игрового мира
                     // возможно, придётся разобраться и реализовать
                     input.BaseStream.Seek(header.SectionSize, SeekOrigin.Current);
                     break;

                  case SectionType.Data:
                     ParseDataSection(header.SectionSize, parentType, header.Version);
                     break;

                  case SectionType.String:
                     ParseStringSection(header.SectionSize, parentType);
                     break;

                  default:
                     input.BaseStream.Seek(header.SectionSize, SeekOrigin.Current);
                     break;
               }
            }
         }


         private void ParseDataSection(int sectionSize, SectionType parent, DffVersion version)
         {
            switch(parent)
            {
               case SectionType.Geometry:
                  ParseGeometry(sectionSize, parent, version);
                  break;

               default:
                  input.BaseStream.Seek(sectionSize, SeekOrigin.Current);
                  Log.Instance.Print("ParseDataSection was invoked for unknown section", MessageType.Warning);
                  break;
            }
         }


         private void ParseStringSection(int size, SectionType parentType)
         {
            string data = Encoding.ASCII.GetString(input.ReadBytes(size));
            if (parentType == SectionType.Texture)
            {
               if (data.Length > 0)
               {
                  
               } else 
               {
                  Log.Instance.Print("String section of texture information is empty", MessageType.Warning);
               }
            }
         }


         private void ParseGeometry(int sectionSize, SectionType parent, DffVersion version)
         {

         }
      }

   }
}
