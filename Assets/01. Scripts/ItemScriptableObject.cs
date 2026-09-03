using UnityEngine;

namespace WeGotOne
{
    public enum ItemType
    {
        Plank,
        Rope,
        Fish,
    }
    
    [CreateAssetMenu(fileName = "ItemScriptableObject", 
        menuName = "WeGotOne SAO/ItemSAO", order = 1)]
    public class ItemScriptableObject : ScriptableObject
    {
        public int      Key;            // 아이템을 찾을 Key
        public string   Name;           // 명칭
        public Sprite   Icon;           // 아이템의 Sprite 아이콘
        public int      Fullness;       // 채워주는 허기
        [Min(0)] public int BuildValue;
        public ItemType  ItemType;
    }
}
