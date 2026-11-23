using System.Threading;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using UnityEngine;

public class TspController : MonoBehaviour
{
    public LineRenderer _lr;

    public GameObject cityPrefab;
    private GeneticAlgorithm _ga;
    private Thread _gaThread;

    public int _numberOfCities = 30;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        GA();
        SpawnCities();
    }

    // Update is called once per frame
    void Update()
    {
        DrawRoute();
    }


    public void GA()
    {
        var fitness = new TspFitness(_numberOfCities);
        var chromosome = new TspChromossome(_numberOfCities);
        var crossover = new OrderedCrossover();
        var mutation =  new ReverseSequenceMutation();
        var selection = new RouletteWheelSelection();
        var population =  new Population(50,100,chromosome);
        _ga = new GeneticAlgorithm(population,fitness,selection,crossover,mutation);
        _ga.Termination = new TimeEvolvingTermination(System.TimeSpan.FromHours(1));
        _ga.TaskExecutor = new ParallelTaskExecutor
        {
            MinThreads =100,
            MaxThreads =200
        };
        _ga.GenerationRan += delegate
        {
        var distance = ((TspChromossome)_ga.BestChromosome).Distance;
        Debug.Log($"Gneration:{_ga.GenerationsNumber}-Distance:${distance}");
        };

        _gaThread = new Thread(()=> _ga.Start());
        _gaThread.Start();

        



    }

    private void Oestroy()
    {
        _ga.Stop();
        _gaThread.Abort();
    }


    void SpawnCities()
    {
        var cities = ((TspFitness)_ga.Fitness).Cities;

        for(int i = 0; i < _numberOfCities; i++)
        {
            var city = cities[i];
            var gO = Instantiate(cityPrefab,city.Position,Quaternion.identity)as GameObject;
        }
    }

   

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = _numberOfCities +1;
    }


    void DrawRoute()
    {
        var c = _ga.Population.CurrentGeneration.BestChromosome as TspChromossome;
        if (c != null)
        {
            var genes = c.GetGenes();
            var cities = ((TspFitness)_ga.Fitness).Cities;
            for(int i = 0; i < genes.Length; i++)
            {
                var city = cities[(int)genes[i].Value];
                _lr.SetPosition(i,city.Position);

                var firstCity = cities[(int)genes[0].Value];
                _lr.SetPosition(_numberOfCities,firstCity.Position);
            }
        }
    }




}
