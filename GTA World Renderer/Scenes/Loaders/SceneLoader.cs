using System;
using GTAWorldRenderer.Logging;
using System.Collections.Generic;
using GTAWorldRenderer.Scenes.Rasterization;

namespace GTAWorldRenderer.Scenes.Loaders
{

   class SceneLoader
   {
      private static Log Logger = Log.Instance;


      public Scene LoadScene()
      {
         using (Logger.EnterTimingStage("Loading scene"))
         {
            var sceneObjectsLoader = new SceneObjectsLoader();
            var sceneObjects = sceneObjectsLoader.LoadScene();
            // TODO :: reset TextureStorage
            var scene = CreateScene(sceneObjects);
            GC.Collect();
            return scene;
         }
      }


      private Scene CreateScene(RawSceneObjectsList rawSceneData)
      {
         var scene = new Scene();

         Action<List<RawSceneObject>, List<CompiledSceneObject>> CompileObjecstList =
            delegate(List<RawSceneObject> rawObjects, List<CompiledSceneObject> compiledObjects)
            {
               foreach (var obj in rawObjects)
               {
                  var compiledObject = Model3dFactory.CreateModel(obj.Model);
                  compiledObjects.Add(new CompiledSceneObject(compiledObject, obj.WorldMatrix));
               }
            };

         CompileObjecstList(rawSceneData.LowDetailedObjects, scene.LowDetailedObjects);
         CompileObjecstList(rawSceneData.HighDetailedObjects, scene.HighDetailedObjects);

         scene.ShadowsStartIdx = rawSceneData.ShadowsStartIdx;
         scene.Grid = new Grid(rawSceneData);
         scene.Grid.Build();
         return scene;
      }


      void PrintMemoryUsed(Scene scene)
      {
         int totalIndexBufferBytes = 0;
         int totalVertexBufferBytes = 0;

         Action<CompiledSceneObject> CalculateMemoryForObj = delegate(CompiledSceneObject obj)
         {
            int curVertSize, curIndSize;
            obj.Model.GetMemoryUsed(out curVertSize, out curIndSize);
            totalIndexBufferBytes += curIndSize;
            totalVertexBufferBytes += curVertSize;
         };

         scene.HighDetailedObjects.ForEach(CalculateMemoryForObj);
         scene.LowDetailedObjects.ForEach(CalculateMemoryForObj);

         Action<string, int> PrintInfo = delegate(string msg, int bytes)
         {
            double mb = bytes / (1024.0 * 11024.0);
            Logger.Print(String.Format("... {0}: {1} bytes ({2:f2} MegaBytes)", msg, bytes, mb));
         };

         Logger.Print("Memory used:");
         PrintInfo("vertex buffers", totalVertexBufferBytes);
         PrintInfo("index buffers", totalIndexBufferBytes);
         PrintInfo("textures", TexturesStorage.Instance.GetMemoryUsed());
      }



   }

}
