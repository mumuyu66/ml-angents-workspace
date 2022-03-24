using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateType
{
    Idle,
    Move,
    Attack,
    UnNormal,
    Dead,
}

public interface IState
{
    // Start is called before the first frame update
    void Start();

    // Update is called once pr frame
    void Update();

    void Enter();
    void Exit();

    StateType Type();

}
