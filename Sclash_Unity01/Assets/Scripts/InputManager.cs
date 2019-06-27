﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputManager : MonoBehaviour
{
    [HideInInspector] public bool score = false;
    //[SerializeField] TextMeshPro[] controllersText = null;

    public struct PlayerInputs
    {
        public float horizontal;
        public bool kick;
        public bool kickDown;
        public bool anyKeyDown;
        public bool anyKey;
    }

    [HideInInspector] public PlayerInputs[] playerInputs = new PlayerInputs[2];










    // BASE FUNCTIONS   
    // Start is called before the first frame update
    void Start()
    {
        /*
        if (Input.GetJoystickNames().Length > 1)
        {
            for (int i = 0; i < Input.GetJoystickNames().Length; i++)
            {
                Debug.Log(Input.GetJoystickNames()[i]);
                controllersText[i].text = Input.GetJoystickNames()[i];
            }
        }
        */
    }

    // Update is called once per graphic frame
    void Update()
    {
        
    }

    // FixedUpdate is called 30 times per second
    private void FixedUpdate()
    {
        score = Input.GetButton("Score");
        ManageHorizontalInput();
        ManageKick();
        ManageAnyKey();
    }







    // HORIZONTAL
    void ManageHorizontalInput()
    {
        for (int i = 0; i < playerInputs.Length; i++)
        {
            playerInputs[i].horizontal = Input.GetAxis("Horizontal" + (i + 1));
        }
    }





    // KICK
    void ManageKick()
    {
        for (int i = 0; i < playerInputs.Length; i++)
        {
            playerInputs[i].kick = Input.GetButton("Kick" + (i + 1));
            playerInputs[i].kickDown = Input.GetButtonDown("Kick" + (i + 1));    
        }
    }





    // ANY KEY
    void ManageAnyKey()
    {
        for (int i = 0; i < playerInputs.Length; i++)
        {
            playerInputs[i].anyKeyDown = (Input.GetButtonDown("Kick" + (i + 1))
                || Input.GetButtonDown("Parry" + (i + 1))
                || Input.GetButtonDown("Fire" + (i + 1))
                || Input.GetAxis("Fire" + (i + 1)) > 0.1
                || Input.GetAxis("Parry" + (i + 1)) < - 0.1f
                || Input.GetButtonDown("Kick" + (i + 1)));
            //playerInputs[i].anyKey = Input.GetButtonDown("Kick" + (i + 1));
        }
    }
}
