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
            Space space = GetSpace();
            List<Person> population = GeneratePopulation(space);
            int generation = 1;
            long minDurationReached = long.MaxValue;
            int consistency = 0;
            int maximumConsitency = 20;
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
            Console.WriteLine(population[0]);
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
                var list = new List<CPMModels.Activity>();
                foreach (var activity in person.activities)
                {
                    list.Add(Map(activity, person));
                }
                list = GetFreeEndActivities(list);
                var newfitness = Output(list.Shuffle().CriticalPath(p => p.Predecessors, l => (long)l.Duration));
                if(newfitness > fitness) fitness = newfitness;
            }
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

        private CPMModels.Activity Map(ActivityIteration activityIteration, Person person)
        {
            CPMModels.Activity activity = new CPMModels.Activity();
            activity.Id = activityIteration.Id;
            activity.Duration = activityIteration.Iteration.Duration;
            activity.Description = activityIteration.Name;
            foreach (var predecessor in activityIteration.Dependencies)
            {
                var cpmiteration = person.activities.Where(a => a.Id == predecessor).FirstOrDefault();
                if(cpmiteration != default)
                    activity.Predecessors.Add(Map(cpmiteration, person));
            }
            return activity;
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
                if (activity.Id == "END") continue;
                totalDuration += activity.Duration;
            }
            return totalDuration;
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
            act2.Dependencies.Add("A35770");
            space.activities.Add(act3);


            /*var input = System.IO.File.ReadAllLines("input.txt");
            foreach (var line in input)
            {
                var activity = new Models.Activity();
                var elements = line.Split(' ');
                activity.Id = elements[0];
                activity.Name = elements[1];

                activity.Duration = int.Parse(elements[2]);

                int np = int.Parse(elements[3]);
            }*/
            return space;
        }
    }
}