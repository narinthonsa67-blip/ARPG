using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [Header("EXP Value")]
    [SerializeField] private int expAmount = 50;

    [Header("Visual Effects")]
    [SerializeField] private float rotateSpeed = 100f;

    [Header("Magnet Settings")]
    [SerializeField] private float magnetRange = 3f;   // ระยะที่เริ่มดูดเข้าหาผู้เล่น
    [SerializeField] private float flySpeed = 8f;       // ความเร็วตอนพุ่งเข้าหา

    private Transform playerTransform;
    private bool isFlyingToPlayer = false;

    private void Start()
    {
        // ค้นหาตัวผู้เล่นจาก Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        // 1. หมุนตัวเบาๆ
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // 2. ระบบดูดเข้าหาผู้เล่น
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (distance <= magnetRange)
            {
                isFlyingToPlayer = true;
            }

            if (isFlyingToPlayer)
            {
                // พุ่งเข้าหาตำแหน่งตัวผู้เล่น (ยกแกน Y ขึ้นเล็กน้อยระดับอก/เอว)
                Vector3 targetPos = playerTransform.position + Vector3.up * 1f;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // เมื่อดูดมาแตะโดนตัวผู้เล่น
        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = other.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.AddExperience(expAmount);
            Destroy(gameObject); // ชนแล้วทำลายทิ้ง
        }
    }
}