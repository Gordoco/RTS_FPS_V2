using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RTS_FPS_V2.Networking;
using UnityEngine;
using UnityEngine.TestTools;

namespace RTS_FPS_V2.Tests.EditMode.Networking
{
    public class SpatialGridTests
    {
        [Test]
        public void IsAdjacentCell_ReturnsTrue_ForNeighborCell()
        {
            var grid = new SpatialGrid(10f);
            var entityPosition = new Vector3(15f, 0f, 0f);
            var observerPosition = new Vector3(5f, 0f, 0f);

            Assert.That(grid.IsAdjacentCell(entityPosition, observerPosition), Is.True);
        }

        [Test]
        public void IsRelevant_ReturnsFalse_ForDistantCell()
        {
            var grid = new SpatialGrid(10f);
            var entityPosition = new Vector3(100f, 0f, 0f);
            var observerPosition = Vector3.zero;

            Assert.That(grid.IsRelevant(entityPosition, observerPosition, 1), Is.False);
        }
    }
}
