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
            IndexBuffer indexBuffer = new IndexBuffer(GraphicsDeviceHolder.Device, mesh.SumIndicesCount * sizeof(short),
               BufferUsage.WriteOnly, IndexElementSize.SixteenBits);

            var meshParrts3d = new List<ModelMeshPart3D>();
            int offset = 0;
            foreach (ModelMeshPartData part in mesh.MeshParts)
            {
               indexBuffer.SetData(offset * sizeof(short), part.Indices.ToArray(), 0, part.Indices.Count);
               meshParrts3d.Add(new ModelMeshPart3D(offset, mesh.TriangleStrip? part.Indices.Count - 2 : part.Indices.Count / 3, part.MaterialId));
               offset += part.Indices.Count;
            }
            if (offset != mesh.SumIndicesCount)
               TerminateWithError("Incorrect total indices amount!");

            var textures = new List<Texture2D>();
            foreach (var textureName in mesh.Textures)
               textures.Add(TexturesStorage.Instance.GetTexture(textureName, texturesPath));

            return new ModelMesh3D(new VertexDeclaration(GraphicsDeviceHolder.Device, VertexPositionNormalColorFormat.VertexElements),
               vertexBuffer, indexBuffer, mesh.TriangleStrip, VertexPositionNormalColorFormat.SizeInBytes, "Default", textures, meshParrts3d);
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
