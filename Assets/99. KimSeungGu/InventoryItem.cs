using UnityEngine;

namespace Study_Inventory
{
    //  프로젝트하실때는 그냥 Item으로 선언하셔도 됩니다
    // 네이밍 겹칠까봐 Inventory 접두사 붙히는거임

    // ScriptableObject란?
    //  Unity에서 제공하는 데이터 클래스 템플릿.
    // 기본적인 게임 데이터 뿐만 아니라 유니티에서 사용하는 Asset들의
    // 참조도 데이터 단위로 묶을수 있어서 편리합니다.

    //  게임 데이터를 화용하는데에 매우 유용하지만
    // 대량의 데이터를 게임에 추가하기에는 적절하지는 못합니다.
    // 왜냐하면 불편해서... 보통 CSV, TSV, JSON 등을 사용합니다

    //   ScriptableObject는 아래의 코드를 입력해서
    //  에디터에서 직접 데이터 객체를 생성해야 합니다.
    [CreateAssetMenu(fileName = "InventoryItem", 
        menuName = "Study SAO/InventoryItem", order = 1)]
    public class InventoryItem : ScriptableObject
    {
        public int      Key;            // 아이템을 찾을 Key
        public string   Name;           // 명칭
        public string   Description;  // 설명
        public Sprite   Icon;           // 아이템의 Sprite 아이콘
        public int      Fullness;          // 가격
    }
}