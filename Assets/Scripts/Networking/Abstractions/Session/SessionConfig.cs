using System;
using UnityEngine;

namespace RTS_FPS_V2.Networking
{
    [CreateAssetMenu(fileName = "SessionConfig", menuName = "RTS FPS V2/Networking/Session Config")]
    public class SessionConfig : ScriptableObject
    {
        [SerializeField] ushort port = 7777;
        [SerializeField] int maxPlayers = 8;
        [SerializeField] float serverTickRate = 30f;
        [SerializeField] float interestTickInterval = 0.5f;
        [SerializeField] float fullFidelityDistance = 50f;
        [SerializeField] float dormantDistance = 150f;

        public ushort Port => port;
        public int MaxPlayers => maxPlayers;
        public float ServerTickRate => serverTickRate;
        public float InterestTickInterval => interestTickInterval;
        public float FullFidelityDistance => fullFidelityDistance;
        public float DormantDistance => dormantDistance;
    }
}
