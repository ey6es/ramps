using UnityEngine;

public class Platform : Ramp {
  public Vector3 size = new Vector3(20.0f, 0.25f, 20.0f);
  
  protected override void PopulateVisibleMesh (Mesh mesh) {
    var halfSize = size * 0.5f;
    PopulateMesh(mesh, MeshTopology.Quads, new[] {
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
      new Vector3(halfSize.x, 0.0f, -halfSize.z),
    });
  }

  protected override void PopulateCollisionMesh (Mesh mesh) {
    var halfSize = size * 0.5f;
    mesh.SetVertices(new[] {
      new Vector3(-halfSize.x, 0.0f, -halfSize.z),
      new Vector3(-halfSize.x, 0.0f, halfSize.z),
      new Vector3(halfSize.x, 0.0f, halfSize.z),
      new Vector3(halfSize.x, 0.0f, -halfSize.z),
      new Vector3(-halfSize.x, railHeight, -halfSize.z),
      new Vector3(-halfSize.x, railHeight, halfSize.z),
      new Vector3(halfSize.x, railHeight, halfSize.z),
      new Vector3(halfSize.x, railHeight, -halfSize.z),
    });
    mesh.SetIndices(new[] {
      0, 1, 2, 3,
      0, 4, 5, 1,
      1, 5, 6, 2,
      2, 6, 7, 3,
      3, 7, 4, 0}, MeshTopology.Quads, 0);
  }
}
