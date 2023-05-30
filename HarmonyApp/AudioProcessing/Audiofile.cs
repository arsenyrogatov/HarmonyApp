using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyApp
{
    public class Audiofile: BindableBase
    {
        public Audiofile? Parent;
        public static int FilesCount { get; set; }

        public bool IsChild { get; }
        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; RaisePropertyChanged(nameof(IsSelected)); } }
        private bool _isSelected = false;

        public string DisplayPath { get { return IsChild ? $"       {_path}" : _path; } }
        public string _path;

        public string FileName { get { return System.IO.Path.GetFileName(_path); } }
        public string Duration { get; }

        public string Size { get { return GetDisplayFileSize(_sizeBytes); } }
        public long _sizeBytes;

        public string Rating { get; }
        public double RatingValue { get; }

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

        public string TrackNum { get; set; }
        public string TrackName { get; set; }
        public string TrackArtist { get; set; }
        public string TrackAlbum { get; set; }
        public string TrackGenre { get; set; }
        public string TrackYear { get; set; }
        public string TrackComment { get; set; }
        public byte[]? TrackCover { get; }

        public Audiofile(string path, Audiofile? parent = null, SoundFingerprinting.Query.ResultEntry? entry = null)
        {
            _path = path;
            IsChild = entry != null;
            Parent = parent;

            FileInfo _audiofileInfo = new(_path);
            TagLib.File _audiofileTags = TagLib.File.Create(_path);
            _sizeBytes = _audiofileInfo.Length;

            Similarity = IsChild && entry != null ? String.Format("{0:0.##}", entry.Coverage.Confidence * 100) + "%" : "";
            MatchedAt = IsChild && entry != null ? TimeSpan.FromSeconds(entry.TrackStartsAt).ToString(@"hh\:mm\:ss") : "";

            CreationTime = _audiofileInfo.CreationTime.ToString("F");
            WriteTime = _audiofileInfo.LastWriteTime.ToString("F");

            _bitrate = _audiofileTags.Properties?.AudioBitrate;
            _sampleRate = _audiofileTags.Properties?.AudioSampleRate;
            _channels = _audiofileTags.Properties?.AudioChannels;

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

            var rating = GetRating(tagsCount);
            if (rating > 100) rating = 100;
            RatingValue = rating;
            if (rating >= 75)
            {
                Rating = "Отлично";
            }
            else if (rating >= 50)
            {
                Rating = "Хорошо";
            }
            else if (rating >= 30)
            {
                Rating = "Плохо";
            }
            else
                Rating = "Ужасно";
            Rating += String.Format(" ({0:0.##}", rating) + "%)";

            //переписать!!
            Duration = TimeSpan.Zero.ToString(@"hh\:mm\:ss");
            try
            {
                if (_audiofileTags.Properties != null)
                {
                    Duration = _audiofileTags.Properties.Duration.ToString(@"hh\:mm\:ss");
                }
            }
            catch
            {

            }
            _audiofileTags.Dispose();

            CreateSpectrogram = new DelegateCommand(() => {
                GetSpectrogram();
            });
        }

        public void UpdateCheckedState (bool value)
        {
            IsSelected = value;
            RaisePropertyChanged(nameof(IsSelected));
        }

        public void InverseCheckedState ()
        {
            UpdateCheckedState(!IsSelected);
        }

        public void UpdateTags (string newPath)
        {
            _path = newPath;
            TagLib.File _audiofileTags = TagLib.File.Create(_path);
            if (_audiofileTags.Tag.Track != 0)
            {
                TrackNum = _audiofileTags.Tag.Track.ToString();
            }
            else
                TrackNum = "<Нет>";

            if (_audiofileTags.Tag.Title != null)
            {
                TrackName = _audiofileTags.Tag.Title;
            }
            else
                TrackName = "<Без названия>";

            if (_audiofileTags.Tag.Performers.Length > 0)
            {
                TrackArtist = _audiofileTags.Tag.JoinedPerformers;
            }
            else
                TrackArtist = "<Неизвестен>";

            if (_audiofileTags.Tag.Album != null)
            {
                TrackAlbum = _audiofileTags.Tag.Album;
            }
            else
                TrackAlbum = "<Неизвестен>";

            if (_audiofileTags.Tag.Genres.Length > 0)
            {
                TrackGenre = _audiofileTags.Tag.JoinedGenres;
            }
            else
                TrackGenre = "<Нет>";

            if (_audiofileTags.Tag.Year != 0)
            {
                TrackYear = _audiofileTags.Tag.Year.ToString();
            }
            else
                TrackYear = "<Нет>";

            if (_audiofileTags.Tag.Comment != null)
            {
                TrackComment = _audiofileTags.Tag.Comment;
            }
            else
                TrackComment = "<Нет>";

            RaisePropertyChanged(nameof(FileName));
            RaisePropertyChanged(nameof(DisplayPath));
            RaisePropertyChanged(nameof(TrackName));
            RaisePropertyChanged(nameof(TrackArtist));
            RaisePropertyChanged(nameof(TrackAlbum));
            RaisePropertyChanged(nameof(TrackGenre));
            RaisePropertyChanged(nameof(TrackYear));
            RaisePropertyChanged(nameof(TrackComment));
            RaisePropertyChanged(nameof(TrackNum));
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

            return bitrateToCompute * 30.0 / 512.0 + sampleRateToCompute * 30.0 / 48000.0 + channelsToCompute * 30.0 / 8.0 + tagsCount * 10.0 / 7.0;
        }

        static readonly string[] SizeSuffixes = { "Б", "КБ", "МБ", "ГБ", "ТБ", "ПБ" };
        public static string GetDisplayFileSize(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException(nameof(decimalPlaces)); }
            if (value < 0) { return "-" + GetDisplayFileSize(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} Б", 0); }

            int mag = (int)Math.Log(value, 1024);

            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        public DelegateCommand CreateSpectrogram { get; }

        public void GetSpectrogram()
        {
            Windows.SpectrogramWindow spectrogramWindow = new(this);
            spectrogramWindow.Show();
        }
    }
}
