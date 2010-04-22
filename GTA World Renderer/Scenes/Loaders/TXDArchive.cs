using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GTAWorldRenderer.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace GTAWorldRenderer.Scenes.Loaders
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
      private FileProxy archiveFile;
      private string txdName;
      private short texturesAmount = short.MaxValue; // it will be read from header
      private short processedTextures = 0;

      private Dictionary<string, Texture2D> files = new Dictionary<string, Texture2D>();


      public TXDArchive(string filePath)
         : this(new FileProxy(filePath))
      {
      }


      public TXDArchive(FileProxy archiveFile)
      {
         txdName = Path.GetFileNameWithoutExtension(archiveFile.Name);
         this.archiveFile = archiveFile;
         fin = new BinaryReader(new MemoryStream(archiveFile.GetData()));
      }


      private bool ArchiveShouldBeIgnored()
      {
         /*
          * В GTAIII этот файл имеет какую-то неправильную структуру и не может корректно зугрузиться.
          * Судя по всему, в нём не содержится ничего ценного (вероятно, только текстуры для low-detailed).
          */
         return archiveFile.Name == "islandlodcomindnt.txd";
      }


      public Dictionary<string, Texture2D> Load()
      {
         using (Log.Instance.EnterStage("Loading TXD archive: " + txdName))
         {
            if (ArchiveShouldBeIgnored())
               return files;

            // There is only one root node in file, read it
            SectionType sectionType = (SectionType)fin.ReadInt32();
            int sectionSize = fin.ReadInt32();
            fin.BaseStream.Seek(4, SeekOrigin.Current);

            // Now we can recursively read all the tree
            ParseSection(sectionSize, sectionType);

            fin.Close();

            if (processedTextures != texturesAmount)
            {
               Utils.TerminateWithError(String.Format("Something failed during TXD loading. Textures in archive header: {0}, found textures: {1}", 
                  texturesAmount, processedTextures));
            }

            Log.Instance.Print(String.Format("Loaded {0} entries", files.Count));
            return files;
         }
      }


      private void ParseSection(int size, SectionType parentType)
      {
         int positionEnd = (int)fin.BaseStream.Position + size;

         while (fin.BaseStream.Position < positionEnd && fin.BaseStream.Position < fin.BaseStream.Length 
            && processedTextures < texturesAmount)
         {
            SectionType sectionType = (SectionType)fin.ReadInt32();

            int sectionSize = fin.ReadInt32();
            fin.BaseStream.Seek(4, SeekOrigin.Current);

            switch (sectionType)
            {
               case SectionType.Data:
                  if (parentType == SectionType.TextureNative)
                     ReadTexture(sectionSize, parentType);
                  else if (parentType == SectionType.Dictionary)
                  {
                     texturesAmount = fin.ReadInt16();
                     fin.BaseStream.Seek(sizeof(short), SeekOrigin.Current);
                  }
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


      void ReadTexture(int size, SectionType type)
      {
         int position = (int)fin.BaseStream.Position;
         fin.BaseStream.Seek(8, SeekOrigin.Current);

         byte[] diffuseTextureName = new byte[32];
         byte[] alphaTextureName = new byte[32];
         fin.Read(diffuseTextureName, 0, diffuseTextureName.Length);
         fin.Read(alphaTextureName, 0, alphaTextureName.Length);

         var texture = new GTATextureLoader(fin).Load();

         Func<byte[], string> ToFullName = delegate(byte[] name) 
         {
            int nilIdx = Array.IndexOf(name, (byte)0);
            int nameLen = nilIdx == -1 ? name.Length : nilIdx;
            return (txdName + "/" + Encoding.ASCII.GetString(name, 0, nameLen) + ".gtatexture").ToLower();
         };

         files[ToFullName(diffuseTextureName)] = texture;

         if (alphaTextureName[0] != 0)
            files[ToFullName(alphaTextureName)] = texture;

         ++processedTextures;
      }

   }

}
