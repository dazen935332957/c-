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
    /// ����Բ���岹�����+�յ�+Բ�뾶����������㡢�յ㡢�뾶�����Ӽ��ٺ��ٶȹ滮�� Բ���岹����Բ����XYƽ�棬Z�����Բ岹��	
    /// �Զ��жϲ�ѡ����ʵ�Բ�ĺ�Բ������֧�ּӼ��ٺ��ٶȹ滮����Բ���Զ����١�����㡢�յ�Ͱ뾶�޷�����Բ�����׳��쳣��
    /// ��������Բ�岹
    /// </summary>
    /// <param name="x0">���X</param>
    /// <param name="y0">���Y</param>
    /// <param name="z0">���Z</param>
    /// <param name="x1">�յ�X</param>
    /// <param name="y1">�յ�Y</param>
    /// <param name="z1">�յ�Z</param>
    /// <param name="radius">Բ���뾶</param>
    /// <param name="isCCW">�Ƿ���ʱ��</param>
    /// <param name="vMax">����ٶ�</param>
    /// <param name="aMax">�����ٶ�</param>
    /// <param name="dt">�岹����</param>
    /// <returns>�岹·�����б�</returns>

    public static List<InterpolationPoint> Interpolate(
        double x0, double y0, double z0,
        double x1, double y1, double z1,
        double radius,
        bool isCCW,
        double vMax, double aMax, double dt)
    {
        var points = new List<InterpolationPoint>();

        // ����Բ�ģ�����Բ����XYƽ�棩
        double dx = x1 - x0;
        double dy = y1 - y0;
        double d = Math.Sqrt(dx * dx + dy * dy);

        bool isFullCircle = d == 0;
        double cx, cy, theta0, dtheta;

        if (isFullCircle)
        {
            // ��ѡһ��Բ�ģ���������Ϸ���y ����
            cx = x0;
            cy = y0 + (isCCW ? -radius : radius); // ��֤����һ��
            theta0 = Math.Atan2(y0 - cy, x0 - cx);
            dtheta = isCCW ? 2 * Math.PI : -2 * Math.PI;
        }
        else
        {
            if (d > 2 * Math.Abs(radius))
                throw new ArgumentException("�޷����ݸ����뾶����ֹ��ȷ��Բ��");

            // �е�
            double mx = (x0 + x1) / 2.0;
            double my = (y0 + y1) / 2.0;

            // ��ֱ����
            double perp_dx = -dy / d;
            double perp_dy = dx / d;

            // Բ�ĵ��е�ľ���
            double h = Math.Sqrt(radius * radius - (d / 2) * (d / 2));

            // �������ܵ�Բ��
            double cx1 = mx + perp_dx * h;
            double cy1 = my + perp_dy * h;
            double cx2 = mx - perp_dx * h;
            double cy2 = my - perp_dy * h;

            // ѡ����ȷ��Բ��
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

            // ѡ��ǶȾ���ֵ��С��Բ��
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

        // �ٶȹ滮
        double tAcc = vMax / aMax;
        double sAcc = 0.5 * aMax * tAcc * tAcc;
        double sConst = arcLength - 2 * sAcc;
        bool hasConst = sConst > 0;
        double tConst = hasConst ? sConst / vMax : 0;

        if (!hasConst)
        {
            // �����ʱ�����������ٶ����ߣ����ٵ���ֵ���������٣���ֵ�ٶ�С��vMax��
            tAcc = Math.Sqrt(arcLength / aMax);
            tConst = 0;
            sAcc = 0.5 * aMax * tAcc * tAcc;
            vMax = aMax * tAcc; // ��ֵ�ٶ�����Ϊʵ�ʿɴ������ٶ�
        }

        double totalTime = 2 * tAcc + tConst;
        int totalSteps = (int)Math.Ceiling(totalTime / dt);

        for (int i = 0; i <= totalSteps; i++)
        {
            double t = i * dt;
            double s, v;

            if (t < tAcc) // ���ٶ�
            {
                v = aMax * t;
                s = 0.5 * aMax * t * t;
            }
            else if (t < tAcc + tConst) // ���ٶ�
            {
                v = vMax;
                s = sAcc + vMax * (t - tAcc);
            }
            else // ���ٶ�
            {
                double tDec = t - tAcc - tConst;
                v = vMax - aMax * tDec;
                if (!hasConst)
                {
                    // �����ʱ��tAcc = totalTime / 2
                    double tPeak = totalTime / 2.0;
                    if (t <= tPeak) // ���ٶ�
                    {
                        v = aMax * t;
                        s = 0.5 * aMax * t * t;
                    }
                    else // ���ٶ�
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
                // ��֤�ٶȲ�Ϊ��
                if (v < 0) v = Math.Abs(v);
            }

            if (s > arcLength) s = arcLength;

            // ��ǰ�Ƕ�
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