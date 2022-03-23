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

		private class ButtonData
		{
			public int Code;
			public string Name;
			public string IconName;

			public bool Activated = false;

			public BitmapColor OffColor = BitmapColor.Black;
			public BitmapColor OnColor = new BitmapColor(0, 57, 148);
			public BitmapImage Icon;
		}

		private IDictionary<string, ButtonData> buttonData = new Dictionary<string, ButtonData>();

		public MackieCommand() {
			string group = "Mackie control";

			AddButton(new ButtonData
			{
				Code = 97,
				Name = "Play",
				IconName = "play"
			});
			AddButton(new ButtonData
			{
				Code = 93,
				Name = "Stop",
				IconName = "stop"
			});
			AddButton(new ButtonData
			{
				Code = 95,
				Name = "Record",
				IconName = "record",
				OnColor = new BitmapColor(128, 0, 0)
			}); ;
			AddButton(new ButtonData
			{
				Code = 92,
				Name = "Fast forward",
				IconName = "fast_forward"
			});
			AddButton(new ButtonData
			{
				Code = 91,
				Name = "Rewind",
				IconName = "rewind"
			});
		}

		protected override bool OnLoad() {
			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;
			plugin.MackieNoteReceived += OnMackieNoteReceived;

			return base.OnLoad();
		}

		protected void OnMackieNoteReceived(object sender, NoteOnEvent e) {
			string param = e.NoteNumber.ToString();

			if (!buttonData.ContainsKey(param))
				return;

			ButtonData bd = buttonData[param];
			bd.Activated = e.Velocity > 0;
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
			if (actionParameter == null)
				return null;

			if (!buttonData.ContainsKey(actionParameter))
				return null;

			ButtonData bd = buttonData[actionParameter];

			var bb = new BitmapBuilder(imageSize);
			bb.FillRectangle(0, 0, bb.Width, bb.Height, bd.Activated ? bd.OnColor : bd.OffColor);

			if (bd.Icon != null)
				bb.DrawImage(bd.Icon);

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

		private void AddButton(ButtonData bd) {
			if (bd.IconName != null)
				bd.Icon = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("${bd.IconName}_64px.png"));

			buttonData[bd.Code.ToString()] = bd;
			AddParameter(bd.Code.ToString(), bd.Name, "Mackie control");
		}

	}
}
