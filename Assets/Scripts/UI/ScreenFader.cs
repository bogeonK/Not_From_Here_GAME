using System;
using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float defaultDuration = 0.35f;

    private Coroutine routine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void FadeOutIn(Action midAction, float duration = -1f)
    {
        if (duration <= 0f) duration = defaultDuration;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeOutInRoutine(midAction, duration));
    }

    private IEnumerator FadeOutInRoutine(Action midAction, float duration)
    {
        //페이드아웃
        canvasGroup.blocksRaycasts = true;
        yield return Fade(0f, 1f, duration);

        //순간이동
        midAction?.Invoke();

        yield return null;

        //페이드인
        yield return Fade(1f, 0f, duration);
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        canvasGroup.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, a);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}