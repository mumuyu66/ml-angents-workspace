using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataProvider")]
public class DataProvider:ScriptableObject
{
    public ActorConfig[] actorConfigs;

    public ActorConfig FindActorConfig(int id)
    {
        foreach (var config in actorConfigs)
        {
            if (config != null && config.id == id)
            {
                return config;
            }
        }
        return null;
    }
}
