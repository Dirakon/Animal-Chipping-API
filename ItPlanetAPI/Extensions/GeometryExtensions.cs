using g3;
using ItPlanetAPI.Models;
using ISpatial = ItPlanetAPI.Models.ISpatial;

namespace ItPlanetAPI.Extensions;

public static class GeometryExtensions
{
    public const double StandardGeometryEpsilon = 0.0001;

    public static IEnumerable<Segment2d> AsSegments(this IEnumerable<ISpatial> points)
    {
        using var enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
            yield break;
        var firstPoint = enumerator.Current.AsVector();
        var previousPoint = firstPoint;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current.AsVector();
            yield return new Segment2d(previousPoint, somePoint);
            previousPoint = somePoint;
        }

        if (!firstPoint.EpsilonEqual(previousPoint, StandardGeometryEpsilon))
            yield return new Segment2d(previousPoint, firstPoint);
    }

    public static RelativePosition RelativePositionOf(this Segment2d segment2d, Vector2d somePoint)
    {
        var sideOfTheLine = segment2d.WhichSide(somePoint, StandardGeometryEpsilon);
        if (sideOfTheLine != 0)
            return sideOfTheLine == -1 ? RelativePosition.ToRightOfSegment : RelativePosition.ToLeftOfSegment;
        if (segment2d.P0.EpsilonEqual(somePoint, StandardGeometryEpsilon) ||
            segment2d.P1.EpsilonEqual(somePoint, StandardGeometryEpsilon))
            return RelativePosition.OnSegment;
        var dirToP0 = (segment2d.P0 - somePoint).Normalized;
        var dirToP1 = (segment2d.P1 - somePoint).Normalized;
        var pointBetweenThem = dirToP0.EpsilonEqual(-dirToP1, StandardGeometryEpsilon);
        return pointBetweenThem ? RelativePosition.OnSegment : RelativePosition.OutsideOnTheLine;
    }

    public static bool Includes(this Segment2d segment2d, Vector2d somePoint)
    {
        return segment2d.RelativePositionOf(somePoint) == RelativePosition.OnSegment;
    }

    public static bool ClosedShapeSelfIntersects(this IEnumerable<Segment2d> segments)
    {
        var segment2ds = segments as Segment2d[] ?? segments.ToArray();
        for (var seg1Index = 0; seg1Index < segment2ds.Length; seg1Index++)
        {
            var seg1 = segment2ds[seg1Index];
            for (var seg2Index = seg1Index + 1; seg2Index < segment2ds.Length; seg2Index++)
            {
                var seg2 = segment2ds[seg2Index];
                if (seg2Index == seg1Index + 1)
                {
                    // If the next segment goes into the previousOne
                    if (seg2.Direction.EpsilonEqual(-seg1.Direction, StandardGeometryEpsilon))
                        return true;
                    continue;
                }

                if (seg1Index == 0 && seg2Index == segment2ds.Length - 1)
                {
                    // If the first segment goes into the last one
                    if (seg2.Direction.EpsilonEqual(-seg1.Direction, StandardGeometryEpsilon))
                        return true;
                    continue;
                }

                if (seg1.Intersects(seg2))
                    return true;
            }
        }

        return false;
    }
    public record ByDistanceFromPoint(Vector2d OriginPoint) : IComparer<Vector2d>
    {
        public int Compare(Vector2d point1, Vector2d point2)
        {
            return point1.EpsilonEqual(point2, StandardGeometryEpsilon) ? 0 
                : Comparer<double>.Default.Compare(OriginPoint.Distance(point1), OriginPoint.Distance(point2));
        }
    }

    /**
     * Total version of Line2d.IntersectionPoint, which return null instead of Vector2d.MaxValue when no
     * intersection can be found.
     */
    public static Vector2d? IntersectionPointTotal(this Line2d line, Line2d otherLine)
    {
        return line.IntersectionPoint(ref otherLine, StandardGeometryEpsilon) switch
        {
            var intersectionPoint when intersectionPoint == Vector2d.MaxValue => null,
            var intersectionPoint => intersectionPoint
        };
    }

    public static Line2d AsLine(this Segment2d segment)
    {
        return new Line2d(segment.Center, segment.Direction);
    }
    public static bool ContainsSomeOf(this Polygon2d polygon1, Polygon2d polygon2)
    {
        if (!polygon1.GetBounds().Intersects(polygon2.GetBounds()))
            return false;
        foreach (var seg1 in polygon1.SegmentItr())
        {
            SortedSet<Vector2d> segmentPointsSplitByIntersections = new(new ByDistanceFromPoint(seg1.P0))
            {
                seg1.P0,
                seg1.P1
            };
            foreach (var seg2 in polygon2.SegmentItr())
            {
                if (!seg1.Intersects(seg2)) continue;
                if (seg1.AsLine().IntersectionPointTotal(seg2.AsLine()) is { } someValidIntersectionPoint)
                {
                    segmentPointsSplitByIntersections.Add(someValidIntersectionPoint);
                }
            }

            List<Vector2d> splitSegmentCenters = new(segmentPointsSplitByIntersections.Count - 1);
            Vector2d previousPoint = segmentPointsSplitByIntersections.First();
            foreach (var nextPoint in segmentPointsSplitByIntersections.Skip(1))
            {
                splitSegmentCenters.Add((nextPoint + previousPoint)/2);

                previousPoint = nextPoint;
            }

            foreach (var splitSegmentCenter in splitSegmentCenters)
            {
                if (polygon2.SegmentItr().Any(segment => segment.Includes(splitSegmentCenter)))
                    continue;
                if (polygon2.Contains(splitSegmentCenter))
                    return true;
            }

            // RelativePosition? polygon2PointsRelativeToPolygon1 = null;
            // foreach (var seg2 in polygon2.SegmentItr())
            // {
            //     if (!seg1.Intersects(seg2)) continue;
            //
            //     // TODO: fix overlooked problem when two segments one by one go through the polygon bounds
            //     // by intersecting "boundary-wise" from different sides
            //     var relativePositionOfPoints = new[] {seg2.P0, seg2.P1}
            //         .Select(seg2Point => seg1.RelativePositionOf(seg2Point))
            //         .ToArray();
            //     bool isToLeftSide = relativePositionOfPoints.Contains(RelativePosition.ToLeftOfSegment);
            //     bool isToRightSide = relativePositionOfPoints.Contains(RelativePosition.ToRightOfSegment);
            //     bool isOneTheLineOutside = relativePositionOfPoints.Contains(RelativePosition.OutsideOnTheLine);
            //
            //     switch (isToLeftSide,isToRightSide)
            //     {
            //         case (true,true):
            //             // Seg2 goes straight through seg1
            //             return true;
            //         
            //         case (true,false) when polygon2PointsRelativeToPolygon1 != null && polygon2PointsRelativeToPolygon1!= RelativePosition.ToLeftOfSegment:
            //             //
            //             return true;
            //         case (true,false):
            //             polygon2PointsRelativeToPolygon1 = RelativePosition.ToLeftOfSegment;
            //             break;
            //         
            //         case (false,true) when polygon2PointsRelativeToPolygon1 != null && polygon2PointsRelativeToPolygon1 != RelativePosition.ToRightOfSegment:
            //             return true;
            //         case (false,true):
            //             polygon2PointsRelativeToPolygon1 = RelativePosition.ToRightOfSegment;
            //             break;
            //         
            //         case (false,false):
            //             continue;
            //     }
            // }
        }

        return false;
    }

    public static bool IntersectsNonBoundaryWise(this Polygon2d polygon1, Polygon2d polygon2)
    {
        if (!polygon1.GetBounds().Intersects(polygon2.GetBounds()))
            return false;
        foreach (var seg1 in polygon1.SegmentItr())
        {
            SortedSet<Vector2d> segmentPointsSplitByIntersections = new(new ByDistanceFromPoint(seg1.P0))
            {
                seg1.P0,
                seg1.P1
            };
            foreach (var seg2 in polygon2.SegmentItr())
            {
                if (!seg1.Intersects(seg2)) continue;
                if (seg1.AsLine().IntersectionPointTotal(seg2.AsLine()) is { } someValidIntersectionPoint)
                {
                    segmentPointsSplitByIntersections.Add(someValidIntersectionPoint);
                }
            }

            List<Vector2d> splitSegmentCenters = new(segmentPointsSplitByIntersections.Count - 1);
            Vector2d previousPoint = segmentPointsSplitByIntersections.First();
            foreach (var nextPoint in segmentPointsSplitByIntersections.Skip(1))
            {
                splitSegmentCenters.Add((nextPoint + previousPoint)/2);

                previousPoint = nextPoint;
            }

            foreach (var splitSegmentCenter in splitSegmentCenters)
            {
                if (polygon2.SegmentItr().Any(segment => segment.Includes(splitSegmentCenter)))
                    continue;
                if (polygon2.Contains(splitSegmentCenter))
                    return true;
            }

            // RelativePosition? polygon2PointsRelativeToPolygon1 = null;
            // foreach (var seg2 in polygon2.SegmentItr())
            // {
            //     if (!seg1.Intersects(seg2)) continue;
            //
            //     // TODO: fix overlooked problem when two segments one by one go through the polygon bounds
            //     // by intersecting "boundary-wise" from different sides
            //     var relativePositionOfPoints = new[] {seg2.P0, seg2.P1}
            //         .Select(seg2Point => seg1.RelativePositionOf(seg2Point))
            //         .ToArray();
            //     bool isToLeftSide = relativePositionOfPoints.Contains(RelativePosition.ToLeftOfSegment);
            //     bool isToRightSide = relativePositionOfPoints.Contains(RelativePosition.ToRightOfSegment);
            //     bool isOneTheLineOutside = relativePositionOfPoints.Contains(RelativePosition.OutsideOnTheLine);
            //
            //     switch (isToLeftSide,isToRightSide)
            //     {
            //         case (true,true):
            //             // Seg2 goes straight through seg1
            //             return true;
            //         
            //         case (true,false) when polygon2PointsRelativeToPolygon1 != null && polygon2PointsRelativeToPolygon1!= RelativePosition.ToLeftOfSegment:
            //             //
            //             return true;
            //         case (true,false):
            //             polygon2PointsRelativeToPolygon1 = RelativePosition.ToLeftOfSegment;
            //             break;
            //         
            //         case (false,true) when polygon2PointsRelativeToPolygon1 != null && polygon2PointsRelativeToPolygon1 != RelativePosition.ToRightOfSegment:
            //             return true;
            //         case (false,true):
            //             polygon2PointsRelativeToPolygon1 = RelativePosition.ToRightOfSegment;
            //             break;
            //         
            //         case (false,false):
            //             continue;
            //     }
            // }
        }

        return false;
    }

    public static Polygon2d AsPolygon(this IEnumerable<ISpatial> points)
    {
        return new Polygon2d(points.Select(point => point.AsVector()));
    }

    public static bool EpsilonEqual(this Segment2d seg1, Segment2d seg2, double epsilon)
    {
        return seg1.P0.EpsilonEqual(seg2.P0, epsilon) && seg1.P1.EpsilonEqual(seg2.P1, epsilon);
    }

    public static bool AreOnOneLine(this IEnumerable<Vector2d> points)
    {
        using var enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
            return false;
        var firstPoint = enumerator.Current;
        Vector2d? firstNonZeroDirection = null;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current;
            var direction = somePoint - firstPoint;
            if (direction.EpsilonEqual(Vector2d.Zero, StandardGeometryEpsilon))
                continue;
            direction = direction.Normalized;
            if (firstNonZeroDirection is { } firstDirection)
            {
                if (!direction.EpsilonEqual(firstDirection, StandardGeometryEpsilon) && !direction.EpsilonEqual(-firstDirection, StandardGeometryEpsilon))
                    return false;
            }
            else
            {
                firstNonZeroDirection = direction;
            }
        }

        return true;
    }

    public static bool AreOnOneLine(this IEnumerable<ISpatial> points)
    {
        return points.Select(point => point.AsVector()).AreOnOneLine();
    }
}