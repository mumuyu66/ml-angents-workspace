using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ActorConfig")]
public class ActorConfig : ScriptableObject
{
    public int id;
    public new string name;
    public int components;
    public GameObject avatar;
    public PropConfig prop;

}
