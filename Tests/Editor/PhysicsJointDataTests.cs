// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Five Squared Interactive. All rights reserved.

using NUnit.Framework;
using OMI.Extensions.PhysicsJoint;
using UnityEngine;

namespace OMI.Tests.Editor
{
    /// <summary>
    /// Tests for OMI_physics_joint data parsing and conversion.
    /// </summary>
    [TestFixture]
    public class PhysicsJointDataTests
    {
        [Test]
        public void FixedJoint_ParsesCorrectly()
        {
            var data = new OMIPhysicsJointData
            {
                ConnectedNode = 1
            };

            // Fixed joint is the default when no constraints are specified
            Assert.AreEqual(1, data.ConnectedNode);
        }

        [Test]
        public void HingeJoint_ParsesCorrectly()
        {
            var data = new OMIPhysicsJointData
            {
                ConnectedNode = 2,
                Constraints = new OMIPhysicsJointConstraint[]
                {
                    new OMIPhysicsJointConstraint
                    {
                        AngularAxes = new int[] { 0 },
                        LowerLimit = -1.57f,
                        UpperLimit = 1.57f
                    }
                }
            };

            Assert.AreEqual(2, data.ConnectedNode);
            Assert.AreEqual(1, data.Constraints.Length);
            Assert.AreEqual(-1.57f, data.Constraints[0].GetLowerLimit());
            Assert.AreEqual(1.57f, data.Constraints[0].GetUpperLimit());
        }

        [Test]
        public void SliderJoint_ParsesCorrectly()
        {
            var data = new OMIPhysicsJointData
            {
                ConnectedNode = 3,
                Constraints = new OMIPhysicsJointConstraint[]
                {
                    new OMIPhysicsJointConstraint
                    {
                        LinearAxes = new int[] { 0 },
                        LowerLimit = 0f,
                        UpperLimit = 2f
                    }
                }
            };

            Assert.AreEqual(3, data.ConnectedNode);
            Assert.AreEqual(1, data.Constraints.Length);
            Assert.Contains(0, data.Constraints[0].LinearAxes);
        }

        [Test]
        public void BallSocketJoint_ParsesCorrectly()
        {
            var data = new OMIPhysicsJointData
            {
                ConnectedNode = 4,
                Constraints = new OMIPhysicsJointConstraint[]
                {
                    new OMIPhysicsJointConstraint
                    {
                        AngularAxes = new int[] { 0, 1, 2 }
                    }
                }
            };

            Assert.AreEqual(4, data.ConnectedNode);
            Assert.AreEqual(3, data.Constraints[0].AngularAxes.Length);
        }

        [Test]
        public void Constraint_DefaultLimits_AreCorrect()
        {
            var constraint = new OMIPhysicsJointConstraint();

            Assert.AreEqual(float.NegativeInfinity, constraint.GetLowerLimit());
            Assert.AreEqual(float.PositiveInfinity, constraint.GetUpperLimit());
        }

        [Test]
        public void Constraint_Stiffness_ParsesCorrectly()
        {
            var constraint = new OMIPhysicsJointConstraint
            {
                Stiffness = 1000f
            };

            Assert.AreEqual(1000f, constraint.GetStiffness());
        }

        [Test]
        public void Constraint_Damping_ParsesCorrectly()
        {
            var constraint = new OMIPhysicsJointConstraint
            {
                Damping = 50f
            };

            Assert.AreEqual(50f, constraint.GetDamping());
        }

        [Test]
        public void Constraint_DefaultStiffness_IsInfinity()
        {
            var constraint = new OMIPhysicsJointConstraint();

            Assert.AreEqual(float.PositiveInfinity, constraint.GetStiffness());
        }

        [Test]
        public void Constraint_DefaultDamping_IsOne()
        {
            var constraint = new OMIPhysicsJointConstraint();

            Assert.AreEqual(1f, constraint.GetDamping());
        }

        [Test]
        public void Constraint_HasLinearAxes_ReturnsTrue()
        {
            var constraint = new OMIPhysicsJointConstraint
            {
                LinearAxes = new int[] { 0, 1 }
            };

            Assert.IsTrue(constraint.HasLinearAxes);
            Assert.IsFalse(constraint.HasAngularAxes);
        }

        [Test]
        public void Constraint_HasAngularAxes_ReturnsTrue()
        {
            var constraint = new OMIPhysicsJointConstraint
            {
                AngularAxes = new int[] { 2 }
            };

            Assert.IsFalse(constraint.HasLinearAxes);
            Assert.IsTrue(constraint.HasAngularAxes);
        }
    }
}
