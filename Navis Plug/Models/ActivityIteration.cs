using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navis_Plug.Models
{
    public class ActivityIteration
    {
        public ActivityIteration()
        {
            Dependencies = new List<string>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Dependencies { get; set; }
        public Iteration Iteration { get; set; }
    }
}
