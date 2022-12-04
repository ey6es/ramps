using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public abstract class Ramp : MonoBehaviour {
  public float lipRadius = 0.1f;
  public int lipDetail = 16;
  public float railHeight = 1.0f;

  Material material;
  Bounds? lastRebuildBounds;

  int lipDivisions => (int)(lipRadius * Mathf.PI * lipDetail);

  public void SetMaterial (Material material) {
    this.material = material;
    if (TryGetComponent<MeshRenderer>(out var meshRenderer)) meshRenderer.material = material;
  }

  void OnEnable () {
    if (Application.isEditor) EditorApplication.update += OnUpdate;
  }

  void OnDisable () {
    if (Application.isEditor) EditorApplication.update -= OnUpdate;
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
    var meshCollider = GetComponent<MeshCollider>();
    if (!meshCollider) {
      meshCollider = gameObject.AddComponent<MeshCollider>();
      meshCollider.sharedMesh = new Mesh();
    }

    var builder = new MeshBuilder(this);
    PopulateMeshBuilder(builder);
    builder.Populate(meshFilter.sharedMesh, meshCollider.sharedMesh);
    meshCollider.sharedMesh = meshCollider.sharedMesh; // force reload
    meshFilter.sharedMesh.RecalculateBounds();
    meshCollider.sharedMesh.RecalculateBounds();

    var meshRenderer = GetComponent<MeshRenderer>();
    if (!meshRenderer) {
      meshRenderer = gameObject.AddComponent<MeshRenderer>();
      meshRenderer.material = material;
    }
  }

  void OnDestroy () {
    RebuildOverlapping();
    if (TryGetComponent<MeshFilter>(out var meshFilter)) {
      if (Application.isEditor) Object.DestroyImmediate(meshFilter.sharedMesh);
      else Object.Destroy(meshFilter.sharedMesh);
    }
  }

  void OnUpdate () {
    if (transform.hasChanged) {
      Regenerate();
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
      bounds.center, bounds.extents, Quaternion.identity, 1 << gameObject.layer, QueryTriggerInteraction.Ignore);
    foreach (var collider in colliders) {
      var ramp = collider.GetComponent<Ramp>();
      if (ramp && ramp != this) ramp.Rebuild();
    }
  }

  protected Bounds GetWorldBounds () {
    var bounds = GetLocalBounds();
    bounds.Expand(lipRadius * 2.0f);
    return transform.TransformBounds(bounds);
  }

  protected abstract Bounds GetLocalBounds ();

  protected abstract void PopulateMeshBuilder (MeshBuilder builder);

  protected struct Cutout {
    public Vector3 start;
    public Vector3 end;

    public float length => (end - start).magnitude;
    public Vector3 dir => (end - start).normalized;

    public Cutout (Vector3 start, Vector3 end) {
      this.start = start;
      this.end = end;
    }

    public Cutout Transform (Transform src, Transform dest) {
      return new Cutout(dest.InverseTransformPoint(src.TransformPoint(start)),
        dest.InverseTransformPoint(src.TransformPoint(end)));
    }
  }

  protected virtual void PopulateCutouts (List<Cutout> cutouts) {
    // nothing by default
  }

  protected class MeshBuilder {
    Ramp outer;
    List<Cutout> cutouts = new List<Cutout>();
    List<Vector3> visibleVertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<int> visibleIndices = new List<int>();
    List<Vector3> collisionVertices = new List<Vector3>();
    List<int> collisionIndices = new List<int>();

    public MeshBuilder (Ramp outer) {
      this.outer = outer;
      var bounds = outer.GetWorldBounds();
      var colliders = Physics.OverlapBox(
        bounds.center, bounds.extents, Quaternion.identity, 1 << outer.gameObject.layer, QueryTriggerInteraction.Ignore);
      foreach (var collider in colliders) {
        var ramp = collider.GetComponent<Ramp>();
        if (ramp && ramp != outer) {
          var previousCount = cutouts.Count;
          ramp.PopulateCutouts(cutouts);
          for (var ii = previousCount; ii < cutouts.Count; ++ii) {
            cutouts[ii] = cutouts[ii].Transform(ramp.transform, outer.transform);
          }
        }
      }
    }

    public MeshBuilder AddQuads (params Vector3[] quadVertices) {
      for (var ii = 0; ii < quadVertices.Length; ) {
        var (v0, v1, v2, v3) = (quadVertices[ii++], quadVertices[ii++], quadVertices[ii++], quadVertices[ii++]);
        AddClockwiseQuadIndices(visibleIndices, visibleVertices.Count);

        visibleVertices.Add(v0);
        visibleVertices.Add(v1);
        visibleVertices.Add(v2);
        visibleVertices.Add(v3);

        var normal = Vector3.Cross(v1 - v0, v3 - v0).normalized;
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);

        AddClockwiseQuadIndices(collisionIndices, collisionVertices.Count);

        collisionVertices.Add(v0);
        collisionVertices.Add(v1);
        collisionVertices.Add(v2);
        collisionVertices.Add(v3);
      }
      return this;
    }

    public MeshBuilder AddQuadStrip (params Vector3[] quadVerticesAndNormals) {
      for (int ii = 0, nn = (quadVerticesAndNormals.Length - 4) / 4; ii < nn; ++ii) {
        AddParallelQuadIndices(visibleIndices, visibleVertices.Count + ii * 2);
        AddParallelQuadIndices(collisionIndices, collisionVertices.Count + ii * 2);
      }
      for (var ii = 0; ii < quadVerticesAndNormals.Length; ii += 2) {
        var vertex = quadVerticesAndNormals[ii];
        visibleVertices.Add(vertex);
        collisionVertices.Add(vertex);
        normals.Add(quadVerticesAndNormals[ii + 1]);
      }

      return this;
    }

    public MeshBuilder AddLipLoop (params Vector3[] waypoints) {
      for (var ii = 0; ii < waypoints.Length; ii += 2) {
        AddLipLoopSegment(
          waypoints[(ii + waypoints.Length - 2) % waypoints.Length],
          waypoints[ii], waypoints[ii + 1],
          waypoints[(ii + 2) % waypoints.Length], waypoints[(ii + 3) % waypoints.Length],
          waypoints[(ii + 4) % waypoints.Length]);
      }
      return this;
    }

    void AddLipLoopSegment (
        Vector3 prev, Vector3 start, Vector3 startUp, Vector3 end, Vector3 endUp, Vector3 next) {
      var forward = end - start;
      var length = forward.magnitude;
      var dir = forward / length;

      const float kSmall = 0.001f;
      foreach (var cutout in cutouts) {
          if (Vector3.Dot(dir, cutout.dir) > -1.0f + kSmall) {
            // consider adjusting prev/next based on cutout
            var endProj = Vector3.Dot(end - cutout.start, cutout.dir);
            if (endProj >= 0.0f && endProj <= cutout.length) {
              var point = cutout.start + endProj * cutout.dir;
              if (Vector3.Distance(end, point) <= outer.lipRadius) {
                next = end + (endProj > cutout.length - outer.lipRadius ? GetNextCutoutDir(cutout, dir) : cutout.dir);
              }
            }
            var startProj = Vector3.Dot(start - cutout.start, cutout.dir);
            if (startProj >= 0.0f && startProj <= cutout.length) {
              var point = cutout.start + startProj * cutout.dir;
              if (Vector3.Distance(start, point) <= outer.lipRadius) {
                prev = start - (startProj < outer.lipRadius ? GetPrevCutoutDir(cutout, dir) : cutout.dir);
              }
            }
          } else {
            // cutout and segment aligned; consider cutting out of segment
            var startProj = Vector3.Dot(cutout.start - start, dir);
            if (startProj < kSmall) continue;

            var endProj = Vector3.Dot(cutout.end - start, dir);
            if (endProj > length - kSmall) continue;

            var startPoint = start + dir * startProj;
            var endPoint = start + dir * endProj;
            if (Vector3.Distance(startPoint, cutout.start) > outer.lipRadius ||
                Vector3.Distance(endPoint, cutout.end) > outer.lipRadius) continue;
            if (endProj > 0.0f) {
              var endPointUp = Vector3.Lerp(startUp, endUp, endProj / length).normalized;
              AddLipLoopSegment(
                prev, start, startUp, endPoint, endPointUp,
                endPoint + GetNextCutoutDir(cutout, Vector3.Cross(dir, endPointUp)));
            }
            if (startProj < length) {
              var startPointUp = Vector3.Lerp(startUp, endUp, startProj / length).normalized;
              AddLipLoopSegment(
                startPoint - GetPrevCutoutDir(cutout, Vector3.Cross(startPointUp, dir)),
                startPoint, startPointUp, end, endUp, next);
            }
            return;
          }
        }

        for (var ii = 0; ii < outer.lipDivisions; ++ii) {
          AddParallelQuadIndices(visibleIndices, visibleVertices.Count + ii * 2);
        }
        var startRight = Vector3.Cross(startUp, dir).normalized;
        var endRight = Vector3.Cross(endUp, dir).normalized;
        var startPlane = new Plane(((start - prev).normalized + dir).normalized, start);
        var endPlane = new Plane((dir + (next - end).normalized).normalized, end);
        for (var ii = 0; ii <= outer.lipDivisions; ++ii) {
          var angle = ii * Mathf.PI / outer.lipDivisions;
          var sina = Mathf.Sin(angle);
          var cosa = Mathf.Cos(angle);

          var startVector = sina * startUp - cosa * startRight;
          var startRay = new Ray(start + startVector * outer.lipRadius, dir);
          startPlane.Raycast(startRay, out var startEnter);
          visibleVertices.Add(startRay.GetPoint(startEnter));
          normals.Add(startVector);

          var endVector = sina * endUp - cosa * endRight;
          var endRay = new Ray(end + endVector * outer.lipRadius, dir);
          endPlane.Raycast(endRay, out var endEnter);
          visibleVertices.Add(endRay.GetPoint(endEnter));
          normals.Add(endVector);
        }

        AddParallelQuadIndices(collisionIndices, collisionVertices.Count);

        collisionVertices.Add(end + endUp * outer.railHeight);
        collisionVertices.Add(end);
        collisionVertices.Add(start + startUp * outer.railHeight);
        collisionVertices.Add(start);
    }

    void AddClockwiseQuadIndices (List<int> indices, int firstIndex) {
      indices.Add(firstIndex);
      indices.Add(firstIndex + 1);
      indices.Add(firstIndex + 2);

      indices.Add(firstIndex + 2);
      indices.Add(firstIndex + 3);
      indices.Add(firstIndex);
    }

    void AddParallelQuadIndices (List<int> indices, int firstIndex) {
      indices.Add(firstIndex);
      indices.Add(firstIndex + 1);
      indices.Add(firstIndex + 3);

      indices.Add(firstIndex + 3);
      indices.Add(firstIndex + 2);
      indices.Add(firstIndex);
    }

    Vector3 GetPrevCutoutDir (Cutout cutout, Vector3 defaultDir) {
      foreach (var other in cutouts) {
        if (Vector3.Distance(cutout.start, other.end) <= outer.lipRadius) return other.dir;
      }
      return defaultDir;
    }

    Vector3 GetNextCutoutDir (Cutout cutout, Vector3 defaultDir) {
      foreach (var other in cutouts) {
        if (Vector3.Distance(cutout.end, other.start) <= outer.lipRadius) return other.dir;
      }
      return defaultDir;
    }

    public void Populate (Mesh visibleMesh, Mesh collisionMesh) {
      visibleMesh.Clear();
      visibleMesh.SetVertices(visibleVertices);
      visibleMesh.SetNormals(normals);
      visibleMesh.SetIndices(visibleIndices, MeshTopology.Triangles, 0);

      collisionMesh.Clear();
      collisionMesh.SetVertices(collisionVertices);
      collisionMesh.SetIndices(collisionIndices, MeshTopology.Triangles, 0);
    }
  }
}
