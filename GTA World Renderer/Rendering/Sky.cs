using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Rendering
{
   class SkyRenderer : Renderer
   {
      private Camera camera;
      private Model skyDome;
      private Matrix projectionMatrix;
      private Effect effect;

      private RenderTarget2D cloudsRenderTarget;
      private Texture2D cloudMap;
      private Texture2D cloudStaticMap;
      private VertexDeclaration fullScreenVertexDeclaration;
      VertexPositionTexture[] fullScreenVertices;


      public SkyRenderer(ContentManager contentManager, Camera camera)
         : base(contentManager)
      {
         this.camera = camera;
         skyDome = Content.Load<Model>("sky_dome");
         effect = Content.Load<Effect>("sky");
         skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone(Device);
         projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Device.Viewport.AspectRatio,
            Config.Instance.Rendering.NearClippingDistance, Config.Instance.Rendering.FarClippingDistance);

         var pp = Device.PresentationParameters;
         cloudsRenderTarget = new RenderTarget2D(Device, pp.BackBufferWidth, pp.BackBufferHeight, 1, Device.DisplayMode.Format);
         cloudStaticMap = CreateStaticMap(32);

         fullScreenVertexDeclaration = new VertexDeclaration(Device, VertexPositionTexture.VertexElements);
         fullScreenVertices = SetUpFullscreenVertices();
      }


      private VertexPositionTexture[] SetUpFullscreenVertices()
      {
         VertexPositionTexture[] vertices = new VertexPositionTexture[4];

         vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
         vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
         vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
         vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

         return vertices;
      }


      private Texture2D CreateStaticMap(int resolution)
      {
         Random rand = new Random();
         Color[] noisyColors = new Color[resolution * resolution];
         for (int x = 0; x < resolution; x++)
            for (int y = 0; y < resolution; y++)
               noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

         Texture2D noiseImage = new Texture2D(Device, resolution, resolution, 1, TextureUsage.None, SurfaceFormat.Color);
         noiseImage.SetData(noisyColors);
         return noiseImage;
      }


      private void GeneratePerlinNoise(float time)
      {
         Device.RenderState.AlphaBlendEnable = false;
         Device.RenderState.AlphaTestEnable = false;

         Device.SetRenderTarget(0, cloudsRenderTarget);
         effect.CurrentTechnique = effect.Techniques["PerlinNoise"];
         effect.Parameters["xTexture"].SetValue(cloudStaticMap);
         effect.Parameters["xOvercast"].SetValue(1.1f);
         effect.Parameters["xTime"].SetValue(time / 1000.0f);

         effect.Begin();
         foreach (EffectPass pass in effect.CurrentTechnique.Passes)
         {
            pass.Begin();

            Device.VertexDeclaration = fullScreenVertexDeclaration;
            Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, fullScreenVertices, 0, 2);

            pass.End();
         }
         effect.End();

         Device.RenderState.AlphaTestEnable = true;
         Device.RenderState.AlphaBlendEnable = true;
         Device.SetRenderTarget(0, null);
         cloudMap = cloudsRenderTarget.GetTexture();
      }


      public override void Update(GameTime gameTime)
      {
      }


      public override void Draw(GameTime gameTime)
      {
         float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
         GeneratePerlinNoise(time);

         Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

         Device.RenderState.DepthBufferWriteEnable = false;
         Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
         skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

         Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(100) * Matrix.CreateTranslation(camera.Position);
         foreach (ModelMesh mesh in skyDome.Meshes)
         {
            foreach (Effect currentEffect in mesh.Effects)
            {
               Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;

               currentEffect.CurrentTechnique = currentEffect.Techniques["Sky"];
               currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
               currentEffect.Parameters["xView"].SetValue(camera.ViewMatrix);
               currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
               currentEffect.Parameters["xTexture"].SetValue(cloudMap);
            }
            mesh.Draw();
         }
         Device.RenderState.DepthBufferWriteEnable = true;
      }
   }
}
