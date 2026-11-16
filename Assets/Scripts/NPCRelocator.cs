
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCRelocator : MonoBehaviour
{
    [Header("Destination")]
    public Transform destination;

    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";

    private NavMeshAgent agent;
    private bool isWalking;
    private bool hasSpeedParameter;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Intro Cutscene-ში არ ჩაერიოს
        agent.enabled = false;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == speedParameter &&
                    p.type == AnimatorControllerParameterType.Float)
                {
                    hasSpeedParameter = true;
                    break;
                }
            }
        }
    }

    [ContextMenu("Move Now")]
    public void MoveNow()
    {
        if (destination == null)
        {
            Debug.LogWarning("Destination is missing!");
            return;
        }

        agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Nika is not on NavMesh!");
            agent.enabled = false;
            return;
        }

        if (animator != null)
            animator.applyRootMotion = false;

        agent.speed = 1.7f;
        agent.stoppingDistance = 0.25f;
        agent.isStopped = false;

        if (!agent.SetDestination(destination.position))
        {
            Debug.LogWarning("Could not request path!");
            agent.enabled = false;
            return;
        }

        isWalking = true;
        Debug.Log("Nika started walking upstairs.");
    }

    void Update()
    {
        if (!isWalking) return;

        if (animator != null && hasSpeedParameter)
        {
            float speed = agent.velocity.sqrMagnitude > 0.02f
                ? 0.5f : 0f;

            animator.SetFloat(speedParameter, speed);
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.15f &&
            agent.velocity.sqrMagnitude < 0.01f)
        {
            isWalking = false;
            agent.isStopped = true;

            if (animator != null && hasSpeedParameter)
                animator.SetFloat(speedParameter, 0f);

            Debug.Log("Nika reached upstairs.");
        }
    }
}
