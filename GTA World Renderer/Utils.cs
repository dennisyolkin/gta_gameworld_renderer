using System;
using GTAWorldRenderer.Logging;

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
