using System.Collections.Generic;
using UnityEngine;

public class RampTraverser : MonoBehaviour {
  public float raycastHeight = 1.0f;
  public float raycastMaxDistance = 2.0f;
  public LayerMask raycastLayerMask;

  public int creationOrder { get; } = currentCreationOrder++;
  public Ramp ramp { get; private set; }
  public object rampData { get; private set; }
  public Stack<RampTraverser> counterparts { get; } = new Stack<RampTraverser>();

  static int currentCreationOrder;

  public void Align (Vector3 position, Vector3 normal) {
    transform.position = position;
    var angle = Vector3.Angle(transform.up, normal);
    transform.Rotate(Vector3.Cross(transform.up, normal), angle, Space.World);
  }

  public void UpdateRamp () {
    RaycastHit hitInfo;
    if (Physics.Raycast(
        transform.position + transform.up * raycastHeight,
        -transform.up,
        out hitInfo,
        raycastMaxDistance,
        raycastLayerMask,
        QueryTriggerInteraction.Ignore)) {
      SetRamp(hitInfo.transform.GetComponent<Ramp>());
      if (ramp) ramp.OnTraverserStay(this, hitInfo, rampData);

    } else SetRamp(null);
  }

  void FixedUpdate () {
    UpdateRamp();
  }

  void OnDestroy () {
    SetRamp(null);    
  }

  void SetRamp (Ramp newRamp) {
    if (ramp == newRamp) return;
    if (ramp) {
      ramp.OnTraverserExit(this, rampData);
      rampData = null;
    }
    if ((ramp = newRamp)) rampData = ramp.OnTraverserEnter(this);
  }
}
