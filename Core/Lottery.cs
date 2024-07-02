using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Diagnostics.CodeAnalysis;


namespace Lottery{

    public class PrizeItem: IComparable<PrizeItem>, IEquatable<PrizeItem>{

        // Begin: internal properties.
        ///<value> Property <c>totalItems</c> represents what number of item are in total. which could be used to generate UID.</value>
        private static uint totalItmes = 0;

        ///<value> Property <c>uid</c> is private property represents prize item's uid. With no accessibility to public.</value>
        [Ignore] private uint uid = 0;
        ///<value> Property <c>name</c> is private property represents prize item's name. With no accessibility to public.</value>
        [Ignore] private string name = null!;
        ///<value> Property <c>url</c> is private property represents prize item's image resource url. With no accessibility to public.</value>
        [Ignore] private string url = null!;
        ///<value> Property <c>weight</c> is private property represents prize item's weight, which is used to distinguish the value of the prize. With no accessibility to public.</value>
        [Ignore] private int weight = 0;
        ///<value> Property <c>number</c> is private property represents prize item's number in total, if it is turned to 0, it will not be generated. With no accessibility to public.</value>
        [Ignore] private int number = 0;
        ///<value> Property <c>number</c> is private property represents prize item's number in total, if it is turned to 0, it will not be generated. With no accessibility to public.</value>
        [Ignore] private int originalNumber = -1;
        ///<value> Property <c>uid</c> is private property represents prize item's uid. With no accessibility to public.</value>
        [Ignore] private bool infinity = false;
        ///<value> Property <c>uid</c> is private property represents prize item's uid. With no accessibility to public.</value>
        [Ignore] private int basicRatio = 0; // between 0~1000

        [Name("UID")] [Optional] public uint UID { get{ return uid; } set{ uid = value; } }
        [Name("Name")] [NullValues("Null")] public string Name { get => name; set => name = value; }
        [Name("Weight")] [NullValues("Null")] public int? Weight { [return: NotNull] get{ return weight; } set{ weight = (value??0) >= 0 ||value is null ? value.GetValueOrDefault(-1) : throw new Exception("Error: invalid input."); } }
        [Name("Number")] public int Num {
            get => number;
            set {
                number = value;
                originalNumber = -1 == originalNumber
                    ? value
                    : originalNumber;
                if(-1 == number){
                    infinity = true;
                }
            }
        }
        [Ignore] public int OriginalNumber{ get => originalNumber; }
        [Name("Url")] [Optional] [NullValues("Null")] public string Url { get => url; set => url = value; }
        [Name("Ratio")] [NullValues("Null")] [Default(0)] public int? Ratio {
            get => basicRatio;
            set{
                if(null == value){
                    basicRatio = -1;
                }else{
                    basicRatio = (value <= 1000 && value >= 0)
                        ? value.GetValueOrDefault(-1)
                        : throw new Exception("Error: ratio out of range.");
                }
            }
        }
        [Ignore] public bool Infinity{ get => infinity; }
        // End internal properties.

        public int CompareTo(PrizeItem? obj){ return this.weight.CompareTo((obj?? new PrizeItem()).weight); }

        public override bool Equals(object? obj){

            if(obj == null){
                return false;
            }
            PrizeItem objIns = (obj as PrizeItem)!;
            if(objIns == null){
                return false;
            }else{
                return Equals(objIns);
            }
        }
        public bool Equals(PrizeItem? other){

            if(other == null){
                return false;
            }
            return(this.uid.Equals(other.uid));
        }

        public override int GetHashCode(){

            return System.Convert.ToInt32(uid);
        }

        public PrizeItem(){}
        public PrizeItem(string name, int weight, int number, int basicRatio = 0){

            this.uid = ++PrizeItem.totalItmes;
            this.name = name;
            this.weight = weight;
            this.number = number;
            this.infinity = (number>=0)?false:true;
        }

        public void init(){

            uid = (uid == 0)
                ? ++PrizeItem.totalItmes
                : uid;
            infinity = !(number>=0);
            if(infinity && weight != -1){
                Utils.ConsoleWrapper.WriteError();
                Console.Error.WriteLine("{0}", "Weight should be Null if the Number of prize item is infinity.");
                throw new Exception("Error: Weight expect Null value.");
            }
        }

