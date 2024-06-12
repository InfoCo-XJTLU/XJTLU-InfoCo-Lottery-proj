using System.Collections;

namespace CSV{
    public class Csv{

        public Csv(){
        }
        ~Csv(){}

        private ArrayList data = new ArrayList();
        private ArrayList? dataTypes;
        private int nlines;
        private int ncolomns;

        public ArrayList Data { get => data; set => data = value; }

        // public static explicit operator ArrayList(Csv val){
        //     return val.data;
        // }

        public string? this[int i,int j]{
            get {
                return (i < nlines&&Data[i]!=null)
                    ? ((j < ncolomns&&((ArrayList)Data[i])[j] != null)
                       ? (string)((ArrayList)Data[i])[j]
                       : null)
                    : null;
            }
            set {}
        }

        public static implicit operator ArrayList(Csv val){
            return val.data;
        }

        public void ReadCSV(string filepath)
        {
            using (var reader = new System.IO.StreamReader(filepath))
            {
                ParseCSV(reader);
            }

            // foreach(var val in Data){
            //   foreach(var va in (ArrayList)val){
            //     Console.WriteLine("{0}",(string)va);
            //   }
            // }

            return;
        }

        private void ParseCSV(in System.IO.StreamReader reader)
        {
            string? line;
            // if((line = reader.ReadLine())!= null){
                // dataTypes
            // }
            while ((line = reader.ReadLine()) != null)
            {
                Data.Add(splitLine(line));
                nlines++;
            }
            return;
        }

        private ArrayList splitLine(string line){
            var strings = line.Split(',', System.Int32.MaxValue);
            var temp = new ArrayList();

            foreach(var val in strings){
                temp.Add(val);
            }

            return temp;
        }
    }
}
