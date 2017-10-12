﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Player : LivingEntity
{
    [SerializeField]
    private int gold = 2000;

    private bool buildMode = false;

    [SerializeField]
    private float ramDamage = 20;

    PlayerController controller;

    private AudioSource audioSource;

    [SerializeField]
    private AudioClip[] goldPickup;

    [SerializeField]
    private AudioClip waves;

    buoyancy buoyant;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(PlayWaves());
    }

    private IEnumerator PlayWaves()
    {
        audioSource.PlayOneShot(waves, Random.Range(0.4f, 0.9f));

        yield return new WaitForSeconds(Random.Range(waves.length, waves.length * 2));

        StartCoroutine(PlayWaves());
    }

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        weaponController = GetComponent<WeaponController>();
        components = GetComponent<ComponentManager>();

        audioSource = GetComponent<AudioSource>();
        buoyant = GetComponentInChildren<buoyancy>();

        // Subscribe to game state
        GameState.buildModeChanged += SetBuildMode;
    }

    private void Update()
    {
        if (Input.GetAxis("Left_Trigger") == 1)
        {
            weaponController.FireWeaponsLeft();
        }

        if (Input.GetAxis("Right_Trigger") == 1)
        {
            weaponController.FireWeaponsRight();
        }
    }

    public int Gold
    {
        get { return gold; }
    }

    public void DeductGold(int _amount)
    {
        gold -= _amount;
    }

    public void GiveGold(int _amount)
    {
        if(goldPickup.Length > 0)
        {
            audioSource.PlayOneShot(goldPickup[Random.Range(0, goldPickup.Length)], Random.Range(0.9f, 1.3f));
        }

        gold += _amount;
    }

    private void SetBuildMode(bool isBuildMode)
    {
        buildMode = isBuildMode;

        buoyant.enabled = !buildMode;

        if (!buildMode)
        {
            weaponController.LeftWeapons = components.GetAttachedLeftWeapons();
            weaponController.RightWeapons = components.GetAttachedRightWeapons();
            controller.setSpeedBonus(components.getSpeedBonus());
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to game state
        GameState.buildModeChanged -= SetBuildMode;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.GetComponent<AttachmentBase>())
        {
            collision.collider.gameObject.GetComponent<AttachmentBase>().TakeDamage(ramDamage);
        }

        if (collision.collider.gameObject.GetComponent<AIAgent>())
        {
            collision.collider.gameObject.GetComponent<AIAgent>().TakeDamage(ramDamage);
        }
    }
}