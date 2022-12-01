using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public abstract class Ramp : MonoBehaviour {
  public float railWidth = 0.1f;
  public float railHeight = 0.1f;

  Material material;
  Bounds? lastRebuildBounds;

  public void SetMaterial (Material material) {
    this.material = material;
    if (TryGetComponent<MeshRenderer>(out var meshRenderer)) meshRenderer.material = material;
  }

  void Awake () {
    if (Application.isEditor) EditorApplication.update += OnUpdate;
  }

  void OnValidate () {
    Invoke("Regenerate", 0.0f);
  }

  void Regenerate () {
    Rebuild();
    RebuildOverlapping();
  }

  void Rebuild () {
    var meshFilter = GetComponent<MeshFilter>();
    if (!meshFilter) {
      meshFilter = gameObject.AddComponent<MeshFilter>();
      meshFilter.sharedMesh = new Mesh();
    }
    PopulateMesh(meshFilter.sharedMesh);
    meshFilter.sharedMesh.RecalculateBounds();

    var meshRenderer = GetComponent<MeshRenderer>();
    if (!meshRenderer) {
      meshRenderer = gameObject.AddComponent<MeshRenderer>();
      meshRenderer.material = material;
    }

    var meshCollider = GetComponent<MeshCollider>();
    if (!meshCollider) {
      meshCollider = gameObject.AddComponent<MeshCollider>();
      meshCollider.sharedMesh = meshFilter.sharedMesh;
    }
  }

  void OnDestroy () {
    RebuildOverlapping();
    if (TryGetComponent<MeshFilter>(out var meshFilter)) {
      if (Application.isEditor) Object.DestroyImmediate(meshFilter.sharedMesh);
      else Object.Destroy(meshFilter.sharedMesh);
    }
    if (Application.isEditor) EditorApplication.update -= OnUpdate;
  }

  void OnUpdate () {
    if (transform.hasChanged) {
      RebuildOverlapping();
      transform.hasChanged = false;
    }
  }

  void RebuildOverlapping () {
    var bounds = GetWorldBounds();

    // if last and current intersect, rebuild the union; otherwise, rebuild last then current
    if (lastRebuildBounds.HasValue) {
      if (lastRebuildBounds.Value.Intersects(bounds)) {
        lastRebuildBounds.Value.Encapsulate(bounds);
        RebuildOverlapping(lastRebuildBounds.Value);
      } else {
        RebuildOverlapping(lastRebuildBounds.Value);
        RebuildOverlapping(bounds);
      }
    } else RebuildOverlapping(bounds);

    lastRebuildBounds = bounds;
  }

  void RebuildOverlapping (Bounds bounds) {
    var colliders = Physics.OverlapBox(
      bounds.center, bounds.extents, Quaternion.identity, gameObject.layer, QueryTriggerInteraction.Ignore);
    foreach (var collider in colliders) {
      var ramp = collider.GetComponent<Ramp>();
      if (ramp && ramp != this) ramp.Rebuild();
    }
  }

  protected Bounds GetWorldBounds () {
    var bounds = GetLocalBounds();
    bounds.Expand(Mathf.Max(railWidth, railHeight));
    return transform.TransformBounds(bounds);
  }

  protected abstract Bounds GetLocalBounds ();

  protected abstract void PopulateMesh (Mesh mesh);

  protected struct Cutout {
    public Vector3 start;
    public Vector3 end;

    public Cutout (Vector3 start, Vector3 end) {
      this.start = start;
      this.end = end;
    }

    public void Transform (Transform src, Transform dest) {
      start = dest.InverseTransformPoint(src.TransformPoint(start));
      end = dest.InverseTransformPoint(src.TransformPoint(end));
    }
  }

  protected abstract void PopulateCutouts (List<Cutout> cutouts);

  protected static void PopulateMesh (Mesh mesh, MeshTopology meshTopology, Vector3[] vertices) {
    mesh.SetVertices(vertices);
    mesh.SetIndices(Enumerable.Range(0, vertices.Length).ToArray(), meshTopology, 0);
    mesh.RecalculateNormals();
  }

  protected class MeshBuilder {
    List<Cutout> cutouts = new List<Cutout>();
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<int> indices = new List<int>();

    public MeshBuilder (Ramp outer) {
      var bounds = outer.GetWorldBounds();
      var colliders = Physics.OverlapBox(
        bounds.center, bounds.extents, Quaternion.identity, outer.gameObject.layer, QueryTriggerInteraction.Ignore);
      foreach (var collider in colliders) {
        var ramp = collider.GetComponent<Ramp>();
        if (ramp && ramp != outer) {
          var previousCount = cutouts.Count;
          ramp.PopulateCutouts(cutouts);
          for (var ii = previousCount; ii < cutouts.Count; ++ii) {
            cutouts[ii].Transform(ramp.transform, outer.transform);
          }
        }
      }
    }

    public MeshBuilder AddQuads (params Vector3[] quads) {
      for (var ii = 0; ii < quads.Length; ) {
        var (v0, v1, v2, v3) = (quads[ii++], quads[ii++], quads[ii++], quads[ii++]);
        indices.Add(vertices.Count);
        indices.Add(vertices.Count + 1);
        indices.Add(vertices.Count + 2);
        indices.Add(vertices.Count + 2);
        indices.Add(vertices.Count + 3);
        indices.Add(vertices.Count);

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        var normal = Vector3.Cross(v1 - v0, v3 - v0).normalized;
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
      }
      return this;
    }

    public MeshBuilder AddRail (bool loop, params Vector3[] vertices) {
      return this;
    }

    public void Populate (Mesh mesh) {
      mesh.SetVertices(vertices);
      mesh.SetNormals(normals);
      mesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }
  }
}
