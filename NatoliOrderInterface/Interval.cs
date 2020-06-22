using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NatoliOrderInterface
{
    public class Interval
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public static Interval CreateInterval(DateTime start, DateTime end)
        {
            if (start > end)
            {
                return new Interval { Start = end, End = start };
            }
            else
            {
                return new Interval { Start = start, End = end };
            }
        }
        public static IEnumerable<Interval> RemoveSundayFromIntervals(IEnumerable<Interval> intervals)
        {
            if (intervals.Count() < 1)
            {
                yield break;
            }
            intervals = intervals.OrderBy(interval => interval.Start);
            foreach(Interval interval in intervals)
            {
                while((int)interval.Start.DayOfWeek > (int)interval.End.DayOfWeek || (interval.End - interval.Start).TotalDays>6)
                {
                    Interval newInterval = new Interval
                    {
                        Start = interval.Start,
                        End = (interval.Start + new TimeSpan((7 - (int)interval.Start.DayOfWeek), 0, 0, 0)).Date
                    };
                    interval.Start = newInterval.End + new TimeSpan(1, 0, 0, 0);
                }
                yield return interval;
            }
        }
        /// <summary>
        /// A day is worth 8 hours of active time.
        /// After days are counted, only counts hours up to 15, then resets and starts counting again, ex. Start 12:00AM, End 4:30PM same day gets 30 mins. Not 15.5 hours.
        /// </summary>
        /// <param name="intervals"></param>
        /// <returns></returns>
        public static TimeSpan GetTimeSpanOfIntervals(IEnumerable<Interval> intervals)
        {
            TimeSpan timeSpan = new TimeSpan(0);
            foreach (Interval interval in intervals)
            {
                TimeSpan _ts = interval.End.Subtract(interval.Start);
                _ts = new TimeSpan(0, _ts.Days * 8 + _ts.Hours % 15, _ts.Minutes, _ts.Seconds);
                timeSpan += _ts;
            }
            return timeSpan;
        }
        public static IEnumerable<Interval> MergeOverlappingIntervals(IEnumerable<Interval> intervals)
        {
            if(intervals.Count()<2)
            {
                yield break;
            }
            intervals = RemoveSundayFromIntervals(intervals);
            intervals = intervals.OrderBy(interval => interval.Start);
            Interval accumulator = intervals.First();
            intervals = intervals.Skip(1);

            foreach (var interval in intervals)
            {
                if (interval.Start <= accumulator.End)
                {
                    accumulator = Combine(accumulator, interval);
                }
                else
                {
                    yield return accumulator;
                    accumulator = interval;
                }
            }

            yield return accumulator;
        }
        private static Interval Combine(Interval start, Interval end)
        {
            return new Interval
            {
                Start = start.Start,
                End = Max(start.End, end.End),
            };
        }
        private static DateTime Max(DateTime left, DateTime right)
        {
            return (left > right) ? left : right;
        }
    }
}
