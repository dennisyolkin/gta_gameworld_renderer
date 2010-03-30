using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Scenes
{

   /// <summary>
   /// Трёхмерная модель в пространстве
   /// </summary>
   class Model3D
   {
      private List<ModelMesh3D> meshes = new List<ModelMesh3D>();


      /// <summary>
      /// Добавление меша в модель
      /// </summary>
      /// <param name="mesh">меш</param>
      public void AddMesh(ModelMesh3D mesh)
      {
         meshes.Add(mesh);
      }


      /// <summary>
      /// Отрисовка модели, используя заданный эффект
      /// </summary>
      /// <param name="effect">эффект</param>
      /// <param name="worldMatrix">Матрица, определяющая местоположение и поворот объекта. Перекрывает значение свойства WorldMatrix</param>
      public void Draw(Effect effect, Matrix worldMatrix)
      {
         foreach (var mesh in meshes)
            mesh.Draw(effect, worldMatrix);
      }
   }


   /// <summary>
   /// Составлябщая часть меша (ModelMesh3D).
   /// Обычно используется, когда в разных частях меша накладываются разные материалы.
   /// Описывает начальный индекс в индекс-буфере меша и количество индексов, а также номер материала
   /// </summary>
   struct ModelMeshPart3D
   {
      public int StartIdx { get; private set; }
      public int PrimitivesCount { get; private set; }
      public int MaterialId { get; private set; }

      public ModelMeshPart3D(int startIdx, int primitivesCount, int materialId) : this()
      {
         this.StartIdx = startIdx;
         this.PrimitivesCount = primitivesCount;
         this.MaterialId = materialId;
      }
   }


   /// <summary>
   /// Часть (mesh) трёхмерной модели в пространстве
   /// </summary>
   class ModelMesh3D
   {
      private readonly VertexDeclaration vertexDeclaration;
      private readonly VertexBuffer vertexBuffer;
      private readonly IndexBuffer indexBuffer;
      private readonly int vertexSize;
      private readonly int verticesCount;
      private readonly bool triangleStrip;
      private readonly List<Material> materials;
      private readonly List<ModelMeshPart3D> meshParts;

      public ModelMesh3D(VertexDeclaration vertexDeclaration, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, bool triangleStrip,
                           int vertexSize, List<Material> materials, List<ModelMeshPart3D> meshParts)
      {
         this.vertexDeclaration = vertexDeclaration;
         this.vertexBuffer = vertexBuffer;
         this.indexBuffer = indexBuffer;
         this.vertexSize = vertexSize;
         this.triangleStrip = triangleStrip;
         this.materials = materials;
         this.meshParts = meshParts;

         verticesCount = this.vertexBuffer.SizeInBytes / vertexSize;
      }



      /// <summary>
      /// Отрисовывает объект
      /// </summary>
      /// <param name="effect">Эффект, который должен испльзоваться для отрисовки. Предполагается, что как минимум матрицы xProjection и xView уже заданы!</param>
      public void Draw(Effect effect, Matrix worldMatrix)
      {
         GraphicsDevice device = vertexDeclaration.GraphicsDevice;
         device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;//None;

         effect.Parameters["xWorld"].SetValue(worldMatrix);

         device.VertexDeclaration = vertexDeclaration;
         device.Vertices[0].SetSource(vertexBuffer, 0, vertexSize);
         device.Indices = indexBuffer;

         foreach (ModelMeshPart3D part in meshParts)
         {
            Material mat = materials[part.MaterialId];

            if (mat.Texture != null)
            {
               effect.CurrentTechnique = effect.Techniques["Textured"];
               effect.Parameters["xTexture"].SetValue(mat.Texture);
            }
            else
            {
               effect.CurrentTechnique = effect.Techniques["Colored"];
               effect.Parameters["xSolidColor"].SetValue(mat.Color.ToVector4());
            }

            effect.Begin();
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
               pass.Begin();

               device.DrawIndexedPrimitives(triangleStrip ? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList, 0, 0,
                  verticesCount, part.StartIdx, part.PrimitivesCount);

               pass.End();
            }
            effect.End();
         }

      }


   }
}
