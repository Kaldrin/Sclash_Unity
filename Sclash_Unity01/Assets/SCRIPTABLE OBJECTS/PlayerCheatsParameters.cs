﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCheatsSettings01", menuName = "ScriptableObjects/Player cheats settings")]
public class PlayerCheatsParameters : ScriptableObject
{
    public KeyCode clashCheatKey = KeyCode.Alpha1;
    public KeyCode deathCheatKey = KeyCode.Alpha2;
    public KeyCode staminaCheatKey = KeyCode.Alpha4;
    public KeyCode stopStaminaRegenCheatKey = KeyCode.Alpha6;
    public KeyCode triggerStaminaRecupAnim = KeyCode.Alpha7;

    public bool useTransparencyForDodgeFrames = true;
    public bool useExtraDiegeticFX = true;
    public bool infiniteStamina = false;
}
