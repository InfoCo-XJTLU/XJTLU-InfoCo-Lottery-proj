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

        [Ignore] private Dictionary<char, string> choice = null!; // split by semi-comma, <choice>: <contents> // TODO: Using choice processing function
        [Ignore] private string answerType = null!;
        [Ignore] private string answer = null!;

        [Name("UID")] [Optional] public uint UID { get => uid;  set => uid = value;  }
        [Name("Contents")] public string Contents { get => contents; set => contents = value; }
        [Name("Difficulty")] public EDifficulty Difficulty { get => difficulty; set => difficulty = value; }
        [Name("Weight")] [NullValues("Null")] public int? Weight { get => weight; set => weight = (value??0) >= 0 || value is null ? value.GetValueOrDefault(-1) : throw new Exception("Error: invalid weight."); }
        [Name("Ratio")] public int Ratio { get => adjustRatio; set => adjustRatio = value >= 0 && value <= 1000 ? value : throw new Exception("Error: invalid ratio."); }
        [Name("Choices")] [NullValues("Null")] public string Choices {
        set {
            choice = splitChoices(value);

            if(value is null){
                answerType = "Input";
            }else{
                answerType = "Choice";
            }
        }
        } // Null means input question. // TODO: Add choice process function
        [Name("Answer")] public string Answer {
        get => answer;
        set => answer = value;
        }

        public int CompareTo(QuestionEntry? obj){ return obj is not null ? this.weight.CompareTo(obj.weight) : this.weight.CompareTo(0); }
        public override bool Equals(object? obj){
            if (obj is null){
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

        public Dictionary<char, string> splitChoices(string? input){

            var dict = new Dictionary<char, string>();
            if (input is null ) {
                return dict;
            }
            string[] arr = System.Text.RegularExpressions.Regex.Split(input, "(?=(?:(?:[^\"]*\"){2})*[^\"]*$);");
            foreach(var val in arr){
                string[] entry = System.Text.RegularExpressions.Regex.Split(val, "(?=(?:(?:[^\"]*\"){2})*[^\"]*$):");
                if(entry.Length > 2){
                    throw new Exception("Invalid input, follow the pattern `<choice>: <contents>'");
                }
            }

            return dict;
        }

        public bool checkAnswer(in string input){ return input == answer; }
    }

    public class QuestionEngine{

        private string dbpath = null!;
        private Dictionary<uint, QuestionEntry> questiondb = new Dictionary<uint, QuestionEntry>()!;
        private Dictionary<int, uint[]> questionset = new Dictionary<int, uint[]>()!;

        public Dictionary<uint, QuestionEntry> QuestionDB{
            get => questiondb;
        }

        public QuestionEntry this[int weight, int item]{
            get => questiondb[questionset[weight][item]];
        }

        public QuestionEntry[] this[int weight]{
            get => questiondb.Where(x => weight == x.Value.Weight)
                .ToDictionary()
                .Values.ToArray();
        }

        public QuestionEntry this[uint uid]{
            get => questiondb[uid];
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

            initQuestionSet();
        }
        ~QuestionEngine(){}

        private void initQuestionSet(){

            foreach(var val in questiondb.GroupBy(x => x.Value.Weight ?? -1).ToArray()){
                questionset.Add(val.Key, (from v in val
                                          select v.Value.UID).ToArray());
            }
            return;
        }

        public int[] GetWeights(){
            return questionset.Keys.ToArray();
        }

        public uint[] GetQuestionByDifficulty(EDifficulty difficulty){
            return questiondb.Where(x => difficulty == x.Value.Difficulty)
                .Select(x => x.Value.UID)
                .ToArray();
        }
    }

}
