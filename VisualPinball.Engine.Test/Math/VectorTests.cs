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

using FluentAssertions;
using NUnit.Framework;
using VisualPinball.Engine.Math;

namespace VisualPinball.Engine.Test.Common
{
	public class VectorTests
	{
		[Test]
		public void ShouldCorrectlyOperateVectors()
		{
			(new Vertex3D(2f, 3f, 4f) + new Vertex3D(10f, 20f, 50f)).Should().BeEquivalentTo(new Vertex3D(12f, 23f, 54f));
			(new Vertex3D(5f, 1f, 4f) - new Vertex3D(2f, -5f, 1.5f)).Should().BeEquivalentTo(new Vertex3D(3f, 6f, 2.5f));
			(new Vertex3D(2f, 3f, 4f) * 4f).Should().BeEquivalentTo(new Vertex3D(8f, 12f, 16f));
			(4f * new Vertex3D(2f, 3f, 4f)).Should().BeEquivalentTo(new Vertex3D(8f, 12f, 16f));
			(new Vertex3D(2f, 3f, 4f) / 2f).Should().BeEquivalentTo(new Vertex3D(1f, 1.5f, 2f));
		}

		[Test]
		public void ShouldCorrectlySetVectors()
		{
			new Vertex3D(2f, 3f, 4f)
				.Set(1f, 2f, 3f)
				.Should().BeEquivalentTo(new Vertex3D(1f, 2f, 3f));

			new Vertex3D(2f, 3f, 4f)
				.Set(new Vertex3D(1f, 2f, 3f))
				.Should().BeEquivalentTo(new Vertex3D(1f, 2f, 3f));
		}

		[Test]
		public void ShouldCorrectlyCrossVectors()
		{
			Vertex3D.CrossVectors(new Vertex3D(1.5f, 2.5f, 4f), new Vertex3D(3.5f, 100f, 95f))
				.Should().BeEquivalentTo(new Vertex3D(-162.5f, -128.5f, 141.25f));
		}

		[Test]
		public void ShouldCorrectlyLerpDragPoints()
		{
			var dp0 = new DragPointData(2f, 3f);
			var dp1 = new DragPointData(8f, 12f);

			dp0.Lerp(dp1, 0).Center.Should().Be(new Vertex3D(2f, 3f, 0f));
			dp0.Lerp(dp1, 0.2f).Center.Should().Be(new Vertex3D(3.2f, 4.8f, 0f));
			dp0.Lerp(dp1, 1).Center.Should().Be(new Vertex3D(8f, 12f, 0f));
		}
	}
}
