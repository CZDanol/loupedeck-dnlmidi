using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin.Controls
{
	class MackieSelectedChannelBoolPropertyCommand : PluginDynamicCommand
	{

		Loupedeck_DNLMIDIPlugin plugin;

		public MackieSelectedChannelBoolPropertyCommand() {
			this.Description = "Control for currently selected Mackie channel";

			for (int i = 0; i < Loupedeck_DNLMIDIPlugin.MackieChannelCount + 1; i++) {
				string prefix = $"{i}:";
				string chstr = i == Loupedeck_DNLMIDIPlugin.MackieChannelCount ? " (Selected channel)" : $" (CH {i + 1})";

				AddParameter(prefix + ((int)ChannelProperty.BoolType.Mute).ToString(), "Mute" + chstr, "Mackie mute");
				AddParameter(prefix + ((int)ChannelProperty.BoolType.Solo).ToString(), "Solo " + chstr, "Mackie solo");
				AddParameter(prefix + ((int)ChannelProperty.BoolType.Arm).ToString(), "Arm/rec" + chstr, "Mackie arm/rec");
			}
		}

		protected override bool OnLoad() {
			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;

			plugin.MackieDataChanged += (object sender, EventArgs a) => {
				ActionImageChanged();
			};

			return true;
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			ParamData pd = GetParamData(actionParameter);
			MackieChannelData cd = pd.channelData;
			int param = pd.param;

			var bb = new BitmapBuilder(imageSize);

			BitmapColor c = ChannelProperty.boolPropertyColor[param];
			bb.FillRectangle(0, 0, bb.Width, bb.Height, BitmapColor.Black);
			bb.FillRectangle(0, 0, bb.Width, bb.Height, new BitmapColor(c.R, c.G, c.B, cd.BoolProperty[param] ? 255 : 32));

			const int trackNameH = 24;
			bb.DrawText(cd.TrackName, 0, 0, bb.Width, trackNameH);
			bb.DrawText(ChannelProperty.boolPropertyLetter[param], 0, trackNameH, bb.Width, bb.Height - trackNameH, null, 32);

			return bb.ToImage();
		}
		protected override void RunCommand(string actionParameter) {
			if (plugin.mackieMidiOut == null) {
				plugin.OpenConfigWindow();
				return;
			}

			ParamData pd = GetParamData(actionParameter);
			MackieChannelData cd = pd.channelData;
			int param = pd.param;

			if (cd.IsMasterChannel)
				return;

			cd.EmitBoolPropertyPress((ChannelProperty.BoolType)param);
		}

		private ParamData GetParamData(string actionParameter) {
			var dt = actionParameter.Split(':');
			return new ParamData
			{
				param = Int32.Parse(dt[1]),
				channelData = (dt[0] == Loupedeck_DNLMIDIPlugin.MackieChannelCount.ToString()) ? plugin.MackieSelectedChannel : plugin.mackieChannelData[dt[0]]
			};
		}

		private class ParamData
		{
			public int param;
			public MackieChannelData channelData;
		}

	}
}
