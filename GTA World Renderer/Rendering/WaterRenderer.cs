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
      private VertexDeclaration vertexDeclaration;
      private VertexBuffer vertexBuffer;
      private int primitivesToDraw;

      public WaterRenderer(ContentManager contentManager, Scene scene, Effect effect)
         : base(contentManager)
      {
         this.effect = effect;

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
      }


      public override void Update(GameTime gameTime)
      {

      }


      public override void Draw(GameTime gameTime)
      {
         Device.RenderState.CullMode = CullMode.None;
         effect.Parameters["xWorld"].SetValue(Matrix.Identity);
         effect.Parameters["xSolidColor"].SetValue(Color.DarkBlue.ToVector4());
         effect.CurrentTechnique = effect.Techniques["SolidColored"];
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
