using Lottery;
using Question;
using CsvHelper;

public class Program{

    private static void initDB<T>(ref string dbpath,out T[] db){

        if(!File.Exists(dbpath)){
            Console.Error.WriteLine("Error. {0}", "No file called `(" + typeof(T).Name + ")Database.csv" + "' found." );
            string? tmppath = "Y";
            do{
                Console.WriteLine("Try specified another one? [Y/n]:");
                tmppath = Console.ReadLine();
                if(string.IsNullOrEmpty(tmppath)||string.IsNullOrWhiteSpace(tmppath)){
                    tmppath = "Y";
                }
                if(tmppath.Contains('Y')){
                    Console.Write("{0}", "Given a path:");
                    tmppath = Console.ReadLine();
                }else{
                    throw new Exception("Error: Expect a database with extension of csv.");
                }
                if(File.Exists(tmppath)&&new FileInfo(tmppath).Extension == ".csv"){
                    dbpath = tmppath;
                    break;
                }else{
                    Console.Error.WriteLine("Error. {0}","File not exist or not specified request.");
                }
            }while(true);
        }
        using (var reader = new StreamReader(dbpath))
        {
            using (var csvdbreader = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
                db=csvdbreader.GetRecords<T>().ToArray();
                Array.Sort(db);
            }
        }
    }

    private static void mainLoop(in List<QuestionEntry> questiondb){
    }

    public static int Main(string[] args)
    {
        var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
        var questionDatabase = baseDir + "cfg/questionDatabase.csv";
        var prizeDatabase = baseDir + "cfg/prizeDatabase.csv";
        QuestionEntry[] questiondb = null!;
        initDB<QuestionEntry>(ref questionDatabase,out questiondb);

        var lot = new LotteryEngine(prizeDatabase);
        Console.WriteLine("{0}", lot.GenPrizeItem(
                              LotteryEngine.GenSumSet(
                                  LotteryEngine.AdjustRatio(
                                      LotteryEngine.GenUIDRatioDict(
                                          lot.GetPrizeItemByWeight(
                                              LotteryEngine.GenPrizeWeigt(
                                                  LotteryEngine.GenSumSet(
                                                      LotteryEngine.AdjustRatio(lot)))))))).Name);
        // LotteryEngine.GenSumSet(lot.AdjustRatio(new Dictionary<int , int>{{9, 20},{-1 ,30}}.ToArray()));

        return 0;
    }
    }
