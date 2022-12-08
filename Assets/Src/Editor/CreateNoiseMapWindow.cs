using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateNoiseMapWindow : EditorWindow {
  int size = 512;
  float scale = 0.25f;
  Texture2D texture;

  [MenuItem("Assets/Create/Noise Map")]
  static void CreateNoiseMap () {
    EditorWindow.GetWindow<CreateNoiseMapWindow>(true, "Create Noise Map", true);
  }

  void OnGUI () {
    var newSize = EditorGUILayout.IntField("Size", size);
    var newScale = EditorGUILayout.FloatField("Scale", scale);
    if (newSize != size || newScale != scale || texture == null) {
      size = newSize;
      scale = newScale;
      CreateTexture();
    }
    var textureRect = EditorGUILayout.GetControlRect(GUILayout.Width(size), GUILayout.Height(size));
    EditorGUI.DrawPreviewTexture(textureRect, texture);
    if (GUILayout.Button("Save")) {
      var path = EditorUtility.SaveFilePanelInProject("Save map", "Noise Map.png", "png", "Select the filename to save as");
      if (path.Length > 0) File.WriteAllBytesAsync(path, texture.EncodeToPNG());
    }
  }

  void CreateTexture () {
    if (texture) Object.DestroyImmediate(texture);
    texture = new Texture2D(size, size, TextureFormat.RGB24, false, false);
    var colors = new Color[size * size];
    for (var row = 0; row < size; ++row) {
      for (var col = 0; col < size; ++col) {
        var normal = (new Vector3(0.0f, 0.0f, 1.0f) + Random.insideUnitSphere * scale).normalized * 0.5f +
          new Vector3(0.5f, 0.5f, 0.5f);
        colors[row * size + col] = new Color(normal.x, normal.y, normal.z, 1.0f);
      }
    }
    texture.SetPixels(colors);
    texture.Apply();
  }
}
