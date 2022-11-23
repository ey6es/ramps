using UnityEditor;
using UnityEngine;

public class RampMenu : MonoBehaviour {

  [MenuItem("GameObject/Ramp/Platform")]
  static void CreatePlatform (MenuCommand menuCommand) {
    var gameObject = new GameObject("Platform");
    var platform = gameObject.AddComponent<Platform>();
    foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra")) {
      if (obj.name.Equals("Default-Material")) platform.SetMaterial(obj as Material);
    }
    GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
    Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
    Selection.activeObject = gameObject;
  }
}
