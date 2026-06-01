using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    [Serializable]
    public struct NetworkEntityRegistryEntry
    {
        public string Key;
        public GameObject Prefab;
    }

    [CreateAssetMenu(fileName = "NetworkEntityRegistry", menuName = "RTS FPS V2/Networking/Entity Registry")]
    public sealed class NetworkEntityRegistry : ScriptableObject
    {
        [SerializeField] List<NetworkEntityRegistryEntry> entries = new List<NetworkEntityRegistryEntry>();

        readonly Dictionary<string, GameObject> lookup = new Dictionary<string, GameObject>();

        public IReadOnlyList<NetworkEntityRegistryEntry> Entries => entries;

        public void ReplaceEntries(IEnumerable<NetworkEntityRegistryEntry> newEntries)
        {
            entries.Clear();
            entries.AddRange(newEntries);
            RebuildLookup();
        }

        public bool TryGetPrefab(string key, out GameObject prefab)
        {
            if (lookup.Count == 0)
            {
                RebuildLookup();
            }

            return lookup.TryGetValue(key, out prefab);
        }

        void RebuildLookup()
        {
            lookup.Clear();
            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Prefab == null)
                {
                    continue;
                }

                lookup[entry.Key] = entry.Prefab;
            }
        }

        void OnValidate() => RebuildLookup();
    }
}
