using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GTAWorldRenderer.Rendering
{
   class SceneRenderer3D : CompositeRenderer
   {
      private const float rotationSpeed = 0.3f;

      public Scene SceneContent { get; set; }

      GraphicsDevice device;
      Camera camera;
      InfoPanelFor3Dview textInfoPanel;
      MouseState originalMouseState;
      Effect[] effect = new Effect[2];
      Matrix projectionMatrix;

      /*
       * TODO :: 
       * в C# 4.0 появятся параметры по умолчанию.
       * Заменить набор конструкторов и метод Construct на единый конструктор с параметрами по умолчанию.
       */

      public SceneRenderer3D(ContentManager contentManager)
         : base(contentManager)
      {
         Initialize();
      }


      public SceneRenderer3D(ContentManager contentManager, Scene scene)
         : base(contentManager)
      {
         SceneContent = scene;
         Initialize();
      }


      public void Initialize()
      {
         device = GraphicsDeviceHolder.Device;
         camera = new Camera();

         textInfoPanel = new InfoPanelFor3Dview(Content);
         textInfoPanel.Camera = camera;
         AddSubRenderer(textInfoPanel);

         projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.1f, 5000.0f);
         effect[0] = Content.Load<Effect>("effect");
         effect[1] = effect[0].Clone(device);

         Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
         originalMouseState = Mouse.GetState();
      }


      public override void DoUpdate(GameTime gameTime)
      {
         float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

         ProcessMouse(gameTime, timeDifference);
         ProcessKeyboard(gameTime, timeDifference);
      }

      bool wasPressed = false;

      private void ProcessMouse(GameTime gameTime, float amount)
      {
         MouseState currentMouseState = Mouse.GetState();
         if (currentMouseState.LeftButton == ButtonState.Pressed)
         {
            wasPressed = true;
            if (currentMouseState != originalMouseState)
            {
               float xDifference = -currentMouseState.X + originalMouseState.X;
               float yDifference = -currentMouseState.Y + originalMouseState.Y;
               float leftrightRot = rotationSpeed * xDifference * amount;
               float updownRot = rotationSpeed * yDifference * amount;
               camera.UpdateRotation(leftrightRot, updownRot);
            }
         } else
            if (wasPressed)
            {
               camera.FixRotation(); // TODO :: move it to camera!!!
               wasPressed = false;
            }
         Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
      }

      private int objectsToDraw = 1;
      KeyboardState oldKeyboardState = Keyboard.GetState();

      private void ProcessKeyboard(GameTime gameTime, float amount)
      {
         amount *= 50;

         Vector3 moveVector = new Vector3(0, 0, 0);
         KeyboardState keyState = Keyboard.GetState();
         if (keyState.IsKeyDown(Keys.Up))
            moveVector += Vector3.Forward;
         if (keyState.IsKeyDown(Keys.Down))
            moveVector += Vector3.Backward;
         if (keyState.IsKeyDown(Keys.Right))
            moveVector += Vector3.Right;
         if (keyState.IsKeyDown(Keys.Left))
            moveVector += Vector3.Left;
         if (keyState.IsKeyDown(Keys.PageUp))
            moveVector += Vector3.Up;
         if (keyState.IsKeyDown(Keys.PageDown))
            moveVector += Vector3.Down;

         if (!oldKeyboardState.IsKeyDown(Keys.A) && keyState.IsKeyDown(Keys.A))
            if (objectsToDraw + 1 <= SceneContent.SceneObjects.Count)
            {
               ++objectsToDraw;
               textInfoPanel.Data["Objects to draw"] = objectsToDraw;
               textInfoPanel.Data["Last model"] = SceneContent.SceneObjects[objectsToDraw - 1].ModelFilename;
            }

         if (!oldKeyboardState.IsKeyDown(Keys.Z) && keyState.IsKeyDown(Keys.Z))
            if (objectsToDraw - 1 >= 0)
            {
               --objectsToDraw;
               textInfoPanel.Data["Objects to draw"] = objectsToDraw;
               if (objectsToDraw > 0)
                  textInfoPanel.Data["Last model"] = SceneContent.SceneObjects[objectsToDraw - 1].ModelFilename;
            }

         oldKeyboardState = keyState;

         if (keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift))
            moveVector *= 10;

         camera.UpdatePosition(moveVector * amount);
      }


      public override void DoDraw(GameTime gameTime)
      {
         device.Clear(Color.Black);

         if (SceneContent == null)
            return;




         for (int i = 0; i != SceneContent.SceneObjects.Count; ++i)
         {
            effect[0].Parameters["xView"].SetValue(camera.ViewMatrix);
            effect[0].Parameters["xProjection"].SetValue(projectionMatrix);
            effect[1].Parameters["xView"].SetValue(camera.ViewMatrix);
            effect[1].Parameters["xProjection"].SetValue(projectionMatrix);
            SceneContent.SceneObjects[i].Model.Draw(effect[0], SceneContent.SceneObjects[i].WorldMatrix);
         }

         //foreach (var obj in SceneContent.SceneObjects)
         //   obj.Model.Draw(effect, obj.WorldMatrix);
      }
   }
}
