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

		public MackieChannelData(int channelID) {
			ChannelID = channelID;
		}

	}
}
