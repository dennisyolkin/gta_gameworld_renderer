using Microsoft.Xna.Framework.Graphics;
using GTAWorldRenderer.Scenes.VertexFormats;
using System.Collections.Generic;

namespace GTAWorldRenderer.Scenes
{
   partial class SceneLoader
   {

      static class Model3dFactory
      {

         /*
          * На первый взгляд (при тестировании в LOD-варианте) показалось, что цвета довольно бесполезны.
          * Константа позволяет отключить использование заданных цветов и заполнять все подели константным цветом
          * 
          * TODO :: вернуться к этому вопросу, когда будет реализовано текстурирование.
          */
         const bool IgnoreModelColors = true;

         public static ModelMesh3D CreateModelMesh(ModelMeshData mesh, string texturesPath)
         {
            /*
             * TODO :: здесь должен быть большоя switch, по которому должен определяться VertexFormat.
             * Но пока поддерживается только простейший VertexPositionNormal формат, поэтому всё просто.
             */

            if (mesh.Normals == null)
               GeometryUtils.EvaluateNormals(mesh);

            if (mesh.Colors == null || IgnoreModelColors)
            {
               // TODO :: временное решение. В дальнейшем либо у модели есть цвета вершин, либо текстура. Не придётся заполнять цвета вручную
               mesh.Colors = new List<Color>(mesh.Vertices.Count);
               for (int i = 0; i != mesh.Vertices.Count; ++i)
                  mesh.Colors.Add(Color.Green);
            }

            // создаём VertexBuffer
            VertexPositionNormalColorFormat[] vertices = new VertexPositionNormalColorFormat[mesh.Vertices.Count];
            for (var i = 0; i != mesh.Vertices.Count; ++i)
               vertices[i] = new VertexPositionNormalColorFormat(mesh.Vertices[i], mesh.Normals[i], mesh.Colors[i]);
            VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDeviceHolder.Device,
               mesh.Vertices.Count * VertexPositionNormalColorFormat.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            // создаём IndexBuffer
            IndexBuffer indexBuffer = new IndexBuffer(GraphicsDeviceHolder.Device, mesh.Indices.Count * sizeof(short),
               BufferUsage.WriteOnly, IndexElementSize.SixteenBits);
            indexBuffer.SetData(mesh.Indices.ToArray());

            var textures = new List<Texture2D>();
            foreach (var textureName in mesh.Textures)
               textures.Add(TexturesStorage.Instance.GetTexture(textureName, texturesPath));

            return new ModelMesh3D(new VertexDeclaration(GraphicsDeviceHolder.Device, VertexPositionNormalColorFormat.VertexElements),
               vertexBuffer, indexBuffer, mesh.TriangleStrip, VertexPositionNormalColorFormat.SizeInBytes, "Default", textures);
         }


         public static Model3D CreateModel(ModelData modelData, string texturesPath)
         {
            Model3D model = new Model3D();
            foreach (var mesh in modelData.Meshes)
               model.AddMesh(CreateModelMesh(mesh, texturesPath));
            return model;
         }

      }

   }
}
