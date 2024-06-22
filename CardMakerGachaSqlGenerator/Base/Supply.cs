using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardMakerGachaSqlGenerator.Base
{
    public partial class Supply : ObservableObject
    {
        [ObservableProperty]
        [SqlColnum("supply_id")]
        private int supplyId;

        [ObservableProperty]
        [SqlColnum("place_id")]
        private int placeId;

        [ObservableProperty]
        [SqlColnum("user_supply_date")]
        private DateTime? userSupplyDate;

        [ObservableProperty]
        [SqlColnum("user_id")]
        private int userId;

        [ObservableProperty]
        private ObservableCollection<SupplyGachaCard> cards = new();
    }
}
