using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardMakerGachaSqlGenerator.Base.Resp
{
    public partial class GetGameTheaterApiResp : ObservableObject
    {
        [ObservableProperty]
        private int length;

        [ObservableProperty]
        private Theater[] gameTheaterList;

        [ObservableProperty]
        private int[] registIdList;
    }
}
