using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navis_Plug.Models
{
    public class Activity
    {
        public Activity()
        {
            Dependencies = new();
            Iterations = new();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Dependencies { get; set; }
        public List<Iteration> Iterations { get; set; }
    }
}
