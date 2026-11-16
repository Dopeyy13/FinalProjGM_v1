
using UnityEngine;
using TMPro;

public class InspectionController : MonoBehaviour
{
    [System.Serializable]
    public class InspectSpot
    {
        public string locationName;
        public Transform inspectPoint;
    }

    public InspectSpot[] spots;

    public Camera mainCamera;
    public Camera inspectionCamera;

    public Transform player;
    public ThirdPersonController playerMovement;
    public DialogueManager dialogueManager;
    public TMP_Text inspectionPrompt;

    // უკვე არსებული EvidenceManager
    public EvidenceManager evidenceManager;

    public float interactionDistance = 2.5f;

    // ახალი: მაქსიმალური სიმაღლის სხვაობა
    public float maxHeightDifference = 1.8f;

    public float mouseSensitivity = 2f;
    public float maxHorizontalAngle = 35f;
    public float maxVerticalAngle = 25f;

    private bool isInspecting;
    private InspectSpot activeSpot;

    private float horizontalRotation;
    private float verticalRotation;

    void Start()
    {
        inspectionCamera.enabled = false;
        inspectionPrompt.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isInspecting)
        {
            // ჯერ მტკიცებულება იხურება, შემდეგ კამერა
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (evidenceManager != null &&
                    evidenceManager.IsOpen)
                {
                    evidenceManager.CloseEvidence();
                }
                else
                {
                    ExitInspection();
                }

                return;
            }

            // მაუსის მარცხენა ღილაკი
            if (evidenceManager != null &&
                !evidenceManager.IsOpen &&
                Input.GetMouseButtonDown(0))
            {
                TryClickEvidence();
            }

            return;
        }

        inspectionPrompt.gameObject.SetActive(false);

        if (!playerMovement.enabled) return;

        if (dialogueManager != null &&
            dialogueManager.IsDialogueOpen)
            return;

        InspectSpot nearbySpot = null;
        float closestDistance = interactionDistance;

        foreach (InspectSpot spot in spots)
        {
            if (spot.inspectPoint == null) continue;

            // ახალი: სხვადასხვა სართულზე ყოფნის შემოწმება
            float heightDifference = Mathf.Abs(
                player.position.y -
                spot.inspectPoint.position.y
            );

            if (heightDifference > maxHeightDifference)
                continue;

            Vector3 a = player.position;
            Vector3 b = spot.inspectPoint.position;

            a.y = b.y = 0;

            float distance = Vector3.Distance(a, b);

            if (distance <= closestDistance)
            {
                closestDistance = distance;
                nearbySpot = spot;
            }
        }

        if (nearbySpot != null)
        {
            inspectionPrompt.text =
                "[E] " + nearbySpot.locationName;

            inspectionPrompt.gameObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
                EnterInspection(nearbySpot);
        }
    }

    void EnterInspection(InspectSpot spot)
    {
        activeSpot = spot;
        isInspecting = true;

        horizontalRotation = 0;
        verticalRotation = 0;

        playerMovement.enabled = false;
        inspectionPrompt.gameObject.SetActive(false);

        inspectionCamera.transform.SetPositionAndRotation(
            spot.inspectPoint.position,
            spot.inspectPoint.rotation
        );

        mainCamera.enabled = false;
        inspectionCamera.enabled = true;

        // ფიქსირებული კამერა, თავისუფალი კურსორი
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // მაუსით კონკრეტული მტკიცებულების ამოცნობა
    void TryClickEvidence()
    {
        Ray ray = inspectionCamera.ScreenPointToRay(
            Input.mousePosition
        );

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            8f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        int selectedIndex = -1;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < evidenceManager.evidences.Length; i++)
        {
            Transform item = evidenceManager.evidences[i].item;

            if (item == null) continue;

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == item ||
                    hit.transform.IsChildOf(item))
                {
                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        selectedIndex = i;
                    }
                }
            }
        }

        if (selectedIndex != -1)
        {
            evidenceManager.OpenEvidence(selectedIndex);
        }
    }

    // ძველი მეთოდი შენარჩუნებულია,
    // მაგრამ ფიქსირებული კამერის დროს აღარ გამოიძახება
    void RotateCamera()
    {
        horizontalRotation +=
            Input.GetAxis("Mouse X") * mouseSensitivity;

        verticalRotation -=
            Input.GetAxis("Mouse Y") * mouseSensitivity;

        horizontalRotation = Mathf.Clamp(
            horizontalRotation,
            -maxHorizontalAngle,
            maxHorizontalAngle
        );

        verticalRotation = Mathf.Clamp(
            verticalRotation,
            -maxVerticalAngle,
            maxVerticalAngle
        );

        inspectionCamera.transform.SetPositionAndRotation(
            activeSpot.inspectPoint.position,
            activeSpot.inspectPoint.rotation *
            Quaternion.Euler(
                verticalRotation,
                horizontalRotation,
                0f
            )
        );
    }

    void ExitInspection()
    {
        isInspecting = false;

        inspectionCamera.enabled = false;
        mainCamera.enabled = true;

        playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
