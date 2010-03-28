using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Scenes
{
   namespace VertexFormats
   {
      /*
       * TODO :: 
       * 
       * эти "нестандартные" форматы, скорее всего, нафиг не нужны. Как только 
       * текстурирование будет полностью отлажено, их можно будет похерить.
       */

      /*
      public struct VertexPositionNormalFormat
      {
         private Vector3 position;
         private Vector3 normal;

         public VertexPositionNormalFormat(Vector3 position, Vector3 normal)
         {
            this.position = position;
            this.normal = normal;
         }

         public static VertexElement[] VertexElements = 
            {
               new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
               new VertexElement(0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0)
            };

         public static int SizeInBytes = sizeof(float) * (3 + 3);
      }*/


      /*
      public struct VertexPositionNormalColorFormat
      {
         private Vector3 position;
         private Vector3 normal;
         private Color color;

         public VertexPositionNormalColorFormat(Vector3 position, Vector3 normal, Color color)
         {
            this.position = position;
            this.normal = normal;
            this.color = color;
         }

         public static VertexElement[] VertexElements = 
            {
               new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
               new VertexElement(0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
               new VertexElement(0, sizeof(float) * 6, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0)
            };

         public static int SizeInBytes = sizeof(float) * (3 + 3) + 4;
      }*/

   }
}
