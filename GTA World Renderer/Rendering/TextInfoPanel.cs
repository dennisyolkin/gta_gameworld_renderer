using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GTAWorldRenderer.Rendering
{
   /// <summary>
   /// Реализация панели, на которой будет выводиться текстовая информация
   /// </summary>
   class TextInfoPanel : DrawableGameComponent
   {
      private const int LINE_HEIGHT = 20;

      public Camera Camera{ get; set; }
      private int fps;

      private SpriteFont font;
      private SpriteBatch spriteBatch;


      public TextInfoPanel(Game game) : base(game)
      {
      }


      protected override void LoadContent()
      {
         spriteBatch = new SpriteBatch(GraphicsDeviceHolder.Device);
         font = Game.Content.Load<SpriteFont>("font");
      }


      public override void Draw(GameTime gameTime)
      {
         GraphicsDeviceHolder.Device.RenderState.DepthBufferEnable = false;
         spriteBatch.Begin();

         int y = 5;
         spriteBatch.DrawString(font, String.Format("FPS: {0}", fps), new Vector2(5, y), Color.Yellow);

         if (Camera != null)
         {
            y += LINE_HEIGHT;
            spriteBatch.DrawString(font, String.Format("Camera position: X:{0:f2} Y:{1:f2} Z:{2:f2}", Camera.Position.X, Camera.Position.Y, Camera.Position.Z), new Vector2(5, y), Color.Yellow);
            y += LINE_HEIGHT;
            spriteBatch.DrawString(font, String.Format("Camera rotation: LR={0:f2} UD={1:f2}", Camera.LeftRightRotation, Camera.UpDownRotation), new Vector2(5, y), Color.Yellow);
         }

         spriteBatch.End();
         GraphicsDeviceHolder.Device.RenderState.DepthBufferEnable = true;
      }
   }
}
