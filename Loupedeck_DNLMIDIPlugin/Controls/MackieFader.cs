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

			for (int i = 0; i < Loupedeck_DNLMIDIPlugin.MackieChannelCount; i++)
				AddParameter(i.ToString(), $"Mackie fader (CH {i + 1})", "Mackie faders");

			AddParameter(Loupedeck_DNLMIDIPlugin.MackieChannelCount.ToString(), $"Mackie master fader", "Mackie faders");
		}

		protected override bool OnLoad() {
			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;

			plugin.MackieDataChanged += (object sender, EventArgs e) => {
				ActionImageChanged();
			};

			return true;
		}

		protected override void ApplyAdjustment(string actionParameter, int diff) {
			if (plugin.mackieMidiOut == null) {
				plugin.OpenConfigWindow();
				return;
			}

			MackieChannelData cd = GetChannel(actionParameter);

			cd.Volume = Math.Min(1, Math.Max(0, (float)Math.Round(cd.Volume * 127 + diff) / 127.0f));
			cd.EmitVolumeUpdate();

			plugin.MackieSelectedChannel = cd;
		}

		protected override void RunCommand(string actionParameter) {
			MackieChannelData cd = GetChannel(actionParameter);
			plugin.MackieSelectedChannel = cd;
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			MackieChannelData cd = GetChannel(actionParameter);

			var bb = new BitmapBuilder(imageSize);

			if (plugin.MackieSelectedChannel == cd)
				bb.FillRectangle(0, 0, 16, 4, ChannelProperty.selectionColor);

			if (cd.Armed)
				bb.FillRectangle(bb.Width - 16, 0, 16, 4, ChannelProperty.boolPropertyColor[(int)ChannelProperty.BoolType.Arm]);

			int muteRectSize = bb.Height / 2;
			if (cd.Muted || cd.Solo)
				bb.FillRectangle(
					0, bb.Height - muteRectSize, bb.Width, muteRectSize,
					ChannelProperty.boolPropertyColor[cd.Muted ? (int)ChannelProperty.BoolType.Mute : (int)ChannelProperty.BoolType.Solo]
					);

			int fontSize = imageSize == PluginImageSize.Width60 ? 12 : 15;
			bb.DrawText(cd.TrackName, 0, 0, bb.Width, bb.Height / 2, null, fontSize);
			bb.DrawText($"{Math.Round(cd.Volume * 100.0f)} %", 0, bb.Height / 2, bb.Width, bb.Height / 2, null, fontSize);
			return bb.ToImage();
		}

		private MackieChannelData GetChannel(string actionParameter) {
			return plugin.mackieChannelData[actionParameter];
		}
	}
}
