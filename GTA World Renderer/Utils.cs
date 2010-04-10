using System;
using GTAWorldRenderer.Logging;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

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


      public static List<T> MergeLists<T>(List<List<T>> lists) where T : IComparable
      {
         var allElements = new HashSet<T>();
         foreach (var l in lists)
            foreach (var el in l)
               allElements.Add(el);

         return new List<T>(allElements);
      }

   }
}
