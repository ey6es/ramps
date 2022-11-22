using UnityEngine;

public class PlayerController : MonoBehaviour {
  public float speed = 10.0f;
  public float sensitivity = 5.0f;
  public Transform head;

  float pitch;

  void Start () {
    Cursor.lockState = CursorLockMode.Locked;
  }

  void Update () {
    transform.Rotate(Vector3.up, -Input.GetAxis("Mouse X") * sensitivity);
    transform.Translate(new Vector3(
      Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")) * speed * Time.deltaTime);

    head.localRotation = Quaternion.AngleAxis(
      pitch = Mathf.Clamp(pitch + Input.GetAxis("Mouse Y") * sensitivity, -90.0f, 90.0f),
      Vector3.right);    
  }
}
