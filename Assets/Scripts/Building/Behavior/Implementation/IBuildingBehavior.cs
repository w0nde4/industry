public interface IBuildingBehavior
{
    void Initialize(PlacedBuilding owner, BuildingData data);
    void OnTick(float deltaTime);
    void OnResourceReceived(ConnectionPoint input, ResourceInstance resource);
    void CleanUp();
}