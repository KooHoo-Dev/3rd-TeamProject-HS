using Study_Inventory;

namespace _99._KimSeungGu
{
    public interface IScriptableObjectKind
    {
        ItemScriptableObject ThisItem { get; }
        void OnPickup();
    }
}