using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static CardMakerGachaSqlGenerator.Base.Gacha;

namespace CardMakerGachaSqlGenerator.Base.Resp
{
    public partial class CMGetUserGachaSupplyApiResp : ObservableObject
    {
        [ObservableProperty]
        private int[] supplyCardList;

        [ObservableProperty]
        private int supplyId;
    }
}
