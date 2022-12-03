using System.Collections.Generic;
using UnityEngine;

public class Platform : Ramp {
  public Vector3 size = new Vector3(20.0f, 0.25f, 20.0f);
  
  Vector3 extents => size * 0.5f;
  Vector3 lipExtents => extents + new Vector3(lipRadius, 0.0f, lipRadius);

  protected override Bounds GetLocalBounds () {
    return new Bounds(new Vector3(0.0f, -extents.y, 0.0f), size);
  }

  protected override void PopulateMesh (Mesh mesh) {
    new MeshBuilder(this)
      .AddQuads(
        new Vector3(-lipExtents.x, -size.y, -lipExtents.z),
        new Vector3(lipExtents.x, -size.y, -lipExtents.z),
        new Vector3(lipExtents.x, -size.y, lipExtents.z),
        new Vector3(-lipExtents.x, -size.y, lipExtents.z),

        new Vector3(-lipExtents.x, -size.y, -lipExtents.z),
        new Vector3(-lipExtents.x, -size.y, lipExtents.z),
        new Vector3(-lipExtents.x, 0.0f, lipExtents.z),
        new Vector3(-lipExtents.x, 0.0f, -lipExtents.z),

        new Vector3(-lipExtents.x, -size.y, lipExtents.z),
        new Vector3(lipExtents.x, -size.y, lipExtents.z),
        new Vector3(lipExtents.x, 0.0f, lipExtents.z),
        new Vector3(-lipExtents.x, 0.0f, lipExtents.z),

        new Vector3(lipExtents.x, -size.y, lipExtents.z),
        new Vector3(lipExtents.x, -size.y, -lipExtents.z),
        new Vector3(lipExtents.x, 0.0f, -lipExtents.z),
        new Vector3(lipExtents.x, 0.0f, lipExtents.z),

        new Vector3(lipExtents.x, -size.y, -lipExtents.z),
        new Vector3(-lipExtents.x, -size.y, -lipExtents.z),
        new Vector3(-lipExtents.x, 0.0f, -lipExtents.z),
        new Vector3(lipExtents.x, 0.0f, -lipExtents.z),

        new Vector3(-lipExtents.x, 0.0f, -lipExtents.z),
        new Vector3(-lipExtents.x, 0.0f, lipExtents.z),
        new Vector3(lipExtents.x, 0.0f, lipExtents.z),
        new Vector3(lipExtents.x, 0.0f, -lipExtents.z))
      .AddLip(true,
        new Vector3(-extents.x, -size.y, -extents.z), Vector3.down,
        new Vector3(extents.x, -size.y, -extents.z), Vector3.down,
        new Vector3(extents.x, -size.y, extents.z), Vector3.down,
        new Vector3(-extents.x, -size.y, extents.z), Vector3.down) 
      .AddLip(true,
        new Vector3(-extents.x, 0.0f, -extents.z), Vector3.up,
        new Vector3(-extents.x, 0.0f, extents.z), Vector3.up,
        new Vector3(extents.x, 0.0f, extents.z), Vector3.up,
        new Vector3(extents.x, 0.0f, -extents.z), Vector3.up)
      .Populate(mesh);
  }

  protected override void PopulateCutouts (List<Cutout> cutouts) {
    cutouts.AddRange(new[] {
      new Cutout(new Vector3(-extents.x, 0.0f, -extents.z), new Vector3(-extents.x, 0.0f, extents.z)),
      new Cutout(new Vector3(-extents.x, 0.0f, extents.z), new Vector3(extents.x, 0.0f, extents.z)),
      new Cutout(new Vector3(extents.x, 0.0f, extents.z), new Vector3(extents.x, 0.0f, -extents.z)),
      new Cutout(new Vector3(extents.x, 0.0f, -extents.z), new Vector3(-extents.x, 0.0f, -extents.z)),
      
      new Cutout(new Vector3(-extents.x, -size.y, -extents.z), new Vector3(extents.x, -size.y, -extents.z)),
      new Cutout(new Vector3(extents.x, -size.y, -extents.z), new Vector3(extents.x, -size.y, extents.z)),
      new Cutout(new Vector3(extents.x, -size.y, extents.z), new Vector3(-extents.x, -size.y, extents.z)),
      new Cutout(new Vector3(-extents.x, -size.y, extents.z), new Vector3(-extents.x, -size.y, -extents.z))});
  }
}
