
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class NPCInteraction : MonoBehaviour
{
    [System.Serializable]
    public class DialogueChoiceOption
    {
        [TextArea(2, 4)]
        public string question;

        [TextArea(2, 4)]
        public string answer;
    }

    [System.Serializable]
    public class DialogueChoiceGroup
    {
        [Tooltip("0 = პირველი Dialogue Line")]
        public int showAfterLine;

        public DialogueChoiceOption[] options =
            new DialogueChoiceOption[3];
    }

    [Header("References")]
    public Transform player;
    public PlayableDirector introDirector;
    public DialogueManager dialogueManager;

    [Header("Interaction")]
    public float interactionDistance = 4f;
    public float textDelay = 2f;

    [Header("Interaction UI")]
    public GameObject interactionPrompt;
    public Transform interactionCenter;

    [Header("After Cutscene (Optional)")]
    public Transform playerPositionAfterCutscene;

    [Header("Dialogue")]
    public string speakerName = "ოფიცერი";

    [Header("Dialogue Speakers")]
    public string[] lineSpeakers;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    public string[] dialogueLines =
    {
        "დეტექტივო, მადლობა, რომ მოხვედით.",
        "დეტალებზე გადავიდეთ.",
        "მანამდე თქვენი თანაშემწე მინდა გაგაცნოთ."
    };

    [Header("Dialogue Choices (Optional)")]
    public DialogueChoiceGroup[] dialogueChoices;

    [Header("Dialogue Settings")]
    public bool talkOnlyOnce = true;

    public UnityEvent onDialogueEnded;

    private float timer;
    private bool canInteract;
    private bool isDialogueOpen;
    private bool dialogueFinished;
    private bool wasCutsceneActive;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (player == null)
            return;

        bool cutsceneActive = IsCutsceneActive();

        if (wasCutsceneActive && !cutsceneActive)
            OnCutsceneFinished();

        wasCutsceneActive = cutsceneActive;

        if (dialogueFinished || isDialogueOpen ||
            (dialogueManager != null &&
             dialogueManager.IsDialogueOpen))
        {
            canInteract = false;
            return;
        }

        Transform center = interactionCenter != null
            ? interactionCenter
            : transform;

        Vector3 a = player.position;
        Vector3 b = center.position;

        a.y = 0f;
        b.y = 0f;

        float distance = Vector3.Distance(a, b);

        if (cutsceneActive || distance > interactionDistance)
        {
            timer = 0f;
            canInteract = false;
            return;
        }

        timer += Time.deltaTime;
        canInteract = timer >= textDelay;

        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueManager == null)
            {
                Debug.LogWarning(
                    "DialogueManager is missing on: " + name
                );
                return;
            }

            isDialogueOpen = dialogueManager.StartDialogue(
                speakerName,
                dialogueLines,
                this,
                lineSpeakers
            );

            canInteract = false;
        }
    }

    bool IsCutsceneActive()
    {
        if (introDirector == null)
            return false;

        if (introDirector.state != PlayState.Playing)
            return false;

        return introDirector.time <
            introDirector.duration - 0.05;
    }

    void OnCutsceneFinished()
    {
        timer = textDelay;

        if (playerPositionAfterCutscene == null)
            return;

        CharacterController cc =
            player.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        player.SetPositionAndRotation(
            playerPositionAfterCutscene.position,
            playerPositionAfterCutscene.rotation
        );

        if (cc != null)
            cc.enabled = true;
    }

    void LateUpdate()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(canInteract);
    }

    void OnDisable()
    {
        canInteract = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void EndDialogue()
    {
        isDialogueOpen = false;
        canInteract = false;
        timer = 0f;

        if (talkOnlyOnce)
            dialogueFinished = true;

        onDialogueEnded?.Invoke();
    }

    void OnDrawGizmosSelected()
    {
        Transform center = interactionCenter != null
            ? interactionCenter
            : transform;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            center.position,
            interactionDistance
        );
    }
}
