using Navis_Plug.CPMModels;
using Navis_Plug.Models;
using IronXL;
using System.Linq;

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
            var runTimesInput = numericUpDown1.Value;
            var targetCostPercentage = numericUpDown3.Value;
            var minimmumTotalDuration = long.MaxValue;
            string fileNameForMinDuration = "";
            for (int runTimes = 0; runTimes < runTimesInput; runTimes++)
            {
                // intializ variables in this function //

                //targetDuration is used for set the most prefered deuration that we want to reach..
                //it's impossible to reach to 0 but we but it to make our algorithm try to get it
                int targetDuration = 0;
                double targetCost;
                Space space = GetSpace(out targetCost, (int)targetCostPercentage);
                List<Person> population = GeneratePopulation(space);
                int generation = 1;
                long minDurationReached = long.MaxValue;

                int consistency = 0;

                // maximumConsitency used for set the limit of iterations when the fitness score have same value//
                int maximumConsitency = (int)numericUpDown2.Value;
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
                var output = population[targetIndex];
                var date = DateTime.Now;
                var stringDate = date.ToString("yyyyMMddHmmss");
                using (StreamWriter writer = new StreamWriter("output\\" + stringDate + ".csv", false))
                {
                    writer.WriteLine("Activity Name,Activity ID,Duration,Cost,Predecessors");
                    writer.WriteLine("All Activities,," + output.TotalDuration + "," + cost + ",");
                    if (output.TotalDuration < minimmumTotalDuration)
                    {
                        minimmumTotalDuration = output.TotalDuration;
                        fileNameForMinDuration = stringDate;
                    }
                    foreach (var act in output.activities)
                    {
                        var str = act.Name + "," + act.Id + "," + act.Iteration.Duration + "," + act.Iteration.Cost + ",";
                        foreach (var dep in act.Dependencies)
                        {
                            str += dep + "-";
                        }
                        writer.WriteLine(str.Substring(0, str.Length - 1));
                    }
                }
            }
            using (StreamWriter writer = new StreamWriter("output\\minDurationFileName.txt", false))
            {
                writer.WriteLine(fileNameForMinDuration);
            }
            this.textBox1.Text = "DONE!";

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

        private Space GetSpace(out double targetCost, int targetPercentage)
        {
            WorkBook workbook = WorkBook.Load("test.xlsx");
            WorkSheet sheet = workbook.WorkSheets.First();
            targetCost = sheet.Rows[1].Columns[4].DoubleValue * (1 + targetPercentage/100);
            var numberOfActivities = sheet.RowCount - 2;
            var space = new Space();
            space.activities = new();
            for (int i = 0; i < numberOfActivities; i++)
            {
                var row = sheet.Rows[i + 2];
                var act = new Models.Activity()
                {
                    Name = row.Columns[0].StringValue.Trim(),
                    Id = row.Columns[1].StringValue.Trim(),
                    Iterations = new(),
                    Dependencies = new()
                };
                act.Iterations.Add(new() { Cost = row.Columns[4].DoubleValue, Duration = row.Columns[3].IntValue });
                act.Iterations.Add(new() { Cost = row.Columns[6].DoubleValue, Duration = row.Columns[5].IntValue });
                act.Iterations.Add(new() { Cost = row.Columns[8].DoubleValue, Duration = row.Columns[7].IntValue });
                act.Iterations.Add(new() { Cost = row.Columns[10].DoubleValue, Duration = row.Columns[9].IntValue });
                act.Iterations.Add(new() { Cost = row.Columns[12].DoubleValue, Duration = row.Columns[11].IntValue });
                var deps = row.Columns[2].StringValue.Replace(" ", "").Split(',');
                foreach (var dep in deps)
                {
                    if (dep != null && dep != "")
                    {
                        act.Dependencies.Add(dep);
                    }
                }

                space.activities.Add(act);


            }

            return space;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}