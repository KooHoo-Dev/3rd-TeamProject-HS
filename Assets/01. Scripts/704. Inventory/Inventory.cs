using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeGotOne
{
    public class Inventory : MonoBehaviour
    {
        // Inventory의 역할?
        // - Slot이 어디에 속한 요소인지 판별
        // - Slot을 묶어서 관리해주는 용도

        [Header("Inventory Settings")]
        [field: SerializeField] public int SizeRow { get; private set; } = 8;
        [field: SerializeField] public int SizeColumn { get; private set; } = 5;

        public InventorySlot[,] SlotGrid { get; private set; }

        private readonly Dictionary<int, InventorySlot> itemSlots = new();
        private InventoryItemDB inventoryItemDB;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => InventoryItemDB.Instance.IsInitialized);

            inventoryItemDB = InventoryItemDB.Instance;
            inventoryItemDB.ItemCountChanged += RenderItem;
            InitSlotGrid();
            InitItemSlots();
            RenderAllItems();
        }

        // private void OnDisable()
        // {
        //     if (inventoryItemDB != null)
        //     {
        //         inventoryItemDB.ItemCountChanged -= RenderItem;
        //     }
        // }

        private void InitSlotGrid()
        {
            SlotGrid = new InventorySlot[SizeRow, SizeColumn];

            InventorySlot[] slots = GetComponentsInChildren<InventorySlot>();
            int slotCount = Mathf.Min(slots.Length, SizeRow * SizeColumn);

            if (slots.Length > slotCount)
            {
                Debug.LogWarning("[Inventory] SlotGrid 크기보다 InventorySlot이 많습니다.");
            }

            for (int i = 0; i < slotCount; ++i)
            {
                int rowIndex = i / SizeColumn;
                int columnIndex = i % SizeColumn;
                SlotGrid[rowIndex, columnIndex] = slots[i];
            }
        }

        private void InitItemSlots()
        {
            itemSlots.Clear();

            List<ItemRef> validItems = new();
            foreach (ItemRef item in ItemManager.Instance.Items)
            {
                validItems.Add(item);
            }

            validItems.Sort((left, right) => left.ItemId.CompareTo(right.ItemId));

            int itemIndex = 0;
            for (int row = 0; row < SizeRow; ++row)
            {
                for (int column = 0; column < SizeColumn; ++column)
                {
                    InventorySlot slot = SlotGrid[row, column];
                    if (slot == null)
                    {
                        continue;
                    }

                    slot.Render(null, 0);

                    if (itemIndex >= validItems.Count)
                    {
                        continue;
                    }

                    ItemRef item = validItems[itemIndex++];
                    itemSlots.Add(item.ItemId, slot);
                }
            }

            if (itemIndex < validItems.Count)
            {
                Debug.LogWarning("[Inventory] 인벤토리 아이템 수가 사용 가능한 슬롯 수보다 많습니다.");
                for (int i = 0; i < validItems.Count; i++)
                {
                    ItemRef item = validItems[i];
                    Debug.LogWarning($"[Inventory] 사용하지 않는 아이템 = {item.ItemId}");
                }
            }
        }
        
        /// <summary>
        /// 매개변수의 slot이 inventory 안에 존재하는지 검사
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public bool IsIn(InventorySlot slot)
        {
            if (slot == null) return false;

            // 선형 탐색으로 구현합시다.
            for(int x = 0; x < SlotGrid.GetLength(0); ++x)
            {
                for (int y = 0; y < SlotGrid.GetLength(1); ++y)
                {
                    if (SlotGrid[x, y] == slot) return true;
                }
            }

            // Foreach로도 쌉가능
            //foreach(InventorySlot s in SlotGrid)
            //{
            //    if (s == slot) return true;
            //}

            return false;
        }

        private void RenderAllItems()
        {
            foreach (KeyValuePair<int, InventorySlot> pair in itemSlots)
            {
                RenderItem(pair.Key, inventoryItemDB.GetCount(pair.Key));
            }
        }

        private void RenderItem(int itemId, int count)
        {
            if (itemSlots.TryGetValue(itemId, out InventorySlot slot) == false)
            {
                return;
            }

            slot.Render(inventoryItemDB.GetItem(itemId), count);
        }
    }

}

