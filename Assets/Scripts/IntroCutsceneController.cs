using UnityEngine;
using UnityEngine.Playables;

public class IntroCutsceneController : MonoBehaviour
{
    public PlayableDirector introDirector;
    public ThirdPersonController playerMovement;

    private bool locked;

    void Awake()
    {
        // თამაშის დაწყებისთანავე ვბლოკავთ მოძრაობას
        locked = true;
        if (playerMovement != null) playerMovement.enabled = false;
    }

    // დაცვა: თუ რამემ შემთხვევით ჩართო, ისევ გამოირთვება
    void LateUpdate()
    {
        if (locked && playerMovement != null && playerMovement.enabled)
            playerMovement.enabled = false;
    }

    // DialogueManager იძახებს პირველი დიალოგის დასრულებისას
    public void UnlockMovement()
    {
        locked = false;
        if (playerMovement != null) playerMovement.enabled = true;
    }
}