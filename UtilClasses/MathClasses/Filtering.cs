using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using UtilClasses.Extensions.Enumerables;

namespace UtilClasses.MathClasses
{
    public static class Filtering
    {
        public enum Boundary
        {
            Repeat,
            WindowShrink,
            Wraparound,
            Drop
        }

        public enum TreatNull
        {
            Smallest,
            Disregard,
            NullAll
        }

        //private static IEnumerable<T>
        //    MedianFiltering<T>(this IEnumerable<T> lst, Boundary boundary, int windowSize = 3) =>
        //    MedianFiltering(lst, boundary, (l) => l.Median(x => x), windowSize);

        public static IEnumerable<double> MedianFiltering(this IEnumerable<double> lst, int windowSize = 3) =>
            MedianFiltering(lst, Boundary.WindowShrink, (l) => l.Median(x=>x), windowSize);

            public static IEnumerable<T2> MedianFiltering<T, T2>(this IEnumerable<T> lst, Boundary boundary, Func<IEnumerable<T>, T2> median, int windowSize = 3)
        {

            if (windowSize < 3) throw new ArgumentException("WindowSize needs to be at least 3");
            if ((windowSize % 2) == 0) throw new ArgumentException("Even windowSize not allowed");

            switch (boundary)
            {
                case Boundary.Repeat:
                    return MedianFilteringRepeated(lst, median, windowSize);
                case Boundary.WindowShrink:
                    return MedianFilteringShrink(lst, median, windowSize);
                case Boundary.Wraparound:
                    return MedianFilteringWraparound(lst, median, windowSize);
                case Boundary.Drop:
                    return MedianFilteringDrop(lst, median, windowSize);
                default:
                    throw new ArgumentException("Illegal boundary specified");

            }
        }

        //Översätt till pekare i array istället
        //Func<List<T>, int, int, T>

        private static IEnumerable<T2> MedianFilteringRepeated<T, T2>(this IEnumerable<T> lst, Func<List<T>, T2> median, int windowSize = 3)
        {
            var inputLst = lst.ToList();
            var ret = new List<T2>();

            //List adjusted for boundary issues
            var adjList = new List<T>();

            //Add inital elements
            for (var i = 0; i < (windowSize / 2); i++)
            {
                adjList.Add(inputLst.First());
            }

            adjList.AddRange(inputLst);

            for (var i = 0; i < (windowSize / 2); i++)
            {
                adjList.Add(inputLst.Last());
            }

            var window = new Queue<T>();

            foreach (var p in adjList)
            {
                window.Enqueue(p);

                if (window.Count >= windowSize)
                {
                    ret.Add(median(window.ToList()));
                    window.Dequeue();
                }
            }

            return ret;
        }

        private static IEnumerable<T2> MedianFilteringDrop<T, T2>(this IEnumerable<T> lst, Func<List<T>, T2> median,
            int windowSize = 3)
        {

            var inputLst = lst.ToList();
            var ret = new List<T2>();

            var window = new Queue<T>();

            foreach (var p in inputLst)
            {
                window.Enqueue(p);

                if (window.Count >= windowSize)
                {
                    ret.Add(median(window.ToList()));
                    window.Dequeue();
                }
            }

            return ret;
        }

        private static IEnumerable<T2> MedianFilteringShrink<T, T2>(this IEnumerable<T> lst, Func<List<T>, T2> median, int windowSize = 3)
        {
            var inputLst = lst.ToList();
            var ret = new List<T2>();

            //List adjusted for boundary issues
            var adjList = new List<T>();
            adjList.Add(inputLst.First());
            adjList.AddRange(inputLst);
            adjList.Add(inputLst.Last());

            //Create queue representing the window
            var window = new Queue<T>();
            //Store all input as queue
            var points = new Queue<T>(adjList);

            //Initial add to get first element
            window.Enqueue(points.Dequeue());

            //Loop through all elements
            while (points.Count > 0 || (window.Count >= 3))
            {
                if (points.Count <= 0)
                {
                    //No more elements, Decrease window
                    window.Dequeue();
                    window.Dequeue();
                }
                else if (window.Count < windowSize)
                {
                    //Increase window
                    window.Enqueue(points.Dequeue());
                    window.Enqueue(points.Dequeue());
                }
                else
                {
                    //Move window
                    window.Dequeue();
                    window.Enqueue(points.Dequeue());
                }

                if (window.Count >= 3)
                {
                    ret.Add(median(window.ToList()));
                }
            }

            return ret;
        }

        private static IEnumerable<T2> MedianFilteringWraparound<T, T2>(this IEnumerable<T> lst, Func<IEnumerable<T>, T2> median, int windowSize = 3)
        {
            throw new NotImplementedException();
        }
    }
}
