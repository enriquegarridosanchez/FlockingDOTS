using Unity.Entities;
using UnityEngine;

public abstract class BehaviorPresetConfig : ScriptableObject
{
    public float weight = 1f;
    public abstract void AddComponent(ref SystemState state, Entity entity);
}