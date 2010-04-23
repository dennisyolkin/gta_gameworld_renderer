using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Rendering
{
   class WaterRenderer : Renderer
   {
      private Color LightingColor = new Color(128, 128, 128, 255);

      private Effect effect;
      Camera camera;
      private VertexDeclaration vertexDeclaration;
      private VertexBuffer vertexBuffer;
      private int primitivesToDraw;
      Matrix projectionMatrix;

      public WaterRenderer(ContentManager contentManager, Scene scene, Camera camera)
         : base(contentManager)
      {
         effect = Content.Load<Effect>("water");
         this.camera = camera;

         var vertices = new VertexPositionColor[6 * scene.Water.WaterQuads.Count];
         int idx = 0;
         foreach (var quad in scene.Water.WaterQuads)
         {
            vertices[idx++] = new VertexPositionColor(new Vector3(quad.Xmin, quad.Level, -quad.Ymin), LightingColor);
            vertices[idx++] = new VertexPositionColor(new Vector3(quad.Xmax, quad.Level, -quad.Ymin), LightingColor);
            vertices[idx++] = new VertexPositionColor(new Vector3(quad.Xmin, quad.Level, -quad.Ymax), LightingColor);

            vertices[idx++] = new VertexPositionColor(new Vector3(quad.Xmax, quad.Level, -quad.Ymin), LightingColor);
            vertices[idx++] = new VertexPositionColor(new Vector3(quad.Xmax, quad.Level, -quad.Ymax), LightingColor);
            vertices[idx++] = new VertexPositionColor(new Vector3(quad.Xmin, quad.Level, -quad.Ymax), LightingColor);
         }

         vertexBuffer = new VertexBuffer(Device, vertices.Length * VertexPositionColor.SizeInBytes, BufferUsage.None);
         vertexBuffer.SetData(vertices);
         vertexDeclaration = new VertexDeclaration(Device, VertexPositionColor.VertexElements);
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
         effect.CurrentTechnique = effect.Techniques["Water"];
         Device.VertexDeclaration = vertexDeclaration;
         Device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);

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
