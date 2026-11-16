
using UnityEngine;

public class InterrogationStartTrigger : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject startButton;

    [Header("Interrogation")]
    public Transform detectivePoint;
    public ThirdPersonController playerMovement;
    public InterrogationManager interrogationManager;

    [Header("Interrogation Camera")]
    public Camera mainCamera;
    public Transform interrogationCameraPoint;

    [Header("Investigation")]
    public bool investigationComplete = true;

    private bool playerInside;
    private bool buttonVisible;
    private bool interrogationStarted;

    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    void Start()
    {
        startButton.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        CharacterController cc =
            other.GetComponentInParent<CharacterController>();

        if (cc == null || cc.transform != player)
            return;

        playerInside = true;

        if (investigationComplete && !interrogationStarted)
            ShowButton();
    }

    void OnTriggerExit(Collider other)
    {
        CharacterController cc =
            other.GetComponentInParent<CharacterController>();

        if (cc == null || cc.transform != player)
            return;

        playerInside = false;
        HideButton();
    }

    void Update()
    {
        if (playerInside &&
            investigationComplete &&
            !interrogationStarted &&
            !buttonVisible)
        {
            ShowButton();
        }
    }

    void LateUpdate()
    {
        if (interrogationStarted)
        {
            // Lock movement during interrogation
            if (playerMovement != null)
                playerMovement.enabled = false;

            // Keep camera at interrogation position
            if (mainCamera != null &&
                interrogationCameraPoint != null)
            {
                mainCamera.transform.SetPositionAndRotation(
                    interrogationCameraPoint.position,
                    interrogationCameraPoint.rotation
                );
            }

            return;
        }

        if (buttonVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void ShowButton()
    {
        if (buttonVisible || interrogationStarted)
            return;

        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        startButton.SetActive(true);
        buttonVisible = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HideButton()
    {
        if (!buttonVisible)
            return;

        startButton.SetActive(false);
        buttonVisible = false;

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
    }

    public void BeginInterrogation()
    {
        if (interrogationStarted ||
            !playerInside ||
            !investigationComplete)
            return;

        if (player == null ||
            detectivePoint == null ||
            playerMovement == null ||
            interrogationManager == null ||
            mainCamera == null ||
            interrogationCameraPoint == null)
        {
            Debug.LogWarning(
                "Interrogation references are missing!"
            );
            return;
        }

        interrogationStarted = true;

        HideButton();

        // Disable CharacterController temporarily
        CharacterController cc =
            player.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        // Move detective to interrogation position
        player.SetPositionAndRotation(
            detectivePoint.position,
            detectivePoint.rotation
        );

        if (cc != null)
            cc.enabled = true;

        // Disable detective movement
        playerMovement.enabled = false;

        // Move camera to interrogation view
        mainCamera.transform.SetPositionAndRotation(
            interrogationCameraPoint.position,
            interrogationCameraPoint.rotation
        );

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Call the first suspect
        interrogationManager.BeginInterrogations();
    }

    public void EndInterrogation()
    {
        if (!interrogationStarted)
            return;

        interrogationStarted = false;
        investigationComplete = false;

        playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
