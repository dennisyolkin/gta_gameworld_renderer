using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes.Loaders
{
   /// <summary>
   /// Реализация загрузки и работы с IMG архивами.
   /// в IMG архивах хранятся текстуры.
   /// 
   /// Описание формата IMG есть здесь: http://gtamodding.ru/wiki/IMG_%D0%B0%D1%80%D1%85%D0%B8%D0%B2
   /// </summary>
   class IMGArchive  //  TODO :: возможно, следует создать интерфейс IArchive
   {

      private FileProxy archiveFile;
      private GtaVersion gtaVersion;
      private List<FileProxy> files = new List<FileProxy>();

      public IMGArchive(string filePath, GtaVersion gtaVersion)
      {
         this.gtaVersion = gtaVersion;
         archiveFile = new FileProxy(filePath);
      }


      public IEnumerable<FileProxy> Load()
      {
         using (Log.Instance.EnterStage("Loading IMG archive: " + archiveFile.FilePath))
         {
            switch (gtaVersion)
            {
               case GtaVersion.III:
               case GtaVersion.ViceCity:
                  string dirFilePath = Path.ChangeExtension(archiveFile.FilePath, "dir");
                  using (BinaryReader inputDir = new BinaryReader(new FileStream(dirFilePath, FileMode.Open)))
                  {
                     int entries = (int)inputDir.BaseStream.Length / 32;
                     LoadArchiveContents(inputDir, entries);
                  }
                  break;

               case GtaVersion.SanAndreas:
                  using (BinaryReader inputImg = new BinaryReader(new MemoryStream(archiveFile.GetData())))
                  {
                     byte[] header = new byte[4];
                     inputImg.Read(header, 0, header.Length);
                     if (Encoding.ASCII.GetString(header) != "VER2")
                        Utils.TerminateWithError("Incorrect IMG archive for GTA San Andreas. Expected IMG archive ver2.");
                     int entries = inputImg.ReadInt32();
                     LoadArchiveContents(inputImg, entries);
                  }
                  break;

               default:
                  Utils.TerminateWithError("IMGArchive lodaer does not suppport this GTA version.");
                  break;
            }

            Log.Instance.Print(String.Format("Loaded {0} entries", files.Count));
            return files;
         }
      }


      private void LoadArchiveContents(BinaryReader input, int entriesInArchive)
      {
         for (int i = 0; i != entriesInArchive; ++i)
         {
            int pos = input.ReadInt32() * 2048;
            int length = input.ReadInt32() * 2048;
            byte[] name = new byte[24];
            input.Read(name, 0, name.Length);

            int nameLen = name.Length;
            while (nameLen > 0 && name[nameLen - 1] == 0)
               --nameLen;

            string strName = Encoding.ASCII.GetString(name, 0, nameLen).ToLower();
            FileProxy entry = new FileProxy(archiveFile, strName, pos, length);
            files.Add(entry);
         }
      }

   }

}
