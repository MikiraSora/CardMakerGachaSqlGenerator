using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CardMakerGachaSqlGenerator.Base
{
    public partial class Gacha : ObservableObject
    {
        public enum GachaType
        {
            Normal
        }

        public enum GachaKind
        {
            Normal = 0,
            Pickup = 1,
            BonusRestored = 2,
            Free = 3,
            PickupBonusRestored = 4,
        }

        [ObservableProperty]
        [SqlColnum("gacha_id")]
        private int gachaId;

        [SqlColnum("gacha_name")]
        [ObservableProperty]
        private string gachaName;

        [SqlColnum("type")]
        [ObservableProperty]
        private GachaType type;

        [SqlColnum("kind")]
        [ObservableProperty]
        private GachaKind kind;

        [SqlColnum("is_ceiling")]
        [ObservableProperty]
        private bool isCeiling;

        [SqlColnum("max_select_point")]
        [ObservableProperty]
        private int maxSelectPoint;

        [SqlColnum("ceiling_cnt")]
        [ObservableProperty]
        private int ceilingCnt;

        [SqlColnum("change_rate_cnt1")]
        [ObservableProperty]
        private int changeRateCnt1;

        [SqlColnum("change_rate_cnt2")]
        [ObservableProperty]
        private int changeRateCnt2;

        [SqlColnum("start_date")]
        [ObservableProperty]
        private DateTime startDate;

        [SqlColnum("end_date")]
        [ObservableProperty]
        private DateTime endDate;

        [SqlColnum("notice_start_date")]
        [ObservableProperty]
        private DateTime noticeStartDate;

        [SqlColnum("notice_end_date")]
        [ObservableProperty]
        private DateTime noticeEndDate;

        [SqlColnum("convert_end_date")]
        [ObservableProperty]
        private DateTime convertEndDate;

        public override string ToString()
        {
            return $"{GachaId}: {GachaName}";
        }

        [ObservableProperty]
        private ObservableCollection<GachaCard> cards = new();
    }
}
