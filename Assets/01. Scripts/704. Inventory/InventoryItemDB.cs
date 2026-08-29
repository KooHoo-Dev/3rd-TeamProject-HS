using System.Collections.Generic;
using UnityEngine;

namespace WeGotOne
{
    public class InventoryItemDB : SingletonBase<InventoryItemDB>
    {
        private const int DEFAULT_ITEM_KEY = 0;
        
        public ItemScriptableObject[] items;

        private Dictionary<int, ItemScriptableObject> itemDic = new();
        public Dictionary<int, int> ItemCountDic = new();
        
        // 제공해드린 싱글톤을 상속받은 개체의 초기화는
        // OnInitialize(), Start()를 중심으로 하되
        // Awake 꼭 필요한 상황에서는 override를 하시고
        // base.Awake()를 호출해 주세요

        protected override void OnInitialize()
        {
            base.OnInitialize();

            // 초기화 시점에 items배열을 순회하면서 딕셔너리에 등록한다
            for(int i = 0; i < items.Length; ++i)
            {
                ItemScriptableObject item = items[i];
                
                if (itemDic.ContainsKey(item.Key))
                {
                    Debug.LogWarning($"[InventoryItemDB] 중복 Key = {item.Key}");
                    continue; 
                }

                itemDic.Add(item.Key, item);
            }
        }

        // 견본(ScriptableObject 참조변수)를 반환하는 함수입니다.
        // - 지금은 단순 참조로 충분하므로 불필요한 인스턴스 생성을 피합니다.
        public ItemScriptableObject GetItem(int id)
        {
            if(itemDic.ContainsKey(id) == false)
            {
                Debug.LogWarning($"[InventoryItemDB] 없는 Key = {id}");
                return itemDic[DEFAULT_ITEM_KEY];
            }
           
            return itemDic[id];
        }

    }

}

