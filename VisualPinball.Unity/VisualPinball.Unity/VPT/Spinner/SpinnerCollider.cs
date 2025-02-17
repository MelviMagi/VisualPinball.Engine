﻿// Visual Pinball Engine
// Copyright (C) 2022 freezy and VPE Team
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using VisualPinball.Engine.Common;

namespace VisualPinball.Unity
{
	internal struct SpinnerCollider : ICollider
	{
		public int Id => _header.Id;

		private ColliderHeader _header;

		public readonly LineCollider LineSeg0;
		public readonly LineCollider LineSeg1;

		public ColliderBounds Bounds { get; private set; }

		public SpinnerCollider(SpinnerComponent component, float height, ColliderInfo info) : this()
		{
			_header.Init(info, ColliderType.Spinner);

			var halfLength = component.Length * 0.5f;

			var radAngle = math.radians(component.Rotation);
			var sn = math.sin(radAngle);
			var cs = math.cos(radAngle);

			var v1 = new float2(
				component.Position.x - cs * (halfLength + PhysicsConstants.PhysSkin), // through the edge of the
				component.Position.y - sn * (halfLength + PhysicsConstants.PhysSkin)  // spinner
			);
			var v2 = new float2(
				component.Position.x + cs * (halfLength + PhysicsConstants.PhysSkin), // oversize by the ball radius
				component.Position.y + sn * (halfLength + PhysicsConstants.PhysSkin)  // this will prevent clipping
			);

			LineSeg0 = new LineCollider(v1, v2, height, height + 2.0f * PhysicsConstants.PhysSkin, info);
			LineSeg1 = new LineCollider(v2, v1, height, height + 2.0f * PhysicsConstants.PhysSkin, info);

			Bounds = LineSeg0.Bounds;
		}

		public unsafe void Allocate(BlobBuilder builder, ref BlobBuilderArray<BlobPtr<Collider>> colliders, int colliderId)
		{
			_header.Id = colliderId;
			var bounds = Bounds;
			bounds.ColliderId = colliderId;
			Bounds = bounds;
			ref var ptr = ref UnsafeUtility.As<BlobPtr<Collider>, BlobPtr<SpinnerCollider>>(ref colliders[_header.Id]);
			ref var collider = ref builder.Allocate(ref ptr);
			UnsafeUtility.MemCpy(
				UnsafeUtility.AddressOf(ref collider),
				UnsafeUtility.AddressOf(ref this),
				sizeof(SpinnerCollider)
			);
		}

		#region Narrowphase

		public float HitTest(ref CollisionEventData collEvent, ref DynamicBuffer<BallInsideOfBufferElement> insideOfs, in BallData ball, float dTime)
		{
			// todo
			// if (!m_enabled) return -1.0f;

			var hitTime = LineCollider.HitTestBasic(ref collEvent, ref insideOfs, in LineSeg0, in ball, dTime, false, true, false); // any face, lateral, non-rigid
			if (hitTime >= 0.0f) {
				// signal the Collide() function that the hit is on the front or back side
				collEvent.HitFlag = true;
				return hitTime;
			}

			hitTime = LineCollider.HitTestBasic(ref collEvent, ref insideOfs, in LineSeg1, in ball, dTime, false, true, false); // any face, lateral, non-rigid
			if (hitTime >= 0.0f) {
				// signal the Collide() function that the hit is on the front or back side
				collEvent.HitFlag = false;
				return hitTime;
			}

			return -1.0f;
		}

		#endregion

		#region Collision

		public static void Collide(in BallData ball, ref CollisionEventData collEvent, ref SpinnerMovementData movementData, in SpinnerStaticData data)
		{
			var dot = math.dot(collEvent.HitNormal, ball.Velocity);

			// hit from back doesn't count
			if (dot < 0.0f) {
				return;
			}

			var h = data.Height * 0.5f;
			// linear speed = ball speed
			// angular speed = linear/radius (height of hit)

			// h is the height of the spinner axis;
			// Since the spinner has no mass in our equation, the spot
			// h -coll.m_radius will be moving a at linear rate of
			// 'speed'. We can calculate the angular speed from that.

			movementData.AngleSpeed = math.abs(dot); // use this until a better value comes along

			if (math.abs(h) > 1.0f) {
				// avoid divide by zero
				movementData.AngleSpeed /= h;
			}

			movementData.AngleSpeed *= data.Damping;

			// We encoded which side of the spinner the ball hit
			if (collEvent.HitFlag) {
				movementData.AngleSpeed = -movementData.AngleSpeed;
			}
		}

		#endregion
	}
}
