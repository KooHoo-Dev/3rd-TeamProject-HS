using UnityEngine;

namespace WeGotOne
{
    public sealed class FishSpawnerGroup : MonoBehaviour
    {
        private FishSpawner[] fishSpawners;

        private void Start()
        {
            fishSpawners = GetComponentsInChildren<FishSpawner>(true);
        }

        public void Spawn()
        {
            foreach (FishSpawner fishSpawner in fishSpawners)
            {
                fishSpawner.Spawn();
            }
        }
    }
}
