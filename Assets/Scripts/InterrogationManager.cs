
using UnityEngine;

public class InterrogationManager : MonoBehaviour
{
    public InterrogationActor[] suspects;

    private int currentIndex = -1;
    private bool started = false;

    public void BeginInterrogations()
    {
        if (started || suspects == null || suspects.Length == 0)
            return;

        started = true;
        currentIndex = 0;

        CallNextSuspect();
    }

    void CallNextSuspect()
    {
        if (currentIndex >= suspects.Length)
        {
            Debug.Log("All interrogations finished!");
            return;
        }

        InterrogationActor actor = suspects[currentIndex];

        if (actor == null)
            return;

        actor.onExitFinished.AddListener(OnSuspectExited);
        actor.EnterRoom();
    }

    void OnSuspectExited()
    {
        suspects[currentIndex].onExitFinished
            .RemoveListener(OnSuspectExited);

        currentIndex++;

        CallNextSuspect();
    }
}
