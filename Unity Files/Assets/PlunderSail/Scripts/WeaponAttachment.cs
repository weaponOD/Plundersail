﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttachment : AttachmentBase
{
    [SerializeField]
    protected float minFireTime = 0;

    [SerializeField]
    protected float maxFireTime = 0;

    [SerializeField]
    protected float damage;

    [SerializeField]
    protected float projectileForce;

    protected int numberOfFirePoints = 0;

    [Header("Effects resources")]
    [SerializeField]
    protected AudioClip[] shootSound;

    [SerializeField]
    protected ParticleSystem shootParticle;

    [SerializeField]
    protected GameObject projectilePrefab;

    protected AudioSource audioSource;

    protected Transform[] firePoints;

    protected Transform[] effectPoints;

    protected bool facingLeft;

    protected LivingEntity entity;

    protected Vector3 shipVelocity;

    protected int pointCount = 0;

    protected override void Awake()
    {
        audioSource = transform.root.GetComponent<AudioSource>();

        if(transform.root.GetComponent<Player>())
        {
            entity = transform.root.GetComponent<Player>();
        }
        else
        {
            entity = transform.root.GetComponent<AIAgent>();
        }

        firePoints = new Transform[numberOfFirePoints];

        foreach(Transform child in transform)
        {
            if(child.CompareTag("FirePoint"))
            {
                firePoints[pointCount] = child;
                pointCount++;
            }
        }
    }

    public bool FacingLeft
    {
        get
        {
            facingLeft = (transform.localEulerAngles.y > 95f);
            return facingLeft;
        }
        set { facingLeft = value; }
    }

    public void FireWeapon()
    {
        foreach (Transform firePoint in firePoints)
        {
            StartCoroutine(Fire(firePoint));
        }
    }

    protected virtual IEnumerator Fire(Transform _firePoint)
    {
        yield return new WaitForSeconds(Random.Range(minFireTime, maxFireTime));

        shipVelocity = entity.Velocity;

        Projectile shot = Instantiate(projectilePrefab, _firePoint.position, _firePoint.rotation).GetComponent<Projectile>();
        shot.Damage = damage;
        shot.FireProjectile(shipVelocity, projectileForce);

        if(shootParticle != null)
        {
            Destroy(Instantiate(shootParticle.gameObject, _firePoint.position, _firePoint.rotation) as GameObject, shootParticle.main.startLifetime.constant);
        }

        PlayRandomSound();
    }

    protected void PlayRandomSound()
    {
        if(shootSound.Length > 0)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.volume = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(shootSound[Random.Range(0, shootSound.Length)], 0.4F);
        }
    }
}