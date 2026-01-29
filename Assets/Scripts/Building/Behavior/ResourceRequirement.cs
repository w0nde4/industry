using Sirenix.OdinInspector;

[System.Serializable]
public class ResourceRequirement
{
    public ResourceData resource;
    [MinValue(1)]
    public int amount = 1;
}