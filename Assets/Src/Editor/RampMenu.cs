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

  [MenuItem("GameObject/Ramp/Scale")]
  static void CreateScaleRamp (MenuCommand menuCommand) {
    CreateRamp<ScaleRamp>(menuCommand, "Scale Ramp");
  }

  static void CreateRamp<T> (MenuCommand menuCommand, string name) where T : Ramp {
    var gameObject = new GameObject(name);
    gameObject.layer = LayerMask.NameToLayer("Ramp");
    var platform = gameObject.AddComponent<T>();
    platform.SetMaterial(AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Ramp.mat"));
    GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
    Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
    Selection.activeObject = gameObject;
  }
}
