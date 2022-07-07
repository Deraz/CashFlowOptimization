using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navis_Plug.Models
{
    public class Person
    {
        public Person()
        {
            activities = new();
        }
        public List<ActivityIteration> activities { get; set; }
    }
}
