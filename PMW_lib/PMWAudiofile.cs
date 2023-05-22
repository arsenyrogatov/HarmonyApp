using Prism.Commands;

namespace PMW_lib
{
    public class PMWAudiofile
    {
        bool isChild;
        public string Path { get { return isChild ? $"     {_path}" : _path; } }
        private string _path;

        public string FileName { get { return System.IO.Path.GetFileName(_path); } }
        public string Duration { get; }

        public string Size { get { return GetDisplayFileSize(_sizeBytes); } }
        private long _sizeBytes;

        public string Rating { get; }

        public string Similarity { get; }

        public string MatchedAt { get; }

        public string CreationTime { get; }
        public string WriteTime { get; }

        public string Bitrate { get { return _bitrate != null ? $"{_bitrate} Кбит/сек" : "-"; } }
        private int? _bitrate;

        public string SampleRate { get { return _sampleRate != null ? $"{_sampleRate} Гц" : "-"; } }
        private int? _sampleRate;

        public string Channels { get { return _channels != null ? _channels == 1 ? "Моно" : $"Стерео ({_channels})" : "-"; } }
        private int? _channels;

        public string TrackNum { get; }
        public string TrackName { get; }
        public string TrackArtist { get; }
        public string TrackAlbum { get; }
        public string TrackGenre { get; }
        public string TrackYear { get; }
        public string TrackComment { get; }
        public byte[]? TrackCover { get; }

        public PMWAudiofile(string path, SoundFingerprinting.Query.ResultEntry? entry = null)
        {
            _path = path;
            isChild = entry != null;

            FileInfo _audiofileInfo = new(_path);
            TagLib.File _audiofileTags = TagLib.File.Create(_path);
            _sizeBytes = _audiofileInfo.Length;

            Similarity = isChild && entry != null ? String.Format("{0:0.##}", entry.Coverage.Confidence * 100) + "%" : "";
            MatchedAt = isChild && entry != null ? TimeSpan.FromSeconds(entry.TrackMatchStartsAt).ToString(@"hh\:mm\:ss\.fff") : "";

            CreationTime = _audiofileInfo.CreationTime.ToString("F");
            WriteTime = _audiofileInfo.LastWriteTime.ToString("F");

            _bitrate = _audiofileTags.Properties != null ? _audiofileTags.Properties.AudioBitrate : null;
            _sampleRate = _audiofileTags.Properties != null ? _audiofileTags.Properties.AudioSampleRate : null;
            _channels = _audiofileTags.Properties != null ? _audiofileTags.Properties.AudioChannels : null;

            int tagsCount = 0;

            if (_audiofileTags.Tag.Track != 0)
            {
                TrackNum = _audiofileTags.Tag.Track.ToString();
                tagsCount++;
            }
            else
                TrackNum = "<Нет>";

            if (_audiofileTags.Tag.Title != null)
            {
                TrackName = _audiofileTags.Tag.Title;
                tagsCount++;
            }
            else
                TrackName = "<Без названия>";

            if (_audiofileTags.Tag.Performers.Length > 0)
            {
                TrackArtist = _audiofileTags.Tag.JoinedPerformers;
                tagsCount++;
            }
            else
                TrackArtist = "<Неизвестен>";

            if (_audiofileTags.Tag.Album != null)
            {
                TrackAlbum = _audiofileTags.Tag.Album;
                tagsCount++;
            }
            else
                TrackAlbum = "<Неизвестен>";

            if (_audiofileTags.Tag.Genres.Length > 0)
            {
                TrackGenre = _audiofileTags.Tag.JoinedGenres;
                tagsCount++;
            }
            else
                TrackGenre = "<Нет>";

            if (_audiofileTags.Tag.Year != 0)
            {
                TrackYear = _audiofileTags.Tag.Year.ToString();
                tagsCount++;
            }
            else
                TrackYear = "<Нет>";

            if (_audiofileTags.Tag.Comment != null)
            {
                TrackComment = _audiofileTags.Tag.Comment;
                tagsCount++;
            }
            else
                TrackComment = "<Нет>";

            TrackCover = null;
            foreach (var picture in _audiofileTags.Tag.Pictures)
            {
                if (picture.Type == TagLib.PictureType.FrontCover)
                {
                    TrackCover = picture.Data.Data;
                }
            }
            
            Rating = String.Format("{0:0.##}", GetRating(tagsCount)) + "%";

            //переписать!!
            Duration = TimeSpan.Zero.ToString(@"hh\:mm\:ss\.fff");
            try
            {
                if (_audiofileTags.Properties != null)
                {
                    Duration = _audiofileTags.Properties.Duration.ToString(@"hh\:mm\:ss\.fff");
                }
            }
            catch
            {
                
            }

            OpenSelectedFile = new DelegateCommand(() => {
                if (!File.Exists(_path))
                {
                    return;
                }

                string argument = "/select, \"" + _path + "\"";

                System.Diagnostics.Process.Start("explorer.exe", argument);
            });
            PlaySelectedFileInAssociatedApp = new DelegateCommand(() => {
                if (!File.Exists(_path))
                {
                    return;
                }

                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
                processStartInfo.FileName = _path;
                processStartInfo.UseShellExecute = true;

                System.Diagnostics.Process.Start(processStartInfo);
            });
            PlaySelectedFile = new DelegateCommand(() => {
                if (!File.Exists(_path))
                {
                    return;
                }

                
            });
        }

        public DelegateCommand OpenSelectedFile { get; }
        public DelegateCommand PlaySelectedFileInAssociatedApp { get; }
        public DelegateCommand PlaySelectedFile { get; }

        public void InitializeMediaPlayer()
        {
            
        }

        private double GetRating(int tagsCount)
        {
            int bitrateToCompute = 0;
            if (_bitrate != null)
            {
                if (_bitrate > 320)
                    bitrateToCompute = 320;
                else
                    bitrateToCompute = (int)_bitrate;
            }

            int sampleRateToCompute = 0;
            if (_sampleRate != null)
            {
                if (_sampleRate > 48000)
                    sampleRateToCompute = 48000;
                else
                    sampleRateToCompute = (int)_sampleRate;
            }

            int channelsToCompute = 0;
            if (_channels != null)
            {
                if (_channels > 2)
                    channelsToCompute = 2;
                else
                    channelsToCompute = (int)_channels;
            }

            return bitrateToCompute * 25.0 / 320.0 + sampleRateToCompute * 25.0 / 48000.0 + channelsToCompute * 25.0 / 2.0 + tagsCount * 25.0 / 7.0;
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