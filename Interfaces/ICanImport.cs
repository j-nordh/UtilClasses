using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
namespace UtilClasses.Interfaces;

public interface ICanImport<in T>
{
    void Import(T obj);
}

