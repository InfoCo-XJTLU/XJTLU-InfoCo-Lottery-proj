using Lottery;
using Question;
using CsvHelper;

public class Program{

    private static LotteryEngine lottery= null!;
    private static QuestionEngine quiz = null!;

    private static void mainLoop(in List<QuestionEntry> questiondb){
    }

    private static void displayQuestions(){

        var itemsNum = quiz.GetWeights();
    }

    private static void genprize(Dictionary<int, int> adj){

        var val = lottery.GenPrizeItem(
            lottery.GenSumSet(
                lottery.AdjustRatio(
                    lottery.GenUIDRatioDictByWeight(
                        lottery.GenPrizeWeigt(
                            lottery.GenSumSet(
                                lottery.AdjustRatio(
                                    lottery.GenWeightRatioDict(
                                        false),
                                    adj)))))));

        lottery.DecreasePrize(val.UID);

        Console.WriteLine("{0}", val.Name);

        LotteryEngine.DbgUtils.printDbgInfoPrizePool(lottery);
    }

    public static int Main(string[] args)
    {
        var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
        var questionDatabase = baseDir + "cfg/questionDatabase.csv";
        var defaultPrizeDatabase = baseDir + "cfg/prizeDatabase.csv";

        quiz = new QuestionEngine(questionDatabase);
        lottery = new LotteryEngine(defaultPrizeDatabase);

        LotteryEngine.DbgUtils.printDbgInfoPrizeDB(lottery);
        LotteryEngine.DbgUtils.printDbgInfoPrizePool(lottery);


        return 0;
    }
    }
