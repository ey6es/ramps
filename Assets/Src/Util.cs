using UnityEngine;

public static class Util {

  public static Bounds TransformBounds (this Transform transform, Bounds bounds) {
    Bounds transformedBounds = new Bounds(transform.TransformPoint(bounds.min), new Vector3());
    for (var ii = 1; ii < 8; ++ii) {
      transformedBounds.Encapsulate(transform.TransformPoint(new Vector3(
        (ii & 1) == 0 ? bounds.min.x : bounds.max.x,
        (ii & 2) == 0 ? bounds.min.y : bounds.max.y,
        (ii & 4) == 0 ? bounds.min.z : bounds.max.z)));
    }
    return transformedBounds;
  }
}
