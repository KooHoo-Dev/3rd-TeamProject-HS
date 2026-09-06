#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jay.FileIO;

namespace WeGotOne
{
    // Shared until the next domain reload, including a failed load (no implicit retries).
    public static class ItemEditorCache
    {
        private static readonly Lazy<Task> loadTask = new(LoadAsync);
        private static Dictionary<int, ItemDto> itemsById;

        public static Task EnsureLoadedAsync() => loadTask.Value;

        public static ItemDto Get(int itemId)
        {
            if (itemsById.TryGetValue(itemId, out ItemDto itemDto))
            {
                return itemDto;
            }

            throw new KeyNotFoundException($"존재하지 않는 ItemId입니다: {itemId}");
        }

        private static async Task LoadAsync()
        {
            List<ItemDto> itemDtos = await TSVReader.ReadTableAsync<ItemDto>("ItemData", true);
            if (itemDtos == null)
            {
                throw new InvalidOperationException("ItemData.tsv를 읽을 수 없습니다.");
            }

            var loadedItems = new Dictionary<int, ItemDto>();
            foreach (ItemDto itemDto in itemDtos)
            {
                if (itemDto == null)
                {
                    throw new InvalidOperationException("ItemData.tsv에 null ItemDto가 있습니다.");
                }

                if (loadedItems.ContainsKey(itemDto.ItemId))
                {
                    throw new InvalidOperationException($"중복된 ItemId가 있습니다: {itemDto.ItemId}");
                }

                loadedItems.Add(itemDto.ItemId, itemDto);
            }

            itemsById = loadedItems;
        }
    }
}
#endif
