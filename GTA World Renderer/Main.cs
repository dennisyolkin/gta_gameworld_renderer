using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Scenes;
using System.Threading;
using System.Globalization;
using GTAWorldRenderer.Rendering;

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
         //Log.Instance.AddLogWriter(ConsoleLogWriter.Instance);
         Log.Instance.AddLogWriter(new FileLogWriter("log.log"));
      }

      private const float rotationSpeed = 0.3f;

      Camera camera;
      TextInfoPanel textInfoPanel;
      MouseState originalMouseState;

      protected override void Initialize()
      {
         camera = new Camera();

         base.Initialize();
      }


      protected override void LoadContent()
      {
         GraphicsDeviceHolder.InitDevice();
         device = GraphicsDeviceHolder.Device;

         // загружаем сцену
         Scene scene = new Scene();
         scene.LoadScene();
         Log.Instance.PrintStatistic();
         GC.Collect();

         textInfoPanel = new TextInfoPanel(Content, device);
         textInfoPanel.Camera = camera;

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

         textInfoPanel.Draw();

         base.Draw(gameTime);
      }


      private void ProcessMouse(GameTime gameTime, float amount)
      {
         MouseState currentMouseState = Mouse.GetState();
         if (currentMouseState != originalMouseState && currentMouseState.LeftButton == ButtonState.Pressed)
         {
            float xDifference = currentMouseState.X - originalMouseState.X;
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
