using System;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public PlatformBaseStateSO currentState;

    private void Start()
    {
        currentState?.EnterState(this);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void OnStepped(GameObject player)
    {
        currentState?.OnStepped(this, player);
    }

    public void SetState(PlatformBaseStateSO newState)
    {
        currentState = newState;
        currentState.EnterState(this);
    }
}
