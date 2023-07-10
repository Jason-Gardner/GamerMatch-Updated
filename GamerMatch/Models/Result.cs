using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamerMatch.Models
{
    public class Result
    {
        public AspNetUsers user { get; set; }
        public int rating { get; set; }

        public Result(AspNetUsers User, int Rating)
        {
            user = User;
            rating = Rating;
        }
    }
}
