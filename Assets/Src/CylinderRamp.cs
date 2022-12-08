using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CylinderRamp : Ramp {
  public float width = 2.0f;
  public float height = 0.5f;
  public float radius = 0.5f;
  public int angle = 180;
  public int detail = 16;

  float halfWidth => width * 0.5f;
  int divisions => System.Math.Max(1, (int)(radius * Mathf.Abs(angle * Mathf.Deg2Rad) * detail));
  Vector3 center => new Vector3(0.0f, angle > 0 ? -radius : radius, lipRadius);

  public override void OnTraverserStay (RampTraverser traverser, RaycastHit hitInfo, ref object data) {
    var traverserTransform = traverser.GetComponent<Transform>();
    var localPosition = transform.InverseTransformPoint(traverserTransform.position);
    var sign = Mathf.Sign(angle);
    var normal = new Vector3(0.0f, localPosition.y - center.y, localPosition.z - center.z).normalized * sign;
    var rampPosition = center + normal * radius * sign;
    traverser.Align(
      transform.TransformPoint(new Vector3(localPosition.x, rampPosition.y, rampPosition.z)),
      transform.TransformDirection(normal));
  }

  protected override Bounds GetLocalBounds () {
    return new Bounds(center, new Vector3(width, 2.0f * radius, 2.0f * radius));
  }

  protected override void PopulateMeshBuilder (MeshBuilder builder) {
    if (angle == 0) return;

    var topVerticesAndNormals = new List<Vector3>();
    var bottomVerticesAndNormals = new List<Vector3>();
    var leftVerticesAndNormals = new List<Vector3>();
    var rightVerticesAndNormals = new List<Vector3>();
    var leftLipVerticesAndUps = new LinkedList<Vector3>();
    var rightLipVerticesAndUps = new LinkedList<Vector3>();

    var leftX = -halfWidth - lipRadius;
    var rightX = halfWidth + lipRadius;
    leftLipVerticesAndUps.AddLast(new Vector3(-halfWidth, 0.0f, 0.0f));
    leftLipVerticesAndUps.AddLast(new Vector3(0.0f, 1.0f, 0.0f));
    rightLipVerticesAndUps.AddFirst(new Vector3(0.0f, 1.0f, 0.0f));
    rightLipVerticesAndUps.AddFirst(new Vector3(halfWidth, 0.0f, 0.0f));
    
    var totalRads = Mathf.Abs(angle * Mathf.Deg2Rad);
    var sign = Mathf.Sign(angle);
    for (var ii = 0; ii <= divisions; ++ii) {
      var rads = ii * totalRads / divisions;
      var sinr = Mathf.Sin(rads);
      var cosr = Mathf.Cos(rads);
      var topY = radius * (cosr - 1.0f) * sign;
      var topZ = lipRadius + radius * sinr;
      var normal = new Vector3(0.0f, cosr, sinr * sign);

      topVerticesAndNormals.Add(new Vector3(rightX, topY, topZ));
      topVerticesAndNormals.Add(normal);

      topVerticesAndNormals.Add(new Vector3(leftX, topY, topZ));
      topVerticesAndNormals.Add(normal);

      var negativeNormal = -normal;
      var bottomY = topY + negativeNormal.y * height;
      var bottomZ = topZ + negativeNormal.z * height;
      bottomVerticesAndNormals.Add(new Vector3(leftX, bottomY, bottomZ));
      bottomVerticesAndNormals.Add(negativeNormal);

      bottomVerticesAndNormals.Add(new Vector3(rightX, bottomY, bottomZ));
      bottomVerticesAndNormals.Add(negativeNormal);

      leftVerticesAndNormals.Add(new Vector3(leftX, topY, topZ));
      leftVerticesAndNormals.Add(Vector3.left);

      leftVerticesAndNormals.Add(new Vector3(leftX, bottomY, bottomZ));
      leftVerticesAndNormals.Add(Vector3.left);

      rightVerticesAndNormals.Add(new Vector3(rightX, bottomY, bottomZ));
      rightVerticesAndNormals.Add(Vector3.right);

      rightVerticesAndNormals.Add(new Vector3(rightX, topY, topZ));
      rightVerticesAndNormals.Add(Vector3.right);

      leftLipVerticesAndUps.AddLast(new Vector3(-halfWidth, topY, topZ));
      leftLipVerticesAndUps.AddLast(normal);

      rightLipVerticesAndUps.AddFirst(normal);
      rightLipVerticesAndUps.AddFirst(new Vector3(halfWidth, topY, topZ));
      
      if (ii == divisions) {
        var extendedY = topY - lipRadius * sinr * sign;
        var extendedZ = topZ + lipRadius * cosr;
        leftLipVerticesAndUps.AddLast(new Vector3(-halfWidth, extendedY, extendedZ));
        leftLipVerticesAndUps.AddLast(normal);
        rightLipVerticesAndUps.AddFirst(normal);
        rightLipVerticesAndUps.AddFirst(new Vector3(halfWidth, extendedY, extendedZ));
      }
    }
    builder
      .AddQuadStrip(topVerticesAndNormals.ToArray())
      .AddQuadStrip(bottomVerticesAndNormals.ToArray())
      .AddQuadStrip(leftVerticesAndNormals.ToArray())
      .AddQuadStrip(rightVerticesAndNormals.ToArray())
      .AddContinuousLip(leftLipVerticesAndUps.ToArray())
      .AddContinuousLip(rightLipVerticesAndUps.ToArray());
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
