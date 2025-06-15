using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace DEMO
//{
    public   class gcode 
    {
    public delegate void log(string i);
    public static event log logout;

    public  string readtextpath;//文本路径名称
    public ArrayList stcode = new ArrayList();//文本流式读取存放
    public void code() 
    {
        StreamReader sr1 = new StreamReader(@readtextpath);
        string nextline;
        
        while ((nextline=sr1.ReadLine())!=null)
        {
            stcode.Add(nextline);
        
        }
        ;
        sr1.Close();

    }

    public void MOVEL() {

        var path = ThreeAxisLinearInterpolatorWithSpeed.Interpolate(
    0, 0, 0, 0, 0, 1, // 起点和终点
    50, 100, 0.01         // 最大速度、最大加速度、插补周期
);
        foreach (var pt in path)
        {
            Console.WriteLine($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
            logout?.Invoke($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
        }


    }
    public void MOVECxc() {
        var arcPath = ThreeAxisArcInterpolatorWithSpeed.Interpolate(
0, 0, 0,      // 起点
0, 0, 0,      // 终点
1, 1, 0,       // 圆心
true,          // 逆时针
20, 50, 0.01   // 最大速度、最大加速度、插补周期
);
        foreach (var pt in arcPath)
        {
            Console.WriteLine($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
            logout?.Invoke($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
        }
    }
    public void MOVECrad() {
        var arcPath = ThreeAxisArcInterpolatorWithRadius.Interpolate(
        0, 0, 0,      // 起点
        0, 0, 0,      // 终点
        1,            // 半径
        true,          // 逆时针
        20, 50, 0.01   // 最大速度、最大加速度、插补周期
    );
        foreach (var pt in arcPath)
        {
            Console.WriteLine($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
            logout?.Invoke($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
        }
    }


    



    public void GM() //执行stcode内容解析
    {
        double curX = 0, curY = 0, curZ = 0;
        double vMax = 100, aMax = 300, dt = 0.01; // 可根据G代码F参数动态调整
        double x = 0, y = 0, z = 0, i = 0, j = 0;

        bool zen=false ;//true为增量

        var lines = new List<string>
{
   
};

        foreach (var item in stcode)
        {
            lines.Add(item.ToString());
        }
        var commands = GCodeParser.ParseLines(lines);


        foreach (var cmd in commands)
        {

            if (cmd.Type == "G" && (cmd.Number == 90 || cmd.Number == 91))//G90绝对插补，G91增量插补
            {
                if (cmd.Number == 90)
                {
                    zen = false;
                }
                if (cmd.Number == 91)
                {
                    zen = true;
                }
              
            } 

            if (cmd.Type == "G" && (cmd.Number == 0 || cmd.Number == 1))
            {
                // 直线插补
                if (!zen)//绝对
                {
                     x = cmd.Parameters.ContainsKey("X") ? cmd.Parameters["X"] : curX;
                     y = cmd.Parameters.ContainsKey("Y") ? cmd.Parameters["Y"] : curY;
                     z = cmd.Parameters.ContainsKey("Z") ? cmd.Parameters["Z"] : curZ;
                    if (cmd.Parameters.ContainsKey("F")) vMax = cmd.Parameters["F"]/100;
                    //aMax = vMax + 300;
                }
                if (zen)//增量
                {
                    x = cmd.Parameters.ContainsKey("X") ? cmd.Parameters["X"] : curX+x;
                     y = cmd.Parameters.ContainsKey("Y") ? cmd.Parameters["Y"] : curY+y;
                     z = cmd.Parameters.ContainsKey("Z") ? cmd.Parameters["Z"] : curZ+z;
                   if (cmd.Parameters.ContainsKey("F")) vMax = cmd.Parameters["F"]/100;
                    //aMax = vMax + 300;
                }

                var path = ThreeAxisLinearInterpolatorWithSpeed.Interpolate(
                    curX, curY, curZ, x, y, z, vMax, aMax, dt);

                //var path = ThreeAxisLinearInterpolatorWithSpeed.Interpolate(
                //    curX, curY, curZ, x, y, z, 50, 100, dt);

                // 处理插补点（如输出、驱动电机等）
                foreach (var pt in path)
                {
                    Console.WriteLine($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
                    logout?.Invoke($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
                }
                curX = x; curY = y; curZ = z;
            }
            else if (cmd.Type == "G" && (cmd.Number == 2 || cmd.Number == 3))
            {
                // 圆弧插补（以I/J为例，R可用圆弧半径插补）
                 x = cmd.Parameters.ContainsKey("X") ? cmd.Parameters["X"] : curX;
                 y = cmd.Parameters.ContainsKey("Y") ? cmd.Parameters["Y"] : curY;
                 z = cmd.Parameters.ContainsKey("Z") ? cmd.Parameters["Z"] : curZ;
                 i = cmd.Parameters.ContainsKey("I") ? cmd.Parameters["I"] : 0;
                 j = cmd.Parameters.ContainsKey("J") ? cmd.Parameters["J"] : 0;
                bool isCCW = cmd.Number == 3; // G2顺时针，G3逆时针
                if (cmd.Parameters.ContainsKey("F")) vMax = cmd.Parameters["F"] /100;

                var arcPath = ThreeAxisArcInterpolatorWithSpeed.Interpolate(
                    curX, curY, curZ,
                    x, y, z,
                    curX + i, curY + j, curZ, // 圆心
                    isCCW,
                    vMax, aMax, dt);

                foreach (var pt in arcPath)
                {
                    Console.WriteLine($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
                    logout?.Invoke($"X={pt.X:F2}, Y={pt.Y:F2}, Z={pt.Z:F2}, V={pt.V:F2}");
                }

                curX = x; curY = y; curZ = z;
            }
            // 可扩展M代码等其他命令
            else if (cmd.Type == "M")
            {
                switch (cmd.Number)
                {
                    case 3:
                        // 主轴顺时针启动（如M3 S1000）
                        if (cmd.Parameters.ContainsKey("S"))
                        {
                            double spindleSpeed = cmd.Parameters["S"];
                            Console.WriteLine($"主轴顺时针启动，转速: {spindleSpeed}");
                        }
                        else
                        {
                            Console.WriteLine("主轴顺时针启动");
                        }
                        break;
                    case 5:
                        // 主轴停止
                        Console.WriteLine("主轴停止");
                        break;
                    // 可继续扩展其他M代码
                    default:
                        Console.WriteLine($"M代码: M{cmd.Number}，参数: {string.Join(", ", cmd.Parameters)}");
                        break;
                }
            }
            // 可扩展M代码等其他命令
        }
    }

    }
//}
