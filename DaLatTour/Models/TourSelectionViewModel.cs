using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class TourSelectionViewModel
    {
        public string TourType { get; set; } 
        public IEnumerable<Tour_Detail> RegularTours { get; set; }
        public IEnumerable<TourComboDeparture> ComboTours { get; set; }
    }
}