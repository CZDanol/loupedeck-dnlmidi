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
		public ConfigWindow() {
			InitializeComponent();
			UpdateDeviceList();
		}

		private void UpdateDeviceList() {
			{
				var lst = midiIn;
				lst.Items.Clear();
				foreach (var d in InputDevice.GetAll())
					lst.Items.Add(d.Name);
			}

			{
				var lst = midiOut;
				lst.Items.Clear();
				foreach (var d in OutputDevice.GetAll())
					lst.Items.Add(d.Name);
			}
		}
	}
}
