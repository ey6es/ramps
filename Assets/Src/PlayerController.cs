using UnityEngine;

public class PlayerController : MonoBehaviour {
  public float speed = 0.2f;
  public float sensitivity = 5.0f;
  public Transform head;

  [SerializeField]
  float pitch;

  [SerializeField]
  bool mouseEnabled;

  void Start () {
    Cursor.lockState = CursorLockMode.Locked;
  }

  void Update () {
    if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

    // ignore first mouse input to avoid sudden jump
    var mouseX = Input.GetAxis("Mouse X");
    var mouseY = Input.GetAxis("Mouse Y");
    if (mouseEnabled) {
      transform.Rotate(Vector3.up, mouseX * sensitivity);
      head.localRotation = Quaternion.AngleAxis(
        pitch = Mathf.Clamp(pitch - mouseY * sensitivity, -90.0f, 90.0f),
        Vector3.right);

    } else if (mouseX != 0.0f || mouseY != 0.0f) mouseEnabled = true;
  }

  void FixedUpdate () {
    var movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")) * speed;
    GetComponent<CharacterController>().Move(transform.TransformVector(movement));
  }
}
