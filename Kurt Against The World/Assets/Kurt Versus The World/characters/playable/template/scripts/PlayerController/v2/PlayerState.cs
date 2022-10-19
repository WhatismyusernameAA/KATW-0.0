using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    // declare functions to be filled in later for each instance
    public abstract void OnStateInit(PlayerStateManager currentPlayer);
    public abstract void OnStateEntered(PlayerStateManager currentPlayer);
    public abstract void OnStateExit(PlayerStateManager currentPlayer);

    public abstract void StateFixedUpdate(PlayerStateManager currentPlayer);
}
