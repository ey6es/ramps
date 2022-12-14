using System.Collections.Generic;
using UnityEngine;

public class ScaleRamp : Ramp {
  public Vector3 size = new Vector3(2.0f, 1.0f, 2.0f);
  public float scaleFactor = 0.5f;

  Vector3 extents => size * 0.5f;
  Vector3 lipExtents => extents + new Vector3(lipRadius, 0.0f, 0.0f);

  class TraverserData {
    public float initialScale;
    public bool enteredFront;

    public TraverserData (float initialScale, bool enteredFront) {
      this.initialScale = initialScale;
      this.enteredFront = enteredFront;
    }
  }

  public override object OnTraverserEnter (RampTraverser traverser) {
    return new TraverserData(traverser.transform.localScale.x, IsTraverserAtFront(traverser));
  }

  public override void OnTraverserStay (RampTraverser traverser, RaycastHit hitInfo, object data) {
    base.OnTraverserStay(traverser, hitInfo, data);

    UpdateTraverserScale(traverser, (TraverserData)data,
      (transform.InverseTransformPoint(traverser.transform.position).z - lipRadius) / size.z);
  }

  public override void OnTraverserExit (RampTraverser traverser, object data) {
    UpdateTraverserScale(traverser, (TraverserData)data, IsTraverserAtFront(traverser) ? 0.0f : 1.0f);
  }

  void UpdateTraverserScale (RampTraverser traverser, TraverserData data, float alpha) {
    var (startScale, endScale) = data.enteredFront
      ? (data.initialScale, data.initialScale * scaleFactor)
      : (data.initialScale / scaleFactor, data.initialScale);
    var scale = Mathf.Lerp(startScale, endScale, alpha);
    traverser.transform.localScale = new Vector3(scale, scale, scale);
  }

  bool IsTraverserAtFront (RampTraverser traverser) {
    return transform.InverseTransformPoint(traverser.transform.position).z - lipRadius < extents.z;
  }

  protected override Bounds GetLocalBounds () {
    var maxSize = size;
    if (scaleFactor > 1.0f) maxSize = new Vector3(size.x * scaleFactor, size.y * scaleFactor, size.z);
    return new Bounds(new Vector3(0.0f, -extents.y, extents.z + lipRadius), new Vector3(maxSize.x, maxSize.y, size.z));
  }

  protected override void PopulateMeshBuilder (MeshBuilder builder) {
    var farX = lipExtents.x * scaleFactor;
    var farZ = lipRadius + size.z;
    var farLipZ = lipRadius + size.z + lipRadius * scaleFactor;
    builder
      .AddContinuousLip(
        new Vector3(-extents.x, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(-extents.x, 0.0f, lipRadius), new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(-extents.x * scaleFactor, 0.0f, lipRadius + size.z), new Vector3(0.0f, scaleFactor, 0.0f),
        new Vector3(-extents.x * scaleFactor, 0.0f, farLipZ), new Vector3(0.0f, scaleFactor, 0.0f))
      .AddContinuousLip(
        new Vector3(extents.x * scaleFactor, 0.0f, farLipZ), new Vector3(0.0f, scaleFactor, 0.0f),
        new Vector3(extents.x * scaleFactor, 0.0f, lipRadius + size.z), new Vector3(0.0f, scaleFactor, 0.0f),
        new Vector3(extents.x, 0.0f, lipRadius), new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(extents.x, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f))
      .AddQuads(
        new Vector3(lipExtents.x, 0.0f, lipRadius),
        new Vector3(-lipExtents.x, 0.0f, lipRadius),
        new Vector3(-farX, 0.0f, farZ),
        new Vector3(farX, 0.0f, farZ));
    if (size.y > 0.0f) {
      var farY = size.y * scaleFactor;
      builder.AddQuads(
        new Vector3(-lipExtents.x, -size.y, lipRadius),
        new Vector3(lipExtents.x, -size.y, lipRadius),
        new Vector3(farX, -farY, farZ),
        new Vector3(-farX, -farY, farZ),
        
        new Vector3(lipExtents.x, 0.0f, lipRadius),
        new Vector3(farX, 0.0f, farZ),
        new Vector3(farX, -farY, farZ),
        new Vector3(lipExtents.x, -size.y, lipRadius),

        new Vector3(-lipExtents.x, 0.0f, lipRadius),
        new Vector3(-lipExtents.x, -size.y, lipRadius),
        new Vector3(-farX, -farY, farZ),
        new Vector3(-farX, 0.0f, farZ));
    }
  }

  protected override void PopulateCutouts (List<Cutout> cutouts) {
    var farZ = lipRadius + size.z + lipRadius * scaleFactor;
    cutouts.AddRange(new[] {
      new Cutout(new Vector3(extents.x, 0.0f, 0.0f), new Vector3(-extents.x, 0.0f, 0.0f)),
      new Cutout(new Vector3(-extents.x * scaleFactor, 0.0f, farZ), new Vector3(extents.x * scaleFactor, 0.0f, farZ))});
  }
}
