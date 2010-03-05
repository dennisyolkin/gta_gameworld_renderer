using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTAWorldRenderer.Logging
{
   enum MessageType
   {
      Info = 1,
      Warning = 2,
      Error = 4,
   }

   enum MessagesFilter
   {
      Info = MessageType.Info,
      Warning = MessageType.Warning,
      Error = MessageType.Error,

      InfoError = Info | Error,
      All = Info | Error | Warning
   }

   /// <summary>
   /// Класс, реализующий ведение лога. Реализован в виде Singleton.
   /// Для вывода лога нужно создать и добавить с помощью AddLogWriter экземпляр класса,
   /// реализующего интерфейс ILogWriter
   /// </summary>
   class Log
   {

      /// <summary>
      /// Тип Stage, представляющий "стадию" в процессе загрузки и построения игрового мира.
      /// При вызове Dispose предполагается завершение текущей стадии.
      /// </summary>
      public class Stage : IDisposable
      {
         public delegate void EndStageDelegate();

         EndStageDelegate onEndStage;

         public Stage(EndStageDelegate onEndStage)
         {
            this.onEndStage = onEndStage;
         }

         public void Dispose()
         {
            onEndStage();
         }
      }

      // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = 

      private static Log instance = new Log();
      private List<ILogWriter> writers = new List<ILogWriter>();
      private MessagesFilter messageTypesToOutput = MessagesFilter.All;
      private int indent = 0;
      private int errors = 0, warnings = 0;

      public static Log Instance
      {
         get { return instance;  }
      }


      /// <summary>
      /// Начало новой стадии.
      /// Предполагается использовать следующим способом:
      /// using (ConsoleLogger.Instance.EnterStage) { some actions }
      /// </summary>
      public Stage EnterStage(string stageName)
      {
         Print(stageName + " ...");
         ++indent;
         return new Stage(new Stage.EndStageDelegate(LeaveStage));
      }


      private void LeaveStage()
      {
         --indent;
      }


      public void AddLogWriter(ILogWriter writer)
      {
         writers.Add(writer);
      }


      public void SetOutputFilter(MessagesFilter filter)
      {
         messageTypesToOutput = filter;
      }


      public void Print(string msg, MessageType type)
      {
         if (type == MessageType.Error)
            ++errors;
         else if (type == MessageType.Warning)
            ++warnings;

         foreach (ILogWriter writer in writers)
            if (((int)type & (int)messageTypesToOutput) != 0)
               writer.Print(msg, indent, type);
      }


      public void PrintStatistic()
      {
         foreach (ILogWriter writer in writers)
         {
            writer.PrintStatistic(errors, warnings, indent);
            writer.Flush();
         }
      }


      public void Print(string msg)
      {
         Print(msg, MessageType.Info);
      }


      public void Flush()
      {
         foreach (var writer in writers)
            writer.Flush();
      }

   } // end of 'Log' class
} // end of namespace
