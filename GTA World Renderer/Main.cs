//#define FULL_SCREEN

using System;
using System.Globalization;
using System.Threading;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using GTAWorldRenderer.Rendering;


namespace GTAWorldRenderer
{
   public class Main : Microsoft.Xna.Framework.Game
   {
      Renderer renderer3d;

      public Main()
      {
         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         
         GraphicsDeviceHolder.DeviceManager = new GraphicsDeviceManager(this);
         Content.RootDirectory = "Content";

#if FULL_SCREEN
         GraphicsDeviceHolder.DeviceManager.PreferredBackBufferHeight = 900;
         GraphicsDeviceHolder.DeviceManager.PreferredBackBufferHeight = 1440;
         GraphicsDeviceHolder.DeviceManager.IsFullScreen = true;
         GraphicsDeviceHolder.DeviceManager.ApplyChanges();
#endif

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
         Log.Instance.PrintStatistic();
         GC.Collect();

         renderer3d = new SceneRenderer3D(Content, new SceneLoader().LoadScene());
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
