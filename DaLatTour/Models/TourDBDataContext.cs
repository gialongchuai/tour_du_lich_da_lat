using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public partial class TourDBDataContext
    {
        public DbSet<TourWishlistModel> TourWishlists { get; set; }
        public DbSet<ServiceModel> Services
        {
            get; set;
        }
    }
}