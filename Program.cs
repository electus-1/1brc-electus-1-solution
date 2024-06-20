/*Improvements
 1. DONE. Calculate average at the end. You don't want to do a billion divisions. Trust me.
 2. DONE. Reduce memory usage by removing temperatureMap<string, double[]>, it will also help not to store a billion temperatures in memory.
 3. DONE. Use floats instead of double, it won't hurt.
 4. Use buffered reading and data partitioning.
 5. Use Memory<T> to utilize memory. 
 */
using System.Collections.Concurrent;
using System.Text;

namespace _1BillionRowChallenge;

internal abstract class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            throw new ArgumentException("You must provide the relative path of measurements.txt to the executable as the only argument");
        }
        var filePath = args[0];
        var cityMap = new ConcurrentDictionary<string, City>();
        ReadTemperatureData(filePath, cityMap);
        CalculateMean(cityMap);
        var orderedKeys = OrderKeys(cityMap.Keys);
        OutputFormatted(orderedKeys, cityMap);
    }

    private static void ReadTemperatureData(string filePath, ConcurrentDictionary<string, City> cityMap)
    {
        Parallel.ForEach(File.ReadLines(filePath).AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount), (line) =>
        {
            var data = line.Split(';');
            var cityName = data[0];
            var temperature = (float) double.Parse(data[1]);
            cityMap.AddOrUpdate(cityName,
                _ => new City() { Min = temperature, Max = temperature, Count = 1, Sum = temperature },
                (_, existingCity) =>
                {
                    lock (existingCity)
                    {
                        existingCity.Count++;
                        existingCity.Max = temperature > existingCity.Max ? temperature : existingCity.Max;
                        existingCity.Min = temperature < existingCity.Min ? temperature : existingCity.Min;
                        existingCity.Sum += temperature;
                        return existingCity;
                    }
                });
        });
    }
    
    private static void CalculateMean(ConcurrentDictionary<string, City> cityMap )
    {
    
        Parallel.ForEach(cityMap.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount), pair =>
        {
            var city = pair.Value;
            var average = city.Sum / city.Count;
            city.Sum = average;
        });
    }
    
    private static List<string> OrderKeys(ICollection<string> keys)
    {
        return keys.AsParallel().AsOrdered().ToList();
    }
    
    private static void OutputFormatted(List<string> sortedKeys, ConcurrentDictionary<string, City> cityMap)
    {
        var sb = new StringBuilder("{");
        Parallel.ForEach(sortedKeys, key =>
        {
            string cityInfo = $"{key}={cityMap[key].Min}/{cityMap[key].Sum}/{cityMap[key].Max},";
            lock (sb)
            {
                sb.Append(cityInfo);
            }
        });
        sb.Length--;
        sb.Append("}");
        WriteFormatted("output.txt", sb.ToString());
    }
    
    private static async void WriteFormatted(string path, string output)
    {
        await File.WriteAllTextAsync(path, output);
    }

    private class City
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public float Sum { get; set; }
        public int Count { get; set; }
    }
}




