using System;
using System.Runtime.InteropServices;

namespace Commander.Lib.Services
{
    public interface Debugger
    {
        void Start();
        void Stop();
        void Show();
        void Hide();
        void Toggle();
        bool Active { get; }
    }

    public class DebuggerImpl : Debugger
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        [DllImport("kernel32")]
        static extern bool FreeConsole();

        [DllImport("Kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        static extern int ShowWindow(IntPtr hwnd, int nShow);

        public bool Active { get; private set; }

        private void enable()
        {
            AllocConsole();
            Hide();
            Console.BufferHeight = 9000;
            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(100, 30);
        }

        public void Start()
        {
            if (!Active)
            {
                enable();
            }
        }

        public void Stop()
        {
            FreeConsole();
        }

        public void Toggle()
        {
            if(Active)
            {
                Hide();
            } else
            {
                Show();
            }
        }

        public void Show()
        {
            if (Active)
            {
                return;
            }

            Active = true;
            ShowWindow(GetConsoleWindow(), 5);
        }

        public void Hide()
        {
            Active = false;
            ShowWindow(GetConsoleWindow(), 0);
        }
    }
}
