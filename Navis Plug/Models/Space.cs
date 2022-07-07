using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navis_Plug.Models
{
    public class Space
    {
        public Space()
        {
            activities = new List<Activity>();
        }
        public List<Activity> activities { get; set; }

}
}
