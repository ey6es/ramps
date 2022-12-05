using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CylinderRamp : Ramp {
  public float width = 2.0f;
  public float radius = 0.5f;
  public int angle = 180;
  public int detail = 16;

  float halfWidth => width * 0.5f;
  int divisions => System.Math.Max(1, (int)(radius * Mathf.Abs(angle * Mathf.Deg2Rad) * detail));

  protected override Bounds GetLocalBounds () {
    return new Bounds(
      new Vector3(0.0f, angle > 0 ? -radius : radius, lipRadius), new Vector3(width, 2.0f * radius, 2.0f * radius));
  }

  protected override void PopulateMeshBuilder (MeshBuilder builder) {
    if (angle == 0) return;

    var verticesAndNormals = new List<Vector3>();
    var leftVerticesAndUps = new LinkedList<Vector3>();
    var rightVerticesAndUps = new LinkedList<Vector3>();

    var leftX = -halfWidth - lipRadius;
    var rightX = halfWidth + lipRadius;
    leftVerticesAndUps.AddLast(new Vector3(-halfWidth, 0.0f, 0.0f));
    leftVerticesAndUps.AddLast(new Vector3(0.0f, 1.0f, 0.0f));
    rightVerticesAndUps.AddFirst(new Vector3(0.0f, 1.0f, 0.0f));
    rightVerticesAndUps.AddFirst(new Vector3(halfWidth, 0.0f, 0.0f));
    
    var totalRads = Mathf.Abs(angle * Mathf.Deg2Rad);
    var sign = Mathf.Sign(angle);
    for (var ii = 0; ii <= divisions; ++ii) {
      var rads = ii * totalRads / divisions;
      var sinr = Mathf.Sin(rads);
      var cosr = Mathf.Cos(rads);
      var y = radius * (cosr - 1.0f) * sign;
      var z = lipRadius + radius * sinr;
      var normal = new Vector3(0.0f, cosr, sinr * sign);

      verticesAndNormals.Add(new Vector3(rightX, y, z));
      verticesAndNormals.Add(normal);

      verticesAndNormals.Add(new Vector3(leftX, y, z));
      verticesAndNormals.Add(normal);

      leftVerticesAndUps.AddLast(new Vector3(-halfWidth, y, z));
      leftVerticesAndUps.AddLast(normal);

      rightVerticesAndUps.AddFirst(normal);
      rightVerticesAndUps.AddFirst(new Vector3(halfWidth, y, z));
      
      if (ii == divisions) {
        var extendedY = y - lipRadius * sinr * sign;
        var extendedZ = z + lipRadius * cosr;
        leftVerticesAndUps.AddLast(new Vector3(-halfWidth, extendedY, extendedZ));
        leftVerticesAndUps.AddLast(normal);
        rightVerticesAndUps.AddFirst(normal);
        rightVerticesAndUps.AddFirst(new Vector3(halfWidth, extendedY, extendedZ));
      }
    }
    builder
      .AddQuadStrip(verticesAndNormals.ToArray())
      .AddCurvedLip(leftVerticesAndUps.ToArray())
      .AddCurvedLip(rightVerticesAndUps.ToArray());
  }

  protected override void PopulateCutouts (List<Cutout> cutouts) {
    if (angle == 0) return;

    var rads = Mathf.Abs(angle * Mathf.Deg2Rad);
    var sinr = Mathf.Sin(rads);
    var cosr = Mathf.Cos(rads);
    var y = (radius * (cosr - 1.0f) - lipRadius * sinr) * Mathf.Sign(angle);
    var z = radius * sinr + lipRadius * (1.0f + cosr);
    cutouts.AddRange(new[] {
      new Cutout(new Vector3(halfWidth, 0.0f, 0.0f), new Vector3(-halfWidth, 0.0f, 0.0f)),
      new Cutout(new Vector3(-halfWidth, y, z), new Vector3(halfWidth, y, z))});
  }
}
