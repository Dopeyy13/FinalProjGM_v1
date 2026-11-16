using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text speakerName;
    public TMP_Text dialogueText;
    public Button nextButton;

    [Header("Choice UI")]
    public GameObject choicesPanel;
    public Button[] choiceButtons = new Button[3];
    public string detectiveSpeakerName = "გამომძიებელი";

    [Header("Nika Exit Pause")]
    public float nikaExitPause = 3f;

    private bool isWaitingForNextLine = false;

    [Header("Player")]
    public ThirdPersonController playerMovement;
    public IntroCutsceneController introController;

    [Header("First Scene - References")]
    public NPCInteraction officerInteraction;
    public DetectiveLookAt detectiveLookAt;
    public NPCRelocator nikaRelocator;

    [Header("First Scene - Line Events")]
    public int lookAtNikaLine = 1;
    public int nikaWalkLine = 7;

    private string[] lines;
    private string[] lineSpeakers;
    private string defaultSpeaker;

    private int currentLine;
    private NPCInteraction currentNPC;

    private bool lookTriggered;
    private bool walkTriggered;

    private bool wasCursorVisible;
    private CursorLockMode previousCursorLock;

    private NPCInteraction.DialogueChoiceGroup[] choiceGroups;
    private bool[] usedChoiceGroups;

    private NPCInteraction.DialogueChoiceGroup activeChoice;
    private NPCInteraction.DialogueChoiceOption selectedOption;

    private enum ChoiceStage
    {
        None,
        Options,
        Question,
        Answer
    }

    private ChoiceStage choiceStage = ChoiceStage.None;

    public bool IsDialogueOpen => currentNPC != null;

    void Awake()
    {
        dialoguePanel.SetActive(false);

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        nextButton.onClick.AddListener(NextLine);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;

            if (choiceButtons[i] != null)
            {
                choiceButtons[i].onClick.AddListener(
                    () => SelectChoice(index)
                );
            }
        }
    }

    public bool StartDialogue(
        string speaker,
        string[] newLines,
        NPCInteraction npc,
        string[] speakers = null)
    {
        if (currentNPC != null ||
            newLines == null ||
            newLines.Length == 0 ||
            npc == null)
        {
            return false;
        }

        currentNPC = npc;
        lines = newLines;
        lineSpeakers = speakers;
        defaultSpeaker = speaker;

        currentLine = 0;

        lookTriggered = false;
        walkTriggered = false;
        isWaitingForNextLine = false;

        choiceGroups = npc.dialogueChoices;

        usedChoiceGroups = choiceGroups != null
            ? new bool[choiceGroups.Length]
            : new bool[0];

        choiceStage = ChoiceStage.None;
        activeChoice = null;
        selectedOption = null;

        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        nextButton.gameObject.SetActive(true);

        playerMovement.enabled = false;

        previousCursorLock = Cursor.lockState;
        wasCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        dialoguePanel.SetActive(true);

        ShowLine();
        UpdateButton();

        return true;
    }

    void ShowLine()
    {
        dialogueText.text = lines[currentLine];

        if (lineSpeakers != null &&
            currentLine < lineSpeakers.Length &&
            !string.IsNullOrWhiteSpace(
                lineSpeakers[currentLine]))
        {
            speakerName.text = lineSpeakers[currentLine];
        }
        else
        {
            speakerName.text = defaultSpeaker;
        }

        CheckSceneEvents();
    }

    void CheckSceneEvents()
    {
        if (officerInteraction == null ||
            currentNPC != officerInteraction)
        {
            return;
        }

        if (currentLine == lookAtNikaLine &&
            !lookTriggered)
        {
            lookTriggered = true;

            if (detectiveLookAt != null)
                detectiveLookAt.LookAtNika();
        }

        if (currentLine == nikaWalkLine &&
            !walkTriggered)
        {
            walkTriggered = true;

            if (nikaRelocator != null)
                nikaRelocator.MoveNow();
        }
    }

    public void NextLine()
    {
        if (currentNPC == null || isWaitingForNextLine)
            return;

        if (choiceStage == ChoiceStage.Options)
            return;

        // Show answer to selected question
        if (choiceStage == ChoiceStage.Question)
        {
            choiceStage = ChoiceStage.Answer;

            speakerName.text = defaultSpeaker;
            dialogueText.text = selectedOption.answer;

            UpdateButton();
            return;
        }

        // Continue normal dialogue
        if (choiceStage == ChoiceStage.Answer)
        {
            choiceStage = ChoiceStage.None;
            selectedOption = null;
            activeChoice = null;

            AdvanceLine();
            return;
        }

        // Nika walking pause
        if (currentNPC == officerInteraction &&
            currentLine == nikaWalkLine - 1 &&
            currentLine + 1 < lines.Length)
        {
            StartCoroutine(NikaExitPause());
            return;
        }

        // Show choices when needed
        if (TryShowChoices())
            return;

        AdvanceLine();
    }

    void AdvanceLine()
    {
        currentLine++;

        if (currentLine < lines.Length)
        {
            ShowLine();
            UpdateButton();
        }
        else
        {
            CloseDialogue();
        }
    }

    int FindChoiceGroupIndex()
    {
        if (choiceGroups == null)
            return -1;

        for (int i = 0; i < choiceGroups.Length; i++)
        {
            NPCInteraction.DialogueChoiceGroup group =
                choiceGroups[i];

            if (group == null ||
                usedChoiceGroups[i] ||
                group.showAfterLine != currentLine ||
                group.options == null)
            {
                continue;
            }

            for (int j = 0;
                 j < group.options.Length &&
                 j < choiceButtons.Length;
                 j++)
            {
                if (group.options[j] != null &&
                    !string.IsNullOrWhiteSpace(
                        group.options[j].question))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    bool TryShowChoices()
    {
        int groupIndex = FindChoiceGroupIndex();

        if (groupIndex < 0)
            return false;

        if (choicesPanel == null ||
            choiceButtons == null ||
            choiceButtons.Length == 0)
        {
            Debug.LogWarning(
                "Choice UI is not assigned in DialogueManager."
            );
            return false;
        }

        activeChoice = choiceGroups[groupIndex];
        usedChoiceGroups[groupIndex] = true;

        choiceStage = ChoiceStage.Options;

        int visibleCount = 0;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            Button button = choiceButtons[i];

            if (button == null)
                continue;

            bool valid =
                activeChoice.options != null &&
                i < activeChoice.options.Length &&
                activeChoice.options[i] != null &&
                !string.IsNullOrWhiteSpace(
                    activeChoice.options[i].question);

            button.gameObject.SetActive(valid);

            if (!valid)
                continue;

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text =
                    activeChoice.options[i].question;
            }

            visibleCount++;
        }

        if (visibleCount == 0)
        {
            choiceStage = ChoiceStage.None;
            activeChoice = null;
            return false;
        }

        nextButton.gameObject.SetActive(false);
        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(true);

        return true;
    }

    void SelectChoice(int index)
    {
        if (choiceStage != ChoiceStage.Options ||
            activeChoice == null ||
            activeChoice.options == null ||
            index < 0 ||
            index >= activeChoice.options.Length)
        {
            return;
        }

        selectedOption = activeChoice.options[index];

        if (selectedOption == null)
            return;

        choicesPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        nextButton.gameObject.SetActive(true);

        choiceStage = ChoiceStage.Question;

        speakerName.text = detectiveSpeakerName;
        dialogueText.text = selectedOption.question;

        UpdateButton();
    }

    private IEnumerator NikaExitPause()
    {
        isWaitingForNextLine = true;

        currentLine++;

        CheckSceneEvents();

        dialoguePanel.SetActive(false);

        yield return new WaitForSeconds(nikaExitPause);

        if (currentNPC != null)
        {
            ShowLine();
            UpdateButton();
            dialoguePanel.SetActive(true);
        }

        isWaitingForNextLine = false;
    }

    void UpdateButton()
    {
        TMP_Text buttonText =
            nextButton.GetComponentInChildren<TMP_Text>();

        if (buttonText == null)
            return;

        bool closing =
            currentLine == lines.Length - 1 &&
            (
                choiceStage == ChoiceStage.Answer ||
                (
                    choiceStage == ChoiceStage.None &&
                    FindChoiceGroupIndex() == -1
                )
            );

        buttonText.text =
            closing ? "დახურვა" : "შემდეგი >";
    }

    void CloseDialogue()
    {
        if (choicesPanel != null)
            choicesPanel.SetActive(false);

        nextButton.gameObject.SetActive(true);
        dialoguePanel.SetActive(false);

        choiceStage = ChoiceStage.None;
        activeChoice = null;
        selectedOption = null;

        Cursor.lockState = previousCursorLock;
        Cursor.visible = wasCursorVisible;

        if (introController != null)
            introController.UnlockMovement();

        playerMovement.enabled = true;

        NPCInteraction finishedNPC = currentNPC;
        currentNPC = null;

        if (finishedNPC != null)
            finishedNPC.EndDialogue();

    }
}
