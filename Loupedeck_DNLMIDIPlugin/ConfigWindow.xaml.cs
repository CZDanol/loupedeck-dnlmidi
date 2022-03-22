using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	/// <summary>
	/// Interaction logic for ConfigWindow.xaml
	/// </summary>
	public partial class ConfigWindow : Window
	{
		public ConfigWindow() {
			InitializeComponent();
		}

		private async void UpdateDeviceList() {
			string inQuery = MidiInPort.GetDeviceSelector();
			DeviceInformationCollection midiInputDevices = await DeviceInformation.FindAllAsync(midiInputQueryString);

			midiInPortListBox.Items.Clear();

			// Return if no external devices are connected
			if (midiInputDevices.Count == 0) {
				this.midiInPortListBox.Items.Add("No MIDI input devices found!");
				this.midiInPortListBox.IsEnabled = false;
				return;
			}

			// Else, add each connected input device to the list
			foreach (DeviceInformation deviceInfo in midiInputDevices) {
				this.midiInPortListBox.Items.Add(deviceInfo.Name);
			}
			this.midiInPortListBox.IsEnabled = true;
		}
	}
}
