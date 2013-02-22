using System.Collections.Generic;
using JavaGeneration.Chirper.Models;

namespace JavaGeneration.Chirper.ViewModels
{
    public class TimeLineData
    {
        public int FollowingCount { get; set; }

        public int? ChirpsCount
        {
            get { if (Chirps != null) return Chirps.Count;
                return null;
            }
        }
        public IList<Tweet> Chirps
        { get; set; }

        public int FollowersCount { get; set; }

        public bool ShowStats
        {
            get
            {
                if (IsSummaryVisible)
                    return false;
                return true;
            }
        }

        public bool ShowIndividualChrip { get; set; }
        public bool IsSummaryVisible { get; set; }
    }
}