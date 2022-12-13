using System.Collections.Generic;
using UnityEngine;

public class SplitRamp : Ramp {
  public float entryWidth = 1.2f;
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

  protected override Bounds GetLocalBounds () {
    return new Bounds(new Vector3(0.0f, -height * 0.5f, totalLength * 0.5f + lipRadius),
      new Vector3(totalWidth, height, totalLength));
  }

  protected override void PopulateMeshBuilder (MeshBuilder builder) {
    var innerZ = farZ - lipRadius - innerFarX / Mathf.Tan(halfAngle);
    builder
      .AddContinuousLip(
        new Vector3(-entryWidth * 0.5f, 0.0f, 0.0f), Vector3.up,
        new Vector3(-entryWidth * 0.5f, 0.0f, lipRadius), Vector3.up,
        new Vector3(-centralWidth * 0.5f, 0.0f, lipRadius + legLength), Vector3.up,
        new Vector3(-outerFarX, 0.0f, farZ - lipRadius), Vector3.up,
        new Vector3(-outerFarX, 0.0f, farZ), Vector3.up)
      .AddContinuousLip(
        new Vector3(-innerFarX, 0.0f, farZ), Vector3.up,
        new Vector3(-innerFarX, 0.0f, farZ - lipRadius), Vector3.up,
        new Vector3(0.0f, 0.0f, innerZ), Vector3.up,
        new Vector3(innerFarX, 0.0f, farZ - lipRadius), Vector3.up,
        new Vector3(innerFarX, 0.0f, farZ), Vector3.up)
      .AddContinuousLip(
        new Vector3(outerFarX, 0.0f, farZ), Vector3.up,
        new Vector3(outerFarX, 0.0f, farZ - lipRadius), Vector3.up,
        new Vector3(centralWidth * 0.5f, 0.0f, lipRadius + legLength), Vector3.up,
        new Vector3(entryWidth * 0.5f, 0.0f, lipRadius), Vector3.up,
        new Vector3(entryWidth * 0.5f, 0.0f, 0.0f), Vector3.up)
      .AddQuads(
        new Vector3(entryWidth * 0.5f + lipRadius, 0.0f, lipRadius),
        new Vector3(-entryWidth * 0.5f - lipRadius, 0.0f, lipRadius),
        new Vector3(-centralWidth * 0.5f - lipRadius, 0.0f, lipRadius + legLength),
        new Vector3(centralWidth * 0.5f + lipRadius, 0.0f, lipRadius + legLength));
  }

  protected override void PopulateCutouts (List<Cutout> cutouts) {
    cutouts.AddRange(new[] {
      new Cutout(new Vector3(entryWidth * 0.5f, 0.0f, 0.0f), new Vector3(-entryWidth * 0.5f, 0.0f, 0.0f)),
      new Cutout(new Vector3(-outerFarX, 0.0f, farZ), new Vector3(-innerFarX, 0.0f, farZ)),
      new Cutout(new Vector3(innerFarX, 0.0f, farZ), new Vector3(outerFarX, 0.0f, farZ))});
  }
}
