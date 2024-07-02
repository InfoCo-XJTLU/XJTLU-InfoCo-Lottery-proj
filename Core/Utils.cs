
namespace Utils{

    public static class PathTool{

        public static void initDB<T>(ref string dbpath, out List<T> dbi){

            Utils.PathTool.TryGetPath(ref dbpath, ".csv");

            // Useless, but to prevent compiler error
            // Provide a default empty value;
            dbi = new List<T>();

            using (var reader = new StreamReader(dbpath))
            {
                using (var csvdbreader = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
                    bool flag = true;
                    do{
                        try{
                            dbi = csvdbreader.GetRecords<T>().ToList();
                            flag = false;
                        }catch{
                            Utils.ConsoleWrapper.WriteError();
                            Console.Error.WriteLine("{0}","Not a csv or file dose not match the request.");
                            dbpath = typeof(T).ToString();
                            Utils.PathTool.TryGetPath(ref dbpath, ".csv");
                            flag = true;
                        }finally{
                        }
                    }while(flag);
                }

                // var modinfo = typeof(PrizeItem).GetMethod("init");
                // if(null != modinfo){
                //     for(var i = 0; i < dbi.Count(); i ++){
                //         modinfo.Invoke(dbi[i], null);
                //     }
                // }
                // Obsoleted, for it can be called in context other rather than initdb
            }
        }

        public static void TryGetPath(ref string path, string ft){

            if(!File.Exists(path)){
                ConsoleWrapper.WriteError();
                Console.Error.WriteLine("{0}", "No file called `"+ path + "' was found." );
                string? tmppath = "Y";
                do{
                    Console.WriteLine("Try specified another one? [Y/n]:");
                    tmppath = Console.ReadLine();
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write("{0}", new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop);
                    if(string.IsNullOrEmpty(tmppath) || string.IsNullOrWhiteSpace(tmppath)){
                        tmppath = "Y";
                    }
                    if(tmppath.Contains('Y')|| tmppath.Contains('y')){
                        Console.Write("{0}", "Given a path:");
                        tmppath = Console.ReadLine();
                    }else{
                        ConsoleWrapper.WriteError();
                        Console.Error.WriteLine("{0}", "Expect a database with extension of " + ft);
                        Environment.Exit(1);
                    }
                    if(File.Exists(tmppath) && new FileInfo(tmppath).Extension == ft){
                        path = tmppath;
                        break;
                    }else{
                        ConsoleWrapper.WriteError();
                        Console.Error.WriteLine("{0}","File not exist or not specified request.");
                    }
                }while(true);
            }
        }
    }

    public static class ConsoleWrapper{
        public static void WriteError(){

            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.Write("{0}", "Error: ");
            Console.ForegroundColor = tmp;
        }

        public static void WriteDbg(){

            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("{0}", "Debug: ");
            Console.ForegroundColor = tmp;
        }
    }

    public static class RandomGenerator{

        public static int GenRandom(){

            var csp = System.Security.Cryptography.RandomNumberGenerator.Create();
            var randomNums = new byte[10];
            csp.GetNonZeroBytes(randomNums);
            foreach(var val in randomNums){
                if(val >=0 && val <= 100){
                    return val;
                }
            }
            return 0;
        }
    }
}
