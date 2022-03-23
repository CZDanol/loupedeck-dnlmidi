namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Melanchall.DryWetMidi.Core;
	using Melanchall.DryWetMidi.Multimedia;

	public class Loupedeck_DNLMIDIPlugin : Loupedeck.Plugin
	{
		public override bool HasNoApplication => true;
		public override bool UsesApplicationApiOnly => true;

		public InputDevice midiIn = null, mackieMidiIn = null;
		public OutputDevice midiOut = null, mackieMidiOut = null;

		public const int MackieChannelCount = 8;

		public IDictionary<string, MackieChannelData> mackieChannelData = new Dictionary<string, MackieChannelData>();

		string mackieDisplayData = new string(' ', 56 * 2);
		string midiInName, midiOutName, mackieMidiInName, mackieMidiOutName;
		MackieChannelData mackieSelectedChannel = null;

		public event EventHandler<MackieChannelData> MackieChannelDataChanged;

		public string MidiInName {
			get => midiInName;
			set {
				if (midiIn != null) {
					midiIn.StopEventsListening();
					midiIn.Dispose();
				}

				midiInName = value;
				try {
					midiIn = InputDevice.GetByName(value);
					midiIn.StartEventsListening();
					SetPluginSetting("MidiIn", value, false);
				}
				catch (Exception e) {
					midiIn = null;
				}
			}
		}

		public string MidiOutName {
			get => midiOutName;
			set {
				if (midiOut != null) {
					midiOut.Dispose();
				}

				midiOutName = value;
				try {
					midiOut = OutputDevice.GetByName(value);
					SetPluginSetting("MidiOut", value, false);
				}
				catch (Exception e) {
					midiOut = null;
				}
			}
		}

		public string MackieMidiInName {
			get => mackieMidiInName;
			set {
				if (mackieMidiIn != null) {
					mackieMidiIn.StopEventsListening();
					mackieMidiIn.Dispose();
				}

				mackieMidiInName = value;
				try {
					mackieMidiIn = InputDevice.GetByName(value);
					mackieMidiIn.EventReceived += OnMackieMidiEvent;
					mackieMidiIn.StartEventsListening();
					SetPluginSetting("MackieMidiIn", value, false);
				}
				catch (Exception e) {
					mackieMidiIn = null;
				}
			}
		}

		public string MackieMidiOutName {
			get => mackieMidiOutName;
			set {
				if (mackieMidiOut != null) {
					mackieMidiOut.Dispose();
				}

				mackieMidiOutName = value;
				try {
					mackieMidiOut = OutputDevice.GetByName(value);
					SetPluginSetting("MackieMidiOut", value, false);
				}
				catch (Exception e) {
					mackieMidiOut = null;
				}
			}
		}

		public MackieChannelData MackieSelectedChannel {
			get => mackieSelectedChannel;
			set {
				if (mackieSelectedChannel == value || value.IsMasterChannel)
					return;

				MackieChannelData old = mackieSelectedChannel;
				mackieSelectedChannel = value;

				EmitMackieChannelDataChanged(old);
				EmitMackieChannelDataChanged(value);
			}
		}

		public Loupedeck_DNLMIDIPlugin() {
			// + 1 - last channel is master
			for (int i = 0; i < MackieChannelCount + 1; i++)
				mackieChannelData[i.ToString()] = new MackieChannelData(this, i);

			mackieSelectedChannel = mackieChannelData["0"];
		}

		public override void Load() {
			LoadSettings();
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

		public void EmitMackieChannelDataChanged(MackieChannelData cd) {
			MackieChannelDataChanged.Invoke(this, cd);
		}

		public override void RunCommand(String commandName, String parameter) {
		}

		public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff) {
		}

		private async void LoadSettings() {
			// Workaround - 
			await Task.Delay(100);

			if (TryGetPluginSetting("MidiIn", out midiInName))
				MidiInName = midiInName;

			if (TryGetPluginSetting("MackieMidiIn", out mackieMidiInName))
				MackieMidiInName = mackieMidiInName;

			if (TryGetPluginSetting("MidiOut", out midiOutName))
				MidiOutName = midiOutName;

			if (TryGetPluginSetting("MackieMidiOut", out mackieMidiOutName))
				MackieMidiOutName = mackieMidiOutName;
		}

		private void OnMackieMidiEvent(object sender, MidiEventReceivedEventArgs args) {
			MidiEvent e = args.Event;
			// PitchBend -> volume
			if (e is PitchBendEvent) {
				if (!mackieChannelData.TryGetValue(((int)(e as ChannelEvent).Channel).ToString(), out MackieChannelData cd))
					return;

				var ce = e as PitchBendEvent;
				cd.Volume = ce.PitchValue / 16383.0f;
				MackieChannelDataChanged.Invoke(this, cd);
			}

			// Note event -> solo/mute/...
			else if (e is NoteOnEvent) {
				var ce = e as NoteOnEvent;

				// Rec/Arm
				if (ce.NoteNumber >= 0 && ce.NoteNumber < 8) {
					if (!mackieChannelData.TryGetValue(ce.NoteNumber.ToString(), out MackieChannelData cd))
						return;

					cd.BoolProperty[(int)ChannelProperty.BoolType.Arm] = ce.Velocity > 0;
					MackieChannelDataChanged.Invoke(this, cd);
				}

				// Solo
				else if (ce.NoteNumber >= 8 && ce.NoteNumber < 16) {
					if (!mackieChannelData.TryGetValue((ce.NoteNumber - 8).ToString(), out MackieChannelData cd))
						return;

					cd.BoolProperty[(int)ChannelProperty.BoolType.Solo] = ce.Velocity > 0;
					MackieChannelDataChanged.Invoke(this, cd);
				}

				// Mute
				else if (ce.NoteNumber >= 16 && ce.NoteNumber < 32) {
					if (!mackieChannelData.TryGetValue((ce.NoteNumber - 16).ToString(), out MackieChannelData cd))
						return;

					cd.BoolProperty[(int)ChannelProperty.BoolType.Mute] = ce.Velocity > 0;
					MackieChannelDataChanged.Invoke(this, cd);
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

					if (offset + str.Length > mackieDisplayData.Length)
						return;

					StringBuilder sb = new StringBuilder(mackieDisplayData);
					for (int i = 0; i < str.Length; i++)
						sb[i + offset] = (char)str[i];

					mackieDisplayData = sb.ToString();

					for (int i = 0; i < MackieChannelCount; i++) {
						MackieChannelData cd = mackieChannelData[i.ToString()];
						string newTrackName = mackieDisplayData.Substring(7 * i, 7);
						if (!cd.TrackName.Equals(newTrackName)) {
							cd.TrackName = newTrackName;
							MackieChannelDataChanged.Invoke(this, cd);
						}
					}
				}
			}
		}

	}
}
