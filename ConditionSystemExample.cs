using System;
using ConditionSystem;

namespace ConditionSystemExample
{
    class ConditionSystemExample
    {
        static void Main()
        {
            int r;
            r = Conditions.HandlerBind(
                    typeof(DivideByZeroException),
                    (e) =>
                    {
                        Console.WriteLine("Entering handler callback");
                    },
                    () =>
                    {
                        Console.WriteLine("Entering HandlerBind with DivSignal");
                        var rv = DivSignal(123, 0);
                        Console.WriteLine("Returning {0} from body", rv);
                        return rv;
                    });
            Console.WriteLine("Return value: {0}\n", r);

            r = Conditions.HandlerCase(
                    typeof(DivideByZeroException),
                    (e) =>
                    {
                        Console.WriteLine("Entering handler callback");
                        Console.WriteLine("Returning 0 from handler");
                        return 0;
                    },
                    () =>
                    {
                        Console.WriteLine("Entering HandlerCase with DivError and UnwindProtect");
                        return Conditions.UnwindProtect(
                                    () =>
                                    {
                                        Console.WriteLine("Entering UnwindProtect");
                                        var rv = DivError(123, 0);
                                        Console.WriteLine("This line should not be printed");
                                        return rv;
                                    },
                                    () =>
                                    {
                                        Console.WriteLine("UnwindProtect exit point");
                                    });
                    });
            Console.WriteLine("Return value: {0}\n", r);

            r = Conditions.HandlerBind(
                    typeof(DivideByZeroException),
                    (e) =>
                    {
                        Console.WriteLine("Entering handler callback");
                        Console.WriteLine("Invoking restart ReturnValue with param = 0");
                        Conditions.InvokeRestart("ReturnValue", 0);
                    },
                    () =>
                    {
                        Console.WriteLine("Entering HandlerBind with DivRestart");
                        return DivRestart(123, 0);
                    });
            Console.WriteLine("Return value: {0}", r);
        }

        static int DivSignal(int x, int y)
        {
            if (0 == y)
            {
                Conditions.Signal(new DivideByZeroException());
                return 0;
            }
            else
                return x / y;
        }

        static int DivError(int x, int y)
        {
            if (0 == y)
                Conditions.Error(new DivideByZeroException());
            return x / y;
        }

        static int DivRestart(int x, int y)
        {
            return Conditions.RestartCase(
                        "ReturnValue",
                        (param) =>
                        {
                            Console.WriteLine("Entering restart ReturnValue");
                            Console.WriteLine("Returning {0} from restart", param);
                            return (int)param;
                        },
                        () =>
                        {
                            Console.WriteLine("Entering RestartCase");
                            return DivError(x, y);
                        });
        }
    }
}
