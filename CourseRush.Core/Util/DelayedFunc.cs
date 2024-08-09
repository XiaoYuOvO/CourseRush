namespace CourseRush.Core.Util;

public delegate void DelayedFunc<in TValue, out TResult>(TValue value, Action<TResult> resultAcceptor);