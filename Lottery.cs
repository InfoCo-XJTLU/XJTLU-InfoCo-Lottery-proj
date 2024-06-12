namespace Lottery{
    public class RandomGenerator{
        public int GenRandom(){
            var csp = System.Security.Cryptography.RandomNumberGenerator.Create();
            var byteCsp = new byte[10];
            csp.GetNonZeroBytes(byteCsp);
            foreach(var val in byteCsp){
                if(val >=0 && val <= 100){
                    return val;
                }
            }
            return 0;
        }
    }
    public class Lottery{
        private int numUnPrized;
    }
}
