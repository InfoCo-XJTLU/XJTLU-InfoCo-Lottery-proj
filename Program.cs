using Lottery;
using Question;
using CsvHelper;

public class Program{

    private static void mainLoop(in List<QuestionEntry> questiondb){
    }

    public static int Main(string[] args)
    {
        var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
        var questionDatabase = baseDir + "cfg/questionDatabase.csv";
        var defaultPrizeDatabase = baseDir + "cfg/prizeDatabase.csv";

        var quiz = new QuestionEngine(questionDatabase);
        var lot = new LotteryEngine(defaultPrizeDatabase);

        LotteryEngine.DbgUtils.printDbgInfoPrizeDB(lot);
        for(var i = 1;i < 200; i ++){

            var val = lot.GenPrizeItem(
                lot.GenSumSet(
                    lot.AdjustRatio(
                        lot.GenUIDRatioDictByWeight(
                            lot.GenPrizeWeigt(
                                lot.GenSumSet(
                                    lot.AdjustRatio(
                                        lot.GenWeightRatioDict(
                                            false))))))));

            Console.WriteLine("{0}", val.Name);
            lot.DecreasePrize(val.UID);

            LotteryEngine.DbgUtils.printDbgInfoPrizePool(lot);
        }

        return 0;
    }
    }
