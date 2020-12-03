using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
	public class MemoryBankData : IEditableItemData
	{
		public static List<char> m_hexChars = new List<char>
		{
			'0',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			'A',
			'B',
			'C',
			'D',
			'E',
			'F'
		};

		public DynamicArray<byte> Data = new DynamicArray<byte>();

		public byte LastOutput
		{
			get;
			set;
		}

		public byte Read(int address)
		{
			if (address >= 0 && address < Data.Count)
			{
				return Data.Array[address];
			}
			return 0;
		}

		public void Write(int address, byte data)
		{
			if (address >= 0 && address < Data.Count)
			{
				Data.Array[address] = data;
			}
			else if (address >= 0 && address < 256 && data != 0)
			{
				Data.Count = MathUtils.Max(Data.Count, address + 1);
				Data.Array[address] = data;
			}
		}

		public IEditableItemData Copy()
		{
			return new MemoryBankData
			{
				Data = new DynamicArray<byte>(Data),
				LastOutput = LastOutput
			};
		}

		public void LoadString(string data)
		{
			string[] array = data.Split(new char[1]
			{
				';'
			}, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 1)
			{
				string text = array[0];
				text = text.TrimEnd('0');
				Data.Clear();
				for (int i = 0; i < MathUtils.Min(text.Length, 256); i++)
				{
					int num = m_hexChars.IndexOf(char.ToUpperInvariant(text[i]));
					if (num < 0)
					{
						num = 0;
					}
					Data.Add((byte)num);
				}
			}
			if (array.Length >= 2)
			{
				string text2 = array[1];
				int num2 = m_hexChars.IndexOf(char.ToUpperInvariant(text2[0]));
				if (num2 < 0)
				{
					num2 = 0;
				}
				LastOutput = (byte)num2;
			}
		}

		public string SaveString()
		{
			return SaveString(saveLastOutput: true);
		}

		public string SaveString(bool saveLastOutput)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			for (int i = 0; i < Data.Count; i++)
			{
				if (Data.Array[i] != 0)
				{
					num = i + 1;
				}
			}
			for (int j = 0; j < num; j++)
			{
				int index = MathUtils.Clamp(Data.Array[j], 0, 15);
				stringBuilder.Append(m_hexChars[index]);
			}
			if (saveLastOutput)
			{
				stringBuilder.Append(';');
				stringBuilder.Append(m_hexChars[MathUtils.Clamp(LastOutput, 0, 15)]);
			}
			return stringBuilder.ToString();
		}
	}
}
