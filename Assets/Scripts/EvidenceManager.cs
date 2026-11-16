
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EvidenceManager : MonoBehaviour
{
    [System.Serializable]
    public class Evidence
    {
        public Transform item;
        public Texture2D image;
        public bool found;

        // აუცილებელია თუ არა დაკითხვამდე
        public bool requiredForInterrogation = true;
    }

    public Evidence[] evidences;
    public float interactionDistance = 2.5f;

    public Transform player;
    public ThirdPersonController playerMovement;
    public DialogueManager dialogueManager;

    public GameObject evidencePanel;
    public GameObject evidencePrompt;
    public RawImage evidenceImage;
    public Button closeButton;

    // პირდაპირ E-ით გახსნის ჩართვა/გამორთვა
    public bool useDirectE = false;

    [Header("Interrogation Objective")]
    public GameObject interrogationNoticePanel;
    public float noticeDuration = 5f;

    private bool interrogationNoticeShown = false;

    private bool isOpen;
    private bool wasMovementEnabled;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLock;

    public bool IsOpen => isOpen;

    void Awake()
    {
        evidencePanel.SetActive(false);
        evidencePrompt.SetActive(false);
        closeButton.onClick.AddListener(CloseEvidence);
    }

    void Update()
    {
        // Inspection Mode-ში E-ს აღარ ვამუშავებთ
        if (!useDirectE) return;

        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseEvidence();

            return;
        }

        bool canInteract = player != null;

        if (dialogueManager != null &&
            dialogueManager.IsDialogueOpen)
            canInteract = false;

        if (playerMovement != null &&
            !playerMovement.enabled)
            canInteract = false;

        int nearestIndex = -1;
        float nearestDistance = interactionDistance;

        if (canInteract)
        {
            for (int i = 0; i < evidences.Length; i++)
            {
                if (evidences[i].item == null) continue;

                Vector3 a = player.position;
                Vector3 b = evidences[i].item.position;

                a.y = b.y = 0;

                float distance = Vector3.Distance(a, b);

                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }
        }

        bool showPrompt = nearestIndex != -1;

        if (evidencePrompt.activeSelf != showPrompt)
            evidencePrompt.SetActive(showPrompt);

        if (showPrompt && Input.GetKeyDown(KeyCode.E))
            OpenEvidence(nearestIndex);
    }

    public void OpenEvidence(int index)
    {
        evidenceImage.texture = evidences[index].image;
        evidences[index].found = true;

        isOpen = true;
        evidencePrompt.SetActive(false);

        wasMovementEnabled =
            playerMovement != null && playerMovement.enabled;

        if (playerMovement != null)
            playerMovement.enabled = false;

        previousCursorVisible = Cursor.visible;
        previousCursorLock = Cursor.lockState;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        evidencePanel.SetActive(true);
    }

    public void CloseEvidence()
    {
        if (!isOpen) return;

        isOpen = false;
        evidencePanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = wasMovementEnabled;

        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLock;

        // ყველა აუცილებელი მტკიცებულება ნაპოვნია?
        if (!interrogationNoticeShown &&
            AllRequiredEvidenceFound())
        {
            StartCoroutine(ShowInterrogationNotice());
        }
    }

    bool AllRequiredEvidenceFound()
    {
        if (evidences == null || evidences.Length == 0)
            return false;

        int requiredCount = 0;

        foreach (Evidence evidence in evidences)
        {
            if (evidence == null ||
                !evidence.requiredForInterrogation)
                continue;

            requiredCount++;

            if (!evidence.found)
                return false;
        }

        return requiredCount > 0;
    }

    IEnumerator ShowInterrogationNotice()
    {
        if (interrogationNoticePanel == null)
        {
            Debug.LogWarning(
                "Interrogation Notice Panel is not assigned."
            );
            yield break;
        }

        interrogationNoticeShown = true;
        interrogationNoticePanel.SetActive(true);

        yield return new WaitForSeconds(noticeDuration);

        interrogationNoticePanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseEvidence);
    }
}
