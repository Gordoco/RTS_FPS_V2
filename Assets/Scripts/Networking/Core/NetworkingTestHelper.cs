using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    public static class NetworkingTestHelper
    {
        public static NetworkBootstrap CreateBootstrap(SessionConfig config, NetworkEntityRegistry registry)
        {
            var existing = Object.FindFirstObjectByType<NetworkBootstrap>();
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var go = new GameObject("NetworkBootstrap");
            var networkManager = go.AddComponent<NetworkManager>();
            var transport = go.AddComponent<UnityTransport>();
            networkManager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                EnableSceneManagement = false,
                ConnectionApproval = true
            };

            if (registry != null)
            {
                foreach (var entry in registry.Entries)
                {
                    if (entry.Prefab != null)
                    {
                        networkManager.AddNetworkPrefab(entry.Prefab);
                    }
                }
            }

            go.AddComponent<NgoUnityTransportAdapter>();
            var session = go.AddComponent<NetworkSessionManager>();
            go.AddComponent<NetworkEntitySpawner>();
            go.AddComponent<NetworkRoleManager>();
            go.AddComponent<InterestManager>();
            var bootstrap = go.AddComponent<NetworkBootstrap>();

            SetPrivateField(session, "config", config);
            SetPrivateField(session, "transport", go.GetComponent<NgoUnityTransportAdapter>());
            SetPrivateField(session, "networkManager", networkManager);

            var spawner = go.GetComponent<NetworkEntitySpawner>();
            SetPrivateField(spawner, "registry", registry);

            return bootstrap;
        }

        public static GameObject CreateTestEntityPrefab(string name, EntityCategory category, bool ownerWritable)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.AddComponent<NetworkObject>();
            go.AddComponent<NetworkEntityHandle>();
            var metadata = go.AddComponent<NetworkEntityMetadata>();
            SetPrivateField(metadata, "category", category);
            go.AddComponent<Unity.Netcode.Components.NetworkTransform>();
            go.AddComponent<NetworkTransformSync>();
            go.AddComponent<NetworkStateSync>();
            go.AddComponent<NetworkCommandChannel>();
            return go;
        }

        public static SessionConfig CreateDefaultConfig()
        {
            return ScriptableObject.CreateInstance<SessionConfig>();
        }

        public static NetworkEntityRegistry CreateRegistry(Dictionary<string, GameObject> prefabs)
        {
            var registry = ScriptableObject.CreateInstance<NetworkEntityRegistry>();
            var entries = new List<NetworkEntityRegistryEntry>();
            foreach (var pair in prefabs)
            {
                entries.Add(new NetworkEntityRegistryEntry { Key = pair.Key, Prefab = pair.Value });
            }

            registry.ReplaceEntries(entries);
            return registry;
        }

        public static IEnumerator WaitForCondition(System.Func<bool> condition, float timeoutSeconds = 5f)
        {
            var elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(target, value);
        }
    }
}
