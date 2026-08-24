namespace WeGotOne
{
    public interface IPickupable
    {
        ItemScriptableObject ThisItem { get; }
        void OnPickup();
    }
}