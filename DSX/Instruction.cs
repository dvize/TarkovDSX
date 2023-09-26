namespace DSX
{
    public struct Instruction
    {
        public InstructionType type;
        public object[] parameters;

        public const int ControllerIndex = 0;

        public static Instruction TriggerThreshold(Trigger trigSide, int threshold)
        {
            return new Instruction
            {
                type = InstructionType.TriggerThreshold,
                parameters = new object[]
                {
                    ControllerIndex,          //controller index
                    (int)trigSide,   //trigger side
                    threshold       // threshold value
                }
            };
        }

        //Touchpad LED
        public static Instruction RGBUpdate(int rgb1, int rgb2, int rgb3)
        {
            return new Instruction
            {
                type = InstructionType.RGBUpdate,
                parameters = new object[]
                {
                    ControllerIndex,      //controller index
                    rgb1,                   //rgb value
                    rgb2,                   //rgb value
                    rgb3,                   //rgb value
                }
            };
        }

        public static Instruction PlayerLED(bool light1, bool light2, bool light3, bool light4, bool light5)
        {
            return new Instruction
            {
                type = InstructionType.PlayerLED,
                parameters = new object[]
                {
                    ControllerIndex,          //controller index
                    light1,
                    light2,
                    light3,
                    light4,
                    light5
                }
            };
        }

        // Layout for new revision controller looks like this::
        //  - -x- -  Player 1
        //  - x-x -  Player 2
        //  x -x- x  Player 3
        //  x x-x x  Player 4
        //  x xxx x  Player 5
        public static Instruction PlayerLEDNewRevision(PlayerLEDNewRevision revision)
        {
            return new Instruction
            {
                type = InstructionType.PlayerLEDNewRevision,
                parameters = new object[]
                {
                    ControllerIndex,          //controller index
                    (int)revision
                }
            };
        }

        public static Instruction MicLED(MicLEDMode mode)
        {
            return new Instruction
            {
                type = InstructionType.MicLED,
                parameters = new object[]
                {
                    ControllerIndex,      //controller index
                    (int)mode
                }
            };
        }


        //actual trigger update settings
        public static Instruction Normal(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.Normal }
            };
        }

        public static Instruction GameCube(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.GameCube }
            };
        }

        public static Instruction VerySoft(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.VerySoft }
            };
        }

        public static Instruction Soft(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.Soft }
            };
        }

        public static Instruction Hard(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.Hard }
            };
        }

        public static Instruction VeryHard(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.VeryHard }
            };
        }

        public static Instruction Hardest(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.Hardest }
            };
        }

        public static Instruction Rigid(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.Rigid }
            };
        }

        public static Instruction VibrateTrigger(Trigger trigSide, int frequency)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] {
                    ControllerIndex,
                    (int)trigSide,
                    (int)TriggerMode.VibrateTrigger,
                    frequency > 255 ? 255 : frequency
                }
            };
        }

        public static Instruction Choppy(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.Choppy }
            };
        }

        public static Instruction Medium(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.Medium }
            };
        }

        public static Instruction VibrateTriggerPulse(Trigger trigSide)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] { ControllerIndex, (int)trigSide, (int)TriggerMode.VibrateTriggerPulse }
            };
        }

        public static Instruction CustomTriggerValueMode(Trigger trigSide, CustomTriggerValueMode custValueMode, int force1, int force2, int force3, int force4, int force5, int force6, int force7)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] {
                    ControllerIndex,
                    (int)trigSide,
                    (int)TriggerMode.CustomTriggerValue,
                    custValueMode,
                    force1 < 0 ? 0 : force1 > 255 ? 255 : force1,
                    force2 < 0 ? 0 : force2 > 255 ? 255 : force2,
                    force3 < 0 ? 0 : force3 > 255 ? 255 : force3,
                    force4 < 0 ? 0 : force4 > 255 ? 255 : force4,
                    force5 < 0 ? 0 : force5 > 255 ? 255 : force5,
                    force6 < 0 ? 0 : force6 > 255 ? 255 : force6,
                    force7 < 0 ? 0 : force7 > 255 ? 255 : force7
                }
            };
        }

        public static Instruction Resistance(Trigger trigSide, int start, int force)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] {
                    ControllerIndex,
                    (int)trigSide,
                    (int)TriggerMode.Resistance,
                    start < 0 ? 0 : start > 9 ? 9 : start,
                    force < 0 ? 0 : force > 8 ? 8 : force
                }
            };
        }

        public static Instruction Bow(Trigger trigSide, int start, int end, int force, int snapForce)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] {
                    ControllerIndex,
                    (int)trigSide,
                    (int)TriggerMode.Bow,
                    start < 0 ? 0 : start > 8 ? 8 : start,
                    end < 0 ? 0 : end > 8 ? 8 : end,
                    force < 0 ? 0 : force > 8 ? 8 : force,
                    snapForce < 0 ? 0 : snapForce > 8 ? 8 : snapForce
                }
            };
        }

        public static Instruction Galloping(Trigger trigSide, int start, int end, int firstFoot, int secondFoot, int frequency)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] {
                    ControllerIndex,
                    (int)trigSide,
                    (int)TriggerMode.Galloping,
                    start < 0 ? 0 : start > 8 ? 8 : start,
                    end < 0 ? 0 : end > 9 ? 9 : end,
                    firstFoot < 0 ? 0 : firstFoot > 6 ? 6 : firstFoot,
                    secondFoot < 0 ? 0 : secondFoot > 7 ? 7 : secondFoot,
                    frequency < 0 ? 0 : frequency > 255 ? 255 : frequency
                }
            };
        }

        public static Instruction SemiAutomaticGun(Trigger trigSide, int start, int end, int force)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] {
                    ControllerIndex,
                    (int)trigSide,
                    (int)TriggerMode.SemiAutomaticGun,
                    start < 2 ? 2 : start > 7 ? 7 : start,
                    end < 0 ? 0 : end > 8 ? 8 : end,
                    force < 0 ? 0 : force > 8 ? 8 : force
                }
            };
        }

        public static Instruction AutomaticGun(Trigger trigSide, int start, int strength, int frequency)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] {
                    ControllerIndex,
                    (int)trigSide,
                    (int)TriggerMode.SemiAutomaticGun,
                    start < 0 ? 0 : start > 9 ? 9 : start,
                    strength < 0 ? 0 : strength > 8 ? 8 : strength,
                    frequency < 0 ? 0 : frequency > 255 ? 255 : frequency
                }
            };
        }

        public static Instruction Machine(Trigger trigSide, int start, int end, int strengthA, int strengthB, int frequency, int period)
        {
            return new Instruction
            {
                type = InstructionType.TriggerUpdate,
                parameters = new object[] {
                    ControllerIndex,
                    (int)trigSide,
                    (int)TriggerMode.AutomaticGun,
                    start < 0 ? 0 : start > 8 ? 8 : start,
                    end < 0 ? 0 : end > 9 ? 9 : end,
                    strengthA < 0 ? 0 : strengthA > 7 ? 7 : strengthA,
                    strengthB < 0 ? 0 : strengthB > 7 ? 7 : strengthB,
                    frequency < 0 ? 0 : frequency > 255 ? 255 : frequency,
                    period < 0 ? 0 : period > 2 ? 2 : period
                }
            };
        }



    }


}