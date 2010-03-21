using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GTAWorldRenderer.Rendering
{
   /// <summary>
   /// Наследник TextInfoPanel, заточенный под вывод информации в режиме 3D просмотра.
   /// </summary>
   class InfoPanelFor3Dview : TextInfoPanel
   {
      /// <summary>
      /// Задаёт камеру, чтобы выводить на экран её позицию и поворот
      /// </summary>
      public Camera Camera { get; set; }


      public InfoPanelFor3Dview(ContentManager contentManager)
         : base(contentManager)
      {
         Data.Add("Camera position", null);
         Data.Add("Camera rotation", null);
      }


      public override void Update(GameTime time)
      {
         if (Camera != null)
         {
            Data["Camera position"] = String.Format("X:{0:f2} Y:{1:f2} Z:{2:f2}", Camera.Position.X, Camera.Position.Y, Camera.Position.Z);
            Data["Camera rotation"] = String.Format("LR={0:f2} UD={1:f2}", Camera.LeftRightRotation, Camera.UpDownRotation);
         }

         base.Update(time);
      }
   }
}
