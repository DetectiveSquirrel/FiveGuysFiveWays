using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Nodes;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace FiveGuysFiveWays
{
    public class FiveGuysFiveWays : BaseSettingsPlugin<FiveGuysFiveWaysSettings>
    {
        public KeyboardHelper KeyboardHelper { get; protected set; } = null;
        private CancellationTokenSource _cancellationTokenSource;
        private Thread _thread;
        private Entity legionEndlessInitiator;

        private const float GridToWorldMultiplier = 250 / 23f;
        private Camera Camera => GameController.Game.IngameState.Camera;
        internal static FiveGuysFiveWays Instance;


        public override bool Initialise()
        {
            base.Initialise();
            //Perform one-time initialization here

            if (Instance == null)
                Instance = this;

            RegisterHotkey(Settings.FirstPosition);
            RegisterHotkey(Settings.SecondPosition);
            RegisterHotkey(Settings.ThirdPosition);
            RegisterHotkey(Settings.FourthPosition);

            KeyboardHelper = new KeyboardHelper(GameController);
            _cancellationTokenSource = new CancellationTokenSource();
            _thread = new Thread(ResettingThread);

            return true;
        }

        public void Start()
        {
            if (_thread.ThreadState == ThreadState.Unstarted ||
                _thread.ThreadState == ThreadState.Stopped)
            {
                _thread = new Thread(ResettingThread);
                _cancellationTokenSource = new CancellationTokenSource();
                _thread.Start();
                LogMessage("Resetting Started!", 10);
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();

            // Wait for the thread to finish
            if (_thread.ThreadState == ThreadState.Running)
            {
                _thread.Join(TimeSpan.FromSeconds(0));
                LogMessage("Resetting Stopped!", 10);
            }
        }

        public override void Render()
        {
            //Any Imgui or Graphics calls go here. This is called after Tick
            RenderVisualForTimeless();
        }

        private static void RegisterHotkey(HotkeyNode hotkey)
        {
            Input.RegisterKey(hotkey);
            hotkey.OnValueChanged += () => { Input.RegisterKey(hotkey); };
        }

        public override void AreaChange(AreaInstance area)
        {
            //Perform once-per-zone processing here
            //For example, Radar builds the zone map texture here
        }

        public override Job Tick()
        {
            if (Settings.FirstPosition.PressedOnce())
            {
                MoveToFirstPos();
            }
            if (Settings.SecondPosition.PressedOnce())
            {
                MoveToSecondPos();
            }
            if (Settings.ThirdPosition.PressedOnce())
            {
                Start();
            }
            if (Settings.FourthPosition.PressedOnce())
            {
                Stop();
            }

            if (legionEndlessInitiator != null && !legionEndlessInitiator.IsValid)
            {
                Stop();
                legionEndlessInitiator = null;
            }

            if (legionEndlessInitiator == null)
            {
                if (!ProcessEntities())
                    LogMessage("Couldnt process Process Timeless Initiator", 2, Color.Blue);
                else
                {
                    LogMessage("Processed Timeless Initiator", 10, Color.Green);
                }
            } 
            

            return null;
        }

        public bool ProcessEntities()
        {
            var wantedEntity = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Terrain].FirstOrDefault(x => x.Metadata.Equals("Metadata/Terrain/Leagues/Legion/Objects/LegionEndlessInitiator"));
            if (wantedEntity == null)
            {
                return false;
            }

            legionEndlessInitiator = wantedEntity;

            return true;
        }

        private void ResettingThread()
        {
            if (legionEndlessInitiator == null || !legionEndlessInitiator.IsValid)
            {
                Stop();
                return;
            }

            // Do some work here
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var state = (EncounterState)legionEndlessInitiator?.GetComponent<StateMachine>()?.States.FirstOrDefault(s => s.Name == "checking_control_zone").Value;

                    var Player = GameController.Player;

                    var ShieldChargeSkill = Player.GetComponent<Actor>().ActorSkills.FirstOrDefault(x => x.Name.Equals("NewShieldCharge"));
                    var IsAbleToCharge = ShieldChargeSkill.CanBeUsed;

                    var DashSkill = Player.GetComponent<Actor>().ActorSkills.FirstOrDefault(x => x.Name.Equals("QuickDodge"));
                    var IsAbleToDash = DashSkill.CanBeUsed;

                    var ChargeKey = Keys.W;
                    var DashKey = Keys.Q;

                    var IsInCircle = legionEndlessInitiator.DistancePlayer < 63; // Lower than 60 will count as in, keep a small +3 buffer


                    if (state == EncounterState.EnterToCharge && IsAbleToCharge && !IsInCircle && !ShieldChargeSkill.IsUsing)
                    {
                        KeyboardHelper.KeyPressRelease(ChargeKey);
                    } 
                    else if (state == EncounterState.ResetNeeded && !ShieldChargeSkill.IsUsing && IsAbleToDash && IsInCircle)
                    {
                        KeyboardHelper.KeyPressRelease(DashKey);
                    }

                    ///Old loop
                    //switch (state)
                    //{
                    //    case EncounterState.ResetNeeded:
                    //        KeyboardHelper.KeyPressRelease(DashKey);
                    //        Thread.Sleep(220);
                    //        KeyboardHelper.KeyPressRelease(ChargeKey);
                    //        break;
                    //    case EncounterState.EnterToCharge:
                    //        if (GameController.Player.GetComponent<Actor>()?.Action == ActionFlags.UsingAbility || IsInCircle)
                    //            break;
                    //        KeyboardHelper.KeyPressRelease(ChargeKey);
                    //        Thread.Sleep(300);
                    //        break;
                    //}
                }
                catch (Exception e)
                {
                    LogError("[ResettingThread] " + e.ToString(), 10);
                }
            }
        }
        public class CircleData
        {
            public CircleData(Vector3 position, int radius, Color color, int thickness)
            {
                Position = position;
                Radius = radius;
                Color = color;
                Thickness = thickness;
            }
            public Vector3 Position { get; set; }
            public int Radius { get; set; }
            public Color Color { get; set; }
            public int Thickness { get; set; }
        }
        public enum EncounterState
        {
            ResetNeeded,
            EnterToCharge
        }
        private void RenderVisualForTimeless()
        {
            if (!Settings.TimelessResetText.Value)
                return;

            try
            {

                switch (legionEndlessInitiator?.GetComponent<StateMachine>()?.States.FirstOrDefault(s => s.Name == "checking_control_zone").Value)
                {
                    case 0:
                        using (Graphics.SetTextScale(Settings.TimelessSizeScale))
                        {
                            Graphics.DrawText($"RESET TIME",
                                new Vector2(Settings.TimelessTextX, Settings.TimelessTextY),
                                Color.Green,
                                FontAlign.Center);
                        }
                        break;
                    case 1:
                        using (Graphics.SetTextScale(Settings.TimelessSizeScale))
                        {
                            Graphics.DrawText($"<.CHARGING.>",
                                new Vector2(Settings.TimelessTextX, Settings.TimelessTextY),
                                Color.Red,
                                FontAlign.Center);
                        }
                        break;
                }
                var circleList = new List<CircleData>
                {
                    // Player to visual aid where to stand
                    new CircleData(GameController.Player.PosNum,
                                   8,
                                   Color.White,
                                   Settings.TimelessOuterThickness.Value)
                };

                if (legionEndlessInitiator != null)
                {
                    circleList.Add(new CircleData(legionEndlessInitiator.PosNum,
                               Settings.TimelessOuterCircle,
                               Settings.TimelessCircleColor.Value,
                               Settings.TimelessOuterThickness.Value));
                }

                IEnumerable<CircleData> circleEnumerable = circleList;
                DrawCirclesInWorld(circleList);
            }
            catch (Exception e)
            {
                LogError("[RenderVisualFoeTimeless] " + e.ToString(), 10);
            }
        }
        public void MoveToFirstPos()
        {
            MoveMouseToFloor(new Vector2(6603, 6853));
        }

        public void MoveToSecondPos()
        {
            SetCursorPosCenterMiddle(50);
        }
        private void SetCursorPos(SharpDX.Vector2 v)
        {
            Input.SetCursorPos(GameController.Window.GetWindowRectangleTimeCache.TopLeft + v);
        }
        private void SetCursorPosCenterMiddle(int topOffset)
        {
            var centerScreen = GameController.Window.GetWindowRectangleTimeCache.Width / 2;

            var newVec2 = new SharpDX.Vector2(centerScreen, topOffset);

            var topLeftOffset = GameController.Window.GetWindowRectangleTimeCache.TopLeft;

            Input.SetCursorPos(topLeftOffset + newVec2);
        }
        private void MoveMouseToFloor(Vector2 wantedPos)
        {
            var worldPos = Camera.WorldToScreen(new Vector3(wantedPos.X, wantedPos.Y, 0));
            SetCursorPos(
                    new SharpDX.Vector2(worldPos.X, worldPos.Y)
                );
        }
        private Vector3 ExpandWithTerrainHeight(Vector2 gridPosition)
        {
            return new Vector3(gridPosition.GridToWorld(), GameController.IngameState.Data.GetTerrainHeightAt(gridPosition));
        }

        internal Vector2 GetMousePosition()
        {
            return new Vector2(GameController.IngameState.MousePosX, GameController.IngameState.MousePosY);
        }

        private void DrawCircleInWorld(Vector3 positions, float radius, Color color, int thickness)
        {
            const int segments = 90;
            const int segmentSpan = 360 / segments;
            foreach (var segmentId in Enumerable.Range(0, segments))
            {
                (Vector2, Vector2) GetVector(int i)
                {
                    var (sin, cos) = MathF.SinCos(MathF.PI / 180 * i);
                    var offset = new Vector2(cos, sin) * radius;
                    var xy = positions.Xy() + offset;
                    var screen = Camera.WorldToScreen(ExpandWithTerrainHeight(xy.WorldToGrid()));
                    return (xy, screen);
                }

                var segmentOrigin = segmentId * segmentSpan;
                var (w1, c1) = GetVector(segmentOrigin);
                var (w2, c2) = GetVector(segmentOrigin + segmentSpan);

                Graphics.DrawLine(c1, c2, thickness, color);
            }
        }

        private void DrawCirclesInWorld(List<CircleData> list)
        {
            const int segments = 90;
            const int segmentSpan = 360 / segments;
            var playerPos = GameController.Player.GetComponent<Positioned>().WorldPosNum;
            foreach (var circle in list.Where(x => playerPos.Distance(new Vector2(x.Position.X, x.Position.Y)) < 80 * GridToWorldMultiplier + x.Radius))
            {
                foreach (var segmentId in Enumerable.Range(0, segments))
                {
                    (Vector2, Vector2) GetVector(int i)
                    {
                        var (sin, cos) = MathF.SinCos(MathF.PI / 180 * i);
                        var offset = new Vector2(cos, sin) * circle.Radius;
                        var xy = circle.Position.Xy() + offset;
                        var screen = Camera.WorldToScreen(ExpandWithTerrainHeight(xy.WorldToGrid()));
                        return (xy, screen);
                    }

                    var segmentOrigin = segmentId * segmentSpan;
                    var (w1, c1) = GetVector(segmentOrigin);
                    var (w2, c2) = GetVector(segmentOrigin + segmentSpan);

                    Graphics.DrawLine(c1, c2, circle.Thickness, circle.Color);
                }
            }
        }

        public override void EntityAdded(Entity entity)
        {
            //If you have a reason to process every entity only once,
            //this is a good place to do so.
            //You may want to use a queue and run the actual
            //processing (if any) inside the Tick method.
        }
    }
}