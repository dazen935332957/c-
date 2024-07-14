using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace DEMO
//{
public class TRIG
{
    public class boolc
    {
        #region
        public class BoolWrapper
        {
            private bool _value;

            public event EventHandler<BoolChangedEventArgs> BoolChanged;

            public bool Value
            {
                get { return _value; }
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnBoolChanged(new BoolChangedEventArgs(_value));
                    }
                }
            }

            protected virtual void OnBoolChanged(BoolChangedEventArgs e)
            {
                BoolChanged?.Invoke(this, e);
            }
        }

        public class BoolChangedEventArgs : EventArgs
        {
            public bool NewValue { get; }

            public BoolChangedEventArgs(bool newValue)
            {
                NewValue = newValue;
            }
        }


        #endregion

    }
    public class intc
    {
        #region


        public class IntWrapper
        {
            private int _value;

            public event EventHandler<IntChangedEventArgs> IntChanged;

            public int Value
            {
                get { return _value; }
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnIntChanged(new IntChangedEventArgs(_value));
                    }
                }
            }

            protected virtual void OnIntChanged(IntChangedEventArgs e)
            {
                IntChanged?.Invoke(this, e);
            }
        }

        public class IntChangedEventArgs : EventArgs
        {
            public int NewValue { get; }

            public IntChangedEventArgs(int newValue)
            {
                NewValue = newValue;
            }
        }
        #endregion


    }
    public class doublec
    {
        #region



        public class DoubleWrapper
        {
            private double _value;

            public event EventHandler<DoubleChangedEventArgs> DoubleChanged;

            public double Value
            {
                get { return _value; }
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnDoubleChanged(new DoubleChangedEventArgs(_value));
                    }
                }
            }

            protected virtual void OnDoubleChanged(DoubleChangedEventArgs e)
            {
                DoubleChanged.Invoke(this, e);
            }
        }

        public class DoubleChangedEventArgs : EventArgs
        {
            public double NewValue { get; }

            public DoubleChangedEventArgs(double newValue)
            {
                NewValue = newValue;
            }
        }


        #endregion
    }
}
