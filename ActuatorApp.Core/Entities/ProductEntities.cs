using System;

namespace ActuatorApp.Core.Entities
{
    public class Touch
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImgPath { get; set; }
        public long Version { get; set; }
    }

    public class Tcsync
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string DEC { get; set; }
        public string HEX { get; set; }
        public string McuType { get; set; }
        public string Oscillator { get; set; }
        public string TcName { get; set; }
        public long ChoiesNumber { get; set; }
        public string SynType { get; set; }
        public string ReMark { get; set; }
        public string ImgPath { get; set; }
        public string HexPath { get; set; }
        public long Istouch { get; set; }
        public long IsShow { get; set; }
        public long IsTBB { get; set; }
        public string Defvoltage { get; set; }
        public string Defcurrent { get; set; }
    }

    public class TCS
    {
        public long Id { get; set; }
        public long DEC { get; set; }
        public string HEX { get; set; }
        public string Name { get; set; }
        public string Oscillator { get; set; }
        public string HexPath { get; set; }
    }

    public class Controlbox
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImgPath { get; set; }
        public string HexPath { get; set; }
    }

    public class Control
    {
        public long Id { get; set; }
        public long DEC { get; set; }
        public long HEX { get; set; }
        public string Name { get; set; }
        public string ImgPath { get; set; }
    }

    public class Columns
    {
        public long Id { get; set; }
        public long ActuId { get; set; }
        public string Name { get; set; } // Documentation says INTEGER, but "Name" usually string. Doc says "顯示名稱". Likely string.
        public string ImgPath { get; set; }
        public string Info { get; set; }
        public long Sort { get; set; }
    }

    public class Actuator
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class Actlevel
    {
        public long Id { get; set; }
        public long DEC { get; set; }
        public long HEX { get; set; }
        public long ColumnsId { get; set; }
        public string Columns { get; set; }
        public string Types { get; set; }
        public string MotorType { get; set; }
        public string Speed { get; set; }
        public double Resolution { get; set; } // NUMERIC
        public long ColStarting { get; set; }
        public long ColEnding { get; set; }
    }
}
