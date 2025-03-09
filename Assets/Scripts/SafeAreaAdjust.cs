using UnityEngine;

public class SafeAreaAdjust : MonoBehaviour
{
    void Start()
    {
        Rect safeArea = Screen.safeArea;
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.offsetMin = safeArea.position;
        rectTransform.offsetMax = safeArea.position + safeArea.size - new Vector2(Screen.width, Screen.height);
    }
}
