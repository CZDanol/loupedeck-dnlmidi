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

			MackieChannelData cd = GetChannel(actionParameter);
			cd.Volume = Math.Min(1, Math.Max(0, (float)Math.Round(cd.Volume * 127 + diff) / 127.0f));
			cd.EmitVolumeUpdate();
			plugin.MackieSelectedChannel = cd;
		}

		protected override bool ProcessButtonEvent(string actionParameter, DeviceButtonEvent buttonEvent) {
			MackieChannelData cd = GetChannel(actionParameter);

			if(buttonEvent.IsPressed) {
				cd.Volume = 100.0f / 127.0f;
				cd.EmitVolumeUpdate();
				plugin.MackieSelectedChannel = cd;
			}

			return true;
		}

		protected override bool ProcessTouchEvent(string actionParameter, DeviceTouchEvent touchEvent) {
			MackieChannelData cd = GetChannel(actionParameter);

			plugin.MackieSelectedChannel = cd;

			return true;
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			MackieChannelData cd = GetChannel(actionParameter);

			var bb = new BitmapBuilder(imageSize);

			if (plugin.MackieSelectedChannel == cd)
				bb.FillRectangle(bb.Width - 4, 0, 4, bb.Height, new BitmapColor(52, 155, 235));

			if (cd.Muted)
				bb.FillRectangle(0, 0, 4, bb.Height, new BitmapColor(255, 0, 0));
			else if (cd.Solo)
				bb.FillRectangle(0, 0, 4, bb.Height, new BitmapColor(255, 255, 0));

			bb.DrawText($"{cd.TrackName}\n{Math.Round(cd.Volume * 100.0f)} %");
			return bb.ToImage();
		}

		private MackieChannelData GetChannel(string actionParameter) {
			return plugin.mackieChannelData[actionParameter];
		}
	}
}
