using System;
using UnityEngine;

namespace WeGotOne
{
    public class ItemRef
    {
        public int ItemId { get; }
        public string Name { get; }
        public Sprite Icon { get; }
        public int Fullness { get; }
        public int BuildValue { get; }
        public ItemType ItemType { get; }

        public ItemRef(ItemDto itemDto)
        {
            ItemId = itemDto.ItemId;
            Name = itemDto.Name;
            Fullness = itemDto.Fullness;
            BuildValue = itemDto.BuildValue;
            ItemType = itemDto.ItemType;

            if (string.IsNullOrWhiteSpace(itemDto.IconPath))
            {
                throw new InvalidOperationException($"ItemId {ItemId}의 IconPath가 비어 있습니다.");
            }

            Icon = Resources.Load<Sprite>(itemDto.IconPath);
            if (Icon == null)
            {
                throw new InvalidOperationException(
                    $"ItemId {ItemId}의 Icon을 찾을 수 없습니다: {itemDto.IconPath}");
            }
        }
    }
}
