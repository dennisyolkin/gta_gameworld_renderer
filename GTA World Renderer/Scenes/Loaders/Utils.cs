using System;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes.Loaders
{
   class LoadingException : ApplicationException
   {
      public LoadingException(string msg)
         : base(msg)
      {
      }
   }


   enum GtaVersion
   {
      Unknown, III, ViceCity, SanAndreas
   }


   static class Utils
   {
      public static void TerminateWithError(string errorMessagae)
      {
         Log.Instance.Print("Error during scene loading: " + errorMessagae, MessageType.Error);
         throw new LoadingException(errorMessagae);
      }
   }
}
