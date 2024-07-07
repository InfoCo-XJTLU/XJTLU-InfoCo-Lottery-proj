using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LotteryCore {

  public class PrizeItem : IComparable<PrizeItem>, IEquatable<PrizeItem> {

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

    [Name("UID")][CsvHelper.Configuration.Attributes.Optional] public uint UID { get { return uid; } set { uid = value; } }
    [Name("Name")][NullValues("Null")] public string Name { get => name; set => name = value; }
    [Name("Weight")][NullValues("Null")] public int? Weight { [return: NotNull] get { return weight; } set { weight = (value ?? 0) >= 0 || value is null ? value.GetValueOrDefault(-1) : throw new Exception("Error: invalid input."); } }

    [Name("Number")]
    public int Num {
      get => number;
      set {
        number = value;
        originalNumber = -1 == originalNumber
            ? value
            : originalNumber;
        if (-1 == number) {
          infinity = true;
        }
      }
    }

    [Ignore] public int OriginalNumber { get => originalNumber; }
    [Name("Url")][CsvHelper.Configuration.Attributes.Optional][NullValues("Null")] public string Url { get => url; set => url = value; }

    [Name("Ratio")]
    [NullValues("Null")]
    [Default(0)]
    public int? Ratio {
      get => basicRatio;
      set {
        if (null == value) {
          basicRatio = -1;
        } else {
          basicRatio = (value <= 1000 && value >= 0)
              ? value.GetValueOrDefault(-1)
              : throw new Exception("Error: ratio out of range.");
        }
      }
    }

    [Ignore] public bool Infinity { get => infinity; }
    // End internal properties.

    public int CompareTo(PrizeItem? obj) {
      return this.weight.CompareTo((obj ?? new PrizeItem()).weight);
    }

    public override bool Equals(object? obj) {
      if (obj == null) {
        return false;
      }
      PrizeItem objIns = (obj as PrizeItem)!;
      if (objIns == null) {
        return false;
      } else {
        return Equals(objIns);
      }
    }

    public bool Equals(PrizeItem? other) {
      if (other == null) {
        return false;
      }
      return (this.uid.Equals(other.uid));
    }

    public override int GetHashCode() {
      return System.Convert.ToInt32(uid);
    }

    public PrizeItem() {
    }

    public PrizeItem(string name, int weight, int number) {
      this.uid = ++PrizeItem.totalItmes;
      this.name = name;
      this.weight = weight;
      this.number = number;
      this.infinity = number < 0;
    }

    public void Init() {
      uid = (uid == 0)
          ? ++PrizeItem.totalItmes
          : uid;
      infinity = !(number >= 0);
      if (infinity && weight != -1) {
        LotteryCore.ConsoleWrapper.WriteError();
        Console.Error.WriteLine("{0}", "Weight should be Null if the Number of prize item is infinity.");
        throw new Exception("Error: Weight expect Null value.");
      }
    }

    public void DecreaseNum() {
      if (infinity) {
        return;
      }
      if (number == 0) {
        throw new Exception("Error: Number of the prize is 0! Cannot deliver now.");
      }
      number--;
    }

    // Obviously, it could be composed into only one function
    public void DecreaseNum(int num) {
      if (infinity) {
        return;
      }
      if (number == num - 1) {
        throw new Exception("Error: Left prize is insufficient.");
      }
      number -= num;
      return;
    }
  }

  public class LotteryEngine {
    private readonly string dbpath = null!;
    private readonly Dictionary<uint, PrizeItem> prizedb = []; // uid, prize item
    private readonly Dictionary<int, uint[]> prizePool = [];// weight, prize item uid list, weight to -1, infinity prize item list
    public Dictionary<uint, PrizeItem> Prizedb { get => prizedb; }

    public LotteryEngine(string dbpath) {
      this.dbpath = dbpath;
      LotteryCore.PathTool.IinitDB(ref this.dbpath, out List<PrizeItem> tmpdb);
      tmpdb.Sort();
      tmpdb.Reverse();
      foreach (var val in tmpdb) {
        val.Init();
        prizedb.Add(val.UID, val);
      }
      if (!prizedb.Values.Any(x => x.Infinity)) {
        throw new Exception("Error: Expect at least one infinity item.");
      }

      InitPrizePool();

      var totalRatio = GenWeightRatioDict().Select(x => x.Value).Sum();
      if (totalRatio > 1000) {
        LotteryCore.ConsoleWrapper.WriteError();
        Console.WriteLine("{0}", "the total ratio is over 1000%o!");
        LotteryCore.ConsoleWrapper.WriteError();
        Console.WriteLine("{0}", "Try select another");
        this.dbpath = "";
        LotteryCore.PathTool.IinitDB(ref this.dbpath, out tmpdb);
      }
    }

    public PrizeItem this[uint uid] {
      get => prizedb[uid];
    }

    public PrizeItem[] this[int weight] {
      get => [.. prizedb.Where(x => x.Value.Weight == weight)
        .Select(x => x.Value)
        .ToArray()];
    }

    public static class DbgUtils {

      internal static void PrintDbgInfoPrizeDB(in LotteryEngine obj) {
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "------- Begin Dbg info of prize db -------");
        LotteryCore.ConsoleWrapper.WriteDebug();
        foreach (var val in typeof(PrizeItem).GetProperties().ToList()) {
          Console.Write("{0} ", val.Name);
        }
        Console.WriteLine("");
        foreach (var val in obj.prizedb) {
          if (val.Value == null) {
            break;
          }
          LotteryCore.ConsoleWrapper.WriteDebug();
          foreach (var j in val.Value.GetType().GetProperties()) {
            if (val.Value == null) {
              break;
            }
            Console.Write("{0} ", j.GetValue(val.Value));
          }
          Console.WriteLine("");
        }
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "------- End Dbg info of prize db -------");
      }

      internal static void PrintDbgInfoPrizePool(in LotteryEngine obj) {
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "------- Begin Dbg info of prizePool -------");
        foreach (var val in obj.prizePool) {
          LotteryCore.ConsoleWrapper.WriteDebug();
          Console.Write("Weight {0}: Prize Item UID {1}: \n", val.Key, val.Value.Length);
          foreach (var v in val.Value) {
            LotteryCore.ConsoleWrapper.WriteDebug();
            Console.Write("| ");
            Console.Write("{0}", "".PadLeft(@"Weight : ".Length - 1));
            foreach (var j in obj.prizedb[v].GetType().GetProperties()) {
              Console.Write("{1}:{0}, ", j.GetValue(obj.prizedb[v]), j.Name);
            }
            Console.WriteLine("");
          }
        }
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "------- End Dbg info of prizePool -------");
      }

      internal static void PrintList<T>(in List<T> lst) {
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "----------- Begin dbg info of lst ------------");
        foreach (var val in lst) {
          LotteryCore.ConsoleWrapper.WriteDebug();
          Console.WriteLine("{0}", val);
        }
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "----------- End dbg info of lst ------------");
      }

      internal static void PrintDict(in Dictionary<int, int> dict) {
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "----------- Begin dbg info of dict ------------");
        foreach (var val in dict) {
          LotteryCore.ConsoleWrapper.WriteDebug();
          Console.WriteLine("Key {0}: Value {1}", val.Key, val.Value);
        }
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "----------- End dbg info of lst ------------");
      }

      internal static void PrintDict(in Dictionary<uint, int> dict) {
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "----------- Begin dbg info of dict ------------");
        foreach (var val in dict) {
          LotteryCore.ConsoleWrapper.WriteDebug();
          Console.WriteLine("Key {0}: Value {1}", val.Key, val.Value);
        }
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("{0}", "----------- End dbg info of lst ------------");
      }

      internal static void PrintInt(in int val) {
        LotteryCore.ConsoleWrapper.WriteDebug();
        Console.WriteLine("Value {0}", val);
      }
    }

    private void InitPrizePool() {
      foreach (var val in prizedb.GroupBy(x => x.Value.Weight ?? -1).ToArray()) {
        // simpleRatioPool.Add(val.Key, val.ToArray().Select(x => x.Value.Num > 0 ?
        // x.Value.Ratio * x.Value.Num : 0).Sum() ?? 0);
        prizePool.Add(val.Key, (from v in val
                                select v.Value.UID).ToArray());
      }

      return;
    }

    public void DecreasePrize(uint uid) {
      try {
        prizedb[uid].DecreaseNum();
      } catch {
        LotteryCore.ConsoleWrapper.WriteError();
        Console.Error.WriteLine("{0}", "Insufficient prize");
      }
      DbgUtils.PrintDbgInfoPrizeDB(this);
      return;
    }

    public Dictionary<int, int> GenWeightRatioDict(bool decreaseAsNumChange = false) {
      var dict = new Dictionary<int, int>();
      foreach (var val in prizedb.GroupBy(x => x.Value.Weight ?? -1).ToArray()) {
        dict.Add(val.Key, val.ToArray().Select(x => x.Value.Num > 0 ? x.Value.Ratio * (decreaseAsNumChange ? x.Value.Num : x.Value.OriginalNumber) : 0).Sum() ?? 0);
      }
      return dict;
    }

    ///<summary>
    ///Method <c>GenUIDRatioDictByWeight</c> returns a subset of prizedb
    ///</summary>
    ///<param name="weight">the weight of looking for items.</param>
    ///<returns>A dictionary of UID and PrizeItem. </returns>
    public Dictionary<uint, int> GenUIDRatioDictByWeight(int weight) {
      var dict = new Dictionary<uint, int>();
      foreach (var val in prizePool[weight]) {
        if (prizedb[val].Num > 0 || prizedb[val].Infinity) {
          dict.Add(prizedb[val].UID, prizedb[val].Ratio ?? 0);
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
    public static Dictionary<int, int> AdjustRatio(Dictionary<int, int> dict, Dictionary<int, int> prize) {
      var adjustedDict = new Dictionary<int, int>(dict.Where(x => x.Value > 0 || x.Key == -1).ToDictionary());
      foreach (var val in prize) {
        if (val.Key == -1) {
          throw new Exception("Error: Cannot adjust ratio of infinity object.");
        }
        int va = adjustedDict[val.Key] + val.Value;
        if (va <= 0) {
          adjustedDict[val.Key] = 0;
        } else {
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
    public static Dictionary<int, int> AdjustRatio(Dictionary<int, int> dict, params KeyValuePair<int, int>[] prize) {
      var adjustedDict = new Dictionary<int, int>(dict.Where(x => x.Value > 0 || -1 == x.Key).ToDictionary());
      foreach (var val in prize) {
        if (val.Key == -1) {
          throw new Exception("Error: Cannot adjust ratio of infinity object.");
        }
        int va = adjustedDict[val.Key] + val.Value;
        if (va <= 0) {
          adjustedDict[val.Key] = 0;
        } else {
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
    public Dictionary<uint, int> AdjustRatio(Dictionary<uint, int> ratioPool, Dictionary<uint, int> prize) {
      var adjustedDict = new Dictionary<uint, int>(ratioPool.Where(x => prizedb[x.Key].Num > 0 || prizedb[x.Key].Infinity).ToDictionary());
      foreach (var val in prize) {
        int va = adjustedDict[val.Key] + val.Value;
        if (va <= 0) {
          adjustedDict[val.Key] = 0;
        } else {
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
    public Dictionary<uint, int> AdjustRatio(Dictionary<uint, int> ratioPool, params KeyValuePair<uint, int>[] prize) {
      var adjustedDict = new Dictionary<uint, int>(ratioPool.Where(x => prizedb[x.Key].Num > 0 || prizedb[x.Key].Infinity).ToDictionary());
      foreach (var val in prize) {
        int va = adjustedDict[val.Key] + val.Value;
        if (va <= 0) {
          adjustedDict[val.Key] = 0;
        } else {
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
    public static Dictionary<int, int> GenSumSet(Dictionary<int, int> prize) {
      var prizeSumSet = new Dictionary<int, int>(prize.Count);
      var tmp = 0;
      foreach (var val in prize) {
        tmp += val.Value;
        if (-1 == val.Key) {
          prizeSumSet.Add(val.Key, 0);
        } else {
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
    public static Dictionary<uint, int> GenSumSet(Dictionary<uint, int> prize) {
      var prizeSumSet = new Dictionary<uint, int>(prize.Count);
      var sum = prize.Values.Sum();
      var tmp = 0;
      foreach (var val in prize) {
        tmp += val.Value * 100 / sum;
        prizeSumSet.Add(val.Key, tmp);
      }
      if (100 != prizeSumSet.Last().Value) {
        prizeSumSet[prizeSumSet.Last().Key] = 100;
      }

      return prizeSumSet;
    }

    ///<summary>
    ///Method <c>GenSumSet</c> Generate a sumset of given object.
    ///</summary>
    ///<param name="prize">the adjusting list of weight and sum value.</param>
    ///<returns>prize weight. </returns>
    public static int GenPrizeWeigt(Dictionary<int, int> prize) {
      var rann = LotteryCore.RandomGenerator.GenRandom() * 10;

      // --------------------------------------------------------
      LotteryCore.ConsoleWrapper.WriteDebug();
      Console.WriteLine("{0}", "sumset(weight)");
      LotteryCore.ConsoleWrapper.WriteDebug();
      Console.WriteLine("{0}", "weight/ratio(in 1000%o)");
      DbgUtils.PrintDict(prize);
      DbgUtils.PrintInt(rann);
      // --------------------------------------------------------

      foreach (var val in prize) {
        if (-1 == val.Key) {
          return val.Key;
        }
        if (rann < val.Value) {
          return val.Key;
        }
      }
      return -1;
    }

    public PrizeItem GenPrizeItem(Dictionary<uint, int> prize) {
      var rann = LotteryCore.RandomGenerator.GenRandom();

      // ------------------------------------------------------------------------------------
      LotteryCore.ConsoleWrapper.WriteDebug();
      Console.WriteLine("{0}", "sumset(item)");
      LotteryCore.ConsoleWrapper.WriteDebug();
      Console.WriteLine("{0}", "uid/ratio(in 100%)");
      DbgUtils.PrintDict(prize);
      DbgUtils.PrintInt(rann);
      // ------------------------------------------------------------------------------------

      foreach (var val in prize) {
        if (rann <= val.Value) {
          return prizedb[val.Key];
        }
      }

      return new PrizeItem();
    }
  }//!END: LotteryEngine
}