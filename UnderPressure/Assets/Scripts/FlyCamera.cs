using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Simple fly camera for testing in Play mode
//   Hold right mouse button + move mouse : look around
//   W A S D : move            Q / E : down / up
//   Shift   : move faster     
public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 5f;          // metres per second
    public float fastMultiplier = 4f;     // speed while holding Shift
    public float lookSensitivity = 0.15f; // degrees per pixel of mouse movement

    float yaw, pitch;

    void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;
    }

    void Update()
    {
        Vector2 mouseDelta = Vector2.zero;
        Vector3 move = Vector3.zero;
        bool looking = false, fast = false;
        float scroll = 0f;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        looking = mouse.rightButton.isPressed;
        mouseDelta = mouse.delta.ReadValue();
        fast = kb.leftShiftKey.isPressed;

        if (kb.wKey.isPressed) move += Vector3.forward;
        if (kb.sKey.isPressed) move += Vector3.back;
        if (kb.aKey.isPressed) move += Vector3.left;
        if (kb.dKey.isPressed) move += Vector3.right;
        if (kb.eKey.isPressed) move += Vector3.up;
        if (kb.qKey.isPressed) move += Vector3.down;
#else
        looking = Input.GetMouseButton(1);
        mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 10f;
        fast = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;
        if (Input.GetKey(KeyCode.E)) move += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) move += Vector3.down;
#endif

        // Look (only while right mouse is held)
        if (looking)
        {
            yaw += mouseDelta.x * lookSensitivity;
            pitch -= mouseDelta.y * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // Move relative to where the camera looks
        float speed = moveSpeed * (fast ? fastMultiplier : 1f);
        transform.Translate(move.normalized * speed * Time.deltaTime, Space.Self);
    }
}
