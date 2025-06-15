using System;
using System.Collections.Generic;

public class ThreeAxisArcInterpolatorWithSpeed
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
    /// 三轴圆弧插补（起点+终点+圆心），带加减速和速度规划，圆弧插补假设圆弧在XY平面，Z轴线性插补（如需任意平面插补可扩展）。
    /// isCCW为true时逆时针，否则顺时针。支持加减速和速度规划，短圆弧自动降速。返回每个插补点的三轴坐标和当前速度。
    /// </summary>
    /// <param name="x0">起点X</param>
    /// <param name="y0">起点Y</param>
    /// <param name="z0">起点Z</param>
    /// <param name="x1">终点X</param>
    /// <param name="y1">终点Y</param>
    /// <param name="z1">终点Z</param>
    /// <param name="xc">圆心X</param>
    /// <param name="yc">圆心Y</param>
    /// <param name="zc">圆心Z</param>
    /// <param name="isCCW">是否逆时针</param>
    /// <param name="vMax">最大速度</param>
    /// <param name="aMax">最大加速度</param>
    /// <param name="dt">插补周期</param>
    /// <returns>插补路径点列表</returns>

    public static List<InterpolationPoint> Interpolate(
        double x0, double y0, double z0,
        double x1, double y1, double z1,
        double xc, double yc, double zc,
        bool isCCW,
        double vMax, double aMax, double dt)
    {
        var points = new List<InterpolationPoint>();

        // 计算圆弧平面（假设圆弧在XY平面，Z线性插补）
        double r = Math.Sqrt((x0 - xc) * (x0 - xc) + (y0 - yc) * (y0 - yc));
        double theta0 = Math.Atan2(y0 - yc, x0 - xc);
        double theta1 = Math.Atan2(y1 - yc, x1 - xc);

        // 计算圆弧角度
        double dTheta = theta1 - theta0;
        if (isCCW)
        {
            if (dTheta <= 0) dTheta += 2 * Math.PI;
        }
        else
        {
            if (dTheta >= 0) dTheta -= 2 * Math.PI;
        }
        double arcLength = Math.Abs(r * dTheta);

        // Z轴线性插补
        double dz = z1 - z0;

        // 速度规划
        double tAcc = vMax / aMax;
        double sAcc = 0.5 * aMax * tAcc * tAcc;
        double sConst = arcLength - 2 * sAcc;
        bool hasConst = sConst > 0;
        double tConst = hasConst ? sConst / vMax : 0;

        if (!hasConst)
        {
            // 距离短时，不能超过最大速度，采用抛物线加减速，末速度为0
            tAcc = Math.Sqrt(arcLength / aMax);
            tConst = 0;
            sAcc = arcLength / 2;
            vMax = aMax * tAcc; // 实际最大速度
        }

        double totalTime = 2 * tAcc + tConst;
        int totalSteps = (int)Math.Ceiling(totalTime / dt);

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
                v = Math.Max(0, vMax - aMax * tDec);
                s = sAcc + sConst + vMax * tDec - 0.5 * aMax * tDec * tDec;
                if (s > arcLength) s = arcLength;
                if (v < 0) v = 0;
            }
            // 保证速度不为负
            if (v < 0) v = Math.Abs(v);
            if (s > arcLength) s = arcLength;

            // 当前角度
            double theta = theta0 + (isCCW ? 1 : -1) * Math.Abs(dTheta) * (s / arcLength);
            double x = xc + r * Math.Cos(theta);
            double y = yc + r * Math.Sin(theta);
            double z = z0 + dz * (s / arcLength);

            points.Add(new InterpolationPoint(x, y, z, v));

            if (s >= arcLength) break;
        }

        return points;
    }
}