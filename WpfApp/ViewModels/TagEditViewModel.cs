using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    public class TagEditViewModel : BindableBase
    {
        public TagEditModel? _model;

        public void Initilize(Audiofile audiofile)
        {
            _model = new TagEditModel(audiofile);
            _model.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            _model.GetTags();
        }

        public string? FileName { get => _model?.FileName; set { _model.FileName = value; } }
        public string? TrackName { get => _model?.TrackName; set { _model.TrackName = value; } }
        public string? Artist { get => _model?.Artist; set { _model.Artist = value; } }
        public string? Album { get => _model?.Album; set { _model.Album = value; } }
        public string? Year { get => _model?.Year; set { _model.Year = value; } }
        public string? Number { get => _model?.Number; set { _model.Number = value; } }
        public string? Genre { get { return _model is null ? "" : _model.Genre is null ? "" : String.Join("; ", _model.Genre); } set { _model.Genre = value?.Split("; "); } }
        public string? Comment { get => _model?.Comment; set { _model.Comment = value; } }

    }
}
