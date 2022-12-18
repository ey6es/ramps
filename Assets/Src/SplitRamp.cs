using System.Collections.Generic;
using UnityEngine;

public class SplitRamp : Ramp {
  public float entryWidth = 1.1f;
  public float centralWidth = 1.1f;
  public float height = 0.5f;
  public float legLength = 2.0f;
  public int spreadAngle = 90;

  float halfAngle => spreadAngle * 0.5f * Mathf.Deg2Rad;
  float outerHypot => legLength + centralWidth * Mathf.Tan(halfAngle);
  float totalWidth => centralWidth + 2.0f * Mathf.Sin(halfAngle) * outerHypot;
  float outerFarX => totalWidth * 0.5f;
  float innerFarX => outerFarX - centralWidth * (Mathf.Cos(halfAngle) + Mathf.Sin(halfAngle) * Mathf.Tan(halfAngle));
  float totalLength => legLength + Mathf.Cos(halfAngle) * outerHypot;
  float farZ => lipRadius * 2.0f + totalLength;

  class TraverserData {
    public bool lastFront;
    public Rect targetRect;

    public TraverserData (bool lastFront, Rect targetRect) {
      this.lastFront = lastFront;
      this.targetRect = targetRect;
    }
  }

  public override object OnTraverserEnter (RampTraverser traverser) {
    var midpoint = GetMidpoint(traverser);
    var localPosition = transform.InverseTransformPoint(traverser.transform.position);
    return new TraverserData(localPosition.z <= midpoint, traverser.GetComponentInChildren<Camera>().rect);
  }

  public override void OnTraverserStay (RampTraverser traverser, RaycastHit hitInfo, object data) {
    base.OnTraverserStay(traverser, hitInfo, data);
    var traverserData = (TraverserData)data;
    var lastFront = traverserData.lastFront;

    var midpoint = GetMidpoint(traverser);
    var localPosition = transform.InverseTransformPoint(traverser.transform.position);
    if (localPosition.z > midpoint) {
      traverserData.lastFront = false;
      if (traverser.counterparts.Count > 0) {
        var other = traverser.counterparts.Peek();
        if (other.counterparts.Peek() == traverser && other.ramp == this) {
          traverser.GetComponent<CharacterController>().detectCollisions = false;

          // align the two counterparts
          if (traverser.creationOrder > other.creationOrder) {
            other.transform.position = transform.TransformPoint(localPosition.FlipX());
          }
          return;
        }
      }
      if (lastFront) {
        // split into counterparts
        traverser.GetComponent<CharacterController>().detectCollisions = false;
        var younger = Instantiate(traverser.gameObject).GetComponent<RampTraverser>();
        Destroy(younger.GetComponentInChildren<AudioListener>());
        younger.counterparts.Push(traverser);
        traverser.counterparts.Push(younger);
        younger.UpdateRamp();
        var camera = traverser.GetComponentInChildren<Camera>();
        var rect = camera.rect;
        if (camera.pixelWidth > camera.pixelHeight) {
          var halfWidth = rect.width * 0.5f;
          traverserData.targetRect = new Rect(rect.x, rect.y, halfWidth, rect.height);
          ((TraverserData)younger.rampData).targetRect = new Rect(rect.x + halfWidth, rect.y, halfWidth, rect.height);
        } else {
          var halfHeight = rect.height * 0.5f;
          traverserData.targetRect = new Rect(rect.x, rect.y, rect.width, halfHeight);
          ((TraverserData)younger.rampData).targetRect = new Rect(rect.x, rect.y + halfHeight, rect.width, halfHeight);
        }
      } else {
        localPosition.z = Mathf.Max(localPosition.z, farZ - lipRadius * 2.0f);
        traverser.transform.position = transform.TransformPoint(localPosition);
      }
    } else {
      traverserData.lastFront = true;
      if (traverser.counterparts.Count > 0) {
        var other = traverser.counterparts.Peek();
        if (other.counterparts.Peek() == traverser && other.ramp == this) {
          // merge the two counterparts
          var (older, younger) = traverser.creationOrder < other.creationOrder ? (traverser, other) : (other, traverser);
          var olderData = (TraverserData)older.rampData;
          var youngerRect = ((TraverserData)younger.rampData).targetRect;
          var min = Vector2.Min(olderData.targetRect.min, youngerRect.min);
          var max = Vector2.Max(olderData.targetRect.max, youngerRect.max);
          olderData.targetRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
          older.GetComponentInChildren<Camera>().rect = olderData.targetRect;
          older.counterparts.Pop();
          Destroy(younger.gameObject);
        }
      }
    }
  }

  public override void OnTraverserExit (RampTraverser traverser, object data) {
    traverser.GetComponent<CharacterController>().detectCollisions = true;
    traverser.GetComponentInChildren<Camera>().rect = ((TraverserData)data).targetRect;
  }

  float GetMidpoint (RampTraverser traverser) {
    return lipRadius + legLength + traverser.GetComponent<CharacterController>().radius;
  }

  protected override Bounds GetLocalBounds () {
    return new Bounds(new Vector3(0.0f, -height * 0.5f, totalLength * 0.5f + lipRadius),
      new Vector3(totalWidth, height, totalLength));
  }

