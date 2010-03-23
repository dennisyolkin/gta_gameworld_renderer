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
   /// Часть (mesh) трёхмерной модели в пространстве
   /// </summary>
   class ModelMesh3D
   {
      private VertexDeclaration vertexDeclaration;
      private VertexBuffer vertexBuffer;
      private IndexBuffer indexBuffer;
      private int vertexSize;
      private int indicesCount, verticesCount;
      private string effectTechnique;
      private bool triangleStrip;


      public ModelMesh3D(VertexDeclaration vertexDeclaration, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, bool triangleStrip,
                           int vertexSize, string effectTechnique)
      {
         this.vertexDeclaration = vertexDeclaration;
         this.vertexBuffer = vertexBuffer;
         this.indexBuffer = indexBuffer;
         this.vertexSize = vertexSize;
         this.effectTechnique = effectTechnique;
         this.triangleStrip = triangleStrip;

         indicesCount = this.indexBuffer.SizeInBytes / sizeof(short);
         verticesCount = this.vertexBuffer.SizeInBytes / vertexSize;
      }



      /// <summary>
      /// Отрисовывает объект
      /// </summary>
      /// <param name="effect">Эффект, который должен испльзоваться для отрисовки. Предполагается, что как минимум матрицы xProjection и xView уже заданы!</param>
      public void Draw(Effect effect, Matrix worldMatrix)
      {
         GraphicsDevice device = vertexDeclaration.GraphicsDevice;

         /*
          * TODO :: 
          * Судя по всему, при TriangleStrip некоторые треугольники неправильно завёрнуты.
          * Поэтому для того, чтобы все отрисовывалось корректно, для рисования TriangleStrip-мешей
          * FaceCulling отключается.
          */
         //if (triangleStrip)
         //   device.RenderState.CullMode = CullMode.None;
         //device.RenderState.CullMode = CullMode.CullClockwiseFace;

         bool one = indicesCount == 525;

         device.RenderState.CullMode = CullMode.None;

         effect.CurrentTechnique = effect.Techniques[effectTechnique];
         effect.Parameters["xWorld"].SetValue(worldMatrix);
         effect.Begin();
         foreach (var pass in effect.CurrentTechnique.Passes)
         {
            pass.Begin();
            device.VertexDeclaration = vertexDeclaration;
            device.Vertices[0].SetSource(vertexBuffer, 0, vertexSize);
            device.Indices = indexBuffer;

            device.DrawIndexedPrimitives(triangleStrip? PrimitiveType.TriangleStrip : PrimitiveType.TriangleList, 0, 0, verticesCount, 0, indicesCount / 3);
            pass.End();
         }
         effect.End();

         // Возвращаем FaceCulling в исходное положение
         //if (triangleStrip)
         //   device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
      }
   }
}
