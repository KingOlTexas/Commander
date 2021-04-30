using Decal.Adapter;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Commander.Lib.Services
{
    public interface Logger
    {
        void Info(string message);
        void Error(Exception ex);
        void WriteToChat(string message);
        void WriteToWindow(string message);
        Logger Scope(string scope);
    }

    public class LoggerImpl : Logger
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowText(IntPtr hwnd, string lpString);

        private GlobalProvider _globals;
        private string _scope;
        private Logger _instance;

        public LoggerImpl(GlobalProvider globals)
        {
            _instance = this;
            _globals = globals;
            _scope = "Default";
        }

        public void WriteToWindow(string message)
        {
            IntPtr hwnd = _globals.Host.Decal.Hwnd;
            SetWindowText(hwnd, message);
        }

        public void Info(string message)
        {
            Console.WriteLine(Message(message, "INFO"));
        }

        private string Message(string message, string level)
        {
            return $@"{((level == "IN-GAME") ? String.Empty : DateTime.Now.ToString())}-[{level}]-[{_scope}]: {message}";
        }

        private void ErrorMessage(StreamWriter writer, string message)
        {
            Info(Message(message, "ERROR"));
            writer.WriteLine(message);
        }

        public void Error(Exception e)
        {
            try
            {
                string pluginPath = _globals.PluginPath;
                using (StreamWriter writer = new StreamWriter($@"{pluginPath}\errors.txt", true))
                {
                    ErrorMessage(writer, ("================================="));
                    ErrorMessage(writer, DateTime.Now.ToString());
                    ErrorMessage(writer, "Error: " + e.Message);
                    ErrorMessage(writer, "Source: " + e.Source);
                    ErrorMessage(writer, "Stack: " + e.StackTrace);
                    if (e.InnerException != null)
                    {
                        ErrorMessage(writer, "Inner: " + e.InnerException.Message);
                        ErrorMessage(writer, "Inner Stack: " + e.InnerException.StackTrace);
                    }
                    ErrorMessage(writer, "=================================");
                    ErrorMessage(writer, "");
                    writer.Close();
                }
            } catch (IOException ex) { }
        }

        public void WriteToChat(string message)
        {
            Info(message);
            message = Message(message, "IN-GAME");
            _globals.Host.Actions.AddChatText(message, 5);
        }

        public Logger Scope(string scope)
        {
            _scope = scope;
            return _instance;
        }
    }
}
