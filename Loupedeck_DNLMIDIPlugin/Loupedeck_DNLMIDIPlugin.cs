namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	using System;
	using System.Threading;
	using Melanchall.DryWetMidi.Multimedia;

	public class Loupedeck_DNLMIDIPlugin : Loupedeck.Plugin
	{
		public override bool HasNoApplication => true;
		public override bool UsesApplicationApiOnly => true;

		public InputDevice midiIn = null;
		public OutputDevice midiOut = null;

		public Loupedeck_DNLMIDIPlugin() {
			
		}

		public void OpenConfigWindow() { 
			Thread t = new Thread(() => {
				ConfigWindow w = new ConfigWindow();
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

	}
}
