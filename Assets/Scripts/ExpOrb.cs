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
    private PlayerStats playerStats;
    private bool isFlyingToPlayer = false;

    private void Start()
    {
        // ค้นหาตัวผู้เล่นจาก Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    private void Update()
    {
        // 1. หมุนตัวเบาๆ
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // 2. ระบบดูดเข้าหาผู้เล่น
        if (playerTransform != null)
        {
            Vector3 targetPos = playerTransform.position + Vector3.up * 1f;
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance <= magnetRange)
            {
                isFlyingToPlayer = true;
            }

            if (isFlyingToPlayer)
            {
                // พุ่งเข้าหาตำแหน่งตัวผู้เล่น
                transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);

                // ป้องกันบั๊กฟิสิกส์: ถ้าพุ่งมาใกล้มากแล้ว (ระยะต่ำกว่า 0.4 เมตร) ให้เก็บเข้าตัวทันที
                if (distance < 0.4f)
                {
                    CollectOrb();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // เมื่อดูดมาชนโดน Collider ของผู้เล่น
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerStats>() != null)
        {
            CollectOrb();
        }
    }

    private void CollectOrb()
    {
        if (playerStats != null)
        {
            playerStats.AddExperience(expAmount);
        }

        // ทำลายลูกแก้วทิ้งทันที
        Destroy(gameObject);
    }
}