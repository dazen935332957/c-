using System;
using System.Collections.Generic;

public class ThreeAxisArcInterpolatorWithRadius
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
    /// 三轴圆弧插补（起点+终点+圆半径），输入起点、终点、半径，带加减速和速度规划， 圆弧插补假设圆弧在XY平面，Z轴线性插补。	
    /// 自动判断并选择合适的圆心和圆弧方向。支持加减速和速度规划，短圆弧自动降速。若起点、终点和半径无法构成圆弧会抛出异常。
    /// 允许完整圆插补
    /// </summary>
    /// <param name="x0">起点X</param>
    /// <param name="y0">起点Y</param>
    /// <param name="z0">起点Z</param>
    /// <param name="x1">终点X</param>
    /// <param name="y1">终点Y</param>
    /// <param name="z1">终点Z</param>
    /// <param name="radius">圆弧半径</param>
    /// <param name="isCCW">是否逆时针</param>
    /// <param name="vMax">最大速度</param>
    /// <param name="aMax">最大加速度</param>
    /// <param name="dt">插补周期</param>
    /// <returns>插补路径点列表</returns>

    public static List<InterpolationPoint> Interpolate(
        double x0, double y0, double z0,
        double x1, double y1, double z1,
        double radius,
        bool isCCW,
        double vMax, double aMax, double dt)
    {
        var points = new List<InterpolationPoint>();

        // 计算圆心（假设圆弧在XY平面）
        double dx = x1 - x0;
        double dy = y1 - y0;
        double d = Math.Sqrt(dx * dx + dy * dy);

        bool isFullCircle = d == 0;
        double cx, cy, theta0, dtheta;

        if (isFullCircle)
        {
            // 任选一个圆心（如起点正上方，y 方向）
            cx = x0;
            cy = y0 + (isCCW ? -radius : radius); // 保证方向一致
            theta0 = Math.Atan2(y0 - cy, x0 - cx);
            dtheta = isCCW ? 2 * Math.PI : -2 * Math.PI;
        }
        else
        {
            if (d > 2 * Math.Abs(radius))
                throw new ArgumentException("无法根据给定半径和起止点确定圆心");

            // 中点
            double mx = (x0 + x1) / 2.0;
            double my = (y0 + y1) / 2.0;

            // 垂直方向
            double perp_dx = -dy / d;
            double perp_dy = dx / d;

            // 圆心到中点的距离
            double h = Math.Sqrt(radius * radius - (d / 2) * (d / 2));

            // 两个可能的圆心
            double cx1 = mx + perp_dx * h;
            double cy1 = my + perp_dy * h;
            double cx2 = mx - perp_dx * h;
            double cy2 = my - perp_dy * h;

            // 选择正确的圆心
            double theta0_1 = Math.Atan2(y0 - cy1, x0 - cx1);
            double theta1_1 = Math.Atan2(y1 - cy1, x1 - cx1);
            double dtheta_1 = theta1_1 - theta0_1;
            if (isCCW)
            {
                if (dtheta_1 < 0) dtheta_1 += 2 * Math.PI;
            }
            else
            {
                if (dtheta_1 > 0) dtheta_1 -= 2 * Math.PI;
            }

            double theta0_2 = Math.Atan2(y0 - cy2, x0 - cx2);
            double theta1_2 = Math.Atan2(y1 - cy2, x1 - cx2);
            double dtheta_2 = theta1_2 - theta0_2;
            if (isCCW)
            {
                if (dtheta_2 < 0) dtheta_2 += 2 * Math.PI;
            }
            else
            {
                if (dtheta_2 > 0) dtheta_2 -= 2 * Math.PI;
            }

            // 选择角度绝对值较小的圆心
            if (Math.Abs(dtheta_1) <= Math.Abs(dtheta_2))
            {
                cx = cx1; cy = cy1; theta0 = theta0_1; dtheta = dtheta_1;
            }
            else
            {
                cx = cx2; cy = cy2; theta0 = theta0_2; dtheta = dtheta_2;
            }
        }

        double arcLength = Math.Abs(radius * dtheta);
        double dz = z1 - z0;

        // 速度规划
        double tAcc = vMax / aMax;
        double sAcc = 0.5 * aMax * tAcc * tAcc;
        double sConst = arcLength - 2 * sAcc;
        bool hasConst = sConst > 0;
        double tConst = hasConst ? sConst / vMax : 0;

        if (!hasConst)
        {
            // 距离短时，采用三角速度曲线（加速到峰值后立即减速，峰值速度小于vMax）
            tAcc = Math.Sqrt(arcLength / aMax);
            tConst = 0;
            sAcc = 0.5 * aMax * tAcc * tAcc;
            vMax = aMax * tAcc; // 峰值速度修正为实际可达的最大速度
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
                v = vMax - aMax * tDec;
                if (!hasConst)
                {
                    // 距离短时，tAcc = totalTime / 2
                    double tPeak = totalTime / 2.0;
                    if (t <= tPeak) // 加速段
                    {
                        v = aMax * t;
                        s = 0.5 * aMax * t * t;
                    }
                    else // 减速段
                    {
                        double tDecShort = t - tPeak;
                        v = vMax - aMax * tDecShort;
                        s = 0.5 * aMax * tPeak * tPeak + vMax * tDecShort - 0.5 * aMax * tDecShort * tDecShort;
                    }
                }
                else
                {
                    s = sAcc + sConst + vMax * tDec - 0.5 * aMax * tDec * tDec;
                }
                // 保证速度不为负
                if (v < 0) v = Math.Abs(v);
            }

            if (s > arcLength) s = arcLength;

            // 当前角度
            double theta = theta0 + dtheta * (s / arcLength);
            double x = cx + radius * Math.Cos(theta);
            double y = cy + radius * Math.Sin(theta);
            double z = z0 + dz * (s / arcLength);

            points.Add(new InterpolationPoint(x, y, z, v));

            if (s >= arcLength) break;
        }

        return points;
    }
}