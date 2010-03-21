using System;
using System.Globalization;
using System.Threading;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Rendering;
using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace GTAWorldRenderer
{
   public class Main : Microsoft.Xna.Framework.Game
   {
      GraphicsDevice device;

      public Main()
      {
         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         
         GraphicsDeviceHolder.DeviceManager = new GraphicsDeviceManager(this);
         Content.RootDirectory = "Content";

         // настраиваем лог
         Log.Instance.AddLogWriter(new FileLogWriter("log.log"));
         if (!Debugger.IsAttached)
         {
            // При приаттаченном дебаггере почему-то падает любое обращение к консоли,
            // поэтому используем ConsoleWriter только когда запускаемся без отладчика
            Log.Instance.AddLogWriter(ConsoleLogWriter.Instance);
         }

         textInfoPanel = new InfoPanelFor3Dview(this);
         Components.Add(textInfoPanel);
      }

      private const float rotationSpeed = 0.3f;

      Scene scene;
      Camera camera;
      InfoPanelFor3Dview textInfoPanel;
      MouseState originalMouseState;
      Effect effect; // TODO :: возможно, он должен создаваться и загружаться в сцене...
      Matrix projectionMatrix;

      protected override void Initialize()
      {
         camera = new Camera();
         GraphicsDeviceHolder.InitDevice();
         base.Initialize();
      }


      protected override void LoadContent()
      {
         device = GraphicsDeviceHolder.Device;

         // загружаем сцену
         scene = new Scene();
         scene.LoadScene();
         Log.Instance.PrintStatistic();
         GC.Collect();

         textInfoPanel.Camera = camera;

         projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.1f, 200.0f);
         effect = Content.Load<Effect>("effect");

         Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
         originalMouseState = Mouse.GetState();

      }


      protected override void Update(GameTime gameTime)
      {
         float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

         ProcessMouse(gameTime, timeDifference);
         ProcessKeyboard(gameTime, timeDifference);


         base.Update(gameTime);
      }


      protected override void Draw(GameTime gameTime)
      {
         GraphicsDevice.Clear(Color.Black);

         effect.Parameters["xView"].SetValue(camera.ViewMatrix);
         effect.Parameters["xProjection"].SetValue(projectionMatrix);
         scene.Draw(effect);

         base.Draw(gameTime);
      }


      private void ProcessMouse(GameTime gameTime, float amount)
      {
         MouseState currentMouseState = Mouse.GetState();
         if (currentMouseState != originalMouseState && currentMouseState.LeftButton == ButtonState.Pressed)
         {
            float xDifference = -currentMouseState.X + originalMouseState.X;
            float yDifference = -currentMouseState.Y + originalMouseState.Y;
            float leftrightRot = rotationSpeed * xDifference * amount;
            float updownRot = rotationSpeed * yDifference * amount;
            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            camera.UpdateRotation(leftrightRot, updownRot);
         }
      }


      private void ProcessKeyboard(GameTime gameTime, float amount)
      {
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
         camera.UpdatePosition(moveVector * amount);
      }

   }
}
