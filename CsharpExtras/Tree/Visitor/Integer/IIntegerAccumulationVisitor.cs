﻿namespace CsharpExtras.Tree.Visitor.Integer
{
    interface IIntegerAccumulationVisitor : ITreeVisitor<int>
    {
        int Result { get; }
    }
}