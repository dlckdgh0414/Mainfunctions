namespace Code.Core.Bus.GameEvents.SongEvents
{
    /// <summary>
    /// 결과창 띄울때 사용하는 이벤트. 보내면 결과창이 켜진다.
    /// </summary>
    public struct MusicReleaseResultEvent : IEvent
    {
        public int Gold;
        public int Pen;
        public float Star;
        public int NumberOfPlays;
        public int Exp;

        public MusicReleaseResultEvent(int gold, int pen, float star, int numberOfPlays, int exp)
        {
            Gold = gold;
            Pen = pen;
            Star = star;
            NumberOfPlays = numberOfPlays;
            Exp = exp;
        }
        
    }
}