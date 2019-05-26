﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{


    // COMPONENTS
    Rigidbody2D rb;
    PlayerStats playerStats;
    PlayerAnimations playerAnimations;
    PlayerMovement playerMovement;



    // TAP
    float lastTap;
    float tapTime = 0.25f;
    bool tapping;



    // AATTACK RECOVERY
    [SerializeField] float minRecoveryDuration = 0.4f;
    [SerializeField] float maxRecoveryDuration = 1;
    bool attackRecovery = false;
    float attackRecoveryStartTime;
    float attackRecoveryDuration;



    // CHARGE & ATTACK
    [SerializeField] float durationToNextChargeLevel = 1.5f;
    [SerializeField] float maxHoldDurationAtMaxCharge = 3;
    float maxChargeLevelStartTime;
    bool maxChargeLevelReached = false;
    float chargeStartTime;
    int chargeLevel = 1;
    [SerializeField] int maxChargeLevel = 2;

    bool charging = false;
    bool triggerAttack = false;

    

    // PARRY
    [HideInInspector] public bool parrying;



    // DASH
    int dashStep = 0;
    //The current step to the dash
    // 0 = Nothing done
    // 1 = Player pressed the direction 1 time
    // 2 = Player released the direction after pressing it
    // 3 = Player pressed again the same direction after releasing it, dash
    float dashDirection;
    float tempDashDirection;
    float dashInitStartTime = 0;
    [SerializeField] float dashAllowanceDuration = 0.3f;
    [Range(1.0f, 10.0f)] public float dashSpeed;
    [Range(1.0f, 10.0f)] public float dashDistance;
    [SerializeField] float attackDashDistance = 3;
    float actualDashDistance;
    [HideInInspector] public bool isDashing;
    Vector3 initPos;
    Vector3 targetPos;
    float time;


    void Start()
    {
        //Get components
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
        playerAnimations = GetComponent<PlayerAnimations>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (playerStats.stamina > 0)
        {
            // ATTACK
            ManageCharge();

            // DASH
            ManageDash();
        }

        ManageRecoveries();
    }

    void ManageRecoveries()
    {
        if (attackRecovery)
        {
            if (Time.time - attackRecoveryStartTime >= attackRecoveryDuration)
            {
                attackRecovery = false;
            }
        }
    }

    void ManageCharge()
    {

        //Player presses parry button while charging
        if (Input.GetButtonDown("Parry" + playerStats.playerNum) && charging)
        {
            Parry();
        }


        //Player presses attack button
        if (Input.GetButtonDown("Fire" + playerStats.playerNum) && !attackRecovery)
        {
            playerAnimations.ChargeChange(chargeLevel);
            charging = true;
            chargeStartTime = Time.time;
            playerAnimations.TriggerCharge();
            playerMovement.Charging(true);
        }


        //Player releases attack button
        if (Input.GetButtonUp("Fire" + playerStats.playerNum) && charging)
        {
            ReleaseAttack();
        }


        //When the player is charging the attack
        if (charging)
        {
            if (maxChargeLevelReached)
            {
                if (Time.time - maxChargeLevelStartTime >= maxHoldDurationAtMaxCharge)
                {
                    ReleaseAttack();
                }
            }
            else if (Time.time - chargeStartTime >= durationToNextChargeLevel)
            {
                chargeStartTime = Time.time;

                if (chargeLevel < maxChargeLevel)
                {
                    chargeLevel++;
                    playerAnimations.ChargeChange(chargeLevel);
                    Debug.Log(chargeLevel);
                }
                else if (chargeLevel >= maxChargeLevel)
                {
                    chargeLevel = maxChargeLevel;
                    maxChargeLevelStartTime = Time.time;
                    maxChargeLevelReached = true;
                }
            }
        }
    }

    void ReleaseAttack()
    {
        // Charge
        playerMovement.Charging(false);
        charging = false;
        chargeLevel = 1;
        maxChargeLevelReached = false;
        playerAnimations.TriggerCharge();

        actualDashDistance = attackDashDistance;
        triggerAttack = true;
        playerAnimations.TriggerAttack();
        ApplyDamage();


        // Activate recovery
        attackRecovery = true;
        attackRecoveryStartTime = Time.time;
        attackRecoveryDuration = minRecoveryDuration + (maxRecoveryDuration - minRecoveryDuration) * (chargeLevel - 1) / maxChargeLevel;
        Debug.Log(maxChargeLevel);
    }

    public void Parry()
    {
        StartCoroutine(ParryC());
    }

    IEnumerator ParryC()
    {
        parrying = true;
        playerMovement.Charging(false);
        charging = false;
        chargeLevel = 1;
        maxChargeLevelReached = false;
        playerAnimations.Parry(true);
        playerAnimations.CancelCharge();

        actualDashDistance = attackDashDistance;

        yield return new WaitForSeconds(0.5f);
    }


    //DASH
    void ManageDash()
    {


        if (dashStep == 1 || dashStep == 2)
        {
            if (Time.time - dashInitStartTime > dashAllowanceDuration)
            {
                dashStep = 0;
            }
        }

        //The player needs to let go the input before pressing it again to dash
        if (Mathf.Abs(Input.GetAxis("Horizontal" + playerStats.playerNum)) < 0.2)
        {
            if (dashStep == 1)
            {
                dashStep = 2;
            }

            if (dashStep == 3)
            {
                dashStep = 0;
            }

        }

        //When the player presses the directiosn
        if (Mathf.Abs(Input.GetAxis("Horizontal" + playerStats.playerNum)) > 0.2)
        {
            tempDashDirection = Mathf.Sign(Input.GetAxis("Horizontal" + playerStats.playerNum));

            if (tempDashDirection != dashDirection)
            {
                dashStep = 0;
            }


            if (dashStep == 0)
            {
                dashStep = 1;
                dashDirection = tempDashDirection;
                dashInitStartTime = Time.time;

            }
            else if (dashStep == 2 && dashDirection == tempDashDirection)
            {
                dashStep = 3;
                actualDashDistance = dashDistance;
                isDashing = true;
                initPos = transform.position;
                targetPos = transform.position + new Vector3(dashDistance * tempDashDirection, 0, 0);
            }
        }
    }

    IEnumerator SingleTap()
    {
        yield return new WaitForSeconds(tapTime);
        if (tapping)
        {
            Debug.Log("Single Tap");
            tapping = false;
            dashDirection = 0;
        }
    }

    void ApplyDamage()
    {
        Collider2D[] hitsCol = Physics2D.OverlapBoxAll(new Vector2(transform.position.x + (transform.localScale.x * -1), transform.position.y), new Vector2(1, 1), 0);
        List<GameObject> hits = new List<GameObject>();


        foreach (Collider2D c in hitsCol)
        {
            Debug.Log(c);
            if (c.CompareTag("Player"))
            {
                if (!hits.Contains(c.transform.parent.gameObject))
                {
                    hits.Add(c.transform.parent.gameObject);
                }
            }
        }
        foreach (GameObject g in hits)
        {
            g.GetComponent<PlayerStats>().TakeDamage(gameObject, chargeLevel);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(new Vector3(transform.position.x + (transform.localScale.x * -1), transform.position.y, transform.position.z), new Vector3(1, 1, 1));
    }

    void FixedUpdate()
    {
        if (triggerAttack)
        {
            Vector3 dir = new Vector3(0, 0, 0);
            if (Mathf.Abs(Input.GetAxis("Horizontal" + playerStats.playerNum)) > 0.2)
                dir = new Vector3(dashDirection, 0, 0);

            dir *= dashDistance;

            initPos = transform.position;
            targetPos = transform.position + dir;
            targetPos.y = transform.position.y;

            time = 0.0f;
            rb.velocity = Vector3.zero;

            isDashing = true;
            rb.gravityScale = 0f;

            //rb.AddForce((Vector2)dir * dashMultiplier, ForceMode2D.Impulse);

            triggerAttack = false;
        }

        if (isDashing)
        {
            time += Time.deltaTime * dashSpeed;
            transform.position = Vector3.Lerp(initPos, targetPos, time);
            if (time >= 1.0f)
            {
                time = 0;
                isDashing = false;
            }
        }
    }
}
