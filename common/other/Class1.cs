namespace messageProc{
    public class Bounds
    {
        public static string GetBounds (string data, int lower, int upper)
        {
            string msg = null;
            for(int i = 0; i<=upper; i++)
            {
                if (i > lower){
                    msg += data[i];
                }else if (data[i].ToString() == string.Empty){
                    break;
                }
            }
            return msg;
        }
    }
}