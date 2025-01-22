using System.Runtime.CompilerServices;

namespace Bureau.PoC.CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string one = "something";
            string two = "something";
            T first = new T(55);
            R oneR = new R("something", 4, first);
            R twoR = new R("something", 4, new T(55));

            if (oneR.GetHashCode() == twoR.GetHashCode())
            {
                Console.WriteLine("The same");
            }
            else 
            {
                Console.WriteLine("Not the same");
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://keep.googleapis.com");

            //HttpResponseMessage response = await client.GetAsync("/$discovery/rest?version=v1");
            //Console.WriteLine($"{await response.Content.ReadAsStringAsync()}");

            
            HttpResponseMessage response = await client.GetAsync("/v1/notes");
            Console.WriteLine($"{await response.Content.ReadAsStringAsync()}");

            List<T> list = new List<T>()
        {
            new T(3),
            new T(6)
        };

            Dictionary<int, Action<string>> dict = new Dictionary<int, Action<string>>();
            int i = 0;
            foreach (T item in list)
            {
                Console.WriteLine(list[i].V);

                dict.Add(i, s => {
                    Console.WriteLine("{0} {1}", item.V, s);
                });
                i++;
            }
            list = null;
            //GC.Collect();
            //Thread.Sleep(5000);
            izvrsiNes(dict);
        }

        public static void izvrsiNes(Dictionary<int, Action<string>> dict)
        {
            dict[0]("Prvi");
            dict[1]("Drugi");
        }
        public record R(string what, int when, T yes);
        public class T
        {
            public int V { get; set; }
            public T(int v)
            {
                V = v;
            }
        }

    }
}
