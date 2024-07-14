# c-
c#自建库
c#bool，int，double变化时事件
.
.
private DoubleWrapper doubleWrapper = new DoubleWrapper();
        doubleWrapper.DoubleChanged += DoubleWrapper_DoubleChanged;
      doubleWrapper.Value = 3.14;
        doubleWrapper.Value = 2.718;
    }

    private static void DoubleWrapper_DoubleChanged(object sender, DoubleChangedEventArgs e)
    {
        Console.WriteLine($"Double value changed to: {e.NewValue}");
    }
}
..