        public void DecreaseNum(){

            if(infinity){
                return;
            }
            if(number == 0){
                throw new Exception("Error: Number of the prize is 0! Cannot deliver now.");
            }
            number --;
        }
        // Obviously, it could be composed into only one function
        public void DecreaseNum(int num){

            if(infinity){
                return;
            }
            if(number == num - 1){
                throw new Exception("Error: Left prize is insufficient.");
            }
            number -= num;
            return;
        }

    }

    public class LotteryEngine {

        private string dbpath = null!;
        private Dictionary<uint, PrizeItem> prizedb = new Dictionary<uint, PrizeItem>(); // uid, prize item
        // private Dictionary<int, int> simpleRatioPool = new Dictionary<int, int>();// weight, ratio sum
        private Dictionary<int, uint[]> prizePool = new Dictionary<int, uint[]>();// weight, prize item uid list, weight to -1, infinity prize item list

        public LotteryEngine(string dbpath){

            this.dbpath = dbpath;
            List<PrizeItem> tmpdb;
            Utils.PathTool.initDB(ref this.dbpath, out tmpdb);
            tmpdb.Sort();
            tmpdb.Reverse();
            foreach(var val in tmpdb){
                val.init();
                prizedb.Add(val.UID, val);
            }
            if(!prizedb.Values.Any(x=> x.Infinity)){
                throw new Exception("Error: Expect at least one infinity item.");
            }

            initPrizePool();

            var totalRatio = GenWeightRatioDict().Select( x => x.Value).Sum();
            if(totalRatio > 1000){
                Utils.ConsoleWrapper.WriteError();
                Console.WriteLine("{0}", "the total ratio is over 1000%o!");
                Utils.ConsoleWrapper.WriteError();
                Console.WriteLine("{0}", "Try select another");
                this.dbpath = "";
                Utils.PathTool.initDB(ref this.dbpath, out tmpdb);
            }

        }

        public static class DbgUtils{

            internal static void printDbgInfoPrizeDB(in LotteryEngine obj){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "------- Begin Dbg info of prize db -------");
                Utils.ConsoleWrapper.WriteDbg();
                foreach(var val in typeof(PrizeItem).GetProperties().ToList()){
                    Console.Write("{0} ", val.Name);
                }
                Console.WriteLine("");
                foreach(var val in obj.prizedb){
                    if(val.Value == null){
                        break;
                    }
                    Utils.ConsoleWrapper.WriteDbg();
                    foreach(var j in val.Value.GetType().GetProperties()){
                        if(val.Value == null){
                            break;
                        }
                        Console.Write("{0} ", j.GetValue(val.Value));
                    }
                    Console.WriteLine("");
                }
                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "------- End Dbg info of prize db -------");
            }

