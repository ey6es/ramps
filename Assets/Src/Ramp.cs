using System.Linq;
using UnityEngine;

public abstract class Ramp : MonoBehaviour {
  public float railHeight = 1.0f;

  Material material;

  public void SetMaterial (Material material) {
    this.material = material;
    if (TryGetComponent<MeshRenderer>(out var meshRenderer)) meshRenderer.material = material;
  }

  void OnValidate () {
    Invoke("Regenerate", 0.0f);
  }

  void Regenerate () {
    var meshFilter = GetComponent<MeshFilter>();
    if (!meshFilter) {
      meshFilter = gameObject.AddComponent<MeshFilter>();
      meshFilter.sharedMesh = new Mesh();
    }
    PopulateVisibleMesh(meshFilter.sharedMesh);
    meshFilter.sharedMesh.RecalculateBounds();

    var meshRenderer = GetComponent<MeshRenderer>();
    if (!meshRenderer) {
      meshRenderer = gameObject.AddComponent<MeshRenderer>();
      meshRenderer.material = material;
    }

    var meshCollider = GetComponent<MeshCollider>();
    if (!meshCollider) {
      meshCollider = gameObject.AddComponent<MeshCollider>();
      meshCollider.sharedMesh = new Mesh();
    }
    PopulateCollisionMesh(meshCollider.sharedMesh);
    meshCollider.sharedMesh.RecalculateBounds();
  }

  void OnDestroy () {
    if (TryGetComponent<MeshFilter>(out var meshFilter)) Object.Destroy(meshFilter.sharedMesh);
    if (TryGetComponent<MeshCollider>(out var meshCollider)) Object.Destroy(meshCollider.sharedMesh);
  }

  protected abstract void PopulateVisibleMesh (Mesh mesh);

  protected abstract void PopulateCollisionMesh (Mesh mesh);

  protected static void PopulateMesh (Mesh mesh, MeshTopology meshTopology, Vector3[] vertices) {
    mesh.SetVertices(vertices);
    mesh.SetIndices(Enumerable.Range(0, vertices.Length).ToArray(), meshTopology, 0);
    mesh.RecalculateNormals();
  }
}
