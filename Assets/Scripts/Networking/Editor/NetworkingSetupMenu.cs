#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RTS_FPS_V2.Networking.Ngo.Editor
{
    public static class NetworkingSetupMenu
    {
        const string Root = "Assets/Networking";

        [MenuItem("RTS FPS V2/Setup Networking Assets")]
        public static void SetupAll()
        {
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(Root + "/Prefabs");
            Directory.CreateDirectory(Root + "/Config");

            var config = GetOrCreateAsset<SessionConfig>(Root + "/Config/DefaultSessionConfig.asset");
            var registry = GetOrCreateAsset<NetworkEntityRegistry>(Root + "/Config/DefaultEntityRegistry.asset");

            var character = CreateEntityPrefab("TestCharacter", EntityCategory.Character, Root + "/Prefabs/TestCharacter.prefab");
            var unit = CreateEntityPrefab("TestUnit", EntityCategory.Unit, Root + "/Prefabs/TestUnit.prefab");

            registry.ReplaceEntries(new[]
            {
                new NetworkEntityRegistryEntry { Key = "TestCharacter", Prefab = character },
                new NetworkEntityRegistryEntry { Key = "TestUnit", Prefab = unit }
            });
            EditorUtility.SetDirty(registry);

            var bootstrap = CreateBootstrapPrefab(config, registry, Root + "/Prefabs/NetworkBootstrap.prefab");
            CreateTestScene(bootstrap);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Networking assets created under Assets/Networking/");
        }

        static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static GameObject CreateEntityPrefab(string name, EntityCategory category, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.AddComponent<Unity.Netcode.NetworkObject>();
            go.AddComponent<NetworkEntityHandle>();
            var metadata = go.AddComponent<NetworkEntityMetadata>();
            var metadataSO = new SerializedObject(metadata);
            metadataSO.FindProperty("category").enumValueIndex = (int)category;
            metadataSO.ApplyModifiedPropertiesWithoutUndo();
            go.AddComponent<Unity.Netcode.Components.NetworkTransform>();
            go.AddComponent<NetworkTransformSync>();
            go.AddComponent<NetworkStateSync>();
            go.AddComponent<NetworkCommandChannel>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateBootstrapPrefab(SessionConfig config, NetworkEntityRegistry registry, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("NetworkBootstrap");
            var networkManager = go.AddComponent<Unity.Netcode.NetworkManager>();
            var unityTransport = go.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            networkManager.NetworkConfig.NetworkTransport = unityTransport;

            foreach (var entry in registry.Entries)
            {
                if (entry.Prefab != null)
                {
                    networkManager.AddNetworkPrefab(entry.Prefab);
                }
            }

            go.AddComponent<NgoUnityTransportAdapter>();
            var session = go.AddComponent<NetworkSessionManager>();
            var sessionSO = new SerializedObject(session);
            sessionSO.FindProperty("config").objectReferenceValue = config;
            sessionSO.ApplyModifiedPropertiesWithoutUndo();

            var spawner = go.AddComponent<NetworkEntitySpawner>();
            var spawnerSO = new SerializedObject(spawner);
            spawnerSO.FindProperty("registry").objectReferenceValue = registry;
            spawnerSO.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<NetworkRoleManager>();
            go.AddComponent<InterestManager>();
            go.AddComponent<NetworkBootstrap>();
            go.AddComponent<MultiplayerPlayModeAutoConnect>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static void CreateTestScene(GameObject bootstrapPrefab)
        {
            const string scenePath = "Assets/Scenes/MultiplayerTestScene.unity";
            if (File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Object.Instantiate(bootstrapPrefab);

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            CreateButton(canvasGo.transform, "Host", new Vector2(-200, 100), out var hostButton);
            CreateButton(canvasGo.transform, "Join", new Vector2(0, 100), out var joinButton);
            CreateButton(canvasGo.transform, "Shutdown", new Vector2(200, 100), out var shutdownButton);
            CreateButton(canvasGo.transform, "Spawn Character", new Vector2(-150, 0), out var spawnCharacterButton);
            CreateButton(canvasGo.transform, "Spawn Unit", new Vector2(150, 0), out var spawnUnitButton);

            var addressInput = CreateInputField(canvasGo.transform, "127.0.0.1", new Vector2(0, 170));
            var statusText = CreateStatusText(canvasGo.transform, new Vector2(0, -100));

            var ui = canvasGo.AddComponent<MultiplayerTestUI>();
            var uiSO = new SerializedObject(ui);
            uiSO.FindProperty("hostButton").objectReferenceValue = hostButton;
            uiSO.FindProperty("joinButton").objectReferenceValue = joinButton;
            uiSO.FindProperty("shutdownButton").objectReferenceValue = shutdownButton;
            uiSO.FindProperty("spawnCharacterButton").objectReferenceValue = spawnCharacterButton;
            uiSO.FindProperty("spawnUnitButton").objectReferenceValue = spawnUnitButton;
            uiSO.FindProperty("addressInput").objectReferenceValue = addressInput;
            uiSO.FindProperty("statusText").objectReferenceValue = statusText;
            uiSO.ApplyModifiedPropertiesWithoutUndo();

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();
            cameraGo.transform.position = new Vector3(0, 5, -10);
            cameraGo.transform.rotation = Quaternion.Euler(20, 0, 0);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

            EditorSceneManager.SaveScene(scene, scenePath);

            var scenes = EditorBuildSettings.scenes;
            var alreadyListed = false;
            foreach (var entry in scenes)
            {
                if (entry.path == scenePath)
                {
                    alreadyListed = true;
                    break;
                }
            }

            if (!alreadyListed)
            {
                var updated = new EditorBuildSettingsScene[scenes.Length + 1];
                scenes.CopyTo(updated, 0);
                updated[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = updated;
            }
        }

        static void CreateButton(Transform parent, string label, Vector2 anchoredPosition, out Button button)
        {
            var go = new GameObject(label + "Button");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 40);
            rect.anchoredPosition = anchoredPosition;
            go.AddComponent<Image>();
            button = go.AddComponent<Button>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        static InputField CreateInputField(Transform parent, string defaultValue, Vector2 anchoredPosition)
        {
            var go = new GameObject("AddressInput");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220, 30);
            rect.anchoredPosition = anchoredPosition;
            go.AddComponent<Image>();
            var input = go.AddComponent<InputField>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.color = Color.black;
            text.supportRichText = false;
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);
            input.textComponent = text;
            input.text = defaultValue;
            return input;
        }

        static Text CreateStatusText(Transform parent, Vector2 anchoredPosition)
        {
            var go = new GameObject("StatusText");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 40);
            rect.anchoredPosition = anchoredPosition;
            var text = go.AddComponent<Text>();
            text.text = "Offline";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            return text;
        }
    }
}
#endif
