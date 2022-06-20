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
            Space space = new();
            List<Person> population = GeneratePopulation(space);
            int generation = 1;
            int minDurationReached = int.MaxValue;
            int consistency = 0;
            int maximumConsitency = 20;
            while (minDurationReached > targetDuration && consistency < maximumConsitency)
            {
                population = SortPopulation(population, out minDurationReached);
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

        private List<Person> SortPopulation(List<Person> population, out int fitness)
        {
            fitness = 0;
            //TODO: SORTING ALGORITHM FOR THE POPULATION
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
    }
}