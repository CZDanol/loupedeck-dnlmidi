using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	class ChannelProperty
	{

		public enum BoolType
		{
			Mute,
			Solo,
			Arm,
			Count
		}

		public static BitmapColor selectionColor = new BitmapColor(52, 155, 235);

		public static BitmapColor[] boolPropertyColor =
		{
			 new BitmapColor(200, 0, 0), // Mute
			 new BitmapColor(122, 88, 23), // Solo
			 new BitmapColor(103, 52, 235), // Arm
		};

		public static int[] boolPropertyMackieNote = { 16, 8, 0 };

		public static string[] boolPropertyName = { "Mute", "Solo", "Rec" };
		public static string[] boolPropertyLetter = { "M", "S", "R" };

	}
}
