using UnityEngine;

public abstract class BehaviorConfig : ScriptableObject
{
    public abstract IBuildingBehavior CreateBehavior();
}