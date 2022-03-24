using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PropConfig")]
public class PropConfig : ScriptableObject
{
    public int id;
    public int hp;
    public int attack;
    public float attackSpeed;
    public float moveSpeed;
    public float attackRange;
    public float alertRange;
}
