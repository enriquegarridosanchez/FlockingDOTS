using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Flocking/BehaviorStep/CohesionPresetConfig")]
public class CohesionPresetConfig : BehaviorPresetConfig
{
    public override void AddComponent(ref SystemState state, Entity entity)
    {
        state.EntityManager.AddComponent<CohesionData>(entity);
        state.EntityManager.SetComponentData(entity, new CohesionData
        {
            cohesionWeight = weight
        });
    }
}
