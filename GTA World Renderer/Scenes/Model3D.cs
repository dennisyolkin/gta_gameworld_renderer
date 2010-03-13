using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer.Scenes
{
   /// <summary>
   /// Трёхмерная модель в пространстве
   /// 
   /// Пока предполагается, что VertexDeclaration для всех вершин модели одинаковый.
   /// </summary>
   class Model3D
   {
      private VertexDeclaration vertexDeclaration;
      private VertexBuffer vertexBuffer;
      private IndexBuffer indexBuffer;
      private int vertexSize;
      private int indicesCount, verticesCount;
      private string effectTechnique;

      public Matrix WorldMatrix{ get; set; }


      public Model3D(VertexDeclaration vertexDeclaration, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexSize, string effectTechnique)
      {
         this.vertexDeclaration = vertexDeclaration;
         this.vertexBuffer = vertexBuffer;
         this.indexBuffer = indexBuffer;
         this.vertexSize = vertexSize;
         this.effectTechnique = effectTechnique;

         indicesCount = this.indexBuffer.SizeInBytes / sizeof(short);
         verticesCount = this.vertexBuffer.SizeInBytes / vertexSize;

         Initialize();
      }


      /// <summary>
      /// Инициализация объекта.
      /// Вынесена из конструктора для удобочитаемости и удобокодируемости :)
      /// </summary>
      private void Initialize()
      {
         WorldMatrix = Matrix.Identity;
      }


      /// <summary>
      /// Задание позиции и поворота объекта
      /// </summary>
      /// <param name="position">Позиция</param>
      /// <param name="rotation">Поворот. Задаётся кватернионом</param>
      public void PlaceIn3d(Vector3 position, Quaternion rotation)
      {
         WorldMatrix = Matrix.CreateTranslation(position) * Matrix.CreateFromQuaternion(rotation);
      }


      /// <summary>
      /// Отрисовывает объект
      /// </summary>
      /// <param name="effect">Эффект, который должен испльзоваться для отрисовки. Предполагается, что как минимум матрицы xProjection и xView уже заданы!</param>
      public void Draw(Effect effect)
      {
         GraphicsDevice device = vertexDeclaration.GraphicsDevice;
         effect.CurrentTechnique = effect.Techniques[effectTechnique];
         effect.Parameters["xWorld"].SetValue(WorldMatrix);
         effect.Begin();
         foreach (var pass in effect.CurrentTechnique.Passes)
         {
            pass.Begin();
            device.VertexDeclaration = vertexDeclaration;
            device.Vertices[0].SetSource(vertexBuffer, 0, vertexSize);
            device.Indices = indexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verticesCount, 0, indicesCount);
            pass.End();
         }
         effect.End();
      }
   }
}
