using System.Collections.Generic;
using UnityEngine;

public class CylinderRamp : Ramp {
  public float width = 2.0f;
  public float radius = 0.5f;
  public int angle = 180;
  public int detail = 16;

  int divisions => (int)(radius * Mathf.Abs(angle * Mathf.Deg2Rad) * detail);

  protected override Bounds GetLocalBounds () {
    return new Bounds(new Vector3(0.0f, angle > 0 ? -radius : radius, 0.0f), new Vector3(width, 2.0f * radius, 2.0f * radius));
  }

  protected override void PopulateMeshBuilder (MeshBuilder builder) {
    var verticesAndNormals = new List<Vector3>();
    var totalRads = Mathf.Abs(angle * Mathf.Deg2Rad);
    var halfWidth = width * 0.5f;
    var sign = Mathf.Sign(angle);
    for (var ii = 0; ii <= divisions; ++ii) {
      var rads = ii * totalRads / divisions;
      var sinr = Mathf.Sin(rads);
      var cosr = Mathf.Cos(rads);

      verticesAndNormals.Add(new Vector3(halfWidth, radius * (cosr - 1.0f) * sign, radius * sinr));
      verticesAndNormals.Add(new Vector3(0.0f, cosr, sinr * sign));

      verticesAndNormals.Add(new Vector3(-halfWidth, radius * (cosr - 1.0f) * sign, radius * sinr));
      verticesAndNormals.Add(new Vector3(0.0f, cosr, sinr * sign));
    }
    builder.AddQuadStrip(verticesAndNormals.ToArray());
  }

  protected override void PopulateCutouts (List<Cutout> cutouts) {
    var rads = Mathf.Abs(angle * Mathf.Deg2Rad);
    var y = radius * (Mathf.Cos(rads) - 1.0f) * Mathf.Sign(angle);
    var z = radius * Mathf.Sin(rads);
    cutouts.AddRange(new[] {
      new Cutout(new Vector3(width * 0.5f, 0.0f, 0.0f), new Vector3(-width * 0.5f, 0.0f, 0.0f)),
      new Cutout(new Vector3(-width * 0.5f, y, z), new Vector3(width * 0.5f, y, z))});
  }
}
