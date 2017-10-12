﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    private float reloadTime = 2f;
    private float nextReloadTimeRight = 0;
    private float nextReloadTimeLeft = 0;

    private bool canShootRight = true;
    private bool canShootLeft = true;

    private AudioSource audioSource;

    private PlayerController player;

    [Header("Combat Attributes")]
    [SerializeField]
    private float minFireTime;

    [SerializeField]
    private float maxFireTime;

    [SerializeField]
    private AudioClip[] FireShout;

    [Header("Debug Info")]
    [SerializeField]
    private AttachmentWeapon[] leftWeapons;

    [SerializeField]
    private AttachmentWeapon[] rightWeapons;

    private int currentShout;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        player = GetComponent<PlayerController>();

        // Subscribe to game state
        GameState.buildModeChanged += SetBuildMode;
    }

    public AttachmentWeapon[] LeftWeapons
    {
        set { leftWeapons = value; }
    }

    public AttachmentWeapon[] RightWeapons
    {
        set { rightWeapons = value; }
    }

    private void Update()
    {
        if (!GameState.BuildMode)
        {
            if (Time.time > nextReloadTimeRight)
            {
                canShootRight = true;
            }

            if (Time.time > nextReloadTimeLeft)
            {
                canShootLeft = true;
            }
        }
    }

    public void FireWeaponsRight()
    {
        if (canShootRight)
        {
            if (rightWeapons.Length > 0)
            {
                if (FireShout.Length > 0 && Random.Range(0, 100) < 20)
                {
                    currentShout = Random.Range(0, FireShout.Length);
                    audioSource.PlayOneShot(FireShout[currentShout], 0.8F);
                }

                StartCoroutine(FireRight());

                canShootRight = false;
                nextReloadTimeRight = Time.time + reloadTime;
            }
        }
    }

    public void FireWeaponsLeft()
    {
        if (canShootLeft)
        {
            if (leftWeapons.Length > 0)
            {
                if (FireShout.Length > 0 && Random.Range(0, 100) < 20)
                {
                    currentShout = Random.Range(0, FireShout.Length);
                    audioSource.PlayOneShot(FireShout[currentShout], 0.8F);
                }

                StartCoroutine(FireLeft());

                canShootLeft = false;
                nextReloadTimeLeft = Time.time + reloadTime;
            }
        }
    }

    private IEnumerator FireLeft()
    {
        if(FireShout.Length > 0)
        {
            yield return new WaitForSeconds(FireShout[currentShout].length);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }
        

        foreach (AttachmentWeapon weapon in leftWeapons)
        {
            weapon.FireLeft();
            yield return new WaitForSeconds(Random.Range(minFireTime, maxFireTime));
        }
    }

    private IEnumerator FireRight()
    {
        if (FireShout.Length > 0)
        {
            yield return new WaitForSeconds(FireShout[currentShout].length);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        foreach (AttachmentWeapon weapon in rightWeapons)
        {
            weapon.FireRight();
            yield return new WaitForSeconds(Random.Range(minFireTime, maxFireTime));
        }
    }

    private void SetBuildMode(bool isBuildMode)
    {
        canShootRight = !isBuildMode;
        canShootLeft = !isBuildMode;
    }

    private void OnDestroy()
    {
        // Unsubscribe to game state
        GameState.buildModeChanged -= SetBuildMode;
    }
}
