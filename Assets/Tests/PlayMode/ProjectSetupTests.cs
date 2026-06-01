using NUnit.Framework;
using UnityEngine;

namespace RTS_FPS_V2.Tests.PlayMode
{
    public class ProjectSetupTests
    {
        [Test]
        public void PlayModeTestRunner_IsConfigured()
        {
            Assert.That(Application.isPlaying, Is.True);
        }
    }
}
