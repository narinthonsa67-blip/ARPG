using UnityEngine;
using System.Collections;

public class ExpOrb : MonoBehaviour
{
    [Header("EXP Value")]
    [SerializeField] private int expAmount = 10;

    [Header("Visual Effects")]
    [SerializeField] private float rotateSpeed = 120f;

    [Header("Magnet Settings")]
    [SerializeField] private float delayBeforeMagnet = 0.5f; // เวลาหน่วงให้เห็นลูกแก้วกระจายตัวบนพื้น
    [SerializeField] private float magnetRange = 4f;         // ระยะที่เริ่มดูดเข้าหาตัว
    [SerializeField] private float flySpeed = 7f;            // ความเร็วตอนบินเข้าหาตัว

    private Transform playerTransform;
    // --- แก้ไขจุดที่ 1: เปลี่ยนจาก PlayerStats เป็น PlayerController ---
    private PlayerController playerController;
    private Rigidbody rb;
    private bool canMagnet = false;
    private bool isFlyingToPlayer = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // ค้นหาผู้เล่น
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            // --- แก้ไขจุดที่ 2: ดึง Component PlayerController ---
            playerController = player.GetComponent<PlayerController>();
        }

        // ปรับทิศทางแรงดีดให้กระจายตัวออกด้านข้างเลียดพื้น (คงเดิม)
        if (rb != null)
        {
            rb.isKinematic = false;

            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 popDirection = new Vector3(randomCircle.x, 0.35f, randomCircle.y).normalized;

            float popForce = Random.Range(2.5f, 4f);
            rb.AddForce(popDirection * popForce, ForceMode.Impulse);
        }

        StartCoroutine(EnableMagnetRoutine());
    }

    private IEnumerator EnableMagnetRoutine()
    {
        yield return new WaitForSeconds(delayBeforeMagnet);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true; // ล็อคฟิสิกส์เตรียมลอยเข้าหาตัว
        }

        canMagnet = true;
    }

    private void Update()
    {
        // หมุนตัวเพิ่มมิติ (คงเดิม)
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // ระบบดูดเข้าหาผู้เล่น (คงเดิม)
        if (canMagnet && playerTransform != null)
        {
            Vector3 targetPos = playerTransform.position + Vector3.up * 1f;
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance <= magnetRange)
            {
                isFlyingToPlayer = true;
            }

            if (isFlyingToPlayer)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, flySpeed * Time.deltaTime);

                if (distance < 0.4f)
                {
                    CollectOrb();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // --- แก้ไขจุดที่ 3: เช็ก PlayerController แทน PlayerStats ---
        if (canMagnet && (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null))
        {
            CollectOrb();
        }
    }

    private void CollectOrb()
    {
        // --- แก้ไขจุดที่ 4: ส่งค่า EXP เข้า PlayerController ด้วยฟังก์ชัน AddExp ---
        if (playerController != null)
        {
            playerController.AddExp(expAmount);
        }

        Destroy(gameObject);
    }
}