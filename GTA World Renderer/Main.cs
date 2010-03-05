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

         // настраиваем графическое устройство
         graphics = new GraphicsDeviceManager(this);
         Content.RootDirectory = "Content";
      }

      
      protected override void Initialize()
      {
         base.Initialize();
      }


      protected override void LoadContent()
      {
      }


      protected override void Update(GameTime gameTime)
      {
         base.Update(gameTime);
      }


      protected override void Draw(GameTime gameTime)
      {
         GraphicsDevice.Clear(Color.CornflowerBlue);
         base.Draw(gameTime);
      }
   }
}
