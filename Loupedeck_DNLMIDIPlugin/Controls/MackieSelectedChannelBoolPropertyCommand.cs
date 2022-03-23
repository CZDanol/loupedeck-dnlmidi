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

			string group = "Mackie selected channel";
			AddParameter(((int)ChannelProperty.BoolType.Mute).ToString(), "Mute", group);
			AddParameter(((int)ChannelProperty.BoolType.Solo).ToString(), "Solo ", group);
			AddParameter(((int)ChannelProperty.BoolType.Arm).ToString(), "Arm/rec", group);
		}

		protected override bool OnLoad() {
			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;

			plugin.MackieChannelDataChanged += (object sender, MackieChannelData cd) => {
				ActionImageChanged();
			};

			return true;
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			MackieChannelData cd = GetChannel();
			int param = Int32.Parse(actionParameter);

			var bb = new BitmapBuilder(imageSize);

			BitmapColor c = ChannelProperty.boolPropertyColor[param];
			bb.FillRectangle(0, 0, bb.Width, bb.Height, BitmapColor.Black);
			bb.FillRectangle(0, 0, bb.Width, bb.Height, new BitmapColor(c.R, c.G, c.B, cd.BoolProperty[param] ? 255 : 32));
			bb.DrawText($"{cd.TrackName}\n{ChannelProperty.boolPropertyName[param]}");
			return bb.ToImage();
		}
		protected override void RunCommand(string actionParameter) {
			if (plugin.mackieMidiOut == null) {
				plugin.OpenConfigWindow();
				return;
			}

			MackieChannelData cd = GetChannel();
			int param = Int32.Parse(actionParameter);

			var e = new NoteOnEvent();
			e.NoteNumber = (SevenBitNumber)(ChannelProperty.boolPropertyMackieNote[param] + cd.ChannelID);
			e.Velocity = (SevenBitNumber)(!cd.BoolProperty[param] ? 127 : 0);
			plugin.mackieMidiOut.SendEvent(e);

			/*var e2 = new NoteOffEvent();
			e2.NoteNumber = e.NoteNumber;
			e2.Velocity = e.Velocity;
			plugin.mackieMidiOut.SendEvent(e2);*/

			//plugin.EmitMackieChannelDataChanged(cd);
		}

		private MackieChannelData GetChannel() {
			return plugin.MackieSelectedChannel;
		}

	}
}
