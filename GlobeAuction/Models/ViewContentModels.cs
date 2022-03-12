using System.Collections.Generic;

namespace GlobeAuction.Models
{
    public class MenuLayoutButton
    {
        public string Href { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class MenuLayout
    {
        public List<MenuLayoutButton> LeftButtons { get; set; }
        public List<MenuLayoutButton> RightButtons { get; set; }
    }
}