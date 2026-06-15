using UnityEngine;

// Controle em primeira pessoa (andar + olhar com o mouse).
// Como usar: crie uma Capsule, adicione um CharacterController e este script.
// Arraste a Main Camera para dentro da capsule (filha), na altura ~0.6, e ligue no campo "cameraTransform".
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    public float walkSpeed = 4f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;

    CharacterController controller;
    float pitch;
    float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Olhar
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mx);
        pitch = Mathf.Clamp(pitch - my, -85f, 85f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);

        // Andar (WASD / setas)
        Vector3 move = transform.right * Input.GetAxis("Horizontal")
                     + transform.forward * Input.GetAxis("Vertical");
        if (move.magnitude > 1f) move.Normalize();

        if (controller.isGrounded) verticalVelocity = -1f;
        else verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * walkSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}
