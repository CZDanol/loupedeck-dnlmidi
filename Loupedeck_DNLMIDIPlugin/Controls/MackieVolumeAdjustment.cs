using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	/*class MackieVolumeAdjustment : PluginDynamicAdjustment
	{

		Loupedeck_DNLMIDIPlugin plugin;

		class ChannelData
		{
			public int Channel;
			public int value = 0;

			public ChannelData(int channel) {
				Channel = channel;
			}
		}

		private IDictionary<string, ChannelData> channelData = new Dictionary<string, ChannelData>();

		MackieVolumeAdjustment() : base("_", "_", "_", true) {
			plugin = (Loupedeck_DNLMIDIPlugin)base.Plugin;

			this.Description = "Mackie Control compatible channel fader";

			for (int i = 0; i < 8; i++) {
				channelData[i.ToString()] = new ChannelData(i);
				AddParameter(i.ToString(), $"Channel {i + 1} volume", "Mackie Volume");
			}
		}

		protected override void ApplyAdjustment(string actionParameter, int diff) {
			ChannelData cd = channelData[actionParameter];
			cd.value = Math.Min(255, Math.Max(0, cd.value + diff));

			ActionImageChanged(actionParameter);
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			ChannelData cd = channelData[actionParameter];

			var bb = new BitmapBuilder(imageSize);
			bb.DrawText($"Channel {cd.Channel + 1}\n\u00A0\n{((float)cd.value) / 255} %"); // NBSP on the middle line to prevent coalescing
			return bb.ToImage();
		}

	}*/
}
