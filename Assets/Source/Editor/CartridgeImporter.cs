using UnityEngine;
using System.IO;


namespace PicoUnity
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "p8")]
    public class CartridgeImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            string txt = File.ReadAllText(ctx.assetPath);

            var cartridge = ScriptableObject.CreateInstance<CartridgeP8>();
            cartridge.ReadData(txt);

            ctx.AddObjectToAsset("cartridge", cartridge);
            ctx.SetMainObject(cartridge);
        }
    }
}
