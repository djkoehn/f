using System;
using Godot;

namespace Chickensoft.Log
{
    public static class Log
    {
        public static void Info(string message)
        {
            GD.Print($"[INFO] {message}");
        }

        public static void Error(Exception ex, string message)
        {
            GD.PrintErr($"[ERROR] {message}\nException: {ex}");
        }
    }
}