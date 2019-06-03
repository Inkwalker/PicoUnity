using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace PicoUnity
{
    [ScriptedImporter(1, "p8")]
    public class CartridgeImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string txt = File.ReadAllText(ctx.assetPath);

            var cartridge = ScriptableObject.CreateInstance<CartridgeP8>();
            cartridge.ReadData(txt);

            ctx.AddObjectToAsset("cartridge", cartridge);
            ctx.SetMainObject(cartridge);
        }
    }
}
