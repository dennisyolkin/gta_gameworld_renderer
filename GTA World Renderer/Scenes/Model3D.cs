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
   /// </summary>
   class Model3D
   {
      private List<ModelMesh3D> meshes = new List<ModelMesh3D>();

      /// <summary>
      /// Матрица, задающая положение и поворот объекта в игровом пространстве
      /// </summary>
      public Matrix WorldMatrix{ get; set; }


      public Model3D()
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
      public void Draw(Effect effect)
      {
         foreach (var mesh in meshes)
            mesh.Draw(effect, WorldMatrix);
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


      public ModelMesh3D(VertexDeclaration vertexDeclaration, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexSize, string effectTechnique)
      {
         this.vertexDeclaration = vertexDeclaration;
         this.vertexBuffer = vertexBuffer;
         this.indexBuffer = indexBuffer;
         this.vertexSize = vertexSize;
         this.effectTechnique = effectTechnique;

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
         effect.CurrentTechnique = effect.Techniques[effectTechnique];
         effect.Parameters["xWorld"].SetValue(worldMatrix);
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
