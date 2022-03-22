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

		public ConfigCommand() : base("Settings", "Open DNL MIDI settings window", "Control") {

		}
		protected override void RunCommand(string actionParameter) {
			((Loupedeck_DNLMIDIPlugin)base.Plugin).OpenConfigWindow();
		}

	}
}
