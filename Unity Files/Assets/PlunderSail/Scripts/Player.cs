﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LivingEntity
{
    [Header("Player Attributes")]

    [SerializeField]
    private int currentGold = 0;

    private int maxGold = 99999;

    [SerializeField]
    private float ramDamage = 0;

    [Header("Collision Knockback")]

    [SerializeField]
    private float islandKnockBack = 700000;

    [SerializeField]
    [Tooltip("The number of seconds that the player does not move forward after a collision")]
    private float islandStunDuration = 0;

    [SerializeField]
    private float shipKnockBack = 0;

    [SerializeField]
    [Tooltip("The number of seconds that the player does not move forward after a collision")]
    private float stunDuration = 0;

    [SerializeField]
    private string goldGainedSound = "";

    private PlayerController controller = null;

    private CameraController CC;

    private Mesh rangeMesh;

    public delegate void GoldReceiced();

    public event GoldReceiced GoldChanged;

    [Header("Aim visualiser Attributes")]

    [SerializeField]
    [Tooltip("Maximum distance from the ship that the mesh can go.")]
    private float maxRange = 50;

    [Tooltip("minimum distance from the ship that the mesh can go.")]
    [SerializeField]
    private float minimumRange = 0;

    [SerializeField]
    private MeshFilter rangeMeshFilter;

    private float range = 25;

    private float previousRange;

    private bool aiming = false;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        weaponController = GetComponent<WeaponController>();
        components = GetComponent<ComponentManager>();

        CC = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CameraController>();

        // Subscribe to game state
        GameState.buildModeChanged += SetBuildMode;
    }

    protected override void Start()
    {
        base.Start();

        rangeMesh = new Mesh();
        rangeMesh.name = "Range Mesh";
        rangeMeshFilter.mesh = rangeMesh;

        range = minimumRange;

        rangeMeshFilter.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Check Input for aiming
        if(!GameState.BuildMode)
        {
            if (Input.GetAxis("Left_Trigger") == 1)
            {
                weaponController.FireWeaponsLeft();
            }

            if (Input.GetAxis("Right_Trigger") == 1)
            {
                weaponController.FireWeaponsRight();
            }

            if (Input.GetAxis("Left_Bumper") == 1)
            {
                if (!aiming)
                {
                    rangeMeshFilter.transform.localEulerAngles = new Vector3(0f, 270f, 0f);
                    rangeMeshFilter.gameObject.SetActive(true);

                    aiming = true;
                }

                CC.AimLeft();

                foreach (WeaponAttachment weapon in components.GetAttachedLeftWeapons())
                {
                    weapon.Aim(new Vector3((transform.position - transform.right * range).x, 0f, (transform.position - transform.right * range).z));
                }
            }
            else if (Input.GetAxis("Right_Bumper") == 1)
            {
                if (!aiming)
                {
                    rangeMeshFilter.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                    rangeMeshFilter.gameObject.SetActive(true);

                    aiming = true;
                }

                CC.AimRight();

                foreach (WeaponAttachment weapon in components.GetAttachedRightWeapons())
                {
                    weapon.Aim(new Vector3((transform.position + transform.right * range).x, 0f, (transform.position + transform.right * range).z));
                }
            }
            else
            {
                if (aiming)
                {
                    aiming = false;

                    rangeMeshFilter.gameObject.SetActive(false);
                    CC.CancelAim();
                }
            }

            if(aiming)
            {
                range += Input.GetAxis("Mouse_Y");
                range = Mathf.Clamp(range, minimumRange, maxRange);
            }
        }

        velocity = controller.Velocity;
    }

    private void LateUpdate()
    {
        if (aiming)
        {
            Vector3 eular = rangeMeshFilter.gameObject.transform.eulerAngles;
            rangeMeshFilter.gameObject.transform.eulerAngles = new Vector3(0f, eular.y, 0f);

            DrawRangeMesh();
        }
    }

    // Returns how much gold the player currently has
    public int Gold
    {
        get { return currentGold; }
    }

    // Remove the specifed amount of gold from the player
    public void DeductGold(int _amount)
    {
        currentGold -= _amount;
    }

    // Add the specified amount of gold to the player
    public void GiveGold(int _amount)
    {
        AudioManager.instance.PlaySound(goldGainedSound);

        if (GoldChanged != null)
        {
            GoldChanged();
        }

        currentGold += _amount;

        if(currentGold > maxGold)
        {
            currentGold = maxGold;
        }
    }

    private void DrawRangeMesh()
    {
        Vector3[] vertices = new Vector3[4];
        int[] triangles = { 0, 1, 2, 0, 2, 3 };

        vertices[0] = new Vector3(-7, 0, 0);       // left back
        vertices[1] = new Vector3(-2, 0, range);    // left forward
        vertices[2] = new Vector3(2, 0, range);     // right forward
        vertices[3] = new Vector3(7, 0, 0);        // right back

        rangeMesh.Clear();
        rangeMesh.vertices = vertices;
        rangeMesh.triangles = triangles;
        rangeMesh.RecalculateNormals();
    }

    private void SetBuildMode(bool isBuildMode)
    {
        if (!isBuildMode)
        {
            UpdateAttachments();
        }
    }

    // Function which checks which attachments are on the ship.
    public void UpdateAttachments()
    {
        weaponController.LeftWeapons = components.GetAttachedLeftWeapons();
        weaponController.RightWeapons = components.GetAttachedRightWeapons();
        controller.setSpeedBonus(components.getSpeedBonus());
    }

    private void OnDestroy()
    {
        // Unsubscribe to game state
        GameState.buildModeChanged -= SetBuildMode;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // When colliding with an enemy we want the player to knock them back unless they're moving slower.

            //  first check who's going faster
            AIAgent AI = collision.gameObject.GetComponent<AIAgent>();

            if (velocity.magnitude > AI.Velocity.magnitude)
            {
                // player going faster means we knock the AI back instead of us
                AI.AddStun();

                // calculate force vector
                var force = collision.transform.position - transform.position;
                
                // normalize force vector to get direction only and trim magnitude
                force.Normalize();
                AI.GetComponent<Rigidbody>().AddForce(force * shipKnockBack);
            }
            else
            {
                // Stun stops the player controller from moving forward and allows us to add forces to the player
                controller.AddStun(stunDuration);

                // calculate force vector
                var force = transform.position - collision.transform.position;
                
                // normalize force vector to get direction only and trim magnitude
                force.Normalize();
                gameObject.GetComponent<Rigidbody>().AddForce(force * shipKnockBack);
            }
        }

        if (collision.contacts[0].thisCollider.CompareTag("Ram"))
        {
            float hitDamage = collision.relativeVelocity.magnitude;

            // Debug.Log("Hit with Ram with a force of " + hitDamage);

            if (collision.collider.gameObject.GetComponent<AttachmentBase>())
            {
                collision.collider.gameObject.GetComponent<AttachmentBase>().TakeDamage(ramDamage);
            }

            if (collision.collider.gameObject.GetComponent<AIAgent>())
            {
                collision.collider.gameObject.GetComponent<AIAgent>().TakeDamage(ramDamage);
            }
        }

        if(collision.gameObject.layer == 8)
        {
            // Stun stops the player controller from moving forward and allows us to add forces to the player
            controller.AddStun(islandStunDuration);

            // calculate force vector
            var force = transform.position - collision.transform.position;
            // normalize force vector to get direction only and trim magnitude
            force.Normalize();
            gameObject.GetComponent<Rigidbody>().AddForce(force * islandKnockBack);
        }
    }
}