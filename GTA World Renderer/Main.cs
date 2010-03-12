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
      GraphicsDeviceManager graphics;

      public Main()
      {
         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

         // настраиваем лог
         Log.Instance.AddLogWriter(ConsoleLogWriter.Instance);
         Log.Instance.AddLogWriter(new FileLogWriter("log.log"));
         Scene scene = new Scene();

         // загружаем сцену
         scene.LoadScene();
         Log.Instance.PrintStatistic();

         GC.Collect();

         // настраиваем графическое устройство
         graphics = new GraphicsDeviceManager(this);
         Content.RootDirectory = "Content";
      }


      Camera camera;


      protected override void Initialize()
      {
         camera = new Camera();

         base.Initialize();
      }


      protected override void LoadContent()
      {
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
         base.Draw(gameTime);
      }


      private void ProcessMouse(GameTime gameTime, float amount)
      {

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
