using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Rendering
{
   class WaterRenderer : Renderer
   {
      private const float WaterTextureCoordsDivisor = 60.0f;

      private Effect effect;
      Camera camera;
      private VertexDeclaration vertexDeclaration;
      private VertexBuffer vertexBuffer;
      private int primitivesToDraw;
      Matrix projectionMatrix;
      private Texture2D bumpTexture;

      public WaterRenderer(ContentManager contentManager, Scene scene, Camera camera)
         : base(contentManager)
      {
         effect = Content.Load<Effect>("water");
         bumpTexture = Content.Load<Texture2D>("water_texture");

         this.camera = camera;

         var vertices = new VertexPositionTexture[6 * scene.Water.WaterQuads.Count];
         int idx = 0;
         foreach (var quad in scene.Water.WaterQuads)
         {
            vertices[idx++] = new VertexPositionTexture(new Vector3(quad.Xmin, quad.Level, -quad.Ymin),
               new Vector2(quad.Xmin, -quad.Ymin) / WaterTextureCoordsDivisor);
            vertices[idx++] = new VertexPositionTexture(new Vector3(quad.Xmax, quad.Level, -quad.Ymin),
               new Vector2(quad.Xmax, -quad.Ymin) / WaterTextureCoordsDivisor);
            vertices[idx++] = new VertexPositionTexture(new Vector3(quad.Xmin, quad.Level, -quad.Ymax),
               new Vector2(quad.Xmin, -quad.Ymax) / WaterTextureCoordsDivisor);

            vertices[idx++] = new VertexPositionTexture(new Vector3(quad.Xmax, quad.Level, -quad.Ymin),
               new Vector2(quad.Xmax, -quad.Ymin) / WaterTextureCoordsDivisor);
            vertices[idx++] = new VertexPositionTexture(new Vector3(quad.Xmax, quad.Level, -quad.Ymax),
               new Vector2(quad.Xmax, -quad.Ymax) / WaterTextureCoordsDivisor);
            vertices[idx++] = new VertexPositionTexture(new Vector3(quad.Xmin, quad.Level, -quad.Ymax),
               new Vector2(quad.Xmin, -quad.Ymax) / WaterTextureCoordsDivisor);
         }

         vertexBuffer = new VertexBuffer(Device, vertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.None);
         vertexBuffer.SetData(vertices);
         vertexDeclaration = new VertexDeclaration(Device, VertexPositionTexture.VertexElements);
         primitivesToDraw = scene.Water.WaterQuads.Count * 2;

         projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Device.Viewport.AspectRatio,
            Config.Instance.Rendering.NearClippingDistance, Config.Instance.Rendering.FarClippingDistance);
      }


      public override void Update(GameTime gameTime)
      {

      }


      public override void Draw(GameTime gameTime)
      {
         Device.RenderState.CullMode = CullMode.None;
         effect.Parameters["xView"].SetValue(camera.ViewMatrix);
         effect.Parameters["xProjection"].SetValue(projectionMatrix);
         effect.Parameters["xBumpTexture"].SetValue(bumpTexture);
         effect.CurrentTechnique = effect.Techniques["Water"];
         Device.VertexDeclaration = vertexDeclaration;
         Device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionTexture.SizeInBytes);

         effect.Begin();
         foreach (var pass in effect.CurrentTechnique.Passes)
         {
            pass.Begin();
            Device.DrawPrimitives(PrimitiveType.TriangleList, 0, primitivesToDraw);
            pass.End();
         }
         effect.End();
      }

   }
}
