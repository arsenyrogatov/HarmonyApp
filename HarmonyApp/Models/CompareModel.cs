using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace HarmonyApp.Models
{
    public class CompareModel : BindableBase
    {
        public ReadOnlyObservableCollection<Audiofile> PublicAudiofiles { get; set; }

        public CompareModel()
        {
            PublicAudiofiles = new ReadOnlyObservableCollection<Audiofile>(new ObservableCollection<Audiofile>());
        }

        public void Initilize(ObservableCollection<Audiofile> audiofiles)
        {
            PublicAudiofiles = new(audiofiles);
            RaisePropertyChanged(nameof(PublicAudiofiles));
        }
    }
}
