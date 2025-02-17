// Visual Pinball Engine
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

using System;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.Math;
using VisualPinball.Engine.Resources.Meshes;

namespace VisualPinball.Engine.VPT.Bumper
{
	public class BumperMeshGenerator
	{
		public const string Base = "Base";
		public const string Cap = "Cap";
		public const string Ring = "Ring";
		public const string Skirt = "Skirt";

		private static readonly Mesh BaseMesh = new Mesh("Base", BumperBase.Vertices, BumperBase.Indices);
		private static readonly Mesh CapMesh = new Mesh("Cap", BumperCap.Vertices, BumperCap.Indices);
		private static readonly Mesh RingMesh = new Mesh("Ring", BumperRing.Vertices, BumperRing.Indices);
		private static readonly Mesh SocketMesh = new Mesh("Skirt", BumperSocket.Vertices, BumperSocket.Indices);

		private readonly BumperData _data;

		internal BumperMeshGenerator(BumperData data) {
			_data = data;
		}


		public PbrMaterial GetMaterial(string id, Table.Table table)
		{
			switch (id) {
				case Base:
					return new PbrMaterial(table.GetMaterial(_data.BaseMaterial), Texture.BumperBase);
				case Cap:
					return new PbrMaterial(table.GetMaterial(_data.CapMaterial), Texture.BumperCap);
				case Ring:
					return new PbrMaterial(table.GetMaterial(_data.RingMaterial), Texture.BumperRing);
				case Skirt:
					return new PbrMaterial(table.GetMaterial(_data.SocketMaterial), Texture.BumperSocket);
			}
			throw new ArgumentException("Unknown bumper id \"" + id + "\".");
		}

		public Mesh GetMesh(string id, Table.Table table, Origin origin, bool asRightHanded)
		{
			var mesh = GetMesh(id, table, origin);
			return asRightHanded ? mesh.Transform(Matrix3D.RightHanded) : mesh;
		}

		public Mesh GetMesh(string id, Table.Table table, Origin origin) {

			var height = table.GetSurfaceHeight(_data.Surface, _data.Center.X, _data.Center.Y);
			switch (id) {
				case Base: {
					var mesh = BaseMesh.Clone().MakeScale(_data.Radius, _data.Radius, _data.HeightScale);
					return TranslateMesh(mesh, z => z + height, origin);
				}
				case Cap: {
					var mesh = CapMesh.Clone().MakeScale(_data.Radius * 2, _data.Radius * 2, _data.HeightScale);
					return TranslateMesh(mesh, z => z + _data.HeightScale + height, origin);
				}
				case Ring: {
					var mesh = RingMesh.Clone().MakeScale(_data.Radius, _data.Radius, _data.HeightScale);
					return TranslateMesh(mesh, z => z + height, origin);
				}
				case Skirt: {
					var mesh = SocketMesh.Clone().MakeScale(_data.Radius, _data.Radius, _data.HeightScale);
					return TranslateMesh(mesh, z => z + (height + 5.0f), origin);
				}
			}
			throw new ArgumentException("Unknown bumper mesh \"" + id + "\".");
		}

		private Mesh TranslateMesh(Mesh mesh, Func<float, float> zPos, Origin origin) {
			var generatedMesh = mesh.Clone();
			for (var i = 0; i < generatedMesh.Vertices.Length; i++) {
				if (origin == Origin.Global) {
					generatedMesh.Vertices[i].X += _data.Center.X;
					generatedMesh.Vertices[i].Y += _data.Center.Y;
				}

				generatedMesh.Vertices[i].Z = zPos(generatedMesh.Vertices[i].Z);
			}

			return generatedMesh;
		}
	}
}
