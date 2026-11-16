
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class InterrogationActor : MonoBehaviour
{
    [Header("Positions")]
    public Transform entryPoint;
    public Transform talkPoint;
    public Transform exitPoint;

    [Header("Components")]
    public NavMeshAgent agent;
    public Animator animator;
    public NPCInteraction npcInteraction;

    [Header("Events")]
    public UnityEvent onExitFinished;

    private enum State
    {
        Waiting,
        Entering,
        Talking,
        Exiting,
        Finished
    }

    private State state = State.Waiting;

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (npcInteraction == null)
            npcInteraction = GetComponent<NPCInteraction>();

        if (npcInteraction != null)
        {
            npcInteraction.enabled = false;

            if (npcInteraction.interactionPrompt != null)
                npcInteraction.interactionPrompt.SetActive(false);
        }

        SetWalking(false);
    }

    void Update()
    {
        if (state != State.Entering &&
            state != State.Exiting)
            return;

        if (agent == null || !agent.isOnNavMesh)
            return;

        if (agent.pathPending)
            return;

        bool arrived =
            agent.remainingDistance <=
            agent.stoppingDistance + 0.15f &&
            agent.velocity.sqrMagnitude < 0.01f;

        if (!arrived)
            return;

        agent.isStopped = true;
        SetWalking(false);

        if (state == State.Entering)
        {
            state = State.Talking;

            if (npcInteraction != null)
                npcInteraction.enabled = true;
        }
        else
        {
            state = State.Finished;

            onExitFinished?.Invoke();
            gameObject.SetActive(false);
        }
    }

    // Called when this suspect is invited
    public void EnterRoom()
    {
        gameObject.SetActive(true);

        if (agent == null ||
            entryPoint == null ||
            talkPoint == null)
            return;

        if (!NavMesh.SamplePosition(
            entryPoint.position,
            out NavMeshHit hit,
            2f,
            agent.areaMask))
        {
            Debug.LogWarning(
                "No NavMesh near EntryPoint: " + name);
            return;
        }

        if (!agent.Warp(hit.position))
            return;

        agent.isStopped = false;

        if (agent.SetDestination(talkPoint.position))
        {
            state = State.Entering;
            SetWalking(true);
        }
    }

    // Called after dialogue finishes
    public void LeaveRoom()
    {
        if (state != State.Talking ||
            exitPoint == null)
            return;

        if (npcInteraction != null)
        {
            npcInteraction.enabled = false;

            if (npcInteraction.interactionPrompt != null)
                npcInteraction.interactionPrompt.SetActive(false);
        }

        agent.isStopped = false;

        if (agent.SetDestination(exitPoint.position))
        {
            state = State.Exiting;
            SetWalking(true);
        }
    }

    void SetWalking(bool walking)
    {
        if (animator != null)
            animator.SetBool("IsWalking", walking);
    }
}
