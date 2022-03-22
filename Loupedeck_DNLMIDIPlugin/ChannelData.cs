using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	public class ChannelData
	{

		public int ChannelID;
		public int Volume = 0;
		public ChannelData(int channelID) {
			ChannelID = channelID;
		}

	}
}
