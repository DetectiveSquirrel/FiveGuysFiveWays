using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;
using System.Windows.Forms;

namespace FiveGuysFiveWays
{
    public class FiveGuysFiveWaysSettings : ISettings
    {
        //Mandatory setting to allow enabling/disabling your plugin
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
        public ToggleNode TimelessResetText { get; set; } = new ToggleNode(true);
        public RangeNode<float> TimelessSizeScale { get; set; } = new RangeNode<float>(7, 0, 300);
        public RangeNode<int> TimelessTextX { get; set; } = new RangeNode<int>(1284, 0, 2560);
        public RangeNode<int> TimelessTextY { get; set; } = new RangeNode<int>(401, 0, 2560);
        public ColorNode TimelessCircleColor { get; set; } = new ColorNode(Color.Red);
        public RangeNode<int> TimelessOuterCircle { get; set; } = new RangeNode<int>(650, 10, 1200);
        public RangeNode<int> TimelessOuterThickness { get; set; } = new RangeNode<int>(1, 0, 10);
        public ToggleNode ClickOnPositionChange { get; set; } = new ToggleNode(true);
        public HotkeyNode FirstPosition { get; set; } = new HotkeyNode(Keys.F13);
        public HotkeyNode SecondPosition { get; set; } = new HotkeyNode(Keys.F13);
        public HotkeyNode ThirdPosition { get; set; } = new HotkeyNode(Keys.F13);
        public HotkeyNode FourthPosition { get; set; } = new HotkeyNode(Keys.F13);
        public RangeNode<int> DelayBetweenEachMouseEvent { get; set; } = new RangeNode<int>(20, 0, 1000);
        public ToggleNode AddPingIntoDelay { get; set; } = new ToggleNode(false);

        //Put all your settings here if you can.
        //There's a bunch of ready-made setting nodes,
        //nested menu support and even custom callbacks are supported.
        //If you want to override DrawSettings instead, you better have a very good reason.
    }
}