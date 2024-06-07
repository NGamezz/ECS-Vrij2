using System;
using UnityEngine;

[CreateAssetMenu]
public class MoveTarget : ScriptableObject
{
    [NonSerialized] public Transform target;
}