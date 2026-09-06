using System;
using UnityEngine;
using HelloServer;

namespace WeGotOne
{
    public class EasyRoomClient: MonoBehaviour
    {
        private RoomClient roomClient;
        [SerializeField] string code = "11111";
        [SerializeField] string who = "HiHi";

        private void Awake()
        {
            roomClient = FindAnyObjectByType<RoomClient>();
        }

        private void Start()
        {
            roomClient.Join(code, who, Color.white);
        }
    }
}