using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public string sceneName;

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void ExitGame()
    {
        Debug.Log("Game Exited");

#if UNITY_EDITOR
            // หยุดการทำงานของ Play Mode ใน Unity Editor ตอนทดสอบ
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // ปิดโปรแกรมเกมจริงๆ เมื่อ Build เป็นไฟล์ .exe แล้ว
        Application.Quit();
#endif
    }
}