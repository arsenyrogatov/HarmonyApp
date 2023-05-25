using Prism.Mvvm;

namespace HarmonyApp.Models
{
    public class TagEditModel : BindableBase
    {
        Audiofile audiofile;

        public string? FileName { get; set; }
        public string? newPath;
        public string? TrackName { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? Year { get; set; }
        public string? Number { get; set; }
        public string[]? Genre { get; set; }
        public string? Comment { get; set; }

        public TagEditModel(Audiofile curAudiofile)
        {
            audiofile = curAudiofile;
        }

        public void GetTags()
        {
            FileName = audiofile.FileName;
            TrackName = audiofile.TrackName;
            Artist = audiofile.TrackArtist;
            Album = audiofile.TrackAlbum;
            Year = audiofile.TrackYear;
            Number = audiofile.TrackNum;
            Genre = audiofile.TrackGenre.Split("; ");
            Comment = audiofile.TrackComment;
            RaisePropertyChanged(nameof(FileName));
            RaisePropertyChanged(nameof(TrackName));
            RaisePropertyChanged(nameof(Artist));
            RaisePropertyChanged(nameof(Album));
            RaisePropertyChanged(nameof(Year));
            RaisePropertyChanged(nameof(Number));
            RaisePropertyChanged(nameof(Genre));
            RaisePropertyChanged(nameof(Comment));
        }

        public void SaveTags()
        {
            if (FileName is not null && System.IO.Path.GetFileName(audiofile._path) != FileName)
            {
                var parentDir = System.IO.Path.GetDirectoryName(audiofile._path);
                newPath = System.IO.Path.Combine(parentDir, FileName);
                System.IO.File.Move(audiofile._path, newPath);
                audiofile._path = newPath;
            }

            TagLib.File _audiofileTags = TagLib.File.Create(audiofile._path);

            if (uint.TryParse(Number, out uint newNum) && Number != audiofile.TrackNum)
                _audiofileTags.Tag.Track = newNum;

            if (TrackName is not null && TrackName != audiofile.TrackName)
                _audiofileTags.Tag.Title = TrackName;

            if (Artist is not null && Artist != audiofile.TrackArtist)
                _audiofileTags.Tag.Performers = Artist.Split("; ");

            if (Album is not null && Album != audiofile.TrackAlbum)
                _audiofileTags.Tag.Album = Album;

            if (Genre is not null && string.Join("; ", Genre) != audiofile.TrackGenre)
                _audiofileTags.Tag.Genres = Genre;

            if (uint.TryParse(Year, out uint newYear) && Year != audiofile.TrackYear)
                _audiofileTags.Tag.Year = newYear;

            if (Comment is not null && Comment != audiofile.TrackComment)
                _audiofileTags.Tag.Comment = Comment;

            _audiofileTags.Save();
        }
    }
}
