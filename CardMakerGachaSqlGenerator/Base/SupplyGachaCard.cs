using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CardMakerGachaSqlGenerator.Base
{
    public partial class SupplyGachaCard : ObservableObject
    {
        [SqlColnum("card_id")]
        [ObservableProperty]
        private int cardId;

        [SqlColnum("supply_id")]
        [ObservableProperty]
        private int supplyId;

        public string Name => GameResourceManager.GetCardName(CardId);
        public int Rarity => GameResourceManager.GetCardInfo(CardId)?.Rarity ?? -1;
    }
}
