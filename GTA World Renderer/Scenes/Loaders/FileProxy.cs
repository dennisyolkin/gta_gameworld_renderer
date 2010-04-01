using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAWorldRenderer.Scenes.Loaders
{
   /// <summary>
   /// Посредник для файла в файловой системе.
   /// Содержит себе название виртуального файла, путь к реальному файлу, оффсет в реальном файле
   /// и размер виртуального файла.
   /// Эту обёртку удобно использовать при работе с вложенными архивами.
   /// </summary>
   class FileProxy
   {
      public string Name { get; private set; }
      public string FilePath { get; private set; }
      public int Offset { get; private set; }
      public int Size { get; private set; }


      public FileProxy(string filePath)
      {
         Offset = 0;
         Size = (int)new FileInfo(filePath).Length;
         FilePath = filePath;
         Name = Path.GetFileName(FilePath);
      }


      public FileProxy(FileProxy file, string virtualFileName, int offset, int size)
      {
         Size = size;
         Offset = file.Offset + offset;
         FilePath = file.FilePath;
         Name = virtualFileName;

         if (size > file.Size)
            Utils.TerminateWithError("Некорректное описание файла. Виртуальный файл выходит за приделы родительского виртуального файла");
      }


      public byte[] GetData()
      {
         return FileSystemProxy.Instance.GetData(FilePath, Offset, Size);
      }

   }
}
