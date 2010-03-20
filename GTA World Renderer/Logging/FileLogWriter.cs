using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAWorldRenderer.Logging
{
   class FileLogWriter : ILogWriter
   {
      private const int INDENT_SIZE = 3;

      private StreamWriter fout;


      public FileLogWriter(string filename)
      {
         fout = new StreamWriter(filename);
      }


      public void Print(string msg, int indent, MessageType type)
      {
         if (type == MessageType.Warning)
            msg = "[waening] " + msg;
         else if (type == MessageType.Error)
            msg = "[error] " + msg;
         PrintIndent(indent);
         fout.WriteLine(msg);
      }


      public void PrintStatistic(int errors, int warnings, int indent)
      {
         fout.WriteLine(" === {0} error(s), {1} warning(s) === ", errors, warnings);
      }


      public void Flush()
      {
         fout.Flush();
      }


      private void PrintIndent(int indent)
      {
         fout.Write(new String(' ', indent * INDENT_SIZE));
      }


      public void Close()
      {
         fout.Close();
      }
   }
}
