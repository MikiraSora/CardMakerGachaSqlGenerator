using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CardMakerGachaSqlGenerator.Base.Resp
{
    public partial class GetGachaCardApiResp : ObservableObject
    {
        [ObservableProperty]
        private int gachaId;

        [ObservableProperty]
        private int length;

        [ObservableProperty]
        private bool isPickup;

        [ObservableProperty]
        private GachaCard[] gameGachaCardList;

        [ObservableProperty]
        private string[] emissionList;

        [ObservableProperty]
        private string[] afterCalcList;

        [ObservableProperty]
        private string[] ssrBookCalcList;
    }
}
