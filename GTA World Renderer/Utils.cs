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


      /// <summary>
      /// Объединяет набор отсортированных по возрастанию без повторяющихся элементов List-ов в один (тоже отсортированный)
      /// </summary>
      /// <typeparam name="T">Тип элементов списка, должен реализовывать интерфейс IComparable</typeparam>
      /// <param name="lists">Списки. Каждый из списков должен быть отсортирован по возрастанию</param>
      /// <returns>Объединённый отсортированный список. Каждый элемент встречается только один раз</returns>
      public static List<T> MergeSortedLists<T>(List<List<T>> lists) where T : IComparable
      {
         // TODO :: it can be optimized!!!
         var allElemented = new List<T>();
         foreach (var l in lists)
            allElemented.AddRange(l);

         var result = new List<T>();
         if (allElemented.Count == 0)
            return result;

         allElemented.Sort();
         result.Add(allElemented[0]);
         for (var i = 1; i < allElemented.Count; ++i)
            if (allElemented[i].CompareTo(result[result.Count - 1]) != 0)
               result.Add(allElemented[i]);

         return result;
      }

   }
}
