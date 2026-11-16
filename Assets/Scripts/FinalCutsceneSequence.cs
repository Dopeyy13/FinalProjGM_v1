
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class FinalCutsceneSequence : MonoBehaviour
{
    [Header("Cutscene")]
    public PlayableDirector finalDirector;
    public Camera finalCamera;
    public GameObject finalNika;
    public GameObject finalWatch;

    [Header("Ending Screen")]
    public GameObject finalEndingPanel;
    public CanvasGroup blackGroup;
    public CanvasGroup textGroup;

    private bool started = false;

    void Awake()
    {
        if (finalCamera != null)
            finalCamera.enabled = false;

        if (finalNika != null)
            finalNika.SetActive(false);

        if (finalWatch != null)
            finalWatch.SetActive(false);

        if (finalEndingPanel != null)
            finalEndingPanel.SetActive(false);
    }

    public void PlayFinalCutscene()
    {
        if (started) return;

        if (finalDirector == null || finalCamera == null ||
            finalEndingPanel == null || blackGroup == null ||
            textGroup == null)
        {
            Debug.LogError("FinalCutsceneSequence: References missing!");
            return;
        }

        started = true;

        if (finalNika != null)
            finalNika.SetActive(true);

        if (finalWatch != null)
            finalWatch.SetActive(true);

        finalCamera.enabled = true;

        finalDirector.time = 0;
        finalDirector.Play();

        StartCoroutine(ShowEnding());
    }

    IEnumerator ShowEnding()
    {
        double duration = finalDirector.duration;

        if (duration <= 0 || double.IsInfinity(duration))
            duration = 6;

        yield return new WaitForSeconds((float)duration);

        finalEndingPanel.SetActive(true);
        finalEndingPanel.transform.SetAsLastSibling();

        blackGroup.alpha = 0f;
        textGroup.alpha = 0f;

        yield return StartCoroutine(Fade(blackGroup, 0f, 1f, 1.5f));

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(Fade(textGroup, 0f, 1f, 1.5f));
    }

    IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        group.alpha = to;
    }
}
