using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CardMakerGachaSqlGenerator.Base
{
    public partial class Theater : ObservableObject
    {
        [ObservableProperty]
        [SqlColnum("theater_id")]
        private int theaterId;

        [ObservableProperty]
        [SqlColnum("theater_name")]
        private string theaterName;

        [ObservableProperty]
        [SqlColnum("start_date")]
        private DateTime startDate;

        [ObservableProperty]
        [SqlColnum("end_date")]
        private DateTime endDate;

        [ObservableProperty]
        [property: JsonPropertyName("gameSubTheaterList")]
        private ObservableCollection<SubTheater> gameSubTheaterList = new();

        [ObservableProperty]
        [SqlColnum("game_sub_theater_list")]
        private string serializedGameSubTheaterList;
    }
}
