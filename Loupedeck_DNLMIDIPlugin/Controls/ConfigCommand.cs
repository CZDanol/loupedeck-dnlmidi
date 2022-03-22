using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin.Controls
{
	class ConfigCommand : PluginDynamicCommand
	{

		public ConfigCommand() : base("DNL MIDI Settings", "Open DNL MIDI settings window", "Control") {

		}
		protected override void RunCommand(string actionParameter) {
			(base.Plugin as Loupedeck_DNLMIDIPlugin).OpenConfigWindow();
		}

	}
}
