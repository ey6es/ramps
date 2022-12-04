using UnityEditor;
using UnityEngine;

public class RampMenu : MonoBehaviour {

  [MenuItem("GameObject/Ramp/Platform")]
  static void CreatePlatform (MenuCommand menuCommand) {
    CreateRamp<Platform>(menuCommand, "Platform");
  }

  [MenuItem("GameObject/Ramp/Cylinder")]
  static void CreateCylinderRamp (MenuCommand menuCommand) {
    CreateRamp<CylinderRamp>(menuCommand, "Cylinder Ramp");
  }

  static void CreateRamp<T> (MenuCommand menuCommand, string name) where T : Ramp {
    var gameObject = new GameObject(name);
    gameObject.layer = LayerMask.NameToLayer("Ramp");
    var platform = gameObject.AddComponent<T>();
    foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra")) {
      if (obj.name.Equals("Default-Material")) platform.SetMaterial(obj as Material);
    }
    GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
    Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
    Selection.activeObject = gameObject;
  }
}
