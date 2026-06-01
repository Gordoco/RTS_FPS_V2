using System.Collections;
using UnityEngine;

namespace RTS_FPS_V2.Networking.Ngo
{
    /// <summary>
    /// Auto-starts host on the main editor player and joins as client on Multiplayer Play Mode virtual players.
    /// Requires com.unity.multiplayer.playmode and at least one virtual player enabled before entering Play Mode.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MultiplayerPlayModeAutoConnect : MonoBehaviour
    {
        [SerializeField] NetworkSessionManager session;
        [SerializeField] bool autoStartHostOnMainPlayer = true;
        [SerializeField] float clientConnectDelaySeconds = 0.75f;

        void Awake()
        {
            if (session == null)
            {
                session = FindFirstObjectByType<NetworkSessionManager>();
            }
        }

        void Start()
        {
#if UNITY_EDITOR
            if (session == null || session.IsSessionActive)
            {
                return;
            }

            if (MultiplayerPlayModeUtility.IsMainEditorPlayer)
            {
                if (autoStartHostOnMainPlayer)
                {
                    session.StartHost();
                }
            }
            else
            {
                StartCoroutine(ConnectVirtualPlayerAfterDelay());
            }
#endif
        }

#if UNITY_EDITOR
        IEnumerator ConnectVirtualPlayerAfterDelay()
        {
            yield return new WaitForSeconds(clientConnectDelaySeconds);

            if (session == null || session.IsSessionActive || session.Config == null)
            {
                yield break;
            }

            session.StartClient(ConnectionEndpoint.DirectIp("127.0.0.1", session.Config.Port));
        }
#endif
    }
}
