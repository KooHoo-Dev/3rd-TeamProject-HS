using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Study_Inventory
{
    //  InventoryInput은 InventoryCavas(최상위 캔버스)
    // 에서 동작하는 기능입니다. 하위에 있는 모든 Slot, Inventory를
    // 개체에 대한 검사를 수행할 수 있습니다.

    // Held
    // : 특정 개체를 손에 쥐고 끌고가는 듯한 표현
    // Held 기능의 구현
    // 1. 드래그가 시작되면 시작된 위치의 Slot의 아이템 참조를 Cursor에게 Set한다
    // 2. 드래그가 진행되는 동안 Cursor 객체의 위치좌표를 Update한다
    // 3. 드래그가 종료되면 상황에 따라 알맞은 처리를 한다
    //  3.1 종료된 위치에 Slot 자체가 없을 경우
    //  3.2 종료된 위치의 Slot이 Empty일 경우
    //  3.3 종료된 위치의 Slot에 아이템이 있을 경우

    public class InventoryInput : MonoBehaviour
    {
       
    }

}

