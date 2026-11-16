
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class DetectiveLookAt : MonoBehaviour
{
    public Transform lookTarget;

    [Header("Settings")]
    public float lookDuration = 2.5f;
    public float smoothSpeed = 3f;

    private Animator animator;
    private float currentWeight;
    private float targetWeight;
    private Coroutine lookRoutine;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        currentWeight = Mathf.MoveTowards(
            currentWeight,
            targetWeight,
            smoothSpeed * Time.deltaTime
        );
    }

    [ContextMenu("Test Look At Nika")]
    public void LookAtNika()
    {
        if (lookTarget == null) return;

        if (lookRoutine != null)
            StopCoroutine(lookRoutine);

        lookRoutine = StartCoroutine(LookRoutine());
    }

    IEnumerator LookRoutine()
    {
        targetWeight = 1.7f;

        yield return new WaitForSeconds(lookDuration);

        targetWeight = 0f;
        lookRoutine = null;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || lookTarget == null)
            return;

        animator.SetLookAtWeight(
            currentWeight, 0f, 0.7f, 1.5f, 1.3f
        );

        animator.SetLookAtPosition(lookTarget.position);
    }
}
