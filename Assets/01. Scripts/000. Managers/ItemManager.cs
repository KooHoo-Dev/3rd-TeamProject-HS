using System;
using System.Collections.Generic;
using Jay.FileIO;

namespace WeGotOne
{
    public sealed class ItemManager : SingletonBase<ItemManager>
    {
        private readonly Dictionary<int, ItemRef> itemRefsById = new();
        private bool isLoading;

        public new bool IsInitialized { get; private set; }
        public IEnumerable<ItemRef> Items => itemRefsById.Values;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (isLoading)
            {
                return;
            }

            isLoading = true;
            LoadItemsAsync();
        }

        public ItemRef Get(int itemId)
        {
            if (itemRefsById.TryGetValue(itemId, out ItemRef itemRef))
            {
                return itemRef;
            }

            throw new KeyNotFoundException($"존재하지 않는 ItemId입니다: {itemId}");
        }

        private async void LoadItemsAsync()
        {
            List<ItemDto> itemDtos = await TSVReader.ReadTableAsync<ItemDto>("ItemData", true);
            if (itemDtos == null)
            {
                throw new InvalidOperationException("ItemData.tsv를 읽을 수 없습니다.");
            }

            foreach (ItemDto itemDto in itemDtos)
            {
                if (itemDto == null)
                {
                    throw new InvalidOperationException("ItemData.tsv에 null ItemDto가 있습니다.");
                }

                if (itemRefsById.ContainsKey(itemDto.ItemId))
                {
                    throw new InvalidOperationException($"중복된 ItemId가 있습니다: {itemDto.ItemId}");
                }

                itemRefsById.Add(itemDto.ItemId, new ItemRef(itemDto));
            }

            IsInitialized = true;
        }
    }
}
