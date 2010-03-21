using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GTAWorldRenderer.Logging;
using GTAWorldRenderer.Scenes.ArchivesCommon;

namespace GTAWorldRenderer.Scenes
{
   partial class SceneLoader
   {
      /// <summary>
      /// Реализация загрузки и работы с TXD архивами.
      /// в TXD архивах хранятся текстуры.
      /// 
      /// Описание формата TXD есть здесь: http://wiki.multimedia.cx/index.php?title=TXD
      /// </summary>
      class TXDArchive
      {

         enum SectionType
         {
            Data = 1,
            Extension = 3,
            TextureNative = 21,
            Dictionary = 22,
            Unknown = 42134213 // I hope no section will have such identifier for its type :)
         }

         private BinaryReader fin;
         private string txdName, filePath;
         private List<ArchiveEntry> files = new List<ArchiveEntry>();


         public TXDArchive(string filePath)
         {
            this.filePath = filePath;
         }


         public IEnumerable<ArchiveEntry> Load()
         {
            using (Log.Instance.EnterStage("Loading TXD archive: " + filePath))
            {
               txdName = Path.GetFileNameWithoutExtension(filePath);
               using (fin = new BinaryReader(new FileStream(filePath, FileMode.Open)))
               {
                  // There is only one root node in file, read it
                  SectionType sectionType = (SectionType)fin.ReadInt32();
                  int sectionSize = fin.ReadInt32();
                  fin.BaseStream.Seek(4, SeekOrigin.Current);

                  // Now we can recursively read all the tree
                  ParseSection(sectionSize, sectionType);
               }

               Log.Instance.Print(String.Format("Loaded {0} entries", files.Count));
               return files;
            }
         }


         private void ParseSection(int size, SectionType parentType)
         {
            int positionEnd = (int)fin.BaseStream.Position + size;

            while (fin.BaseStream.Position < positionEnd)
            {
               SectionType sectionType = (SectionType)fin.ReadInt32();

               int sectionSize = fin.ReadInt32();
               fin.BaseStream.Seek(4, SeekOrigin.Current);

               switch (sectionType)
               {
                  case SectionType.Data:
                     if (parentType == SectionType.TextureNative)
                        ParseDataSection(sectionSize, parentType);
                     else
                        fin.BaseStream.Seek(sectionSize, SeekOrigin.Current);
                     break;

                  case SectionType.Extension:
                  case SectionType.Dictionary:
                  case SectionType.TextureNative:
                     ParseSection(sectionSize, sectionType);
                     break;

                  default:
                     fin.BaseStream.Seek(sectionSize, SeekOrigin.Current);
                     break;
               }

            }

         }


         void ParseDataSection(int size, SectionType type)
         {
            int position = (int)fin.BaseStream.Position;

            fin.BaseStream.Seek(8, SeekOrigin.Current);

            byte[] diffuseTextureName = new byte[32];
            byte[] alphaTextureName = new byte[32];
            fin.Read(diffuseTextureName, 0, diffuseTextureName.Length);
            fin.Read(alphaTextureName, 0, alphaTextureName.Length);

            int headerSize = 8 + diffuseTextureName.Length + alphaTextureName.Length;
            fin.BaseStream.Seek(size - headerSize, SeekOrigin.Current);

            Func<byte[], string> ToFullName = delegate(byte[] name) 
            {
               int nilIdx = Array.IndexOf(name, (byte)0);
               int nameLen = nilIdx == -1 ? name.Length : nilIdx;
               return txdName + "/" + (Encoding.ASCII.GetString(name, 0, nameLen) + ".gtatexture").ToLower();
            };

            files.Add(new ArchiveEntry(ToFullName(diffuseTextureName), position, size));

            if (alphaTextureName[0] != 0)
               files.Add(new ArchiveEntry(ToFullName(alphaTextureName), position, size));
         }

      }
   }
}
