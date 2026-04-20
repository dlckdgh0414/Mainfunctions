using Code.MainSystem.Song;

namespace Code.Core.Bus.GameEvents.SongEvents
{
    public struct SongUploadOptionEvent : IEvent
    {
        public string SongName;
        public MarketingQuality SongMVBoost;
        public MarketingQuality SongThumbnailBoost;

        public SongUploadOptionEvent(string songName, MarketingQuality songMvBoost, MarketingQuality songThumbnailBoost)
        {
            SongName = songName;
            SongMVBoost = songMvBoost;
            SongThumbnailBoost = songThumbnailBoost;
        }
    }
}