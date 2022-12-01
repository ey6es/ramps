using System.Collections.Generic;
using UnityEngine;

public class Platform : Ramp {
  public Vector3 size = new Vector3(20.0f, 0.25f, 20.0f);
  
  Vector3 halfSize => size * 0.5f;

  protected override Bounds GetLocalBounds () {
    return new Bounds(new Vector3(0.0f, -halfSize.y, 0.0f), size);
  }

  protected override void PopulateMesh (Mesh mesh) {

    new MeshBuilder(this)
      .AddQuads(
        new Vector3(-halfSize.x, -size.y, -halfSize.z),
        new Vector3(halfSize.x, -size.y, -halfSize.z),
        new Vector3(halfSize.x, -size.y, halfSize.z),
        new Vector3(-halfSize.x, -size.y, halfSize.z),

        new Vector3(-halfSize.x, -size.y, -halfSize.z),
        new Vector3(-halfSize.x, -size.y, halfSize.z),
        new Vector3(-halfSize.x, 0.0f, halfSize.z),
        new Vector3(-halfSize.x, 0.0f, -halfSize.z),

        new Vector3(-halfSize.x, -size.y, halfSize.z),
        new Vector3(halfSize.x, -size.y, halfSize.z),
        new Vector3(halfSize.x, 0.0f, halfSize.z),
        new Vector3(-halfSize.x, 0.0f, halfSize.z),

        new Vector3(halfSize.x, -size.y, halfSize.z),
        new Vector3(halfSize.x, -size.y, -halfSize.z),
        new Vector3(halfSize.x, 0.0f, -halfSize.z),
        new Vector3(halfSize.x, 0.0f, halfSize.z),

        new Vector3(halfSize.x, -size.y, -halfSize.z),
        new Vector3(-halfSize.x, -size.y, -halfSize.z),
        new Vector3(-halfSize.x, 0.0f, -halfSize.z),
        new Vector3(halfSize.x, 0.0f, -halfSize.z),

        new Vector3(-halfSize.x, 0.0f, -halfSize.z),
        new Vector3(-halfSize.x, 0.0f, halfSize.z),
        new Vector3(halfSize.x, 0.0f, halfSize.z),
        new Vector3(halfSize.x, 0.0f, -halfSize.z))
      .AddRail(true,
        new Vector3(-halfSize.x, -size.y, -halfSize.z),
        new Vector3(halfSize.x, -size.y, -halfSize.z),
        new Vector3(halfSize.x, -size.y, halfSize.z),
        new Vector3(-halfSize.x, -size.y, halfSize.z))
      .AddRail(true,
        new Vector3(-halfSize.x, 0.0f, -halfSize.z),
        new Vector3(-halfSize.x, 0.0f, halfSize.z),
        new Vector3(halfSize.x, 0.0f, halfSize.z),
        new Vector3(halfSize.x, 0.0f, -halfSize.z))
      .Populate(mesh);
  }

  protected override void PopulateCutouts (List<Cutout> cutouts) {
    cutouts.AddRange(new[] {
      new Cutout(new Vector3(-halfSize.x, 0.0f, -halfSize.z), new Vector3(-halfSize.x, 0.0f, halfSize.z)),
      new Cutout(new Vector3(-halfSize.x, 0.0f, halfSize.z), new Vector3(halfSize.x, 0.0f, halfSize.z)),
      new Cutout(new Vector3(halfSize.x, 0.0f, halfSize.z), new Vector3(halfSize.x, 0.0f, -halfSize.z)),
      new Cutout(new Vector3(halfSize.x, 0.0f, -halfSize.z), new Vector3(-halfSize.x, 0.0f, -halfSize.z)),
      
      new Cutout(new Vector3(-halfSize.x, -size.y, -halfSize.z), new Vector3(halfSize.x, -size.y, -halfSize.z)),
      new Cutout(new Vector3(halfSize.x, -size.y, -halfSize.z), new Vector3(halfSize.x, -size.y, halfSize.z)),
      new Cutout(new Vector3(halfSize.x, -size.y, halfSize.z), new Vector3(-halfSize.x, -size.y, halfSize.z)),
      new Cutout(new Vector3(-halfSize.x, -size.y, halfSize.z), new Vector3(-halfSize.x, -size.y, -halfSize.z))});
  }
}
