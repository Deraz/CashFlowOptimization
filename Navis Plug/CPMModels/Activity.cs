using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navis_Plug.CPMModels
{
    public class Activity
    {
        public Activity()
        {
            Predecessors = new List<Activity>();
        }
        /// <summary>
        /// Identification concerning the activity.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Brief description concerning the activity.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Total amount of time taken by the activity.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Activities that come before the activity.
        /// </summary>
        public ICollection<Activity> Predecessors { get; private set; }

        public override string ToString()
        {
            return Id;
        }
    }
}
