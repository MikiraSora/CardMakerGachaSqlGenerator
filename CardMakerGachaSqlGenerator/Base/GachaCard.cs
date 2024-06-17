﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CardMakerGachaSqlGenerator.Base
{
    public partial class GachaCard : ObservableObject
    {
        [SqlColnum("gacha_id")]
        [ObservableProperty]
        private int gachaId;

        [SqlColnum("card_id")]
        [ObservableProperty]
        private int cardId;

        public string Name => CardManager.GetCardName(CardId);

        [SqlColnum("rarity")]
        [ObservableProperty]
        private int rarity;

        [SqlColnum("weight")]
        [ObservableProperty]
        private int weight;

        [SqlColnum("is_pickup")]
        [ObservableProperty]
        private bool isPickup;

        [SqlColnum("is_select")]
        [ObservableProperty]
        private bool isSelect;
    }
}
