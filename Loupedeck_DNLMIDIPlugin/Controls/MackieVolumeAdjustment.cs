using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin.Controls
{
	class MackieVolumeAdjustment : PluginDynamicAdjustment
	{
		private Loupedeck_DNLMIDIPlugin plugin = null;


		public MackieVolumeAdjustment() : base("_", "_", "_", true) {
			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;

			this.Description = "Mackie Control compatible channel fader";

			for (int i = 0; i < Loupedeck_DNLMIDIPlugin.ChannelCount; i++)
				AddParameter(i.ToString(), $"Channel {i + 1} volume", "Mackie Volume");
		}

		protected override void ApplyAdjustment(string actionParameter, int diff) {
			if (plugin.midiOut == null)
				return;

			ChannelData cd = plugin.channelData[actionParameter];
			cd.Volume = Math.Min(255, Math.Max(0, cd.Volume + diff));

			plugin.midiOut.SendEvent(new ControlChangeEvent((SevenBitNumber)0, (SevenBitNumber)cd.Volume));

			ActionImageChanged(actionParameter);
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			ChannelData cd = plugin.channelData[actionParameter];

			var bb = new BitmapBuilder(imageSize);
			bb.DrawText($"Channel {cd.ChannelID + 1}\n\u00A0\n{((float)cd.Volume) / 255} %"); // NBSP on the middle line to prevent coalescing
			return bb.ToImage();
		}

	}
}
