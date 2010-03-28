using System;
using GTAWorldRenderer.Logging;
using System.IO;
using System.Xml.Schema;
using System.Xml;

namespace GTAWorldRenderer
{
   /// <summary>
   /// Предоставляет параметры приложения, считанные из xml-файлов конфигурации
   /// (основной - config.xml)
   /// </summary>
   class Config
   {
      public class ConfigInitializationFailedException : ApplicationException
      {
         public ConfigInitializationFailedException(string msg)
            : base(msg)
         {
         }

         public ConfigInitializationFailedException(string msg, Exception innerException)
            : base(msg, innerException)
         {
         }
      }

      private static Config instance = new Config();
      private Log Logger = Log.Instance;

      public static Config Instance
      {
         get { return instance; }
      }

      public string GTAFolderPath { private set; get; }
      public bool ShowWarningsIfTextureNotFound { private set; get; }
      public bool FullScreenMode { private set; get; }
      public float NearClippingDistance { private set; get; }
      public float FarClippingDistance { private set; get; }

      public Config()
      {
         using (Logger.EnterStage("Reading config file"))
         {
            Logger.Print("Working directory: " + System.Environment.CurrentDirectory);
            using (Logger.EnterStage("Parsing config.xml file..."))
            {
               if (!File.Exists("config.xml"))
                  TerminateWithError("File not found: config.xml");
               if (!File.Exists("config.xsd"))
                  TerminateWithError("File not found: config.xsd");

               Logger.Print("Validating file...");
               try
               {
                  ValidateXml("config.xml", "config.xsd");
               } catch (Exception er)
               {
                  TerminateWithError("Content of config.xml is not valid! Validation exception details: " + er.Message, er);
               }

               ReadConfig();
            }
         }
         Logger.Print("Application configuration was successfully loaded");
      }

      /// <summary>
      /// Считывает содержимое конфигурационного файла. Предполагается, что файл точно существует и корректен.
      /// </summary>
      private void ReadConfig()
      {
         XmlDocument doc = new XmlDocument();
         doc.Load("config.xml");
         XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
         nsmgr.AddNamespace("ns", "gta-gameworld-renderer");

         GTAFolderPath = doc.SelectSingleNode("/ns:GlobalConfig/ns:GtaFolder", nsmgr).InnerText;
         if (!GTAFolderPath.EndsWith("/") && !GTAFolderPath.EndsWith("\\"))
            GTAFolderPath += "\\";
         Logger.Print("GTA Folder: " + GTAFolderPath);

         ShowWarningsIfTextureNotFound = Boolean.Parse(
            doc.SelectSingleNode("/ns:GlobalConfig/ns:LoadingParams/ns:ShowWarningsIfTextureNotFound", nsmgr).InnerText);

         FullScreenMode = Boolean.Parse(
            doc.SelectSingleNode("/ns:GlobalConfig/ns:RenderingParams/ns:FullScreen", nsmgr).InnerText);

         NearClippingDistance = float.Parse(doc.SelectSingleNode("/ns:GlobalConfig/ns:RenderingParams/ns:NearClippingDist", nsmgr).InnerText);
         FarClippingDistance = float.Parse(doc.SelectSingleNode("/ns:GlobalConfig/ns:RenderingParams/ns:FarClippingDist", nsmgr).InnerText);
      }


      void TerminateWithError(string msg, Exception innerException)
      {
         Log.Instance.Print(msg, MessageType.Error);
         throw new ConfigInitializationFailedException(msg, innerException);
      }


      void TerminateWithError(string error_msg)
      {
         TerminateWithError(error_msg, null);
      }


      /// <summary>
      /// Проверяет соответствие XML файла схеме.
      /// Кидает исключение типа XmlSchemaCalidationFailed, если XML файл не соответствует схеме
      /// </summary>
      private void ValidateXml(string xml_path, string schema_path)
      {
         XmlSchemaSet sc = new XmlSchemaSet();
         sc.Add("gta-gameworld-renderer", schema_path);
         XmlReaderSettings settings = new XmlReaderSettings();
         settings.ValidationType = ValidationType.Schema;
         settings.Schemas = sc;
         XmlReader reader = XmlReader.Create(xml_path, settings);
         while (reader.Read()) ;
      }

   }
}
