namespace msgProc;
public class Bounds
{
    public static string GetBounds(string data, int lower, int upper){
        string msg = null;
        for (int i = lower; i<=upper; i++){
            msg += data[i];
        }
        return msg;
    }
}
