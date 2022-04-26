using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loupedeck.Loupedeck_DNLMIDIPlugin.Controls
{
	class MidiPadFolder : PluginDynamicFolder
	{
		static string[] defaultNoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

		class PadLayout
		{
			public string Name;

			public delegate string NoteNameT(CommandParams p);
			public delegate int NoteNumberT(CommandParams p);

			public NoteNameT NoteName;
			public NoteNumberT NoteNumber;

			public PadLayout() {
				NoteName = ((CommandParams p) => {
					int id = NoteNumber(p);
					return defaultNoteNames[id % defaultNoteNames.Length] + ((id / defaultNoteNames.Length) - 2).ToString();
				});
			}
		}
		IList<PadLayout> layouts;
		PadLayout currentLayout;
		int currentLayoutIx = 0;

		int octaveShift = 0;

		class Adjustment
		{
			public string Name;

			public int value = 0;

			public delegate void AdjustT(int delta);
			public delegate void ReleaseT();

			public AdjustT Adjust;
			public ReleaseT Release;
		}
		IList<Adjustment> adjustments;
		Adjustment currentHorizontalAdjustment, currentVerticalAdjustment;
		int currentHorizontalAdjustmentIx = 0, currentVerticalAdjustmentIx = 1;

		Loupedeck_DNLMIDIPlugin plugin;

		public MidiPadFolder() {
			this.DisplayName = "MIDI Pad";
			this.GroupName = "MIDI Pad";
			this.Navigation = PluginDynamicFolderNavigation.None;

			layouts = new List<PadLayout>();
			adjustments = new List<Adjustment>();

			const int C1MidiCode = 36;

			{
				PadLayout lt = new PadLayout();
				lt.Name = "Halft";
				lt.NoteNumber = (CommandParams p) => C1MidiCode + p.ix + octaveShift;
				layouts.Add(lt);
			}

			{
				int[] nums = { 0, 2, 4, 5, 7, 9, 11 };

				PadLayout lt = new PadLayout();
				lt.Name = "Hepta";
				lt.NoteNumber = (CommandParams p) => C1MidiCode + nums[p.ix % nums.Length] + (p.ix / nums.Length) * 12 + octaveShift;
				layouts.Add(lt);
			}

			{
				int[] nums = { 0, 2, 4, 7, 9 };

				PadLayout lt = new PadLayout();
				lt.Name = "Penta";
				lt.NoteNumber = (CommandParams p) => C1MidiCode + nums[p.ix % nums.Length] + (p.ix / nums.Length) * 12 + octaveShift;
				layouts.Add(lt);
			}

			{
				Adjustment adj = new Adjustment();
				adj.Name = "Mod";
				adj.Adjust = (int delta) => {
					adj.value += delta;

					var e = new ControlChangeEvent();
					e.ControlNumber = (SevenBitNumber)1; // Moduleation wheel
					e.ControlValue = (SevenBitNumber)Math.Max(0, Math.Min(adj.value * 10, 127));
					plugin.midiOut.SendEvent(e);
				};
				adjustments.Add(adj);
			}

			{
				Adjustment adj = new Adjustment();
				adj.Name = "Pitch Bend";
				adj.Adjust = (int delta) => {
					var e = new PitchBendEvent();
					adj.value += delta;
					e.PitchValue = (ushort)(Math.Max(0, Math.Min(8192 + adj.value * 500, 16383)));
					plugin.midiOut.SendEvent(e);
				};
				// Reset the pitch bend after release
				adj.Release = () => {
					adj.value = 0;

					var e = new PitchBendEvent();
					e.PitchValue = 8192;
					plugin.midiOut.SendEvent(e);
				};
				adjustments.Add(adj);
			}

			{
				Adjustment adj = new Adjustment();
				adj.Name = "None";
				adjustments.Add(adj);
			}

			currentLayout = layouts[currentLayoutIx];
			currentHorizontalAdjustment = adjustments[currentHorizontalAdjustmentIx];
			currentVerticalAdjustment = adjustments[currentVerticalAdjustmentIx];
		}

		public override bool Load() {
			var result = base.Load();

			plugin = base.Plugin as Loupedeck_DNLMIDIPlugin;
			return result;
		}

		public override IEnumerable<string> GetButtonPressActionNames() {
			var lst = new List<string>();

			// Commands for buttons
			for (int i = 0; i < 12; i++)
				lst.Add(CreateCommandName(i.ToString()));

			return lst;
		}

		public override IEnumerable<string> GetEncoderRotateActionNames() {
			var lst = new List<string>();

			lst.Add(CreateAdjustmentName("layout"));
			lst.Add(CreateAdjustmentName("hAdj"));
			lst.Add(CreateAdjustmentName("vAdj"));
			lst.Add(CreateAdjustmentName("oct"));

			return lst;
		}

		public override string GetAdjustmentDisplayName(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == "layout")
				return "Grid\n" + currentLayout.Name;

			else if (actionParameter == "hAdj")
				return "<>\n" + currentHorizontalAdjustment.Name;

			else if (actionParameter == "vAdj")
				return "/\\ \\/\n" + currentVerticalAdjustment.Name;

			else if (actionParameter == "oct")
				return "Octave\n" + (octaveShift > 0 ? "+" : "") + octaveShift.ToString();

			return null;
		}
		public override string GetCommandDisplayName(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			return currentLayout.NoteName(CommandParams.parse(actionParameter));
		}

		public override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize) {
			if (actionParameter == null)
				return null;

			var bb = new BitmapBuilder(imageSize);
			bb.FillRectangle(0, 0, bb.Width, bb.Height, BitmapColor.Black);
			bb.DrawText(GetCommandDisplayName(actionParameter, imageSize));
			return bb.ToImage();
		}

		class TouchData
		{
			public int originX, originY;
		}
		IDictionary<int, TouchData> touchData = new Dictionary<int, TouchData>();

		public override bool ProcessTouchEvent(string actionParameter, DeviceTouchEvent touchEvent) {
			if (actionParameter == null)
				return false;

			var p = CommandParams.parse(actionParameter);
			int noteNumber = currentLayout.NoteNumber(p);
			int ix = p.ix;

			if (touchEvent.EventType == DeviceTouchEventType.TouchDown && !touchData.ContainsKey(ix)) {
				var td = new TouchData
				{
					originX = touchEvent.X,
					originY = touchEvent.Y,
				};
				touchData.Add(ix, td);

				NoteOnEvent e = new NoteOnEvent();
				e.Velocity = (SevenBitNumber)(127);
				e.NoteNumber = (SevenBitNumber)(noteNumber);
				plugin.midiOut.SendEvent(e);

				return true;
			}
			else if (touchEvent.EventType == DeviceTouchEventType.TouchUp && touchData.ContainsKey(ix)) {
				touchData.Remove(ix);

				NoteOffEvent e = new NoteOffEvent();
				e.Velocity = (SevenBitNumber)(127);
				e.NoteNumber = (SevenBitNumber)(noteNumber);
				plugin.midiOut.SendEvent(e);

				if (currentHorizontalAdjustment.Release != null)
					currentHorizontalAdjustment.Release();

				if (currentVerticalAdjustment.Release != null)
					currentVerticalAdjustment.Release();

				return true;
			}
			else if (touchData.ContainsKey(ix)) {
				if (touchEvent.DeltaX != 0 && currentHorizontalAdjustment.Adjust != null)
					currentHorizontalAdjustment.Adjust(touchEvent.DeltaX);

				if (touchEvent.DeltaY != 0 && currentVerticalAdjustment != null)
					currentVerticalAdjustment.Adjust(-touchEvent.DeltaY);

				return true;
			}

			return base.ProcessTouchEvent(actionParameter, touchEvent);
		}

		static int mod(int a, int b) {
			int r = a % b;
			if (r < 0)
				r += b;

			return r;
		}

		public override bool ProcessEncoderEvent(string actionParameter, DeviceEncoderEvent encoderEvent) {
			if (actionParameter == "layout") {
				currentLayoutIx = mod(currentLayoutIx + encoderEvent.Clicks, layouts.Count);
				currentLayout = layouts[currentLayoutIx];
				CommandImageChanged(null);
				AdjustmentImageChanged("layout");
			}
			else if (actionParameter == "hAdj") {
				currentHorizontalAdjustmentIx = mod(currentHorizontalAdjustmentIx + encoderEvent.Clicks, adjustments.Count);
				currentHorizontalAdjustment = adjustments[currentHorizontalAdjustmentIx];
				AdjustmentImageChanged("hAdj");
			}
			else if (actionParameter == "vAdj") {
				currentVerticalAdjustmentIx = mod(currentVerticalAdjustmentIx + encoderEvent.Clicks, adjustments.Count);
				currentVerticalAdjustment = adjustments[currentVerticalAdjustmentIx];
				AdjustmentImageChanged("vAdj");
			}
			else if(actionParameter == "oct") {
				octaveShift += encoderEvent.Clicks;
				CommandImageChanged(null);
				AdjustmentImageChanged("oct");
			}

			return base.ProcessEncoderEvent(actionParameter, encoderEvent);
		}

		private class CommandParams
		{
			// Position on the loupedeck grid
			public int x, y;

			// General index of the param
			public int ix;

			public static CommandParams parse(string actionParameter) {
				int ix = Int32.Parse(actionParameter);
				return new CommandParams { x = ix % 4, y = ix / 4, ix = ix };
			}
		}
	}
}
