namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Melanchall.DryWetMidi.Core;
	using Melanchall.DryWetMidi.Multimedia;

	public class Loupedeck_DNLMIDIPlugin : Loupedeck.Plugin
	{
		public override bool HasNoApplication => true;
		public override bool UsesApplicationApiOnly => true;

		public InputDevice midiIn = null;
		public OutputDevice midiOut = null;

		public const int ChannelCount = 8;
		public IDictionary<string, ChannelData> channelData = new Dictionary<string, ChannelData>();

		string midiInName, midiOutName;

		public event EventHandler<ChannelData> ChannelDataChanged;

		public string MidiInName {
			get => midiInName;
			set {
				midiInName = value;
				midiIn = InputDevice.GetByName(value);
				midiIn.EventReceived += OnMidiEvent;
				midiIn.StartEventsListening();
				SetPluginSetting("midiIn", value);
			}
		}

		public string MidiOutName {
			get => midiOutName;
			set {
				midiOutName = value;
				midiOut = OutputDevice.GetByName(value);
				SetPluginSetting("midiOut", value);
			}
		}

		public Loupedeck_DNLMIDIPlugin() {
			for (int i = 0; i < 16; i++)
				channelData[i.ToString()] = new ChannelData(i);
		}

		public override void Load() {
			if (TryGetPluginSetting("midiIn", out midiInName))
				MidiInName = midiInName;

			if (TryGetPluginSetting("midiOut", out midiOutName))
				MidiOutName = midiOutName;
		}

		public void OpenConfigWindow() {
			Thread t = new Thread(() => {
				ConfigWindow w = new ConfigWindow(this);
				w.Show();
				System.Windows.Threading.Dispatcher.Run();
			});

			t.SetApartmentState(ApartmentState.STA);
			t.Start();
		}

		public override void RunCommand(String commandName, String parameter) {
		}

		public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff) {
		}

		private void OnMidiEvent(object sender, MidiEventReceivedEventArgs args) {
			MidiEvent e = args.Event;
			if (e is ChannelEvent) {
				if (!channelData.TryGetValue(((int)(e as ChannelEvent).Channel).ToString(), out ChannelData cd))
					return;

				if (e is ControlChangeEvent) {
					var ce = e as ControlChangeEvent;

					// Volume
					if (ce.ControlNumber == 7) {
						cd.Volume = ce.ControlValue;
						ChannelDataChanged.Invoke(this, cd);
					}
				}
			}
		}

	}
}
