using System.Collections.Generic;
using UnityEngine;

public abstract class FlockingBehaviorStep : ScriptableObject
{
    public abstract Vector3 CalculateMove(FlockingMovementData agent, List<FlockingMovementData> neighbors);
}