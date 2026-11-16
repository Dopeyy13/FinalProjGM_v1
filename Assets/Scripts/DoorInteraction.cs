
using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    public Transform player;

    public float interactionDistance = 2f;
    public float openAngle = 90f;
    public float openDuration = 0.6f;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private bool isOpen = false;
    private bool isMoving = false;

    void Start()
    {
        closedRotation = transform.localRotation;
        openedRotation = closedRotation *
                         Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        if (player == null || isMoving)
            return;

        float distance = Vector3.Distance(
            player.position, transform.position);

        if (distance <= interactionDistance &&
            Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;

            StartCoroutine(RotateDoor(
                isOpen ? openedRotation : closedRotation));
        }
    }

    IEnumerator RotateDoor(Quaternion target)
    {
        isMoving = true;

        Quaternion start = transform.localRotation;
        float time = 0f;

        while (time < openDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / openDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localRotation =
                Quaternion.Slerp(start, target, t);

            yield return null;
        }

        transform.localRotation = target;
        isMoving = false;
    }
}
