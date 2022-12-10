using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
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
				midiIn.Items.Clear();
				mackieMidiIn.Items.Clear();

				foreach (var d in InputDevice.GetAll()) {
					midiIn.Items.Add(d.Name);
					mackieMidiIn.Items.Add(d.Name);
				}

				midiIn.SelectedItem = plugin.MidiInName;
				mackieMidiIn.SelectedItem = plugin.MackieMidiInName;
			}

			{
				midiOut.Items.Clear();
				mackieMidiOut.Items.Clear();

				foreach (var d in OutputDevice.GetAll()) {
					midiOut.Items.Add(d.Name);
					mackieMidiOut.Items.Add(d.Name);
				}

				midiOut.SelectedItem = plugin.MidiOutName;
				mackieMidiOut.SelectedItem = plugin.MackieMidiOutName;
			}
		}

		private void midiIn_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			plugin.MidiInName = midiIn.SelectedItem as string;
		}

		private void midiOut_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			plugin.MidiOutName = midiOut.SelectedItem as string;
		}

		private void mackieMidiIn_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			plugin.MackieMidiInName = mackieMidiIn.SelectedItem as string;
		}

		private void mackieMidiOut_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			plugin.MackieMidiOutName = mackieMidiOut.SelectedItem as string;
		}

		private void openURL(object sender, RequestNavigateEventArgs e) {
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}
