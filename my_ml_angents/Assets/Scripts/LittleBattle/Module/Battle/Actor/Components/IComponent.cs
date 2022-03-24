using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Battle.Actor
{
    [System.Flags]
    public enum ComponentType
    {
        Transform = 1,
        Prop = 2,
        AI = 4,
        Animator = 8,
        LockTarget = 16,
        Move = 32,
        Skill = 64,
        StateMachine = 128,
        Successor = 256,
        Summoner = 512,
        Damage = 1024,
        Input = 2048
    }

    public interface IComponent
    {
        // Start is called before the first frame update
        void Start();


        // Update is called once per frame
        void Update();

    }

}