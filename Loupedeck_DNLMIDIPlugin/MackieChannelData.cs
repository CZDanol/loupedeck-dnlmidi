using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	public class MackieChannelData
	{

		public int ChannelID;
		public float Volume = 0;
		public string TrackName = "";

		public bool[] BoolProperty = new bool[(int)ChannelProperty.BoolType.Count];

		public bool IsMasterChannel = false;

		private Loupedeck_DNLMIDIPlugin plugin;

		public bool Muted {
			get => BoolProperty[(int)ChannelProperty.BoolType.Mute];
			set {
				BoolProperty[(int)ChannelProperty.BoolType.Mute] = value;
			}
		}

		public bool Armed {
			get => BoolProperty[(int)ChannelProperty.BoolType.Arm];
			set {
				BoolProperty[(int)ChannelProperty.BoolType.Arm] = value;
			}
		}

		public bool Solo {
			get => BoolProperty[(int)ChannelProperty.BoolType.Solo];
			set {
				BoolProperty[(int)ChannelProperty.BoolType.Solo] = value;
			}
		}

		public MackieChannelData(Loupedeck_DNLMIDIPlugin plugin, int channelID) {
			this.plugin = plugin;

			ChannelID = channelID;
			IsMasterChannel = channelID == Loupedeck_DNLMIDIPlugin.MackieChannelCount;

			Muted = false;
			Armed = false;
			Solo = false;

			if (IsMasterChannel)
				TrackName = "Master";
			else
				TrackName = $"Channel {channelID + 1}";
		}

		public void EmitVolumeUpdate() {
			var e = new PitchBendEvent();
			e.PitchValue = (ushort)(Volume * 16383);
			e.Channel = (FourBitNumber)ChannelID;
			plugin.mackieMidiOut.SendEvent(e);

			plugin.EmitMackieChannelDataChanged(this);
		}

	}
}
