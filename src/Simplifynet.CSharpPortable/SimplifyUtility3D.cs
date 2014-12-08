using System.Collections.Generic;

namespace Simplifynet
{
    public class SimplifyUtility3D
    {
        // square distance between 2 points
        public static double GetSquareDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X,
                dy = p1.Y - p2.Y,
                dz = p1.Z - p2.Z;

            return (dx*dx) + (dy*dy) + (dz*dz);
        }

        // square distance from a point to a segment
        public static double GetSquareSegmentDistance(Point p, Point p1, Point p2)
        {
            var x = p1.X;
            var y = p1.Y;
            var z = p1.Z;
            var dx = p2.X - x;
            var dy = p2.Y - y;
            var dz = p2.Z - z;

            if (!dx.Equals(0.0) || !dy.Equals(0.0) || !dz.Equals(0.0))
            {
                var t = ((p.X - x) * dx + (p.Y - y) * dy + (p.Z - z) * dz) / (dx * dx + dy * dy + dz * dz);

                if (t > 1)
                {
                    x = p2.X;
                    y = p2.Y;
                    z = p2.Z;
                }
                else if (t > 0)
                {
                    x += dx*t;
                    y += dy*t;
                    z += dz*t;
                }
            }

            dx = p.X - x;
            dy = p.Y - y;
            dz = p.Z - z;

            return (dx*dx) + (dy*dy) + (dz * dz);
        }

        // rest of the code doesn't care about point format

        // basic distance-based simplification
        public static List<Point> SimplifyRadialDistance(Point[] points, double sqTolerance)
        {
            var prevPoint = points[0];
            var newPoints = new List<Point> {prevPoint};
            Point point = null;

            for (var i = 1; i < points.Length; i++)
            {
                point = points[i];

                if (GetSquareDistance(point, prevPoint) > sqTolerance)
                {
                    newPoints.Add(point);
                    prevPoint = point;
                }
            }

            if (point != null && !prevPoint.Equals(point))
                newPoints.Add(point);

            return newPoints;
        }

        // simplification using optimized Douglas-Peucker algorithm with recursion elimination
        public static List<Point> SimplifyDouglasPeucker(Point[] points, double sqTolerance)
        {
            var len = points.Length;
            var markers = new int?[len];
            int? first = 0;
            int? last = len - 1;
            int? index = 0;
            var stack = new List<int?>();
            var newPoints = new List<Point>();

            markers[first.Value] = markers[last.Value] = 1;

            while (last != null)
            {
                var maxSqDist = 0.0d;

                for (int? i = first + 1; i < last; i++)
                {
                    var sqDist = GetSquareSegmentDistance(points[i.Value], points[first.Value], points[last.Value]);

                    if (sqDist > maxSqDist)
                    {
                        index = i;
                        maxSqDist = sqDist;
                    }
                }

                if (maxSqDist > sqTolerance)
                {
                    markers[index.Value] = 1;
                    stack.AddRange(new[] { first, index, index, last });
                }


                if (stack.Count > 0)
                {
                    last = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                    last = null;

                if (stack.Count > 0)
                {
                    first = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                    first = null;
            }

            for (var i = 0; i < len; i++)
            {
                if (markers[i] != null)
                    newPoints.Add(points[i]);
            }

            return newPoints;
        }


        public static List<Point> Simplify(Point[] points, double tolerance = 0.3, bool highestQuality = false)
        {
            if(points == null || points.Length == 0)
                return new List<Point>();

            var sqTolerance = tolerance*tolerance;

            if (!highestQuality)
            {
                List<Point> points2 = SimplifyRadialDistance(points, sqTolerance);
                return SimplifyDouglasPeucker(points2.ToArray(), sqTolerance);
            }

            return SimplifyDouglasPeucker(points, sqTolerance);
        }
    }
}
