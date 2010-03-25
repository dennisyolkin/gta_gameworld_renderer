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
         ModelData modelData = new ModelData();

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
                  // К сожалению, в GTA III есть dff-файлы размера 0
                  if (input.BaseStream.Length > 0)
                  {
                     SectionHeader header = new SectionHeader();
                     header.Read(input);
                     ProcessRenderwareSection(header.SectionSize, 0, header.SectionType);
                  }
               }
               Log.Instance.Print("Model loaded!");

               return Model3dFactory.CreateModel(modelData);
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
                     ParseMaterialSplit(header.SectionSize);
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
                  ParseGeometry(version);
                  break;

               default:
                  input.BaseStream.Seek(sectionSize, SeekOrigin.Current);
                  break;
            }
         }


         private void ParseStringSection(int size, SectionType parentType)
         {
            byte[] data = input.ReadBytes(size);
            if (parentType == SectionType.Texture)
            {
               int len = Array.IndexOf(data, (byte)0);
               if (len == -1)
                  len = data.Length;
               if (len == 0)
                  return;

               string texture = Encoding.ASCII.GetString(data, 0, len);
               ModelMeshData mesh = modelData.Meshes[modelData.Meshes.Count - 1];
               mesh.Textures.Add(texture);
            }
         }


         /// <summary>
         /// Обрабатывает секцию Geometry
         /// Считывает вершины, треугольники (индексы вершин), нормали, текстурные координаты.
         /// </summary>
         private void ParseGeometry(DffVersion version)
         {
            ModelMeshData mesh = new ModelMeshData();

            GeometrySectionFlags flags = (GeometrySectionFlags)input.ReadInt16();

            input.BaseStream.Seek(sizeof(short), SeekOrigin.Current); // unknown

            int trianglesCount = input.ReadInt32();
            int verticesCount = input.ReadInt32();
            input.BaseStream.Seek(sizeof(int), SeekOrigin.Current); // morphTargetCount aka frameCount

            if (version < DffVersion.GTA_VC_2)
            {
               // geometry has lighting data. Ignoring it. 
               // TODO :: we can use it for rendering!!!
               input.BaseStream.Seek(12, SeekOrigin.Current);
            }

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
               ReadTextureCoords(mesh, verticesCount);

            ReadTriangles(mesh, trianglesCount);

            input.BaseStream.Seek(4 * sizeof(float), SeekOrigin.Current); // ignoring bounding sphere (x, y, z, radius)
            input.BaseStream.Seek(2 * sizeof(int), SeekOrigin.Current); // hasPosition, hasNormal (not used)

            ReadVertices(mesh, verticesCount);

            if ((flags & GeometrySectionFlags.HasNormalsInfo) != GeometrySectionFlags.None)
               ReadNormals(mesh, verticesCount);

            modelData.Meshes.Add(mesh);
         }


         private void ReadTextureCoords(ModelMeshData mesh, int verticesCount)
         {
            mesh.TextureCoords = new List<Vector2>(verticesCount);
            for (var i = 0; i != verticesCount; ++i)
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

               mesh.TextureCoords.Add(new Vector2(x, y));
            }
         }


         private void ReadTriangles(ModelMeshData mesh, int trianglesCount)
         {
            mesh.Indices = new List<short>(trianglesCount * 3);

            for (var i = 0; i != trianglesCount; ++i)
            {
               mesh.Indices.Add(input.ReadInt16());
               mesh.Indices.Add(input.ReadInt16());
               input.BaseStream.Seek(sizeof(short), SeekOrigin.Current); // skip index flags
               mesh.Indices.Add(input.ReadInt16());

            }
         }


         private void ReadVertices(ModelMeshData mesh, int verticesCount)
         {
            mesh.Vertices = new List<Vector3>(verticesCount);

            for (var i = 0; i != verticesCount; ++i)
            {
               var x = input.ReadSingle();
               var y = input.ReadSingle(); // y and z coords are exchanged because of different coordinate system
               var z = input.ReadSingle();
               mesh.Vertices.Add(new Vector3(x, z, y));
            }
         }


         private void ReadNormals(ModelMeshData mesh, int verticesCount)
         {
            mesh.Normals = new List<Vector3>(verticesCount);

            for (var i = 0; i != verticesCount; ++i)
            {
               var x = input.ReadSingle();
               var y = input.ReadSingle(); // y and z coords are exchanged because of different coordinate system
               var z = input.ReadSingle();
               mesh.Normals.Add(new Vector3(x, z, y));
            }
         }


         /// <summary>
         /// Обрабатывает секцию MaterialSplit (она же Bin Mesh PLG)
         /// Насколько я понял, эта секция нужна, когда отдельным кускам меша присваивается разный метариел (текстура),
         /// либо когда треугольники перечисляются не в виде TriangleList, а в виде TriangleStrip.
         /// 
         /// Перезаписывает данные в последнем добавленном меше в modelData
         /// </summary>
         private void ParseMaterialSplit(int sectionSize)
         {
            int sectionEnd = (int)input.BaseStream.Position + sectionSize;

            // ParseMaterialSplit вызывается всегда после ParseGeometry, которая добавляет новый mesh в modelData
            ModelMeshData mesh = modelData.Meshes[modelData.Meshes.Count - 1];

            int triangleStrip = input.ReadInt32();
            int splitCount = input.ReadInt32();
            int indicesCount = input.ReadInt32();

            mesh.TriangleStrip = triangleStrip != 0;

            if (triangleStrip == 0)
            {
               // пока что секция MaterialSplit используется только для TriangleStrip
               Log.Instance.Print("MaterialSplit section is provided but ignored because TriangleStrip = 0");
               input.BaseStream.Seek(sectionEnd, SeekOrigin.Begin);
               return;
            }

            Log.Instance.Print("Using TriangleStrip for this mesh!");

            // не знаю пока, как обрабатывать такую ситуацию
            if (splitCount != 1)
               Log.Instance.Print("SplitCount != 1", MessageType.Warning);

            mesh.Indices.Clear(); // перезаписываем данные о треугольниках

            // В настоящее время игнорируется разбиение по материалам, считываются все индексы одним куском
            for (int j = 0; j < splitCount; ++j)
            {
               int localIndicesCount = input.ReadInt32();
               int materialIdx = input.ReadInt32();
               mesh.Indices.Capacity = indicesCount;

               for (int i = 0; i != localIndicesCount; ++i)
                  mesh.Indices.Add((short)input.ReadInt32());
            }
         }



      }


   }
}
