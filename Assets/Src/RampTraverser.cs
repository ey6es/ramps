using UnityEngine;

public class RampTraverser : MonoBehaviour {
  public float raycastHeight = 1.0f;
  public float raycastMaxDistance = 2.0f;
  public LayerMask raycastLayerMask;

  Ramp lastRamp;
  object rampData;

  public void Align (Vector3 position, Vector3 normal) {
    transform.position = position;
    var angle = Vector3.Angle(transform.up, normal);
    transform.Rotate(Vector3.Cross(transform.up, normal), angle, Space.World);
  }

  void FixedUpdate () {
    RaycastHit hitInfo;
    if (Physics.Raycast(
        transform.position + transform.up * raycastHeight,
        -transform.up,
        out hitInfo,
        raycastMaxDistance,
        raycastLayerMask,
        QueryTriggerInteraction.Ignore)) {
      SetRamp(hitInfo.transform.GetComponent<Ramp>());
      if (lastRamp) lastRamp.OnTraverserStay(this, hitInfo, ref rampData);

    } else SetRamp(null);
  }

  void OnDestroy () {
    SetRamp(null);    
  }

  void SetRamp (Ramp ramp) {
    if (lastRamp == ramp) return;
    if (lastRamp) lastRamp.OnTraverserExit(this, rampData);
    lastRamp = ramp;
    if (lastRamp) lastRamp.OnTraverserEnter(this, ref rampData);
  }
}
