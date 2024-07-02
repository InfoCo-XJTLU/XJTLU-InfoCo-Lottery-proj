using CsvHelper.Configuration.Attributes;

namespace Question{

    public enum EDifficulty{
        Hard,
        Normal,
        Simple
    }

    public class QuestionEntry: IComparable<QuestionEntry>, IEquatable<QuestionEntry> {

        // Begin: internal properties.
        ///<value> Property <c>totalItems</c> represents what number of questions are in total. which could be used to generate UID.</value>
        private static uint totalItmes = 0;

        [Ignore] private uint uid = 0;
        [Ignore] private string contents = null!;
        [Ignore] private EDifficulty difficulty = EDifficulty.Normal;
        [Ignore] private int weight = 0;
        [Ignore] private int adjustRatio = 0; // between 0 ~ 1k

        [Ignore] private string choices = ""; // split by semi-comma, <choice>: <contents> // TODO: Using choice processing function
        // [Ignore] private string[] choice;
        [Ignore] private Type type = typeof(int);
        [Ignore] private object? answer;

        [Name("UID")] [Optional] public uint UID { get => uid;  set => uid = value;  }
        [Name("Contents")] public string Contents { get => contents; set => contents = value; }
        [Name("Difficulty")] public EDifficulty Difficulty { get => difficulty; set => difficulty = value; }
        [Name("Weight")] [NullValues("Null")] public int? Weight { get => weight; set => weight = (value??0) >= 0 || value is null ? value.GetValueOrDefault(-1) : throw new Exception("Error: invalid weight."); }
        [Name("Ratio")] public int Ratio { get => adjustRatio; set => adjustRatio = value >= 0 && value <= 1000 ? value : throw new Exception("Error: invalid ratio."); }
        [Name("Choices")] public string Choices { get => choices; set => choices = value; }// TODO: Add choice process function
        [Name("Answer")] public object? Answer {
        get => System.Convert.ToString(answer);
        set {
            switch(value){ default: answer = null; break; case int: answer = System.Convert.ToInt32(value); type = typeof(int); break; case string: answer = System.Convert.ToString(value); type = typeof(string); break; case double: answer = System.Convert.ToDouble(value); type = typeof(double); break; case null: answer = null; break; }; } }

        public int CompareTo(QuestionEntry? obj){ return obj is not null ? this.weight.CompareTo(obj.weight) : this.weight.CompareTo(0); }
        public override bool Equals(object? obj){
            if (obj is null ){
                return false;
            }
            QuestionEntry objIns = (obj as QuestionEntry)!;
            if(objIns is null){
                return false;
            }else{
                return Equals(objIns);
            }
        }
        public bool Equals(QuestionEntry? obj){
            if(obj is null){
                return false;
            }
            return this.uid.Equals(obj.uid);
        }

        public override int GetHashCode(){ return System.Convert.ToInt32(uid); }

        public QuestionEntry(){}
        ~QuestionEntry(){}

        public void init(){

            uid = (uid == 0)
                ? ++QuestionEntry.totalItmes
                : uid;
        }
    }

    public class QuestionEngine{

        private string dbpath = null!;
        private Dictionary<uint, QuestionEntry> questiondb = new Dictionary<uint, QuestionEntry>()!;
        private Dictionary<int, QuestionEntry[]> questionset = new Dictionary<int, QuestionEntry[]>()!;

        public QuestionEntry this[int weight, int item]{
            get => questionset[weight][item];
        }

        public QuestionEngine(in string dbpath){

            this.dbpath = dbpath;
            Utils.PathTool.TryGetPath(ref this.dbpath, ".csv");
            List<QuestionEntry> tmpdb;
            Utils.PathTool.initDB(ref this.dbpath, out tmpdb);
            tmpdb.Sort();
            tmpdb.Reverse();
            foreach(var val in tmpdb){
                val.init();
                questiondb.Add(val.UID, val);
            }
        }
        ~QuestionEngine(){}
    }

}
