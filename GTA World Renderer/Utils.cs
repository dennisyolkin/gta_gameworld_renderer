using System;
using GTAWorldRenderer.Logging;
using Microsoft.Xna.Framework;

namespace GTAWorldRenderer
{
   class Utils
   {
      public static void TerminateWithError(string errorMessage)
      {
         Log.Instance.Print(errorMessage, MessageType.Error);
         throw new ApplicationException(errorMessage);
      }


      public static Vector3 Point2ToVector3(float x, float y)
      {
         return new Vector3(x, 0, y);
      }
   }
}
