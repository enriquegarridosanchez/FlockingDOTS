using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Flocking/BehaviorStep/Cohesion")]
public class Cohesion_FlockingBehaviorStep : FlockingBehaviorStep
{
    public override Vector3 CalculateMove(FlockingMovementData agent, List<FlockingMovementData> neighbors)
    {
        float3 cohesion = float3.zero;

        if(neighbors.Count > 0)
        {
            cohesion = agent.position;

            foreach (FlockingMovementData neighbour in neighbors)
            {
                cohesion += neighbour.position;
            }

            cohesion /= neighbors.Count + 1;
        }

        return cohesion;
    }
}
