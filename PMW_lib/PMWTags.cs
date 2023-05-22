using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMW_lib
{
    public class PMWTags
    {
        //public string FileName { get { return audiofileInfo.Name; } }
        //public string Duration { get { return audiofile._duration.ToString(@"hh\:mm\:ss"); } }
        //public string FileSize { get { return GetDisplayFileSize(audiofile.SizeBytes); } }
        public string CreationTime { get { return audiofileInfo.CreationTime.ToString("F"); } }
        public string WriteTime { get { return audiofileInfo.LastWriteTime.ToString("F"); } }
        public string Bitrate { get { return audiofileTags.Properties != null ? $"{audiofileTags.Properties.AudioBitrate} Кбит/сек" : "-"; } }
        public string SampleRate { get { return audiofileTags.Properties != null ? $"{audiofileTags.Properties.AudioSampleRate} Гц" : "-"; } }
        public string Channels { get { if (audiofileTags.Properties != null) return audiofileTags.Properties.AudioChannels == 1 ? "Моно" : "Стерео"; else return "-"; } }
        public string TrackNum { get { return audiofileTags.Tag.Track == 0 ? "<Нет>" : audiofileTags.Tag.Track.ToString(); } }
        public string TrackName { get { return audiofileTags.Tag.Title != null ? audiofileTags.Tag.Title : "<Без названия>"; } }
        public string TrackArtist { get { return audiofileTags.Tag.Performers.Length > 0 ? audiofileTags.Tag.JoinedPerformers : "<Неизвестен>"; } }
        public string TrackAlbum { get { return audiofileTags.Tag.Album != null ? audiofileTags.Tag.Album : "<Неизвестен>"; } }
        public string TrackGenre { get { return audiofileTags.Tag.Genres.Length > 0 ? audiofileTags.Tag.JoinedGenres : "<Нет>"; } }
        public string TrackYear { get { return audiofileTags.Tag.Year == 0 ? "<Нет>" : audiofileTags.Tag.Year.ToString(); } }
        public string TrackComment { get { return audiofileTags.Tag.Comment != null ? audiofileTags.Tag.Comment : "<Нет>"; } }
        public byte[]? TrackCover
        {
            get
            {
                foreach (var picture in audiofileTags.Tag.Pictures)
                {
                    if (picture.Type == TagLib.PictureType.FrontCover)
                    {
                        return picture.Data.Data;
                    }
                }
                return null;
            }
        }

        private readonly TagLib.File audiofileTags;
        private readonly PMWAudiofile audiofile;
        private readonly FileInfo audiofileInfo;

        public PMWTags(PMWAudiofile Audiofile)
        {
            audiofile = Audiofile;
            audiofileTags = TagLib.File.Create(audiofile.Path);
            audiofileInfo = new FileInfo(audiofile.Path);
        }

        static readonly string[] SizeSuffixes = { "байт", "КБ", "МБ", "ГБ", "ТБ", "ПБ" };
        static string GetDisplayFileSize(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + GetDisplayFileSize(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
