using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin.Controls
{
	class MackieCommand : PluginDynamicCommand
	{
		Loupedeck_DNLMIDIPlugin plugin;

		ISet<string> activatedKeys = new HashSet<string>();

		public MackieCommand() {
			string group = "Mackie control";
			AddParameter("94", "Play", group);
			AddParameter("93", "Stop", group);
			AddParameter("95", "Record", group);
			AddParameter("92", "Fast forward", group);
			AddParameter("91", "Rewind", group);
		}

		protected override bool OnLoad() {
			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;
			plugin.MackieNoteReceived += OnMackieNoteReceived;

			return base.OnLoad();
		}

		protected void OnMackieNoteReceived(object sender, NoteOnEvent e) {
			string param = e.NoteNumber.ToString();

			if (e.Velocity > 0)
				activatedKeys.Add(param);
			else
				activatedKeys.Remove(param);

			ActionImageChanged(param);
		}

		protected override bool ProcessTouchEvent(string actionParameter, DeviceTouchEvent touchEvent) {
			if (touchEvent.EventType == DeviceTouchEventType.TouchDown)
				HandlePress(actionParameter, true);
			else if (touchEvent.EventType == DeviceTouchEventType.TouchUp)
				HandlePress(actionParameter, false);

			return base.ProcessTouchEvent(actionParameter, touchEvent);
		}

		protected override bool ProcessButtonEvent(string actionParameter, DeviceButtonEvent buttonEvent) {
			if (buttonEvent.IsPressed)
				HandlePress(actionParameter, true);
			else
				HandlePress(actionParameter, false);

			return base.ProcessButtonEvent(actionParameter, buttonEvent);
		}

		protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			var bb = new BitmapBuilder(imageSize);
			bb.FillRectangle(0, 0, bb.Width, bb.Height, activatedKeys.Contains(actionParameter) ? new BitmapColor(128, 128, 128) : BitmapColor.Black);
			bb.DrawText(actionParameter);
			return bb.ToImage();
		}

		private void HandlePress(string actionParameter, bool pressed) {
			int param = Int32.Parse(actionParameter);

			NoteOnEvent e = new NoteOnEvent();
			e.Velocity = (SevenBitNumber)(pressed ? 127 : 0);
			e.NoteNumber = (SevenBitNumber)(param);
			plugin.mackieMidiOut.SendEvent(e);

			ActionImageChanged(actionParameter);
		}

	}
}
