using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GTAWorldRenderer.Logging
{

   class ConsoleLogWriter : ILogWriter
   {
      [DllImport("kernel32.dll")]
      public static extern Boolean AllocConsole();

      [DllImport("kernel32.dll")]
      public static extern Boolean FreeConsole();

      private const int INDENT_SIZE = 2;

      private static ConsoleLogWriter instance = null;
      private MessagesFilter filter = MessagesFilter.All;


      public static ConsoleLogWriter Instance
      {
         get
         {
            if (instance == null)
               instance = new ConsoleLogWriter();
            return instance;
         }
      }


      private ConsoleLogWriter()
      {
         AllocConsole();
         Console.Title = "GTA GameWorld Renderer. Console logger.";

         Console.BackgroundColor = ConsoleColor.Black;
         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine("GTA GameWorld Renderer");
         Console.WriteLine("======================\n");
      }


      public void Print(string message, int indent, MessageType type)
      {
         if (type == MessageType.Info)
         {
            if (((int)filter & (int)type) != 0)
            {
               PrintIndent(indent);
               Console.ForegroundColor = ConsoleColor.Gray;
               Console.WriteLine(message);
            }
         }
         else if (type == MessageType.Warning)
         {
            if (((int)filter & (int)type) != 0)
            {
               PrintIndent(indent);
               Console.ForegroundColor = ConsoleColor.Yellow;
               Console.WriteLine("[Warning] " + message);
            }
         }
         else if (type == MessageType.Error)
         {
            if (((int)filter & (int)type) != 0)
            {
               PrintIndent(indent);
               Console.ForegroundColor = ConsoleColor.Red;
               Console.WriteLine("[Error] " + message);
            }
         }
      }


      private void PrintIndent(int indent)
      {
         Console.Write(new String(' ', indent * INDENT_SIZE));
      }


      public void PrintStatistic(int errors, int warnings, int indent)
      {
         PrintIndent(indent);

         Console.ForegroundColor = ConsoleColor.Green;
         Console.Write(" === ");

         Console.ForegroundColor = errors > 0 ? ConsoleColor.Red : ConsoleColor.Green;
         Console.Write("{0} error(s), ", errors);
         Console.ForegroundColor = warnings > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
         Console.Write("{0} warning(s)", warnings);

         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine(" ===");
      }


      public void SetMessagesFilter(MessagesFilter filter)
      {
         this.filter = filter;
      }


      public void Flush()
      {
         // do nothing. Console flushes after each symbol
      }

   }
}
