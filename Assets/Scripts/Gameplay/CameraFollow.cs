using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target; // ตัวละครที่จะให้กล้องตาม

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 10f, -7f); // ระยะห่างมุมสูงเอียงแบบ ARPG
    public float smoothSpeed = 5f; // ความนุ่มนวลเวลาเลื่อนตาม

    void LateUpdate()
    {
        if (target == null) return;

        // คำนวณตำแหน่งเป้าหมายที่กล้องควรอยู่
        Vector3 desiredPosition = target.position + offset;

        // เลื่อนกล้องตามแบบ Smooth (ไม่กระตุก)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}