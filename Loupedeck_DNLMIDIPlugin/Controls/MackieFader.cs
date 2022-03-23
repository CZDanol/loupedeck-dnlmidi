using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin.Controls
{
	class MackieFader : PluginDynamicAdjustment
	{
		private Loupedeck_DNLMIDIPlugin plugin = null;

		public MackieFader() : base(true) {
			this.Description = "Mackie Control compatible channel fader";

			for (int i = 0; i < Loupedeck_DNLMIDIPlugin.ChannelCount; i++)
				AddParameter(i.ToString(), $"Mackie fader (CH {i + 1})", "Mackie faders");

			AddParameter(Loupedeck_DNLMIDIPlugin.ChannelCount.ToString(), $"Mackie master fader", "Mackie faders");
		}

		protected override bool OnLoad() {
			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;

			plugin.MackieChannelDataChanged += (object sender, MackieChannelData cd) => {
				ActionImageChanged(cd.ChannelID.ToString());
			};

			return true;
		}

		protected override void ApplyAdjustment(string actionParameter, int diff) {
			if (plugin.mackieMidiOut == null) {
				plugin.OpenConfigWindow();
				return;
			}

			MackieChannelData cd = plugin.mackieChannelData[actionParameter];
			cd.Volume = Math.Min(1, Math.Max(0, cd.Volume + diff / 127.0f));

			var e = new PitchBendEvent();
			e.PitchValue = (ushort)(cd.Volume * 16383);
			e.Channel = (FourBitNumber)cd.ChannelID;
			plugin.mackieMidiOut.SendEvent(e);

			plugin.EmitMackieChannelDataChanged(cd);
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			MackieChannelData cd = plugin.mackieChannelData[actionParameter];

			string str = (cd.ChannelID == Loupedeck_DNLMIDIPlugin.ChannelCount) ? "Master" : cd.TrackName;

			var bb = new BitmapBuilder(imageSize);
			bb.DrawText($"{str}\n{Math.Round(cd.Volume * 100.0f)} %");
			return bb.ToImage();
		}

	}
}
