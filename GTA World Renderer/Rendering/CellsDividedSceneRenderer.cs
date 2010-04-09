using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GTAWorldRenderer.Rendering
{
   class CellsDividedSceneRenderer : CompositeRenderer
   {
      private const int DefaultCameraHeight = 1000;
      private const int MouseSpeed = 50;

      public Scene SceneContent { get; private set; }

      private TextInfoPanel infoPanel;
      private Effect effect;
      private Vector3 cameraPosition = new Vector3(0, DefaultCameraHeight, 0);
      private Matrix projectionMatrix;
      private MouseState oldMouseState;


      public CellsDividedSceneRenderer(ContentManager contentManager, Scene scene) 
         : base(contentManager)
      {
         SceneContent = scene;

         effect = Content.Load<Effect>("effect2d");
         effect.CurrentTechnique = effect.Techniques["Basic"];

         infoPanel = new TextInfoPanel(Content);
         AddSubRenderer(infoPanel);

         projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Device.Viewport.AspectRatio,
            Config.Instance.Rendering.NearClippingDistance, Config.Instance.Rendering.FarClippingDistance);

         Mouse.SetPosition(Device.Viewport.Width / 2, Device.Viewport.Height / 2);
         oldMouseState = Mouse.GetState();
      }


      public override void DoUpdate(GameTime gameTime)
      {
         float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
         ProcessMouse(timeDifference);
      }


      void ProcessMouse(float timeDifference)
      {
         MouseState mouseState = Mouse.GetState();
         if ((mouseState.LeftButton == ButtonState.Pressed && mouseState != oldMouseState) || mouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue)
         {
            float xDiff = oldMouseState.X - mouseState.X;
            float yDiff = oldMouseState.Y - mouseState.Y;
            int wheelDiff = oldMouseState.ScrollWheelValue - mouseState.ScrollWheelValue;

            cameraPosition.X += xDiff * timeDifference * MouseSpeed;
            cameraPosition.Z += yDiff * timeDifference * MouseSpeed;
            cameraPosition.Y += wheelDiff;

            if (cameraPosition.Y < 0)
               cameraPosition.Y = 0;
         }
         Mouse.SetPosition(Device.Viewport.Width / 2, Device.Viewport.Height / 2);
         oldMouseState = Mouse.GetState();
      }


      public override void DoDraw(GameTime gameTime)
      {
         Device.Clear(Color.Black);

         var cameraTarget = new Vector3(cameraPosition.X, 0, cameraPosition.Z);
         var viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Forward);

         effect.Parameters["xView"].SetValue(viewMatrix);
         effect.Parameters["xProjection"].SetValue(projectionMatrix);
         effect.Parameters["xColor"].SetValue(Color.Green.ToVector4());

         Device.RenderState.FillMode = FillMode.WireFrame;

         foreach (var obj in SceneContent.SceneObjects)
            obj.Model.Draw(effect, obj.WorldMatrix, false);

         Device.RenderState.FillMode = FillMode.Solid;
      }
   }
}