            internal static void printDbgInfoPrizePool(in LotteryEngine obj){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "------- Begin Dbg info of prizePool -------");
                foreach(var val in obj.prizePool){
                    Utils.ConsoleWrapper.WriteDbg();
                    Console.Write("Weight {0}: Prize Item UID {1}: \n", val.Key, val.Value.Count());
                    foreach(var v in val.Value){
                        Utils.ConsoleWrapper.WriteDbg();
                        Console.Write("| ");
                        Console.Write("{0}","".PadLeft(@"Weight : ".Length - 1));
                        foreach(var j in obj.prizedb[v].GetType().GetProperties()){
                            Console.Write("{1}:{0}, ", j.GetValue(obj.prizedb[v]), j.Name);
                        }
                        Console.WriteLine("");
                    }
                }
                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "------- End Dbg info of prizePool -------");
            }

            internal static void printList<T>(in List<T> lst){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "----------- Begin dbg info of lst ------------");
                foreach(var val in lst){
                    Utils.ConsoleWrapper.WriteDbg();
                    Console.WriteLine("{0}", val);
                }
                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "----------- End dbg info of lst ------------");
            }

            internal static void printDict(in Dictionary<int, int> dict){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "----------- Begin dbg info of dict ------------");
                foreach(var val in dict){
                    Utils.ConsoleWrapper.WriteDbg();
                    Console.WriteLine("Key {0}: Value {1}", val.Key, val.Value);
                }
                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "----------- End dbg info of lst ------------");
            }
            internal static void printDict(in Dictionary<uint, int> dict){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "----------- Begin dbg info of dict ------------");
                foreach(var val in dict){
                    Utils.ConsoleWrapper.WriteDbg();
                    Console.WriteLine("Key {0}: Value {1}", val.Key, val.Value);
                }
                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "----------- End dbg info of lst ------------");
            }

            internal static void printInt(in int val){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("Value {0}", val);
            }
        }

        private void initPrizePool(){

            foreach(var val in prizedb.GroupBy(x => x.Value.Weight ?? -1).ToArray()){
                // simpleRatioPool.Add(val.Key, val.ToArray().Select(x => x.Value.Num > 0 ? x.Value.Ratio * x.Value.Num : 0).Sum() ?? 0);
                prizePool.Add(val.Key, (from v in val
                                        select v.Value.UID).ToArray());
            }

            return;
        }

        public void DecreasePrize(uint uid){

            try{
                prizedb[uid].DecreaseNum();
            }catch{
                Utils.ConsoleWrapper.WriteError();
                Console.Error.WriteLine("{0}", "Insufficient prize");
            }

            return;
        }

        public Dictionary<int, int> GenWeightRatioDict(bool decreaseAsNumChange = false){
            var dict = new Dictionary<int, int>();
            foreach(var val in prizedb.GroupBy(x => x.Value.Weight ?? -1).ToArray()){
                dict.Add(val.Key, val.ToArray().Select(x => x.Value.Num > 0 ? x.Value.Ratio * (decreaseAsNumChange ? x.Value.Num : x.Value.OriginalNumber) : 0).Sum() ?? 0);
            }
            return dict;
        }

        ///<summary>
        ///Method <c>GenUIDRatioDictByWeight</c> returns a subset of prizedb
        ///</summary>
        ///<param name="weight">the weight of looking for items.</param>
        ///<returns>A dictionary of UID and PrizeItem. </returns>
        public Dictionary<uint, int> GenUIDRatioDictByWeight(int weight){

            var dict = new Dictionary<uint, int>();
            foreach(var val in prizePool[weight]){
                if(prizedb[val].Num > 0 || prizedb[val].Infinity){
                    dict.Add(prizedb[val].UID, prizedb[val].Ratio??0);
                }
            }
            return dict;
        }

        ///<summary>
        ///Method <c>AdjustRatio</c> adjust ratio pool according to parameters.
        ///</summary>
        ///<param name="dict">the dict to be adjust, <weight, ratio>.</param>
        ///<param name="prize">the adjusting list, <weight, ratio>.</param>
        ///<returns>A dictionary of weight and ratio. </returns>
        public Dictionary<int, int> AdjustRatio(Dictionary<int, int> dict,Dictionary<int, int> prize){

            var adjustedDict = new Dictionary<int, int>(dict.Where(x => x.Value > 0 || x.Key == -1).ToDictionary());
            foreach(var val in prize){
                if(val.Key == -1){
                    throw new Exception("Error: Cannot adjust ratio of infinity object.");
                }
                int va = adjustedDict[val.Key] + val.Value;
                if(va <= 0){
                    adjustedDict[val.Key] = 0;
                }else{
                    adjustedDict[val.Key] = va;
                }
            }
            return adjustedDict;
        }

        ///<summary>
        ///Method <c>AdjustRatio</c> adjust ratio pool according to parameters.
        ///</summary>
        ///<param name="dict">the dict to be adjust, <weight, ratio>.</param>
        ///<param name="prize">the adjusting list, <weight, ratio>.</param>
        ///<returns>A dictionary of weight and ratio. </returns>
        public Dictionary<int, int> AdjustRatio(Dictionary<int, int>dict, params KeyValuePair<int, int>[] prize){

            var adjustedDict = new Dictionary<int, int>(dict.Where(x => x.Value > 0 || -1 == x.Key).ToDictionary());
            foreach(var val in prize){
                if(val.Key == -1){
                    throw new Exception("Error: Cannot adjust ratio of infinity object.");
                }
                int va = adjustedDict[val.Key] + val.Value;
                if(va <= 0){
                    adjustedDict[val.Key] = 0;
                }else{
                    adjustedDict[val.Key] = va;
                }
            }
            return adjustedDict;
        }

        ///<summary>
        ///Method <c>AdjustRatio</c> adjust ratio pool according to parameters.
        ///</summary>
        ///<param name="ratioPool">the operating object, <uid, ratio>.</param>
        ///<param name="prize">the adjusting list, <uid, ratio>.</param>
        ///<returns>A dictionary of UID and ratio. </returns>
        public Dictionary<uint, int> AdjustRatio(Dictionary<uint, int> ratioPool, Dictionary<uint, int> prize){

            var adjustedDict = new Dictionary<uint, int>(ratioPool.Where(x => prizedb[x.Key].Num > 0 || prizedb[x.Key].Infinity).ToDictionary());
            foreach(var val in prize){
                int va = adjustedDict[val.Key] + val.Value;
                if(va <= 0){
                    adjustedDict[val.Key] = 0;
                }else{
                    adjustedDict[val.Key] = va;
                }
            }

            return adjustedDict;
        }

        ///<summary>
        ///Method <c>AdjustRatio</c> adjust ratio pool according to parameters.
        ///</summary>
        ///<param name="ratioPool">the operating object, <uid, ratio>.</param>
        ///<param name="prize">the adjusting list, <uid, ratio>.</param>
        ///<returns>A dictionary of UID and ratio. </returns>
        public Dictionary<uint, int> AdjustRatio(Dictionary<uint, int> ratioPool, params KeyValuePair<uint, int>[] prize){

            var adjustedDict = new Dictionary<uint, int>(ratioPool.Where(x => prizedb[x.Key].Num > 0 || prizedb[x.Key].Infinity).ToDictionary());
            foreach(var val in prize){
                int va = adjustedDict[val.Key] + val.Value;
                if(va <= 0){
                    adjustedDict[val.Key] = 0;
                }else{
                    adjustedDict[val.Key] = va;
                }
            }

            return adjustedDict;
        }

        ///<summary>
        ///Method <c>GenSumSet</c> Generate a sumset of given object.
        ///</summary>
        ///<param name="prize">the adjusting list.</param>
        ///<returns>A dictionary of weight and 累加和. </returns>
        public Dictionary<int, int> GenSumSet(Dictionary<int, int> prize){

            var prizeSumSet = new Dictionary<int, int>(prize.Count());
            var tmp = 0;
            foreach(var val in prize){
                tmp += val.Value;
                if(-1 == val.Key){
                    prizeSumSet.Add(val.Key, 0);
                }else{
                    prizeSumSet.Add(val.Key, tmp);
                }
            }

            return prizeSumSet;
        }

        ///<summary>
        ///Method <c>GenSumSet</c> Generate a sumset of given object.
        ///</summary>
        ///<param name="prize">the adjusting list.</param>
        ///<returns>A dictionary of weight and 累加和. </returns>
        public Dictionary<uint, int> GenSumSet(Dictionary<uint, int> prize){

            var prizeSumSet = new Dictionary<uint, int>(prize.Count());
            var sum = prize.Values.Sum();
            var tmp = 0;
            foreach(var val in prize){
                tmp += val.Value*100/sum;
                prizeSumSet.Add(val.Key, tmp);
            }

            return prizeSumSet;
        }

        ///<summary>
        ///Method <c>GenSumSet</c> Generate a sumset of given object.
        ///</summary>
        ///<param name="prize">the adjusting list of weight and sum value.</param>
        ///<returns>prize weight. </returns>
        public int GenPrizeWeigt(Dictionary<int, int> prize){

            var rann = Utils.RandomGenerator.GenRandom()*10;

            // --------------------------------------------------------
            Utils.ConsoleWrapper.WriteDbg();
            Console.WriteLine("{0}", "sumset(weight)");
            Utils.ConsoleWrapper.WriteDbg();
            Console.WriteLine("{0}", "weight/ratio(in 1000%o)");
            DbgUtils.printDict(prize);
            DbgUtils.printInt(rann);
            // --------------------------------------------------------

            foreach(var val in prize){
                if(-1 == val.Key){
                    return val.Key;
                }
                if(rann < val.Value){
                    return val.Key;
                }
            }
            return -1;
        }

        public PrizeItem GenPrizeItem(Dictionary<uint, int> prize){

            var rann = Utils.RandomGenerator.GenRandom();

            // ------------------------------------------------------------------------------------
            Utils.ConsoleWrapper.WriteDbg();
            Console.WriteLine("{0}", "sumset(item)");
            Utils.ConsoleWrapper.WriteDbg();
            Console.WriteLine("{0}", "uid/ratio(in 100%)");
            DbgUtils.printDict(prize);
            DbgUtils.printInt(rann);
            // ------------------------------------------------------------------------------------

            foreach(var val in prize){
                if(rann <= val.Value){
                    return prizedb[val.Key];
                }
            }

            return new PrizeItem();
        }

    }//!END: LotteryEngine

}
