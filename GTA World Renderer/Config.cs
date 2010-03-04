using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTAWorldRenderer.Logging;
using System.IO;
using System.Xml.Schema;
using System.Xml;
using System.Xml.XPath;

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
      private ConsoleLogger Logger = ConsoleLogger.Instance;

      public static Config Instance
      {
         get { return instance; }
      }

      public string GTAFolderPath { private set; get; }

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
               } catch (XmlSchemaValidationException er)
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
         XPathNavigator nav = doc.CreateNavigator();
         XPathNodeIterator it = nav.Evaluate("GlobalConfig/GTAFolder") as XPathNodeIterator;
         it.MoveNext();
         GTAFolderPath = it.Current.ToString();
         if (!GTAFolderPath.EndsWith("/") && !GTAFolderPath.EndsWith("\\"))
            GTAFolderPath += "\\";
         Logger.Print("GTA Folder: " + GTAFolderPath);
      }


      void TerminateWithError(string msg, Exception innerException)
      {
         ConsoleLogger.Instance.Print(msg, MessageType.Error);
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
