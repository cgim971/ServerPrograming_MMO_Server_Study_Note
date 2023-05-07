using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
    public Transform TargetTs;
    public Vector3 cameraOffset;
    public float rotationSpeed = 1.0f;
    public float mouseXSpeed = 1.0f;
    public float mouseYSpeed = 1.0f;
    public float minYAngle = -30.0f;
    public float maxYAngle = 50.0f;

    private float currentXAngle = 0.0f;
    private float currentYAngle = 0.0f;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate() {
        CameraMove();
    }

    void CameraMove() {
        if (TargetTs == null)
            return;

        currentXAngle += Input.GetAxis("Mouse X") * mouseXSpeed;
        currentYAngle -= Input.GetAxis("Mouse Y") * mouseYSpeed;

        currentYAngle = Mathf.Clamp(currentYAngle, minYAngle, maxYAngle);

        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);
        transform.position = TargetTs.position + rotation * cameraOffset;
        transform.LookAt(TargetTs.position);

        transform.RotateAround(TargetTs.position, Vector3.up, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime);
    }
}