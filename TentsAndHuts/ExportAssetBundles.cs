using UnityEngine;
using UnityEditor;

public class ExportAssetBundles {
	[MenuItem("Assets/Build AssetBundle From Selection - Track dependencies")]
	static void ExportResource () {
		// Bring up save panel
		string path = EditorUtility.SaveFilePanel ("Save Resource", @"C:\Users\Jeff\Documents\GitHub\SDXMods_Development\TentsAndHuts\Resources", "New Resource", "unity3d");
		if (path.Length != 0) {
			// Build the resource file from the active selection.
			Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
			BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, 
			                               BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
			Selection.objects = selection;
		}
	}
	
}