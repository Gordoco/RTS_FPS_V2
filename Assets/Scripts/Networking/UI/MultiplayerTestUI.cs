using UnityEngine;
using UnityEngine.UI;

namespace RTS_FPS_V2.Networking.Ngo
{
    public sealed class MultiplayerTestUI : MonoBehaviour
    {
        [SerializeField] NetworkSessionManager session;
        [SerializeField] NetworkEntitySpawner spawner;
        [SerializeField] NetworkEntityRegistry registry;
        [SerializeField] InputField addressInput;
        [SerializeField] Text statusText;
        [SerializeField] Button hostButton;
        [SerializeField] Button joinButton;
        [SerializeField] Button shutdownButton;
        [SerializeField] Button spawnCharacterButton;
        [SerializeField] Button spawnUnitButton;

        void Awake()
        {
            if (session == null)
            {
                session = FindFirstObjectByType<NetworkSessionManager>();
            }

            if (spawner == null)
            {
                spawner = FindFirstObjectByType<NetworkEntitySpawner>();
            }

            if (hostButton != null)
            {
                hostButton.onClick.AddListener(OnHostClicked);
            }

            if (joinButton != null)
            {
                joinButton.onClick.AddListener(OnJoinClicked);
            }

            if (shutdownButton != null)
            {
                shutdownButton.onClick.AddListener(OnShutdownClicked);
            }

            if (spawnCharacterButton != null)
            {
                spawnCharacterButton.onClick.AddListener(() => SpawnTestEntity("TestCharacter"));
            }

            if (spawnUnitButton != null)
            {
                spawnUnitButton.onClick.AddListener(() => SpawnTestEntity("TestUnit"));
            }
        }

        void OnEnable()
        {
            if (session == null)
            {
                return;
            }

            session.SessionStarted += UpdateStatus;
            session.SessionShutdown += UpdateStatus;
            session.PlayerJoined += _ => UpdateStatus();
            session.PlayerLeft += _ => UpdateStatus();
            session.ConnectionFailed += args => SetStatus($"Error: {args.Message}");
        }

        void OnDisable()
        {
            if (session == null)
            {
                return;
            }

            session.SessionStarted -= UpdateStatus;
            session.SessionShutdown -= UpdateStatus;
        }

        void Start() => UpdateStatus();

        void OnHostClicked()
        {
            if (session != null)
            {
                session.StartHost();
            }

            UpdateStatus();
        }

        void OnJoinClicked()
        {
            if (session == null)
            {
                return;
            }

            var address = addressInput != null && !string.IsNullOrWhiteSpace(addressInput.text)
                ? addressInput.text
                : "127.0.0.1";
            session.StartClient(ConnectionEndpoint.DirectIp(address, session.Config.Port));
            UpdateStatus();
        }

        void OnShutdownClicked()
        {
            session?.Shutdown();
            UpdateStatus();
        }

        void SpawnTestEntity(string key)
        {
            if (spawner == null || session == null || !session.IsServer)
            {
                SetStatus("Only the server can spawn test entities.");
                return;
            }

            var spawnPosition = Random.insideUnitSphere * 3f;
            spawnPosition.y = 1f;
            spawner.Spawn(key, spawnPosition, Quaternion.identity, session.LocalClientId);
            UpdateStatus();
        }

        void UpdateStatus()
        {
            if (session == null)
            {
                SetStatus("No session manager.");
                return;
            }

            if (!session.IsSessionActive)
            {
                SetStatus("Offline");
                return;
            }

            var role = session.IsHost ? "Host" : session.IsServer ? "Server" : "Client";
            var playModeTag = MultiplayerPlayModeUtility.IsAvailable && !MultiplayerPlayModeUtility.IsMainEditorPlayer
                ? " | Virtual Player"
                : string.Empty;
            SetStatus($"{role}{playModeTag} | ClientId: {session.LocalClientId}");
        }

        void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
