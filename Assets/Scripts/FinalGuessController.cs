
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class FinalGuessController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject finalGuessPanel;
    public GameObject guessCard;
    public CanvasGroup mainCanvasGroup;
    public GameObject finalIntroPanel;
    public CanvasGroup introCanvasGroup;

    [Header("Input")]
    public TMP_InputField suspectInput;
    public Button confirmButton;

    [Header("Player")]
    public ThirdPersonController playerMovement;

    [Header("Timing")]
    public float fadeDuration = 1.5f;
    public float introDuration = 8f;

    [Header("Final Cutscene")]
    public UnityEvent onConfirmed;

    private bool started;
    private bool confirmed;

    void Awake()
    {
        finalGuessPanel.SetActive(false);
        confirmButton.onClick.AddListener(ConfirmAnswer);
    }

    public void StartFinalSequence()
    {
        if (started)
            return;

        started = true;
        confirmed = false;

        if (playerMovement != null)
            playerMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        suspectInput.text = "";
        confirmButton.interactable = true;

        guessCard.SetActive(false);
        finalGuessPanel.SetActive(true);
        finalIntroPanel.SetActive(true);

        mainCanvasGroup.alpha = 0f;
        introCanvasGroup.alpha = 1f;

        StartCoroutine(PlayFinalTransition());
    }

    IEnumerator PlayFinalTransition()
    {
        // Black screen fades in
        yield return StartCoroutine(
            Fade(mainCanvasGroup, 0f, 1f)
        );

        // Hold the introductory message
        yield return new WaitForSeconds(introDuration);

        // Fade out introductory text
        yield return StartCoroutine(
            Fade(introCanvasGroup, 1f, 0f)
        );

        finalIntroPanel.SetActive(false);

        // Show guessing card after intro
        guessCard.SetActive(true);

        // Enable the input field
        suspectInput.Select();
        suspectInput.ActivateInputField();
    }

    IEnumerator Fade(
        CanvasGroup group,
        float from,
        float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            group.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        group.alpha = to;
    }

    public void ConfirmAnswer()
    {
        if (!started || confirmed ||
            finalIntroPanel.activeSelf)
            return;

        confirmed = true;
        confirmButton.interactable = false;

        // No name verification: any answer continues.
        onConfirmed?.Invoke();

        StartCoroutine(HideFinalPanel());
    }

    IEnumerator HideFinalPanel()
    {
        yield return StartCoroutine(
            Fade(mainCanvasGroup, 1f, 0f)
        );

        finalGuessPanel.SetActive(false);
    }
}
