using CSV;
using Lottery;

namespace Program{
    public class Program{
        private static Csv questions = new Csv();

        public static void Main(string[] args)
        {
            var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            //Console.WriteLine(baseDir);
            questions.ReadCSV(baseDir+"cfg/questions.csv");

            var ran = new RandomGenerator();

            return;
        }
    }
}