  protected override void PopulateMeshBuilder (MeshBuilder builder) {
    var nearLeft = new Vector3(-entryWidth * 0.5f, 0.0f, lipRadius);
    var middleLeft = new Vector3(-centralWidth * 0.5f, 0.0f, lipRadius + legLength);
    var farLeft = new Vector3(-outerFarX, 0.0f, farZ - lipRadius);
    var nearLeftEdge = GetEdgePoint(null, nearLeft, middleLeft);
    var middleLeftEdge = GetEdgePoint(nearLeft, middleLeft, farLeft);
    var farLeftEdge = GetEdgePoint(middleLeft, farLeft, null);

    var leftInner = new Vector3(-innerFarX, 0.0f, farZ - lipRadius);
    var middleInner = new Vector3(0.0f, 0.0f, farZ - lipRadius - innerFarX / Mathf.Tan(halfAngle));
    var leftInnerEdge = GetEdgePoint(null, leftInner, middleInner);
    var middleInnerEdge = GetEdgePoint(leftInner, middleInner, new Vector3(-leftInner.x, leftInner.y, leftInner.z));

    builder
      .AddLip(
        new Vector3(-entryWidth * 0.5f, 0.0f, 0.0f), Vector3.up,
        nearLeft, Vector3.up,
        middleLeft, Vector3.up,
        farLeft, Vector3.up,
        new Vector3(-outerFarX, 0.0f, farZ), Vector3.up)
      .AddLip(
        new Vector3(-innerFarX, 0.0f, farZ), Vector3.up,
        leftInner, Vector3.up,
        middleInner, Vector3.up,
        leftInner.FlipX(), Vector3.up,
        new Vector3(innerFarX, 0.0f, farZ), Vector3.up)
      .AddLip(
        new Vector3(outerFarX, 0.0f, farZ), Vector3.up,
        farLeft.FlipX(), Vector3.up,
        middleLeft.FlipX(), Vector3.up,
        nearLeft.FlipX(), Vector3.up,
        new Vector3(entryWidth * 0.5f, 0.0f, 0.0f), Vector3.up)
      .AddQuads(
        nearLeftEdge, middleLeftEdge, middleLeftEdge.FlipX(), nearLeftEdge.FlipX(),
        middleLeftEdge, farLeftEdge, leftInnerEdge, middleInnerEdge,
        middleInnerEdge, leftInnerEdge.FlipX(), farLeftEdge.FlipX(), middleLeftEdge.FlipX())
      .AddTriangles(middleLeftEdge, middleInnerEdge, middleLeftEdge.FlipX());
    if (height > 0.0f) {
      var down = new Vector3(0.0f, -height, 0.0f);
      builder
        .AddQuads(
          nearLeftEdge.FlipX() + down, middleLeftEdge.FlipX() + down, middleLeftEdge + down, nearLeftEdge + down,
          middleInnerEdge + down, leftInnerEdge + down, farLeftEdge + down, middleLeftEdge + down,
          middleLeftEdge.FlipX() + down, farLeftEdge.FlipX() + down, leftInnerEdge.FlipX() + down, middleInnerEdge + down,
          nearLeftEdge, nearLeftEdge + down, middleLeftEdge + down, middleLeftEdge,
          middleLeftEdge, middleLeftEdge + down, farLeftEdge + down, farLeftEdge,
          farLeftEdge.FlipX(), farLeftEdge.FlipX() + down, middleLeftEdge.FlipX() + down, middleLeftEdge.FlipX(),
          middleLeftEdge.FlipX(), middleLeftEdge.FlipX() + down, nearLeftEdge.FlipX() + down, nearLeftEdge.FlipX(),
          leftInnerEdge, leftInnerEdge + down, middleInnerEdge + down, middleInnerEdge,
          middleInnerEdge, middleInnerEdge + down, leftInnerEdge.FlipX() + down, leftInnerEdge.FlipX())
        .AddTriangles(middleLeftEdge.FlipX() + down, middleInnerEdge + down, middleLeftEdge + down);
    }
  }

  Vector3 GetEdgePoint (Vector3? prev, Vector3 current, Vector3? next) {
    var dir = (next.HasValue ? next.Value - current : current - prev.Value).normalized;
    var plane = new Plane(prev.HasValue && next.HasValue ? (next.Value - prev.Value).normalized : Vector3.forward, current);
    var left = Vector3.Cross(dir, Vector3.up);
    var ray = new Ray(current + left * lipRadius, dir);
    plane.Raycast(ray, out var enter);
    return ray.GetPoint(enter);
  }

  protected override void PopulateCutouts (List<Cutout> cutouts) {
    cutouts.AddRange(new[] {
      new Cutout(new Vector3(entryWidth * 0.5f, 0.0f, 0.0f), new Vector3(-entryWidth * 0.5f, 0.0f, 0.0f)),
      new Cutout(new Vector3(-outerFarX, 0.0f, farZ), new Vector3(-innerFarX, 0.0f, farZ)),
      new Cutout(new Vector3(innerFarX, 0.0f, farZ), new Vector3(outerFarX, 0.0f, farZ))});
  }
}
