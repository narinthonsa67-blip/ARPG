using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float gravity = -9.81f;

    [Header("Camera / Mouse Look")]
    public Transform cameraRoot;          // ใส่ CameraRoot
    public float mouseSensitivity = 2f;   // ความไวเมาส์
    public float minPitch = -35f;         // ก้มได้สูงสุด
    public float maxPitch = 60f;          // เงยได้สูงสุด

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocity;
    private float cameraPitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        // ล็อกเคอร์เซอร์เมาส์ให้อยู่กลางจอเวลาเล่น
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. รับค่าการขยับเมาส์
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // หันตัวละครซ้าย-ขวา ตามเมาส์แนวนอน
        transform.Rotate(Vector3.up * mouseX);

        // ก้ม-เงยมุมกล้องขึ้น-ลง ตามเมาส์แนวตั้ง
        if (cameraRoot != null)
        {
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
            cameraRoot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        // 2. รับค่าปุ่ม WASD แล้วเดินไปตามทิศทางที่ตัวละครหันหน้าอยู่
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move.normalized * moveSpeed * Time.deltaTime);

        // 3. แรงโน้มถ่วง
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 4. ส่งค่าไป Animator
        anim.SetFloat("InputX", horizontal);
        anim.SetFloat("InputZ", vertical);

        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        anim.SetBool("isWalking", isMoving);

        // กดปุ่ม Esc เพื่อปลดล็อกเมาส์ออกมาคลิกปุ่มใน Unity
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}