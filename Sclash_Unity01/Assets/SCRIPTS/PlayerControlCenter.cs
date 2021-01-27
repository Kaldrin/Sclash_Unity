﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlCenter : MonoBehaviour
{
    PlayerInput m_playerInput;
    [SerializeField]
    int m_playerIndex;
    float m_DashOrientation = 0f;


    Player attachedPlayer;

    private void Start()
    {
        m_playerInput = GetComponent<PlayerInput>();
        m_playerIndex = m_playerInput.playerIndex;
        attachedPlayer = GameManager.Instance.playersList[m_playerIndex].GetComponent<Player>();
    }


    public void OnHorizontal(InputAction.CallbackContext ctx)
    {
        InputManager.Instance.playerInputs[m_playerIndex].horizontal = ctx.ReadValue<float>();
        if (attachedPlayer != null)
            attachedPlayer.ManageMovementsInputs(ctx);
    }

    public void OnVertical(InputAction.CallbackContext ctx)
    {
        InputManager.Instance.playerInputs[m_playerIndex].vertical = ctx.ReadValue<float>();
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            InputManager.Instance.playerInputs[m_playerIndex].attack = true;
            InputManager.Instance.playerInputs[m_playerIndex].attackDown = true;
        }
        else if (ctx.performed)
        {
            InputManager.Instance.playerInputs[m_playerIndex].attackDown = false;
        }
        else if (ctx.canceled)
        {
            InputManager.Instance.playerInputs[m_playerIndex].attack = false;
            InputManager.Instance.playerInputs[m_playerIndex].attackDown = false;
        }
    }

    public void OnParry(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            InputManager.Instance.playerInputs[m_playerIndex].parry = true;
            InputManager.Instance.playerInputs[m_playerIndex].parryDown = true;
        }
        else if (ctx.performed)
        {
            InputManager.Instance.playerInputs[m_playerIndex].parryDown = false;
        }
        else if (ctx.canceled)
        {
            InputManager.Instance.playerInputs[m_playerIndex].parry = false;
            InputManager.Instance.playerInputs[m_playerIndex].parryDown = false;
        }
    }

    public void OnPommel(InputAction.CallbackContext ctx)
    {
        InputManager.Instance.playerInputs[m_playerIndex].kick = ctx.performed;
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            m_DashOrientation = Mathf.Sign(ctx.ReadValue<float>());
        if (ctx.performed)
            InputManager.Instance.playerInputs[m_playerIndex].dash = m_DashOrientation;
        if (ctx.canceled)
        {
            InputManager.Instance.playerInputs[m_playerIndex].dash = 0f;
            m_DashOrientation = 0f;
        }
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {

    }

    public void OnScore(InputAction.CallbackContext ctx)
    {
        InputManager.Instance.scoreInput = ctx.started;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        InputManager.Instance.playerInputs[m_playerIndex].jump = ctx.started;
    }

    public void OnAnyKey(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            InputManager.Instance.playerInputs[m_playerIndex].anyKey = true;
            InputManager.Instance.playerInputs[m_playerIndex].anyKeyDown = true;
        }
        else if (ctx.canceled)
        {
            InputManager.Instance.playerInputs[m_playerIndex].anyKey = false;
            InputManager.Instance.playerInputs[m_playerIndex].anyKeyDown = false;
        }
    }
}