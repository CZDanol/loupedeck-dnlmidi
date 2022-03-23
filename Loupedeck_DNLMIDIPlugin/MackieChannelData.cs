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
		public bool Solo = false;
		public bool Muted = false;
		public bool Armed = false;

		public bool IsMasterChannel = false;

		private Loupedeck_DNLMIDIPlugin plugin;

		public MackieChannelData(Loupedeck_DNLMIDIPlugin plugin, int channelID) {
			this.plugin = plugin;

			ChannelID = channelID;
			IsMasterChannel = channelID == Loupedeck_DNLMIDIPlugin.MackieChannelCount;

			if (IsMasterChannel)
				TrackName = "Master";
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
