using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common
{
    /// <summary>
    /// Author: Jiachen Chen, University of Goettingen
    /// Acknowledgment:
    /// This document has been supported by the GreenICN project 
    /// (GreenICN: Architecture and Applications of Green Information Centric Networking ), 
    /// a research project supported jointly by the European Commission under its 
    /// 7th Framework Program (contract no. 608518) 
    /// and the National Institute of Information and Communications Technology (NICT) 
    /// in Japan (contract no. 167). The views and conclusions contained herein are 
    /// those of the authors and should not be interpreted as necessarily 
    /// representing the official policies or endorsements, either expressed 
    /// or implied, of the GreenICN project, the European Commission, or NICT.
    /// 
    /// Event queue.
    /// </summary>
    public class EventQueue
    {
        private static EventQueue Default = new EventQueue();
        public delegate void DoEventDelegate(params object[] paramValues);

        public static void AddEvent(long time, DoEventDelegate del, params object[] paramValues)
        {
            Default.addEvent(time, del, paramValues);
        }

        public static void Run()
        {
            Default.run();
        }

        public static void Reset()
        {
            Default.reset();
        }

        public static long Now { get { return Default.now; } }

        private LinkedList<Event> events = new LinkedList<Event>();
        private long now { set; get; }
        private void run()
        {
            while (events.Count != 0)
            {
                Event e = events.First.Value;
                now = e.Time;
                e.DoEvent();
                events.RemoveFirst();
            }
        }

        private void addEvent(long time, DoEventDelegate del, params object[] paramValues)
        {
            Trace.Assert(time >= now);
            Event e = new Event { Time = time, Del = del, ParamValues = paramValues };
            //add e into list
            LinkedListNode<Event> n = events.First;
            for (; n != null && n.Value.Time <= time; n = n.Next) ;
            if (n != null) events.AddBefore(n, e);
            else events.AddLast(e);
        }

        private void reset()
        {
            Trace.Assert(events.Count == 0);
            now = 0;
        }

        private class Event
        {
            public long Time;
            public DoEventDelegate Del;
            public object[] ParamValues;
            public void DoEvent()
            {
                Del(ParamValues);
            }
        }
    }

    public class TimeoutEventDispatcher
    {
        public long TimeoutTime { private set; get; }
        public bool Active { private set; get; }
        private EventQueue.DoEventDelegate Delegate;
        private object[] Objs;

        public TimeoutEventDispatcher(long timeoutTime, EventQueue.DoEventDelegate del, params object[] objs)
        {
            TimeoutTime = timeoutTime;
            Delegate = del;
            Objs = objs;
            Active = true;

            EventQueue.AddEvent(timeoutTime, TimeoutHandle);
        }

        public void Delay(long newTime)
        {
            Trace.Assert(newTime > TimeoutTime);
            TimeoutTime = newTime;
        }

        public void Cancel()
        {
            Active = false;
        }

        private void TimeoutHandle(params object[] paramValues)
        {
            if (!Active) return;
            if (TimeoutTime == EventQueue.Now)
            {
                Delegate(Objs);
                Active = false;
            }
            else
                EventQueue.AddEvent(TimeoutTime, TimeoutHandle);
        }
    }

    public static class EventQueueTester
    {
        public static void TestEventQueue()
        {
            EventQueue.AddEvent(EventQueue.Now + 1000, test1, 5);
            EventQueue.AddEvent(EventQueue.Now + 1000, test2, 5);
            EventQueue.Run();
        }

        static void test1(params object[] objs)
        {
            int val = (int)objs[0];
            Console.WriteLine("[Test1] Now: {0}, val: {1}", EventQueue.Now, val);
            if (val > 0)
                EventQueue.AddEvent(EventQueue.Now, test1, val - 1);
        }

        static void test2(params object[] objs)
        {
            int val = (int)objs[0];
            Console.WriteLine("[Test2] Now: {0}, val: {1}", EventQueue.Now, val);
            if (val > 0)
                EventQueue.AddEvent(EventQueue.Now + 1000, test2, val - 1);
            else
                EventQueue.AddEvent(EventQueue.Now + 1000, test1, val + 5);

        }

        public static void TestTimeoutEventDispatcher()
        {
            TimeoutEventDispatcher ted = new TimeoutEventDispatcher(EventQueue.Now + 1000, (objs) =>
            {
                Console.WriteLine("[{0}] Timeout!", EventQueue.Now);
            });
            ted.Delay(EventQueue.Now + 2000);

            EventQueue.AddEvent(EventQueue.Now + 1500, (objs) =>
            {
                Console.WriteLine("[{0}] Delay 2000ms!", EventQueue.Now);
                ted.Delay(EventQueue.Now + 2000);
            });

            EventQueue.AddEvent(EventQueue.Now + 3499, (objs) =>
            {
                //   Console.WriteLine("[{0}] Cancel it!", EventQueue.Now);
                //   ted.Cancel();
            });
            EventQueue.Run();
        }
    }
}

