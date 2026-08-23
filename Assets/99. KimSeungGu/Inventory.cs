using System;
using Jay;
using UnityEngine;

namespace Study_Inventory
{
    public class Inventory : SingletonBase<Inventory>
    {
        // Inventory의 역할?
        // - Slot이 어디에 속한 요소인지 판별
        // - Slot을 묶어서 관리해주는 용도

        [Header("Inventory Settings")]
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int SizeRow { get; private set; } = 8;
        [field: SerializeField] public int SizeColumn { get; private set; } = 5;

        public InventorySlot[,] SlotGrid { get; private set; }

        private void Awake()
        {
            InitSlotGrid();

            // Inner Method를 사용해서 초기화 로직을 만들어 봅시다.
            void InitSlotGrid()
            {
                // 2차원 배열을 인벤토리 크기만큼 선언해 줍니다.
                SlotGrid = new InventorySlot[SizeRow, SizeColumn];

                // 자식 오브젝트들에게서 존재하는 모든 slot을 가져옵니다
                InventorySlot[] slots = GetComponentsInChildren<InventorySlot>();

                // 2차원 배열을 초기화 해줍니다
                for(int i = 0; i < slots.Length; ++i)
                {
                    // 행과 열의 Index를 계산해서
                    int rowIndex = i / SizeColumn;
                    int columnIndex = i % SizeColumn;

                    // 대입해줍니다
                    SlotGrid[rowIndex, columnIndex] = slots[i];
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
    }

}

