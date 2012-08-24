using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ConditionSystem
{
    public static class Conditions
    {
        class UnwindTag<T> : Exception
        {
            public T Value { get; set; }
        }

        static ConditionalWeakTable<Thread, Stack<Tuple<Type, HandlerBindCallback>>> _handlerStacks;
        static ConditionalWeakTable<Thread, Stack<Tuple<string, RestartBindCallback>>> _restartStacks;

        static Conditions()
        {
            _handlerStacks = new ConditionalWeakTable<Thread, Stack<Tuple<Type, HandlerBindCallback>>>();
            _restartStacks = new ConditionalWeakTable<Thread, Stack<Tuple<string, RestartBindCallback>>>();
        }

        public static T HandlerBind<T>(Type exceptionType, HandlerBindCallback handler, HandlerBody<T> body)
        {
            if (null == exceptionType)
                throw new ArgumentNullException("exceptionType");
            if (!exceptionType.IsSubclassOf(typeof(Exception)))
                throw new InvalidOperationException("exceptionType is not a subtype of System.Exception");
            if (null == handler)
                throw new ArgumentNullException("handler");
            if (null == body)
                throw new ArgumentNullException("body");
            Thread currentThread = Thread.CurrentThread;
            var clusters = _handlerStacks.GetOrCreateValue(currentThread);
            clusters.Push(Tuple.Create(exceptionType, handler));
            try
            {
                return body();
            }
            finally
            {
                clusters.Pop();
            }
        }

        public static T HandlerCase<T>(Type exceptionType, HandlerCaseCallback<T> handler, HandlerBody<T> body)
        {
            if (null == exceptionType)
                throw new ArgumentNullException("exceptionType");
            if (!exceptionType.IsSubclassOf(typeof(Exception)))
                throw new InvalidOperationException("exceptionType is not a subtype of System.Exception");
            if (null == handler)
                throw new ArgumentNullException("handler");
            if (null == body)
                throw new ArgumentNullException("body");
            var unwindTag = new UnwindTag<T>();
            HandlerBindCallback handlerCallback = (e) =>
            {
                unwindTag.Value = handler(e);
                throw unwindTag;
            };
            try
            {
                return HandlerBind(exceptionType, handlerCallback, body);
            }
            catch (UnwindTag<T> e)
            {
                if (e == unwindTag)
                {
                    return e.Value;
                }
                else
                    throw;
            }
        }

        public static T UnwindProtect<T>(HandlerBody<T> body, params Action[] actions)
        {
            if (null == body)
                throw new ArgumentNullException("body");
            if (null == actions)
                actions = new Action[0];
            try
            {
                return body();
            }
            finally
            {
                foreach (var a in actions)
                    a();
            }
        }

        public static void Signal<T>(T exception)
            where T : Exception
        {
            if (null == exception)
                throw new ArgumentNullException("exception");
            Thread currentThread = Thread.CurrentThread;
            var clusters = _handlerStacks.GetOrCreateValue(currentThread);
            var i = clusters.GetEnumerator();
            while (i.MoveNext())
            {
                var type = i.Current.Item1;
                var handler = i.Current.Item2;
                if (type.IsInstanceOfType(exception))
                {
                    handler(exception);
                    break;
                }
            }
        }

        public static void Error<T>(T exception)
            where T : Exception
        {
            Signal(exception);
            throw exception;
        }

        public static T RestartBind<T>(string name, RestartBindCallback restart, HandlerBody<T> body)
        {
            if (null == name)
                throw new ArgumentNullException("name");
            if (null == restart)
                throw new ArgumentNullException("restart");
            if (null == body)
                throw new ArgumentNullException("body");
            Thread currentThread = Thread.CurrentThread;
            var clusters = _restartStacks.GetOrCreateValue(currentThread);
            clusters.Push(Tuple.Create(name, restart));
            try
            {
                return body();
            }
            finally
            {
                clusters.Pop();
            }
        }

        public static T RestartCase<T>(string name, RestartCaseCallback<T> restart, HandlerBody<T> body)
        {
            if (null == name)
                throw new ArgumentNullException("name");
            if (null == restart)
                throw new ArgumentNullException("restart");
            if (null == body)
                throw new ArgumentNullException("body");
            var unwindTag = new UnwindTag<T>();
            RestartBindCallback restartCallback = (param) =>
            {
                unwindTag.Value = restart(param);
                throw unwindTag;
            };
            try
            {
                return RestartBind(name, restartCallback, body);
            }
            catch (UnwindTag<T> e)
            {
                if (e == unwindTag)
                {
                    return e.Value;
                }
                else
                    throw;
            }
        }

        public static RestartBindCallback FindRestart(string name, bool throwOnError)
        {
            if (null == name)
                throw new ArgumentNullException("name");
            Thread currentThread = Thread.CurrentThread;
            var clusters = _restartStacks.GetOrCreateValue(currentThread);
            var i = clusters.GetEnumerator();
            while (i.MoveNext())
            {
                var restartName = i.Current.Item1;
                var restart = i.Current.Item2;
                if (name == restartName)
                    return restart;

            }
            if (throwOnError)
                throw new RestartNotFoundException(name);
            else
                return null;
        }

        public static RestartBindCallback FindRestart(string name)
        {
            return FindRestart(name, false);
        }

        public static object InvokeRestart(string name, object param)
        {
            var restart = FindRestart(name, true);
            return restart(param);
        }

        public static IDictionary<string, RestartBindCallback> ComputeRestarts()
        {
            var restarts = new Dictionary<string, RestartBindCallback>();
            Thread currentThread = Thread.CurrentThread;
            var clusters = _restartStacks.GetOrCreateValue(currentThread);
            foreach (var c in clusters)
            {
                restarts.Add(c.Item1, c.Item2);
            }
            return restarts;
        }
    }
}
