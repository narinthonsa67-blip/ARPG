using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Wave Settings")]
    [SerializeField] private int enemiesPerWave = 2;       // เริ่มต้นเวฟละ 2 ตัวพอ
    [SerializeField] private float timeBetweenWaves = 8f;   // พักระหว่างเวฟนานขึ้นเป็น 8 วินาที
    [SerializeField] private float spawnInterval = 2.0f;    // ค่อยๆ ทยอยเกิดห่างกันตัวละ 2 วินาที
    [SerializeField] private int maxAliveEnemies = 5;       // จำกัดมอนสเตอร์ในฉากไม่ให้เกิน 5 ตัวพร้อมกัน

    [Header("Spawn Area (Arena Range)")]
    [SerializeField] private float spawnRadius = 8f;

    private int currentWave = 0;

    private void Start()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        while (true)
        {
            currentWave++;
            Debug.Log($"<color=cyan>--- เริ่ม WAVE {currentWave} (เป้าหมาย: {enemiesPerWave} ตัว) ---</color>");

            int spawnedThisWave = 0;
            while (spawnedThisWave < enemiesPerWave)
            {
                // ตรวจสอบจำนวนมอนสเตอร์ที่มีชีวิตอยู่ในฉาก
                int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

                // ถ้ามอนสเตอร์ยังไม่ล้นฉาก ให้เสกตัวใหม่ได้
                if (currentEnemyCount < maxAliveEnemies)
                {
                    SpawnSingleEnemy();
                    spawnedThisWave++;
                    yield return new WaitForSeconds(spawnInterval);
                }
                else
                {
                    // ถ้ามอนสเตอร์ในสนามยังเยอะอยู่ ให้รอ 1 วินาทีแล้วค่อยเช็กใหม่
                    yield return new WaitForSeconds(1f);
                }
            }

            // จบการปล่อยของเวฟนี้ รอเวลาพักก่อนขึ้นเวฟถัดไป
            yield return new WaitForSeconds(timeBetweenWaves);

            // ค่อยๆ เพิ่มทีละ 1 ตัวในเวฟถัดไป
            enemiesPerWave += 1;
        }
    }

    private void SpawnSingleEnemy()
    {
        if (enemyPrefab == null) return;

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(
            transform.position.x + randomCircle.x,
            transform.position.y,
            transform.position.z + randomCircle.y
        );

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}