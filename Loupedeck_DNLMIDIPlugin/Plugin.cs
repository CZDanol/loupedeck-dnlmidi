namespace Loupedeck.Loupedeck_DNLMIDIPlugin
{
	using System;

	public class Plugin : Loupedeck.Plugin
	{
		public override bool HasNoApplication => true;
		public override bool UsesApplicationApiOnly => true;

		public override void RunCommand(String commandName, String parameter) {
		}

		public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff) {
		}
	}
}
