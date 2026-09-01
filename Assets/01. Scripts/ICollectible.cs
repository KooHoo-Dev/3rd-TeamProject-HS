namespace WeGotOne
{
    public interface ICollectible
    {
        ItemScriptableObject ThisItem { get; }
        void OnPickup();
    }
}
