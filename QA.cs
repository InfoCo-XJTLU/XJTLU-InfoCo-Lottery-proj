namespace Question{

    public enum Difficulty{
        Hard,
        Normal,
        Simple
    }

    public class QuestionEntry: IComparable<QuestionEntry>
    {
        public int UID { get; set; } = 0;
        public string Content { get; set;} = null!;
        public Difficulty Difficulty { get; set; } = Difficulty.Normal;
        public int Weight { get; set; } = 0;
        public int CompareTo(QuestionEntry? obj){
            return (obj??new QuestionEntry()).Weight - Weight;
        }
        public QuestionEntry(){}
        ~QuestionEntry(){}
    }

}
