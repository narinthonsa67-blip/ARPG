using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [Header("EXP Value")]
    [SerializeField] private int expAmount = 50;

    [Header("Visual Effects (Optional)")]
    [SerializeField] private float rotateSpeed = 100f;

    private void Update()
    {
        // หมุนลูกแก้วเบาๆ เพื่อความสวยงาม
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าวัตถุที่เดินมาชนคือ Player หรือมีคอมโพเนนต์ PlayerStats
        if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.AddExperience(expAmount);
            Destroy(gameObject); // เก็บแล้วทำลายลูกแก้วทิ้งทันที
        }
    }
}