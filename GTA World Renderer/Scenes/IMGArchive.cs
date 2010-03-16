using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Scenes.ArchivesCommon;
using System.IO;
using GTAWorldRenderer.Logging;

namespace GTAWorldRenderer.Scenes
{
   partial class SceneLoader
   {
      /// <summary>
      /// Реализация загрузки и работы с IMG архивами.
      /// в IMG архивах хранятся текстуры.
      /// 
      /// Описание формата IMG есть здесь: http://gtamodding.ru/wiki/IMG_%D0%B0%D1%80%D1%85%D0%B8%D0%B2
      /// </summary>
      class IMGArchive  //  TODO :: возможно, следует создать интерфейс IArchive
      {

         private string filePath;
         private GtaVersion gtaVersion;
         private List<ArchiveEntry> files = new List<ArchiveEntry>();

         public IMGArchive(string filePath, GtaVersion gtaVersion)
         {
            this.filePath = filePath;
            this.gtaVersion = gtaVersion;
         }


         public IEnumerable<ArchiveEntry> Load()
         {
            using (Log.Instance.EnterStage("Loading IMG archive: " + filePath))
            {
               switch (gtaVersion)
               {
                  case GtaVersion.III:
                  case GtaVersion.ViceCity:
                     string dirFilePath = filePath.Substring(0, filePath.Length - 3) + "dir";
                     using (BinaryReader inputDir = new BinaryReader(new FileStream(dirFilePath, FileMode.Open)))
                     {
                        int entries = (int)inputDir.BaseStream.Length / 32;
                        LoadArchiveContents(inputDir, entries);
                     }
                     break;

                  case GtaVersion.SanAndreas:
                     using (BinaryReader inputImg = new BinaryReader(new FileStream(filePath, FileMode.Open)))
                     {
                        byte[] header = new byte[4];
                        inputImg.Read(header, 0, header.Length);
                        if (Encoding.ASCII.GetString(header) != "VER2")
                           TerminateWithError("Incorrect IMG archive for GTA San Andreas. Expected IMG archive ver2.");
                        int entries = inputImg.ReadInt32();
                        LoadArchiveContents(inputImg, entries);
                     }
                     break;

                  default:
                     TerminateWithError("IMGArchive lodaer does not suppport this GTA version.");
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

               ArchiveEntry entry = new ArchiveEntry(Encoding.ASCII.GetString(name, 0, nameLen), pos, length);
               files.Add(entry);
            }
         }

      }
   }
}
