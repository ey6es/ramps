using System.Collections.Generic;
using UnityEngine;

public class ScaleRamp : Ramp {
  public Vector3 size = new Vector3(2.0f, 1.0f, 2.0f);
  public float scaleFactor = 0.5f;

  Vector3 extents => size * 0.5f;
  Vector3 lipExtents => extents + new Vector3(lipRadius, 0.0f, 0.0f);

  protected override Bounds GetLocalBounds () {
    var maxSize = size;
    if (scaleFactor > 1.0f) maxSize = new Vector3(size.x * scaleFactor, size.y * scaleFactor, size.z);
    return new Bounds(
      new Vector3(0.0f, -maxSize.y * 0.5f, size.z * 0.5f + lipRadius), new Vector3(maxSize.x, maxSize.y, size.z));
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
