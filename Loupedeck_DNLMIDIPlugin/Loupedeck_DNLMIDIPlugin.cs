namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Timers;
	using Loupedeck.Loupedeck_DNLMIDIPlugin.Controls;
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

		public event EventHandler MackieDataChanged;
		public event EventHandler<NoteOnEvent> MackieNoteReceived;

		public MackieFader mackieFader;

		public bool isConfigWindowOpen = false;

		private System.Timers.Timer mackieDataChangeTimer;

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
				catch (Exception) {
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
				catch (Exception) {
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
				catch (Exception) {
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
				catch (Exception) {
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

			mackieDataChangeTimer = new System.Timers.Timer(10);
			mackieDataChangeTimer.AutoReset = false;
			mackieDataChangeTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => {
				MackieDataChanged.Invoke(this, null);
			};
		}

		public override void Load() {
			this.Info.Icon16x16 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("midi_connector_male_16px.png"));
			this.Info.Icon32x32 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("midi_connector_male_32px.png"));
			this.Info.Icon48x48 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("midi_connector_male_48px.png"));
			this.Info.Icon256x256 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("midi_connector_male_96px.png"));

			LoadSettings();
		}

		public void OpenConfigWindow() {
			if (isConfigWindowOpen)
				return;

			Thread t = new Thread(() => {
				ConfigWindow w = new ConfigWindow(this);
				w.Closed += (_, _) => isConfigWindowOpen = false;
				w.Show();
				System.Windows.Threading.Dispatcher.Run();
			});

			t.SetApartmentState(ApartmentState.STA);
			t.Start();

			isConfigWindowOpen = true;
		}

		public void EmitMackieChannelDataChanged(MackieChannelData cd) {
			mackieDataChangeTimer.Start();
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
				EmitMackieChannelDataChanged(cd);
			}

			// Note event -> solo/mute/...
			else if (e is NoteOnEvent) {
				var ce = e as NoteOnEvent;

				// Rec/Arm
				if (ce.NoteNumber >= 0 && ce.NoteNumber < 8) {
					if (!mackieChannelData.TryGetValue(ce.NoteNumber.ToString(), out MackieChannelData cd))
						return;

					cd.BoolProperty[(int)ChannelProperty.BoolType.Arm] = ce.Velocity > 0;
					EmitMackieChannelDataChanged(cd);
				}

				// Solo
				else if (ce.NoteNumber >= 8 && ce.NoteNumber < 16) {
					if (!mackieChannelData.TryGetValue((ce.NoteNumber - 8).ToString(), out MackieChannelData cd))
						return;

					cd.BoolProperty[(int)ChannelProperty.BoolType.Solo] = ce.Velocity > 0;
					EmitMackieChannelDataChanged(cd);
				}

				// Mute
				else if (ce.NoteNumber >= 16 && ce.NoteNumber < 32) {
					if (!mackieChannelData.TryGetValue((ce.NoteNumber - 16).ToString(), out MackieChannelData cd))
						return;

					cd.BoolProperty[(int)ChannelProperty.BoolType.Mute] = ce.Velocity > 0;
					EmitMackieChannelDataChanged(cd);
				}

				else
					MackieNoteReceived.Invoke(this, ce);
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
						if (!cd.TrackName.Equals(newTrackName))
							cd.TrackName = newTrackName;
					}

					EmitMackieChannelDataChanged(null);
				}
			}
		}

		public override bool TryProcessTouchEvent(string actionName, string actionParameter, DeviceTouchEvent deviceTouchEvent) {
			if (actionName == mackieFader.GetResetActionName())
				return mackieFader.TryProcessTouchEvent(actionParameter, deviceTouchEvent);

			return base.TryProcessTouchEvent(actionName, actionParameter, deviceTouchEvent);
		}

	}
}
