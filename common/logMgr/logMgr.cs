using System;

namespace logMgr;
public class logMgr
{
    public static void log(int type, string txt)
        {
            switch(type){
                //cases:
                //  - 0: general
                //  - 1: error
                //  - 2: success
                //  - 3: init
                case 0:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case 3:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
            }
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }
}
