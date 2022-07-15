using Navis_Plug.CPMModels;
using Navis_Plug.Models;

namespace Navis_Plug
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int targetDuration = 0;
            double targetCost = 4800000;
            Space space = GetSpace();
            List<Person> population = GeneratePopulation(space);
            int generation = 1;
            long minDurationReached = long.MaxValue;
            int consistency = 0;
            int maximumConsitency = 90;
            while (minDurationReached > targetDuration && consistency < maximumConsitency)
            {
                long newMinDuration;
                population = SortPopulation(population, out newMinDuration);
                if (newMinDuration >= minDurationReached) consistency++;
                else minDurationReached = newMinDuration;

                var newPopulation = new List<Person>(population.Take(10));
                var top50 = new List<Person>(population.Take(50));
                for (int i = 0; i < 90; i++)
                {
                    //Complete the new population
                    Random rn = new();
                    var parent1 = top50[rn.Next(50)];
                    var parent2 = top50[rn.Next(50)];
                    var child = MatePersons(parent1, parent2, space);
                    newPopulation.Add(child);
                }

                population = newPopulation;
                generation++;
            }
            population = SortPopulation(population, out _);
            var targetIndex = 0;
            var cost = double.MaxValue;
            for (int i = 0; i < population.Count; i++)
            {
                double totalCost = 0;
                var person = population[i];
                foreach (var activity in person.activities)
                {
                    totalCost += activity.Iteration.Cost;
                }
                if (totalCost <= targetCost)
                {
                    targetIndex = i;
                    cost = totalCost;
                    break;
                }
            }
            Console.WriteLine(population[targetIndex].TotalDuration);
            Console.WriteLine(cost);
        }

        private Person GenerateRandomPerson(Space space)
        {
            var person = new Person();
            foreach (var activity in space.activities)
            {
                Random rnd = new();
                int randomNo = rnd.Next(activity.Iterations.Count); //Count = 5
                person.activities.Add(new()
                {
                    Name = activity.Name,
                    Id = activity.Id,
                    Dependencies = activity.Dependencies,
                    Iteration = activity.Iterations[randomNo]
                });
            }
            return person;
        }

        private List<Person> GeneratePopulation(Space space)
        {
            var population = new List<Person>();
            for (int i = 0; i < 100; i++)
            {
                population.Add(GenerateRandomPerson(space));
            }
            return population;
        }

        private List<Person> SortPopulation(List<Person> population, out long fitness)
        {
            fitness = long.MaxValue;            
            //TODO: SORTING ALGORITHM FOR THE POPULATION
            foreach (var person in population)
            {
                using (StreamWriter writer = new StreamWriter("input.txt", false))
                {
                    foreach (var activity in person.activities)
                    {
                        var str = activity.Id + " " + activity.Name.Replace(" ", "") + " " + activity.Iteration.Duration + " " + activity.Dependencies.Count;
                        foreach (var dep in activity.Dependencies)
                        {
                            str += " " + dep;
                        }
                        writer.WriteLine(str);
                    }
                }

                var list = GetActivities();
                var newfitness = Output(list.Shuffle().CriticalPath(p => p.Predecessors, l => (long)l.Duration));
                if(newfitness < fitness) fitness = newfitness;
                if (newfitness == 0)
                {
                    Console.WriteLine("skfndflgfdkdf");
                }
                person.TotalDuration = newfitness;
            }
            population.Sort(delegate (Person p1, Person p2) { return p1.TotalDuration.CompareTo(p2.TotalDuration); });
            return population;
        }

        private Person MatePersons(Person person1, Person person2, Space space)
        {
            Random rnd = new();
            var random = rnd.NextDouble();
            if (random < 0.35)
                return person1;
            else if (random < 0.7)
                return person2;
            else
            {
                return GenerateRandomPerson(space);
            }
        }

        private static List<CPMModels.Activity> GetFreeEndActivities(List<CPMModels.Activity> list)
        {
            var endActivity = new CPMModels.Activity() { Id = "END", Description = "End Activity", Duration = 0 };
            foreach (var activity in list)
            {
                var foundSuccessors = false;
                foreach (var ac in list)
                {
                    if (ac.Predecessors.Any(a => a.Id == activity.Id))
                    {
                        foundSuccessors = true;
                    }
                }
                if (!foundSuccessors)
                {
                    endActivity.Predecessors.Add(activity);
                }
            }
            list.Add(endActivity);
            return list;
        }

        private static long Output(IEnumerable<CPMModels.Activity> list)
        {
            var totalDuration = 0L;
            foreach (CPMModels.Activity activity in list)
            {
                totalDuration += activity.Duration;
            }
            return totalDuration;
        }

        private static IEnumerable<CPMModels.Activity> GetActivities()
        {
            var list = new List<CPMModels.Activity>();
            var input = System.IO.File.ReadAllLines("input.txt");
            var ad = new Dictionary<string, CPMModels.Activity>();
            var deferredList = new Dictionary<CPMModels.Activity, List<string>>();

            int inx = 0;
            foreach (var line in input)
            {
                var activity = new CPMModels.Activity();
                var elements = line.Split(' ');
                activity.Id = elements[0];
                ad.Add(activity.Id, activity);
                activity.Description = elements[1];
                activity.Duration = int.Parse(elements[2]);
                int np = int.Parse(elements[3]);

                if (np != 0)
                {
                    var allIds = new List<string>();
                    for (int j = 0; j < np; j++)
                    {
                        allIds.Add(elements[4 + j]);
                    }

                    if (allIds.Any(i => !ad.ContainsKey(i)))
                    {
                        // Defer processing on this one
                        deferredList.Add(activity, allIds);
                    }
                    else
                    {
                        foreach (var id in allIds)
                        {
                            var aux = ad[id];

                            activity.Predecessors.Add(aux);
                        }
                    }
                }
                list.Add(activity);
            }

            while (deferredList.Count > 0)
            {
                var processedActivities = new List<CPMModels.Activity>();
                foreach (var activity in deferredList)
                {
                    if (activity.Value.Where(ad.ContainsKey).Count() == activity.Value.Count)
                    {
                        // All dependencies are now loaded
                        foreach (var id in activity.Value)
                        {
                            var aux = ad[id];

                            activity.Key.Predecessors.Add(aux);
                        }
                        processedActivities.Add(activity.Key);
                    }
                }
                foreach (var activity in processedActivities)
                {
                    deferredList.Remove(activity);
                }
            }

            return GetFreeEndActivities(list);
        }

        private Space GetSpace()
        {
            var space = new Space();
            space.activities = new();
            var act1 = new Models.Activity()
            {
                Id = "A35760",
                Name = "Borehole test",
                Dependencies = new(),
                Iterations = new List<Iteration>()
            };
            act1.Iterations.Add(new() { Cost = 0, Duration = 7 });
            act1.Iterations.Add(new() { Cost = 0, Duration = 7 });
            act1.Iterations.Add(new() { Cost = 0, Duration = 7 });
            act1.Iterations.Add(new() { Cost = 0, Duration = 7 });
            act1.Iterations.Add(new() { Cost = 0, Duration = 7 });
            space.activities.Add(act1);

            var act2 = new Models.Activity()
            {
                Id = "A35770",
                Name = "Excavation For foundation",
                Dependencies = new(),
                Iterations = new List<Iteration>()
            };
            act2.Iterations.Add(new() { Cost = 357860, Duration = 30 });
            act2.Iterations.Add(new() { Cost = 369436, Duration = 23 });
            act2.Iterations.Add(new() { Cost = 376140, Duration = 20 });
            act2.Iterations.Add(new() { Cost = 351004, Duration = 35 });
            act2.Iterations.Add(new() { Cost = 347500, Duration = 37 });
            act2.Dependencies.Add("A35760");
            space.activities.Add(act2);

            var act3 = new Models.Activity()
            {
                Id = "A35780",
                Name = "Backfilling between foundation",
                Dependencies = new(),
                Iterations = new List<Iteration>()
            };
            act3.Iterations.Add(new() { Cost = 63370, Duration = 30 });
            act3.Iterations.Add(new() { Cost = 68970, Duration = 25 });
            act3.Iterations.Add(new() { Cost = 77370, Duration = 20 });
            act3.Iterations.Add(new() { Cost = 54970, Duration = 33 });
            act3.Iterations.Add(new() { Cost = 54970, Duration = 33 });
            act3.Dependencies.Add("A35820");
            space.activities.Add(act3);

            var act4 = new Models.Activity()
            {
                Id = "A35810",
                Name = "Pc for foundation",
                Dependencies = new(),
                Iterations = new List<Iteration>()
            };
            act4.Iterations.Add(new() { Cost = 318019, Duration = 2 });
            act4.Iterations.Add(new() { Cost = 317019, Duration = 3 });
            act4.Iterations.Add(new() { Cost = 318019, Duration = 2 });
            act4.Iterations.Add(new() { Cost = 316819, Duration = 4 });
            act4.Iterations.Add(new() { Cost = 316619, Duration = 5 });
            act4.Dependencies.Add("A35770");
            space.activities.Add(act4);

            var act5 = new Models.Activity()
            {
                Id = "A35820",
                Name = "Rc foundation",
                Dependencies = new(),
                Iterations = new List<Iteration>()
            };
            act5.Iterations.Add(new() { Cost = 1813528, Duration = 17 });
            act5.Iterations.Add(new() { Cost = 1814632, Duration = 15 });
            act5.Iterations.Add(new() { Cost = 1815936, Duration = 13 });
            act5.Iterations.Add(new() { Cost = 1812976, Duration = 19 });
            act5.Iterations.Add(new() { Cost = 1812776, Duration = 22 });
            act5.Dependencies.Add("A35810");
            space.activities.Add(act5);

            var act6 = new Models.Activity()
            {
                Id = "A35830",
                Name = "Rc for retaining walls",
                Dependencies = new(),
                Iterations = new List<Iteration>()
            };
            act6.Iterations.Add(new() { Cost = 1148419, Duration = 17 });
            act6.Iterations.Add(new() { Cost = 1150274, Duration = 15 });
            act6.Iterations.Add(new() { Cost = 1151778, Duration = 13 });
            act6.Iterations.Add(new() { Cost = 1148218, Duration = 20 });
            act6.Iterations.Add(new() { Cost = 1148018, Duration = 23 });
            act6.Dependencies.Add("A35820");
            space.activities.Add(act6);

            var act7 = new Models.Activity()
            {
                Id = "A35860",
                Name = "Rc slabs and beams",
                Dependencies = new(),
                Iterations = new List<Iteration>()
            };
            act7.Iterations.Add(new() { Cost = 777314, Duration = 21 });
            act7.Iterations.Add(new() { Cost = 779018, Duration = 18 });
            act7.Iterations.Add(new() { Cost = 780322, Duration = 14 });
            act7.Iterations.Add(new() { Cost = 776762, Duration = 25 });
            act7.Iterations.Add(new() { Cost = 776562, Duration = 27 });
            act7.Dependencies.Add("A35830");
            act7.Dependencies.Add("A35870");
            space.activities.Add(act7);

            var act8 = new Models.Activity()
            {
                Id = "A35870",
                Name = "Rc for columns and walls",
                Dependencies = new(),
                Iterations = new List<Iteration>()
            };
            act8.Iterations.Add(new() { Cost = 300604, Duration = 7 });
            act8.Iterations.Add(new() { Cost = 302460, Duration = 6 });
            act8.Iterations.Add(new() { Cost = 304116, Duration = 5 });
            act8.Iterations.Add(new() { Cost = 299852, Duration = 9 });
            act8.Iterations.Add(new() { Cost = 299300, Duration = 11 });
            act8.Dependencies.Add("A35820");
            space.activities.Add(act8);

            return space;
        }
    }
}