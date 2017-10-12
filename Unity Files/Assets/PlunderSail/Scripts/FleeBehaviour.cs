﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Behaviour steers the AI towards the a point to the lef or right of the player, whichever is closest. This allows the AI to be along side the player rather than behind.
/// </summary>
/// 
public class FleeBehaviour : IBehaviour
{
    public override Vector3 ApplyBehaviour(Vector3 _myPos, Transform _target)
    {
        targetDirection = (_myPos - _target.position).normalized;

        return targetDirection;
    }
}