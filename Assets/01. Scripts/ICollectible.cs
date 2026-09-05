namespace WeGotOne
{
    public interface ICollectible
    {
        ItemRef ThisItem { get; }
        void OnPickup();
    }
}
