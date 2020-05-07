﻿using System;
using System.Windows.Forms;

public class DoubleBufferListView : ListView
{
    public DoubleBufferListView()
    {
        SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
    }
}