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
    private PlayerStats playerStats;
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
            playerStats = player.GetComponent<PlayerStats>();
        }

        // ปรับทิศทางแรงดีดให้กระจายตัวออกด้านข้างเลียดพื้น
        if (rb != null)
        {
            rb.isKinematic = false;

            // สุ่มทิศทางแนวนอน (X, Z) และยกแกน Y ขึ้นเพียงเล็กน้อย (0.35f)
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 popDirection = new Vector3(randomCircle.x, 0.35f, randomCircle.y).normalized;

            // สุ่มแรงผลักให้นุ่มนวล พอดีกับการกระจายตัว
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
        // หมุนตัวเพิ่มมิติ
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // ระบบดูดเข้าหาผู้เล่น
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
        if (canMagnet && (other.CompareTag("Player") || other.GetComponentInParent<PlayerStats>() != null))
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

        Destroy(gameObject);
    }
}