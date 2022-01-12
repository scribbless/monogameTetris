namespace MonogameTetris.TetrisLib
{
    public struct PlayerSettings
    {
        public int DelayTime;
        public int RepeatTime;
        
        public PlayerSettings(int delayTime, int repeatTime)
        {
            this.DelayTime = delayTime;
            this.RepeatTime = repeatTime;
        }
    }
}