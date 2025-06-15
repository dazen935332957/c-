using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace DEMO
//{
    public  class TRIG1 //check有变化；checkup上升沿；checkdown下降沿 new出实例
    {
    public delegate void chek(int e);
    public event chek evnchek;
    private static bool input_1 = false;
    private static bool input_2 = false;
    private static int input_3 = 0;

    //public void evnchekout(int e) {

    //    evnchek?.Invoke(e);

    //}
    //public   bool check (bool input) 
    //    {
    //       // bool input_1;
    //        if (input != input_1)
    //        {


    //            input_1 = input;
    //        evnchek?.Invoke(1);
    //        return true;
            
    //    }
    //        else { return false; }

            
        //} 
        public bool checkup (bool input) {

            if (input==true && input!=input_1)
            {


                input_1 = input;
            evnchek?.Invoke(1);
            return true;
            }
            else {
                input_1 = input;
                return false; }


        }

        public bool checkdown(bool input)
        {

            if (input == false && input != input_2)
            {


            input_2 = input;
            evnchek?.Invoke(0);
            return true;
            }
            else {
            input_2 = input;
                return false; }


        }
    public bool check_int(int input)
    {
        // bool input_1;
        if (input != input_3)
        {


            input_3 = input;
            evnchek?.Invoke(input);
            return true;
        }
        else { return false; }
    }
    }
//}
