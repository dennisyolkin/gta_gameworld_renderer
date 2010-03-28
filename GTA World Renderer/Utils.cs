using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Logging;
using Microsoft.Xna.Framework.Graphics;
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

   }
}
