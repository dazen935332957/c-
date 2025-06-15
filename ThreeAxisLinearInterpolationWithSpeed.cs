using System;
using System.Collections.Generic;

public class ThreeAxisLinearInterpolatorWithSpeed
{
    public class InterpolationPoint
    {
        public double X, Y, Z, V;
        public InterpolationPoint(double x, double y, double z, double v)
        {
            X = x; Y = y; Z = z; V = v;
        }
    }

    /// <summary>
    /// 三轴直线插补，带加减速和速度规划
    /// </summary>
    /// <param name="x0">起点X</param>
    /// <param name="y0">起点Y</param>
    /// <param name="z0">起点Z</param>
    /// <param name="x1">终点X</param>
    /// <param name="y1">终点Y</param>
    /// <param name="z1">终点Z</param>
    /// <param name="vMax">最大速度（单位/秒）</param>
    /// <param name="aMax">最大加速度（单位/秒²）</param>
    /// <param name="dt">插补周期（秒）</param>
    /// <returns>插补路径点列表</returns>
    public static List<InterpolationPoint> Interpolate(
        double x0, double y0, double z0,
        double x1, double y1, double z1,
        double vMax, double aMax, double dt)
    {
        var points = new List<InterpolationPoint>();

        // 计算总距离
        double dx = x1 - x0;
        double dy = y1 - y0;
        double dz = z1 - z0;
        double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

        if (distance == 0) return points;

        // 计算加速/减速所需距离
        double tAcc = vMax / aMax;
        double sAcc = 0.5 * aMax * tAcc * tAcc;

        double sConst = distance - 2 * sAcc;
        bool hasConst = sConst > 0;

        double tConst = hasConst ? sConst / vMax : 0;

        // 如果距离太短，达不到最大速度，重新计算加减速段
        if (!hasConst)
        {
            tAcc = Math.Sqrt(distance / aMax);
            tConst = 0;
            sAcc = 0.5 * aMax * tAcc * tAcc;
            // 修正最大速度为实际能达到的最大速度
            vMax = aMax * tAcc;
        }

        double totalTime = 2 * tAcc + tConst;
        int totalSteps = (int)Math.Ceiling(totalTime / dt);

        // 单位向量
        double ux = dx / distance;
        double uy = dy / distance;
        double uz = dz / distance;

        for (int i = 0; i <= totalSteps; i++)
        {
            double t = i * dt;
            double s, v;

            if (t < tAcc) // 加速段
            {
                v = aMax * t;
                s = 0.5 * aMax * t * t;
            }
            else if (t < tAcc + tConst) // 匀速段
            {
                v = vMax;
                s = sAcc + vMax * (t - tAcc);
            }
            else // 减速段
            {
                double tDec = t - tAcc - tConst;
                v = vMax - aMax * tDec;
                // 修正短距离情况下减速段的s计算，避免sAcc + sConst为负数
                if (sConst < 0)
                {
                    // 只有加速和减速段，sAcc = distance / 2
                    double sAccShort = distance / 2;
                    double tAccShort = Math.Sqrt(2 * sAccShort / aMax);
                    double tDecShort = t - tAccShort;
                    v = aMax * (tAccShort - tDecShort);
                    s = sAccShort + vMax * tDecShort - 0.5 * aMax * tDecShort * tDecShort;
                }
                else
                {
                    s = sAcc + sConst + vMax * tDec - 0.5 * aMax * tDec * tDec;
                }
            }
            if (v < 0) v = Math.Abs(v);
            if (s > distance) s = distance;

            double x = x0 + ux * s;
            double y = y0 + uy * s;
            double z = z0 + uz * s;
            
            points.Add(new InterpolationPoint(x, y, z, v));

            if (s >= distance) break;
        }

        return points;
    }
}