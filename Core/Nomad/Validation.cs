using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class Validation
    {
        public static ValidationReport ValidateGameMode(GameModes gameMode) => new ValidationReport(FCE_Validation_GameMode(gameMode));
        public static ValidationReport ValidateGame() => new ValidationReport(FCE_Validation_Game());

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Validation_GameMode(GameModes gameMode);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Validation_Game();
    }
}