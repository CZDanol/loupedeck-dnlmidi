namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
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

		public IDictionary<string, MackieChannelData> mackieChannelData = new Dictionary<string, MackieChannelData>();
		public string MackieDisplayData = new string(' ', 56 * 2);

		string midiInName, midiOutName;

		public event EventHandler<MackieChannelData> ChannelDataChanged;
		public event EventHandler MackieDisplayChanged;

		public string MidiInName {
			get => midiInName;
			set {
				if (midiIn != null) {
					midiIn.StopEventsListening();
					midiIn.Dispose();
				}

				midiInName = value;
				midiIn = InputDevice.GetByName(value);
				midiIn.EventReceived += OnMidiEvent;
				midiIn.StartEventsListening();
				SetPluginSetting("MidiIn", value, false);
			}
		}

		public string MidiOutName {
			get => midiOutName;
			set {
				if (midiOut != null) {
					midiOut.Dispose();
				}

				midiOutName = value;
				midiOut = OutputDevice.GetByName(value);
				SetPluginSetting("MidiOut", value, false);
			}
		}

		public Loupedeck_DNLMIDIPlugin() {
			for (int i = 0; i < ChannelCount; i++)
				mackieChannelData[i.ToString()] = new MackieChannelData(i);
		}

		public override void Load() {
			if (TryGetPluginSetting("MidiIn", out var midiInName))
				MidiInName = midiInName;
			else
				MidiInName = "DAW2SD"; // TODO REMOVEME

			if (TryGetPluginSetting("MidiOut", out var midiOutName))
				MidiOutName = midiOutName;
			else
				MidiOutName = "SD2DAW"; // TODO REMOVEME
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
				if (!mackieChannelData.TryGetValue(((int)(e as ChannelEvent).Channel).ToString(), out MackieChannelData cd))
					return;

				if (e is PitchBendEvent) {
					var ce = e as PitchBendEvent;
					cd.Volume = ce.PitchValue / 16383.0f;
					ChannelDataChanged.Invoke(this, cd);
				}
			}

			else if (e is NormalSysExEvent) {
				var ce = e as NormalSysExEvent;
				if (ce.Data.Length < 5)
					return;

				// Check if this is mackie control command
				byte[] mackieControlPrefix = { 0x00, 0x00, 0x66 };
				if (!ce.Data.SubArray(0, mackieControlPrefix.Length).SequenceEqual(mackieControlPrefix))
					return;

				// LCD command
				if (ce.Data.Length > 6 && ce.Data[4] == 0x12) {
					byte offset = ce.Data[5];
					byte[] str = ce.Data.SubArray(6, ce.Data.Length - 7);

					if (offset + str.Length > MackieDisplayData.Length)
						return;

					StringBuilder sb = new StringBuilder(MackieDisplayData);
					for (int i = 0; i < str.Length; i++)
						sb[i + offset] = (char)str[i];

					MackieDisplayData = sb.ToString();
					MackieDisplayChanged.Invoke(this, null);
				}
			}
		}

	}
}
