using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Rendering
{
   class Line2D
   {
      struct VertexPosition
      {
         private Vector3 position;

         public VertexPosition(Vector3 position)
         {
            this.position = position;
         }

         public static VertexElement[] VertexElements =
             {
                 new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
             };

         public static int SizeInBytes = sizeof(float) * 3;
      }


      VertexBuffer vertexBuffer;
      private static VertexDeclaration vertexDeclaration = new VertexDeclaration(GraphicsDeviceHolder.Device, VertexPosition.VertexElements);


      public Line2D(Vector3 p1, Vector3 p2)
      {
         vertexBuffer = new VertexBuffer(GraphicsDeviceHolder.Device, sizeof(float) * 6, BufferUsage.None);
         var data = new VertexPosition[] { new VertexPosition(p1), new VertexPosition(p2) };
         vertexBuffer.SetData<VertexPosition>(data);
      }


      public void Draw(Effect effect)
      {
         var device = GraphicsDeviceHolder.Device;
         device.VertexDeclaration = vertexDeclaration;
         device.Vertices[0].SetSource(vertexBuffer, 0, 3 * sizeof(float));

         effect.Parameters["xWorld"].SetValue(Matrix.Identity);
         effect.Begin();
         foreach (var pass in effect.CurrentTechnique.Passes)
         {
            pass.Begin();
            device.DrawPrimitives(PrimitiveType.LineList, 0, 2);
            pass.End();
         }
         effect.End();

      }
   }
}
