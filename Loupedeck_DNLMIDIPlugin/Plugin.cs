namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	using System;
	using Melanchall.DryWetMidi.Multimedia;

	public class Plugin : Loupedeck.Plugin
	{
		public override bool HasNoApplication => true;
		public override bool UsesApplicationApiOnly => true;

		public InputDevice midiIn;
		public OutputDevice midiOut;

		public Plugin() {

		}

		public override void RunCommand(String commandName, String parameter) {
		}

		public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff) {
		}
	}
}
