using HarmonyApp.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyApp.ViewModels
{
    internal class CompareViewModel : BindableBase
    {
        public CompareModel _model;

        public CompareViewModel()
        {
            _model = new CompareModel();
            _model.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }

        public ReadOnlyObservableCollection<Audiofile> PublicAudiofiles { get => _model.PublicAudiofiles; }

        public void Initilize(ObservableCollection<Audiofile> audiofiles)
        {
            _model.Initilize(audiofiles);
        }
    }
}
