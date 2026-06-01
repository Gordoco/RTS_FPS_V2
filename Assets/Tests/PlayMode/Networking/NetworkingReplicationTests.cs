using System.Collections;
using NUnit.Framework;
using RTS_FPS_V2.Networking;
using RTS_FPS_V2.Networking.Ngo;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace RTS_FPS_V2.Tests.PlayMode.Networking
{
    public class NetworkingReplicationTests
    {
        GameObject characterPrefab;
        NetworkBootstrap bootstrap;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            characterPrefab = NetworkingTestHelper.CreateTestEntityPrefab(
                "TestCharacter",
                EntityCategory.Character,
                ownerWritable: true);

            var registry = NetworkingTestHelper.CreateRegistry(
                new System.Collections.Generic.Dictionary<string, GameObject>
                {
                    { "TestCharacter", characterPrefab }
                });

            bootstrap = NetworkingTestHelper.CreateBootstrap(
                NetworkingTestHelper.CreateDefaultConfig(),
                registry);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }

            if (bootstrap != null)
            {
                Object.Destroy(bootstrap.gameObject);
            }

            if (characterPrefab != null)
            {
                Object.Destroy(characterPrefab);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator HostSpawn_CreatesReplicatedEntity()
        {
            var session = bootstrap.Session;
            Assert.That(session.StartHost(), Is.True);

            yield return NetworkingTestHelper.WaitForCondition(() => session.IsSessionActive);

            var spawner = bootstrap.EntitySpawner;
            var entity = spawner.Spawn("TestCharacter", new Vector3(1f, 1f, 1f), Quaternion.identity, session.LocalClientId);
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.IsSpawned, Is.True);
            Assert.That(Vector3.Distance(entity.GameObject.transform.position, new Vector3(1f, 1f, 1f)), Is.LessThan(0.5f));
        }

        [UnityTest]
        public IEnumerator PlayerLeft_CleansUpOwnedEntities()
        {
            var session = bootstrap.Session;
            Assert.That(session.StartHost(), Is.True);
            yield return NetworkingTestHelper.WaitForCondition(() => session.IsSessionActive);

            var spawner = bootstrap.EntitySpawner as NetworkEntitySpawner;
            var entity = spawner.Spawn("TestCharacter", Vector3.up, Quaternion.identity, session.LocalClientId);
            var entityId = entity.Id;

            spawner.CleanupEntitiesOwnedBy(session.LocalClientId);

            yield return null;

            Assert.That(spawner.TryGetEntity(entityId), Is.Null);
        }
    }
}
