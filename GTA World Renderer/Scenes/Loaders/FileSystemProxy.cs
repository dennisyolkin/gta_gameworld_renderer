using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTAWorldRenderer.Scenes.Loaders
{
   /// <summary>
   /// Обёртка над файловой системой.
   /// Используется при чтении файлов-архивов.
   /// После первого обращения к файлу он не закрывается, поэтому последующие обращения к этому файлу производятся быстрее.
   /// </summary>
   class FileSystemProxy
   {
      // Singleton
      public static FileSystemProxy Instance{ get; private set;}

      static FileSystemProxy()
      {
         Instance = new FileSystemProxy();
      }

      private Dictionary<string, BinaryReader> readers = new Dictionary<string, BinaryReader>();

      /// <summary>
      /// Читает данные из файла
      /// </summary>
      /// <param name="file">Путь к файлу</param>
      /// <param name="offset">Смещение относительно начала файла</param>
      /// <param name="size">Размер данных</param>
      /// <returns>Прочитанные данные</returns>
      public byte[] GetData(string file, int offset, int size)
      {
         BinaryReader reader;
         if (!readers.TryGetValue(file, out reader))
         {
            reader = new BinaryReader(new FileStream(file, FileMode.Open));
            readers[file] = reader;
         }

         reader.BaseStream.Seek(offset, SeekOrigin.Begin);
         return reader.ReadBytes(size);
      }
   }
}
