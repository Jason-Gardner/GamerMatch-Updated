using System;
using System.Collections.Generic;

namespace GamerMatch.Models
{
    public partial class BoardGames
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public decimal? Score { get; set; }
        public int? Ratings { get; set; }
        public int? Maxplayers { get; set; }
    }
}
