﻿using ExileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FiveGuysFiveWays
{
    public class KeyboardHelper
    {
        private readonly GameController _gameHandle;

        public KeyboardHelper(GameController g)
        {
            _gameHandle = g;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        /*
                [return: MarshalAs(UnmanagedType.Bool)]
                [DllImport("user32.dll", SetLastError = true)]
                private static extern bool PostMessage(IntPtr hWnd, uint msg, UIntPtr wParam, UIntPtr lParam);
        */
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);
        [DllImport("USER32.dll")]
        private static extern short GetKeyState(int nVirtKey);
        public void KeyDown(Keys key)
        {
            SendMessage(_gameHandle.Window.Process.MainWindowHandle, 0x100, (int)key, 0);
        }

        public static bool IsKeyDown(int nVirtKey)
        {
            return GetKeyState(nVirtKey) < 0;
        }

        public void KeyUp(Keys key)
        {
            SendMessage(_gameHandle.Window.Process.MainWindowHandle, 0x101, (int)key, 0);
        }
        public bool KeyPressRelease(Keys key)
        {
            KeyDown(key);
            var lat = (int)(_gameHandle.Game.IngameState.ServerData.Latency);
            if (lat < 1000)
            {
                Thread.Sleep(lat);
                return true;
            }
            else
            {
                Thread.Sleep(1000);
                return false;
            }
        }
    }
}
