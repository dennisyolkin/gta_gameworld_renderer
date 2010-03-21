using System;
using System.Globalization;
using System.Threading;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Collections.Generic;
using GTAWorldRenderer.Rendering;

namespace GTAWorldRenderer
{
   public class Main : Microsoft.Xna.Framework.Game
   {
      Scene scene;
      IRenderer renderer3d;

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

      }


      protected override void Initialize()
      {
         GraphicsDeviceHolder.InitDevice();
         base.Initialize();
      }


      protected override void LoadContent()
      {
         // загружаем сцену
         scene = new Scene();
         scene.LoadScene();
         Log.Instance.PrintStatistic();
         GC.Collect();

         renderer3d = new SceneRenderer3D(this, scene);
         renderer3d.Initialize();

      }


      protected override void Update(GameTime gameTime)
      {
         renderer3d.Update(gameTime);
      }


      protected override void Draw(GameTime gameTime)
      {
         renderer3d.Draw(gameTime);

         base.Draw(gameTime);
      }



   }
}
