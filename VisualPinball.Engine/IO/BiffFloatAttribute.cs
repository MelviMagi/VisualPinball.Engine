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

using System.IO;
using VisualPinball.Engine.Math;
using VisualPinball.Engine.VPT.Table;

namespace VisualPinball.Engine.IO
{
	public class BiffFloatAttribute : BiffAttribute
	{
		public int QuantizedUnsignedBits = -1;
		public bool AsInt = false;

		public BiffFloatAttribute(string name) : base(name) { }

		public override void Parse<T>(T obj, BinaryReader reader, int len)
		{
			if (!AsInt) {
				ParseValue(obj, reader, len, ReadFloat);
			} else {
				ParseValue(obj, reader, len, ReadInt);
			}
		}

		public override void Write<TItem>(TItem obj, BinaryWriter writer, HashWriter hashWriter)
		{
			if (!AsInt) {
				WriteValue<TItem, float>(obj, writer, WriteFloat, hashWriter);
			} else {
				WriteValue<TItem, int>(obj, writer, WriteFloat, hashWriter);
			}
		}

		private float ReadFloat(BinaryReader reader, int len)
		{
			var f = QuantizedUnsignedBits > 0
				? DequantizeUnsigned(QuantizedUnsignedBits, reader.ReadInt32())
				: reader.ReadSingle();

			return f;
		}

		private int ReadInt(BinaryReader reader, int len)
		{
			return (int) ReadFloat(reader, len);
		}

		private void WriteFloat(BinaryWriter writer, float value)
		{
			if (QuantizedUnsignedBits > 0) {
				writer.Write(QuantizeUnsigned(QuantizedUnsignedBits, value));
			} else {
				writer.Write(value);
			}
		}

		private static void WriteFloat(BinaryWriter writer, int value)
		{
			writer.Write((float)value);
		}

		public static float DequantizeUnsigned(int bits, int i)
		{
			var n = (1 << bits) - 1;
			return MathF.Min(i / (float) n, 1.0f);
		}

		public static uint QuantizeUnsigned(int bits, float x)
		{
			var n = (1 << bits) - 1;
			var np1 = 1 << bits;
			return System.Math.Min((uint)(x * np1), (uint)n);
		}
	}
}
