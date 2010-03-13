using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GTAWorldRenderer.Logging;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Collections;

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

         [Flags]
         enum GeometrySectionFlags
         {
            None = 0,
            TrianglesStrip = 1 << 0,
            HasVertexPositions = 1 << 1,
            HasTextureCoords = 1 << 2,
            HasColorInfo = 1 << 3,
            HasNormalsInfo = 1 << 4,
            // unknown = 1 << 5 (GeometryLight)
            // unknown = 1 << 6 
            MultipleTextureCoords = 1 << 7,
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
         ModelData model = new ModelData();

         public DffLoader(string fileName)
         {
            this.fileName = fileName;
         }


         public Model3D Load()
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

               Log.Instance.Print("Model loaded!");
               Log.Instance.Print(model.Info);

               return Model3dFactory.CreateModel(model);
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
                  model.Textures[model.Textures.Count - 1] = data;
               } else 
               {
                  Log.Instance.Print("String section of texture information is empty", MessageType.Warning);
               }
            }
         }


         private void ParseGeometry(int sectionSize, SectionType parent, DffVersion version)
         {
            GeometrySectionFlags flags = (GeometrySectionFlags)input.ReadInt16();
            input.BaseStream.Seek(sizeof(short), SeekOrigin.Current); // unknown
            int trianglesCount = input.ReadInt32();
            int verticesCount = input.ReadInt32();
            input.BaseStream.Seek(sizeof(short), SeekOrigin.Current); // morphTargetCount aka frameCount

            if (version < DffVersion.GTA_VC_2)
            {
               // geometry has lighting data. Ignoring it. 
               // TODO :: we can use it for rendering!!!
               input.BaseStream.Seek(12, SeekOrigin.Current);
            }

            if ((flags & GeometrySectionFlags.TrianglesStrip) != GeometrySectionFlags.None)
               TerminateWithError("Model use triangle strip, but it is not implemented yet");

            if ((flags & GeometrySectionFlags.MultipleTextureCoords) != GeometrySectionFlags.None)
               Log.Instance.Print("Multiple TexCoords sets are provided but used only the first of it!", MessageType.Warning);

            if ((flags & GeometrySectionFlags.HasColorInfo) != GeometrySectionFlags.None)
            {
               // ignoring color info
               // TODO :: we can use it for rendering, if there is no texture
               input.BaseStream.Seek(4 * verticesCount, SeekOrigin.Current);
               Log.Instance.Print("Ignoring color info", MessageType.Warning);
            }

            if ((flags & GeometrySectionFlags.HasTextureCoords) != GeometrySectionFlags.None)
               ReadTextureCoords(verticesCount);

            ReadTriangles(trianglesCount);

            input.BaseStream.Seek(4 * sizeof(float), SeekOrigin.Current); // ignoring bounding sphere (x, y, z, radius)
            input.BaseStream.Seek(2 * sizeof(int), SeekOrigin.Current); // hasPosition, hasNormal (not used)

            ReadVertices(verticesCount);

            // TODO :: возможно, здесь нужно создать нормали, если их нет изначально в файле. А возможно, это нужно делать в шейдере.
            if ((flags & GeometrySectionFlags.HasNormalsInfo) != GeometrySectionFlags.None)
               ReadNormals(verticesCount);
         }


         private void ReadTextureCoords(int verticesCount)
         {
            model.TextureCoords = new List<Vector2>(verticesCount);
            for (var i = 0; i != model.TextureCoords.Count; ++i)
            {
               float x = input.ReadSingle();
               float y = input.ReadSingle();

               // flip x coordinate
               // TODO :: а оно надо?
               /*
               if (x > 0.5f)
                  x = (float)(0.5f + (0.5 - x));
               else
                  x = (float)(0.5f + (x + 0.5));
                */

               model.TextureCoords.Add(new Vector2(x, y));
            }
         }


         private void ReadTriangles(int trianglesCount)
         {
            // TODO :: remove; it is for debugging purposes
            if (model.Indices != null)
               TerminateWithError("DebugAssertionFailed: recreating Faces buffer!");

            model.Indices = new List<short>(trianglesCount * 3);

            for (var i = 0; i != trianglesCount; ++i)
            {
               model.Indices.Add(input.ReadInt16());
               model.Indices.Add(input.ReadInt16());
               input.BaseStream.Seek(sizeof(short), SeekOrigin.Current); // skip index flags
               model.Indices.Add(input.ReadInt16());

            }
         }


         private void ReadVertices(int verticesCount)
         {
            // TODO :: remove; it is for debugging purposes
            if (model.Vertices != null)
               TerminateWithError("DebugAssertionFailed: recreating Vertices buffer!");

            model.Vertices = new List<Vector3>(verticesCount);

            for (var i = 0; i != verticesCount; ++i)
            {
               var x = input.ReadSingle();
               var y = input.ReadSingle();
               var z = input.ReadSingle();
               model.Vertices.Add(new Vector3(x, y, z));
            }
         }


         private void ReadNormals(int verticesCount)
         {
            // TODO :: remove; it is for debugging purposes
            if (model.Normals != null)
               TerminateWithError("DebugAssertionFailed: recreating Normals buffer!");

            model.Normals = new List<Vector3>(verticesCount);

            for (var i = 0; i != verticesCount; ++i)
            {
               var x = input.ReadSingle();
               var y = input.ReadSingle();
               var z = input.ReadSingle();
               model.Normals.Add(new Vector3(x, y, z));
            }
         }

      }


   }
}
