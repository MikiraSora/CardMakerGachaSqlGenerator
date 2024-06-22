using CommunityToolkit.Mvvm.ComponentModel;

namespace CardMakerGachaSqlGenerator.Base
{
    public partial class SubTheater : ObservableObject
    {
        [ObservableProperty]
        private int theaterId;

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private int no;
    }
}
