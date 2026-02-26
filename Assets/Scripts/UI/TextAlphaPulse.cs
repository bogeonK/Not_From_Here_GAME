using UnityEngine;
using TMPro;

public class TextAlphaPulse : MonoBehaviour
{
    public float speed = 2f;        
    public float minAlpha = 50f;    
    public float maxAlpha = 255f;   

    private TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f);
        float alpha255 = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = text.color;
        c.a = alpha255 / 255f; 
        text.color = c;
    }
}