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

		public MackieVolumeAdjustment() : base(true) {
			this.Description = "Mackie Control compatible channel fader";

			for (int i = 0; i < Loupedeck_DNLMIDIPlugin.ChannelCount; i++)
				AddParameter(i.ToString(), $"CH{i + 1} volume", "Mackie Volume");
		}

		protected override bool OnLoad() {
			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;

			plugin.ChannelDataChanged += (object sender, MackieChannelData cd) => {
				ActionImageChanged(cd.ChannelID.ToString());
			};

			return true;
		}

		protected override void ApplyAdjustment(string actionParameter, int diff) {
			if (plugin.midiOut == null)
				return;

			MackieChannelData cd = plugin.mackieChannelData[actionParameter];
			cd.Volume = Math.Min(1, Math.Max(0, cd.Volume + diff * 0.01f));

			var e = new PitchBendEvent();
			e.PitchValue = (ushort)(cd.Volume * 16383);
			e.Channel = (FourBitNumber)cd.ChannelID;
			plugin.midiOut.SendEvent(e);

			ActionImageChanged(actionParameter);
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			MackieChannelData cd = plugin.mackieChannelData[actionParameter];

			var bb = new BitmapBuilder(imageSize);
			bb.DrawText($"Channel {cd.ChannelID + 1}\n{Math.Round(cd.Volume * 100.0f)} %");
			return bb.ToImage();
		}

	}
}
