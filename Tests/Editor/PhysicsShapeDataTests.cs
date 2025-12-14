// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using OMI.Extensions.PhysicsShape;
using UnityEngine;

namespace OMI.Tests.Editor
{
    /// <summary>
    /// Tests for OMI_physics_shape data parsing and conversion.
    /// </summary>
    [TestFixture]
    public class PhysicsShapeDataTests
    {
        [Test]
        public void BoxShape_ParsesCorrectly()
        {
            var data = new OMIPhysicsShapeData
            {
                Type = "box",
                Box = new OMIPhysicsShapeBoxData
                {
                    Size = new float[] { 2f, 3f, 4f }
                }
            };

            Assert.AreEqual(OMIPhysicsShapeType.Box, data.GetShapeType());
            Assert.IsNotNull(data.Box);
            Assert.AreEqual(2f, data.Box.Size[0]);
            Assert.AreEqual(3f, data.Box.Size[1]);
            Assert.AreEqual(4f, data.Box.Size[2]);
        }

        [Test]
        public void SphereShape_ParsesCorrectly()
        {
            var data = new OMIPhysicsShapeData
            {
                Type = "sphere",
                Sphere = new OMIPhysicsShapeSphereData
                {
                    Radius = 2.5f
                }
            };

            Assert.AreEqual(OMIPhysicsShapeType.Sphere, data.GetShapeType());
            Assert.IsNotNull(data.Sphere);
            Assert.AreEqual(2.5f, data.Sphere.Radius);
        }

        [Test]
        public void CapsuleShape_ParsesCorrectly()
        {
            var data = new OMIPhysicsShapeData
            {
                Type = "capsule",
                Capsule = new OMIPhysicsShapeCapsuleData
                {
                    Radius = 0.5f,
                    Height = 2f
                }
            };

            Assert.AreEqual(OMIPhysicsShapeType.Capsule, data.GetShapeType());
            Assert.IsNotNull(data.Capsule);
            Assert.AreEqual(0.5f, data.Capsule.Radius);
            Assert.AreEqual(2f, data.Capsule.Height);
        }

        [Test]
        public void CylinderShape_ParsesCorrectly()
        {
            var data = new OMIPhysicsShapeData
            {
                Type = "cylinder",
                Cylinder = new OMIPhysicsShapeCylinderData
                {
                    Radius = 1f,
                    Height = 3f
                }
            };

            Assert.AreEqual(OMIPhysicsShapeType.Cylinder, data.GetShapeType());
            Assert.IsNotNull(data.Cylinder);
            Assert.AreEqual(1f, data.Cylinder.Radius);
            Assert.AreEqual(3f, data.Cylinder.Height);
        }

        [Test]
        public void ConvexShape_ParsesCorrectly()
        {
            var data = new OMIPhysicsShapeData
            {
                Type = "convex",
                Convex = new OMIPhysicsShapeConvexData
                {
                    Mesh = 0
                }
            };

            Assert.AreEqual(OMIPhysicsShapeType.Convex, data.GetShapeType());
            Assert.IsNotNull(data.Convex);
            Assert.AreEqual(0, data.Convex.Mesh);
        }

        [Test]
        public void TrimeshShape_ParsesCorrectly()
        {
            var data = new OMIPhysicsShapeData
            {
                Type = "trimesh",
                Trimesh = new OMIPhysicsShapeTrimeshData
                {
                    Mesh = 1
                }
            };

            Assert.AreEqual(OMIPhysicsShapeType.Trimesh, data.GetShapeType());
            Assert.IsNotNull(data.Trimesh);
            Assert.AreEqual(1, data.Trimesh.Mesh);
        }

        [Test]
        public void BoxShape_DefaultSize_IsUnitCube()
        {
            var boxData = new OMIPhysicsShapeBoxData();
            var size = boxData.GetSize();
            
            Assert.AreEqual(1f, size.x);
            Assert.AreEqual(1f, size.y);
            Assert.AreEqual(1f, size.z);
        }

        [Test]
        public void SphereShape_DefaultRadius_IsOne()
        {
            var sphereData = new OMIPhysicsShapeSphereData();
            
            Assert.AreEqual(1f, sphereData.GetRadius());
        }

        [Test]
        public void CapsuleShape_DefaultValues_AreCorrect()
        {
            var capsuleData = new OMIPhysicsShapeCapsuleData();
            
            Assert.AreEqual(0.5f, capsuleData.GetRadius());
            Assert.AreEqual(2f, capsuleData.GetHeight());
        }

        [Test]
        public void UnknownType_ReturnsBox()
        {
            var data = new OMIPhysicsShapeData
            {
                Type = "unknown_shape"
            };

            Assert.AreEqual(OMIPhysicsShapeType.Box, data.GetShapeType());
        }

        [Test]
        public void NullType_ReturnsBox()
        {
            var data = new OMIPhysicsShapeData
            {
                Type = null
            };

            Assert.AreEqual(OMIPhysicsShapeType.Box, data.GetShapeType());
        }
    }
}
