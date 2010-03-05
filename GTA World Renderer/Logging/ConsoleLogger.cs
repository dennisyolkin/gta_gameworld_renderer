using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GTAWorldRenderer.Logging
{
   enum MessageType
   {
      Info = 1, 
      Warning = 2, 
      Error = 4,
   }


   class ConsoleLogger : IDisposable
   {
      [DllImport("kernel32.dll")]
      public static extern Boolean AllocConsole();

      [DllImport("kernel32.dll")]
      public static extern Boolean FreeConsole();

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


      private const int INDENT_SIZE = 2;

      private static ConsoleLogger _instance = new ConsoleLogger();
      private int indent = 0;
      private int errors = 0;
      private int warnings = 0;
      private int messagesTypesToOutput = (int)MessageType.Info | (int)MessageType.Error | (int)MessageType.Warning;

      public static ConsoleLogger Instance
      {
         get { return _instance; }
      }


      public void Dispose()
      {
         Console.WriteLine("Dispose");
      }


      private ConsoleLogger()
      {
         PrepareConsole();
      }


      private void PrepareConsole()
      {
         AllocConsole();
         Console.Title = "GTA GameWorld Renderer: loading gameworld";

         Console.BackgroundColor = ConsoleColor.Black;
         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine("GTA GameWorld Renderer");
         Console.WriteLine("======================\n");
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


      public void Print(string message, MessageType type)
      {
         if (type == MessageType.Info)
         {
            if ((messagesTypesToOutput & (int)type) != 0)
            {
               PrintIndent();
               Console.ForegroundColor = ConsoleColor.Gray;
               Console.WriteLine(message);
            }
         }
         else if (type == MessageType.Warning)
         {
            if ((messagesTypesToOutput & (int)type) != 0)
            {
               PrintIndent();
               Console.ForegroundColor = ConsoleColor.Yellow;
               Console.WriteLine("[Warning] " + message);
            }
            ++warnings;
         }
         else if (type == MessageType.Error)
         {
            if ((messagesTypesToOutput & (int)type) != 0)
            {
               PrintIndent();
               Console.ForegroundColor = ConsoleColor.Red;
               Console.WriteLine("[Error] " + message);
            }
            ++errors;
         }
      }


      public void Print(string message)
      {
         Print(message, MessageType.Info);
      }


      private void PrintIndent()
      {
         Console.Write(new String(' ', indent * INDENT_SIZE));
      }


      public void PrintStatistic()
      {
         PrintIndent();

         Console.ForegroundColor = ConsoleColor.Green;
         Console.Write(" === ");

         Console.ForegroundColor = errors > 0 ? ConsoleColor.Red : ConsoleColor.Green;
         Console.Write("{0} error(s), ", errors);
         Console.ForegroundColor = warnings > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
         Console.Write("{0} warning(s)", warnings);

         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine(" ===");
      }

      // for example, MessageType.Info | MessageType.Error
      public void SetMessagesTypeToOutput(int types)
      {
         messagesTypesToOutput = types;
      }

   }
}
