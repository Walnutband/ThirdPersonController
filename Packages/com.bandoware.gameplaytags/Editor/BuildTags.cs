using System.IO;
using UnityEditor.Build;

namespace BandoWare.GameplayTags.Editor
{

   public class BuildTags : BuildPlayerProcessor
   {
      public override int callbackOrder => 0;

      public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
      {
         string tempPath = Path.GetTempPath();

         string customFolder = Path.Combine(tempPath, "BandoWare", "GameplayTags", "StreamingAssets");

         // clear out directory if it exists
         if (Directory.Exists(customFolder))
            Directory.Delete(customFolder, true);

         Directory.CreateDirectory(customFolder);

         GameplayTagManager.ReloadTags();

         string filePath = Path.Combine(customFolder, "GameplayTags");

         FileStream file = File.Create(filePath);
         BinaryWriter writer = new(file);

         foreach (GameplayTag tag in GameplayTagManager.GetAllTags())
         {
            if (!tag.IsLeaf)
               continue;

            writer.Write(tag.Name);
         }

         writer.Close();
         file.Close();

         buildPlayerContext.AddAdditionalPathToStreamingAssets(customFolder);
      }
   }
}