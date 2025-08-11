namespace FC2Editor.Core.Nomad
{
    internal enum Axis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
        XY = 3,
        XZ = 5,
        YZ = 6
    }

    internal enum AxisType
    {
        Local,
        World
    }

    public enum GameModes
    {
        Deathmatch,
        TeamDeathmatch,
        CaptureTheFlag,
        VIP
    }

    internal enum Pivot
    {
        Center,
        Left,
        Right,
        Down,
        Up
    }

    internal enum RotationDirection
    {
        CW,
        CCW
    }
}