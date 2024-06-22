using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardMakerGachaSqlGenerator.Base
{
    public partial class Config : ObservableObject
    {
        [ObservableProperty]
        private string keychip;

        [ObservableProperty]
        private string aquaURL;

        [ObservableProperty]
        private string clientId;

        [ObservableProperty]
        private string romVersion;

        [ObservableProperty]
        private string packagePath;

        [ObservableProperty]
        private string sdedPackagePath;
    }
}
