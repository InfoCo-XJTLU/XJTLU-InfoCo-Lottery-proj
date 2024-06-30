using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Diagnostics.CodeAnalysis;


namespace Lottery{

    public class PrizeItem: IComparable<PrizeItem>, IEquatable<PrizeItem>{

        private static uint totalItmes = 0;
        [Ignore] private uint uid = 0;
        [Ignore] private string name = null!;
        [Ignore] private string url = null!;
        [Ignore] private int weight = 0;
        [Ignore] private int number = 0;
        [Ignore] private bool infinity = false;
        [Ignore] private int basicRatio = 0; // between 0~1000

        [Name("UID")] [Optional] public uint UID {
        get{ return uid; }
        set{ uid = value; }
        }
        [Name("name")] [NullValues("Null")] public string Name {
        get => name;
        set => name = value;
        }
        [Name("weight")] [NullValues("Null")] public int? Weight {
        [return: NotNull] get{ return weight; }
        set{
            if(value is null){
                weight = -1;
            }else{
                weight = (value ?? 0) >= 0
                    ? value ?? 0
                    : throw new Exception("Error: weight must greater than or equal to 0.");
            }
        }
        }
        [Name("number")] public int Num {
        get => number;
        set {
            number = value;
            if(-1 == number){
                infinity = true;
            }
        }
        }
        [Name("url")] [Optional] [NullValues("Null")] public string Url {
        get => url;
        }
        [Name("ratio")] [NullValues("Null")] [Default(0)] public int? Ratio {
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

        public int CompareTo(PrizeItem? obj){

            return this.weight.CompareTo((obj?? new PrizeItem()).weight);
        }

        public override bool Equals(object? obj){

            if(obj == null){
                return false;
            }
            PrizeItem objIns = (obj as PrizeItem)!;
            if(objIns == null){
                return false;
            }
            else{
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
            this.basicRatio = (number <= 0)?-1:basicRatio;
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

            if(number == 0){
                throw new Exception("Error: Number of the prize is 0! Cannot deliver now.");
            }
            number --;
        }
        // Obviously, it could be composed into only one function
        public void DecreaseNum(int num){

            if(number == num-1){
                throw new Exception("Error: Left prize is insufficient.");
            }
            number -= num;
            return;
        }

    }

    public class LotteryEngine {

        private string dbpath = null!;
        private Dictionary<uint, PrizeItem> prizedb = new Dictionary<uint, PrizeItem>(); // uid, prize item
        private Dictionary<int, int> simpleRatioPool = new Dictionary<int, int>();// weight, ratio sum
        private Dictionary<int, uint[]> prizePool = new Dictionary<int, uint[]>();// weight, prize item uid list, weight to -1, infinity prize item list
        private int totalRatio;

        public LotteryEngine(string dbpath){

            this.dbpath = dbpath;
            // initDB(ref this.dbpath); //used to use.
            List<PrizeItem> tmpdb;
            Utils.PathTool.initDB(ref this.dbpath, out tmpdb);
            tmpdb.Sort();
            tmpdb.Reverse();
            foreach(var val in tmpdb){
                val.init();
                prizedb.Add(val.UID, val);
            }
            if(!prizedb.Values.Any(x=> -1 == x.Weight)){
                throw new Exception("Error: Expect at least one infinity item.");
            }

            initPrizePool();
            totalRatio = simpleRatioPool.Select( x => x.Value).Sum();
            if(totalRatio > 1000){
                Utils.ConsoleWrapper.WriteError();
                Console.WriteLine("{0}", "the total ratio is over 1000%o!");
                Utils.ConsoleWrapper.WriteError();
                Console.WriteLine("{0}", "Try select another");
                this.dbpath = "";
                initDB(ref this.dbpath);
            }

            DbgUtils.printDbgInfoPrizeDB(this);
            DbgUtils.printDbgInfoSimpleRatioPool(this);
            DbgUtils.printDbgInfoPrizePool(this);

        }

        private static class DbgUtils{

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

            internal static void printDbgInfoSimpleRatioPool(in LotteryEngine obj){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "------- Begin Dbg info of SimpleRatioPool -------");
                foreach(var val in obj.simpleRatioPool){
                    Utils.ConsoleWrapper.WriteDbg();
                    Console.WriteLine("Weight {0}: Ratio {1}", val.Key, val.Value);
                }
                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "------- End Dbg info of SimpleRatioPool -------");

            }

            internal static void printDbgInfoPrizePool(in LotteryEngine obj){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("{0}", "------- Begin Dbg info of prizePool -------");
                foreach(var val in obj.prizePool){
                    Utils.ConsoleWrapper.WriteDbg();
                    Console.Write("Weight {0}: Prize Item UID {1}: ", val.Key, val.Value.Count());
                    foreach(var v in val.Value){
                        foreach(var j in obj.prizedb[v].GetType().GetProperties()){
                            Console.Write("{1}:{0}, ", j.GetValue(obj.prizedb[v]), j.Name);
                        }
                        Console.Write(" | ");
                    }
                    Console.WriteLine("");
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

            internal static void printInt(in int val){

                Utils.ConsoleWrapper.WriteDbg();
                Console.WriteLine("Value {0}", val);
            }
        }

        private void initDB(ref string dbpath){
            //Obsoleted

            Utils.PathTool.TryGetPath(ref dbpath, ".csv");

            // Useless, but to prevent compiler error
            // Provide a default empty value;
            prizedb = new Dictionary<uint, PrizeItem>();
            List<PrizeItem> tmp = new List<PrizeItem>();

            using (var reader = new StreamReader(dbpath))
            {
                using (var csvdbreader = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
                    bool flag = true;
                    do{
                        try{
                            tmp = csvdbreader.GetRecords<PrizeItem>().ToList();
                            flag = false;
                        }catch{
                            Utils.ConsoleWrapper.WriteError();
                            Console.Error.WriteLine("{0}","Not a csv or file dose not match the request.");
                            dbpath = "";
                            Utils.PathTool.TryGetPath(ref dbpath, ".csv");
                            flag = true;
                        }finally{
                        }
                    }while(flag);

                    var modinfo = typeof(PrizeItem).GetMethod("init");
                    if(null != modinfo){
                        foreach(var val in tmp){
                            modinfo.Invoke(val, null);
                        }
                    }

                    foreach(var val in tmp){
                        prizedb.Add(val.UID, val);
                    }
                }
            }
        }

        private void initPrizePool(){

            foreach(var val in prizedb.GroupBy(x => x.Value.Weight ?? -1).ToArray()){
                simpleRatioPool.Add(val.Key, val.ToArray().Select(x => x.Value.Num > 0 ? x.Value.Ratio * x.Value.Num : 0).Sum() ?? 0);
                prizePool.Add(val.Key, (from v in val
                                        select v.Value.UID).ToArray());
            }

            return;
        }

        public void DecreasePrize(uint uid, bool decreaseRatio = false){

            try{
                prizedb[uid].DecreaseNum();
            }catch{
                Utils.ConsoleWrapper.WriteError();
                Console.Error.WriteLine("{0}", "In sufficient prize");
            }
            simpleRatioPool[prizedb[uid].Weight??0] -= decreaseRatio ? prizedb[uid].Ratio ?? 0 : 0;

            return;
        }

        ///<summary>
        ///Method <c>GetPrizeItemByWeight</c> returns a subset of prizedb
        ///</summary>
        ///<param name="weight">the weight of looking for items.</param>
        ///<returns>A dictionary of UID and PrizeItem. </returns>
        public Dictionary<uint, PrizeItem> GetPrizeItemByWeight(int weight){

            var dict = new Dictionary<uint, PrizeItem>();
            foreach(var val in prizePool[weight]){
                dict.Add(prizedb[val].UID, prizedb[val]);
            }
            return dict;
        }

        public static Dictionary<uint, int> GenUIDRatioDict(Dictionary<uint, PrizeItem> prize){

            var dict = new Dictionary<uint, int>();
            foreach(var val in prize){
                dict.Add(val.Key, val.Value.Ratio??0);
            }
            return dict;
        }

        ///<summary>
        ///Method <c>AdjustRatio</c> adjust ratio pool according to parameters.
        ///</summary>
        ///<param name="obj">the operating object.</param>
        ///<param name="prize">the adjusting list.</param>
        ///<returns>A dictionary of weight and ratio. </returns>
        public static Dictionary<int, int> AdjustRatio(LotteryEngine obj, Dictionary<int, int> prize){

            var adjustedDict = new Dictionary<int, int>(obj.simpleRatioPool);
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
        ///<param name="obj">the operating object.</param>
        ///<param name="prize">the adjusting list.</param>
        ///<returns>A dictionary of weight and ratio. </returns>
        public static Dictionary<int, int> AdjustRatio(LotteryEngine obj, params KeyValuePair<int, int>[] prize){

            var adjustedDict = new Dictionary<int, int>(obj.simpleRatioPool);
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
        ///<param name="ratioPool">the operating object.</param>
        ///<param name="prize">the adjusting list.</param>
        ///<returns>A dictionary of UID and ratio. </returns>
        public static Dictionary<uint, int> AdjustRatio(Dictionary<uint, int> ratioPool, Dictionary<uint, int> prize){

            var adjustedDict = new Dictionary<uint, int>(ratioPool);
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
        ///<param name="ratioPool">the operating object.</param>
        ///<param name="prize">the adjusting list.</param>
        ///<returns>A dictionary of UID and ratio. </returns>
        public static Dictionary<uint, int> AdjustRatio(Dictionary<uint, int> ratioPool, params KeyValuePair<uint, int>[] prize){

            var adjustedDict = new Dictionary<uint, int>(ratioPool);
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
        public static Dictionary<int, int> GenSumSet(Dictionary<int, int> prize){

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
            DbgUtils.printDict(prizeSumSet);
            return prizeSumSet;
        }

        ///<summary>
        ///Method <c>GenSumSet</c> Generate a sumset of given object.
        ///</summary>
        ///<param name="prize">the adjusting list.</param>
        ///<returns>A dictionary of weight and 累加和. </returns>
        public static Dictionary<uint, int> GenSumSet(Dictionary<uint, int> prize){

            var prizeSumSet = new Dictionary<uint, int>(prize.Count());
            var sum = prize.Values.Sum();
            foreach(var val in prize){
                prizeSumSet.Add(val.Key, val.Value*100/sum);
            }
            return prizeSumSet;
        }

        public static int GenPrizeWeigt(Dictionary<int, int> prize){

            var rann = Utils.RandomGenerator.GenRandom()*10;
            DbgUtils.printInt(rann);

            foreach(var val in prize){
                if(-1 == val.Key){
                    return val.Key;
                }
                if(rann <= val.Value){
                    return val.Key;
                }
            }
            return -1;
        }

        public PrizeItem GenPrizeItem(Dictionary<uint, int> prize){

            var rann = Utils.RandomGenerator.GenRandom();
            DbgUtils.printInt(rann);

            foreach(var val in prize){
                if(rann <= val.Value){
                    return prizedb[val.Key];
                }
            }

            return new PrizeItem();
        }

    }//!END: LotteryEngine

    /*
      public class LotteryStructure{

      private Prize[] PrizePool { get; set; } = null!;
      private List<int> AdjustedRatioPool { set; get; } = new List<int>();
      private int FullRatio { set; get; } = 0;
      public LotteryStructure(ref string dbpath){
      initDB<Prize>(ref dbpath, out PrizePool);
      }

      public void InitPrizePool(in Prize[] prizedb){
            
            var total = prizedb.Length;
            for(var i = 0; i < total; i++){
                PrizePool.Append(
                    -1 == prizedb[i].Number
                    ? prizedb[i].BasicRatio
                    : prizedb[i].BasicRatio*prizedb[i].Number
                );
            }
            foreach(var val in prizedb){
                int tmp = 0;
                int lastweight = -1;
                if(lastweight != val.Weight){
                    AdjustedRatioPool.Append(tmp);
                    tmp = val.BasicRatio;
                    lastweight=val.Weight;
                }else{
                    tmp += val.BasicRatio;
                }
            }
            FullRatio = PrizePool.Sum();
            
            // TODO: delete here
            foreach(var val in PrizePool){
                Console.WriteLine("{0}",val);
            }
            Console.WriteLine();
            foreach(var val in AdjustedRatioPool){
                Console.WriteLine("{0}",val);
            }
            
            return;
        }

        void DecreasePrize(ref Prize[] prizedb,int prize){
            if(0 == (--prizedb[prize].Number)){
                PrizePool[prize] = 0;
            }
            PrizePool[prize] = prizedb[prize].BasicRatio*prizedb[prize].Number;
            return;
        }
        
        string DisplayPrize(ref Prize[] prizedb, int prize){
            Console.WriteLine("{0}",prizedb[prize].PrizeName);
            DecreasePrize(ref prizedb, prize);
            return prizedb[prize].PrizeName;
        }
        
        void AdjustRatio(int weight, int ratio){
            int va = AdjustedRatioPool[weight] + ratio;
            if(va <= 0){
                AdjustedRatioPool[weight] = 0;
            }else{
                AdjustedRatioPool[weight] = va;
            }
            FullRatio = AdjustedRatioPool.Sum();
            return;
        }
        
        void GeneratePrize(ref Prize[] prizedb){
            var ran = new RandomGenerator();
        }
    }
    */
}
