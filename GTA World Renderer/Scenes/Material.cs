using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes
{

   public class Material
   {
      public Color Color { get; private set; }
      public Texture2D Texture { get; private set; }

      public Material(Color color, Texture2D texture)
      {
         this.Color = color;
         this.Texture = texture;
      }

      public Material(Texture2D texture)
      {
         this.Texture = texture;
         this.Color = Color.White;
      }
   }

}
