using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAWorldRenderer.Logging
{
   /// <summary>
   /// Интерфейс для классов, отображающих лог (к примеру, в консоли, в отдельном окне, в файле)
   /// </summary>
   interface ILogWriter
   {
      void Print(string msg, int indent, MessageType type);
      void PrintStatistic(int errors, int warnings, int indent);
      void Flush();
   }
}
