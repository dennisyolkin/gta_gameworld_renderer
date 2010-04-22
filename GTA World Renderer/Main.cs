using System.Globalization;
using System.Threading;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Scenes;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using GTAWorldRenderer.Rendering;
using GTAWorldRenderer.Scenes.Loaders;
using System.Collections.Generic;


namespace GTAWorldRenderer
{
   public class Main : Microsoft.Xna.Framework.Game
   {
      RendererSwitcher renderer;

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
         Scene scene = new SceneLoader().LoadScene();
         if (Config.Instance.Rendering.FullScreen)
         {
            Log.Instance.Print("Switching to fullscreen mode...");
            GraphicsDeviceHolder.DeviceManager.PreferredBackBufferHeight = GraphicsDeviceHolder.Device.DisplayMode.Height;
            GraphicsDeviceHolder.DeviceManager.PreferredBackBufferWidth = GraphicsDeviceHolder.Device.DisplayMode.Width;
            GraphicsDeviceHolder.DeviceManager.IsFullScreen = true;
            GraphicsDeviceHolder.DeviceManager.ApplyChanges();
         }

         renderer = new RendererSwitcher(Content, new SceneRenderer3D(Content, scene));
         renderer.AddRenderer(new CellsDividedSceneRenderer(Content, scene));
      }


      protected override void Update(GameTime gameTime)
      {
         renderer.Update(gameTime);
      }


      protected override void Draw(GameTime gameTime)
      {
         renderer.Draw(gameTime);

         base.Draw(gameTime);
      }



   }
}
