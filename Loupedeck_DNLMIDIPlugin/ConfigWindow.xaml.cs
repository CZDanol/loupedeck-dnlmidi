using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Melanchall.DryWetMidi.Multimedia;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	/// <summary>
	/// Interaction logic for ConfigWindow.xaml
	/// </summary>
	public partial class ConfigWindow : Window
	{
		private Loupedeck_DNLMIDIPlugin plugin;

		public ConfigWindow(Loupedeck_DNLMIDIPlugin plugin) {
			this.plugin = plugin;

			InitializeComponent();
			UpdateDeviceList();
		}

		private void UpdateDeviceList() {
			{
				var lst = midiIn;
				lst.Items.Clear();
				foreach (var d in InputDevice.GetAll())
					lst.Items.Add(d.Name);

				lst.SelectedItem = plugin.MidiInName;
			}

			{
				var lst = midiOut;
				lst.Items.Clear();
				foreach (var d in OutputDevice.GetAll())
					lst.Items.Add(d.Name);

				lst.SelectedItem = plugin.MidiOutName;
			}
		}

		private void midiIn_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			plugin.MidiInName = midiIn.SelectedItem as string;
		}

		private void midiOut_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			plugin.MidiOutName = midiOut.SelectedItem as string;
		}
	}
}
