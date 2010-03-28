
namespace GTAWorldRenderer.Scenes.ArchivesCommon
{
      /// <summary>
      /// Описание файла, входящего в состав архива
      /// </summary>
      public class ArchiveEntry
      {
         public ArchiveEntry(string name, int offset, int size)
         {
            Name = name;
            Offset = offset;
            Size = size;
         }

         public string Name { get; private set; }
         public int Offset { get; private set; }
         public int Size { get; private set; }
      }
   
}
