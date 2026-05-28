using UnityEngine;

public class fps : MonoBehaviour
{
    private float faps;

    void Update()
    {
        faps = 1.0f / Time.unscaledDeltaTime;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), $"FPS: {Mathf.RoundToInt(faps)}");
    }
}
