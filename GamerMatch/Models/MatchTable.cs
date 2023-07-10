using System;
using System.Collections.Generic;

namespace GamerMatch.Models
{
    public partial class MatchTable
    {
        public int Id { get; set; }
        public string UserSend { get; set; }
        public string UserGet { get; set; }
        public int? Status { get; set; }
        public int? Rating { get; set; }
    }
}
