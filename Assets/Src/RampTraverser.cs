using UnityEngine;

public class RampTraverser : MonoBehaviour {
  public float raycastHeight = 1.0f;
  public float raycastMaxDistance = 2.0f;
  public LayerMask raycastLayerMask;

  void FixedUpdate () {
    RaycastHit hitInfo;
    if (Physics.Raycast(
        transform.position + transform.up * raycastHeight,
        -transform.up,
        out hitInfo,
        raycastMaxDistance,
        raycastLayerMask,
        QueryTriggerInteraction.Ignore)) {
      transform.position = hitInfo.point;
      transform.up = hitInfo.normal;
    }
  }
}
