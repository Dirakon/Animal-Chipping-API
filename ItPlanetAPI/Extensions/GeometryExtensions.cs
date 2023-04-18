using System.Numerics;
using ItPlanetAPI.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;
using NetTopologySuite.Triangulate;
using ISpatial = ItPlanetAPI.Models.ISpatial;

namespace ItPlanetAPI.Extensions;

public static class GeometryExtensions
{
    public const double StandardGeometryEpsilon = 0.0001;

    public static IEnumerable<LineSegment> AsSegments(this IEnumerable<ISpatial> points)
    {
        using var enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
            yield break;
        var firstPoint = enumerator.Current.AsCoordinate();
        var previousPoint = firstPoint;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current.AsCoordinate();
            yield return new LineSegment(previousPoint, somePoint);
            previousPoint = somePoint;
        }

        if (!firstPoint.Equals2D(previousPoint, StandardGeometryEpsilon))
            yield return new LineSegment(previousPoint, firstPoint);
    }

    
    public static bool Includes(this LineSegment segment2d, Coordinate somePoint)
    {
        if (segment2d.P0.Equals2D(somePoint, StandardGeometryEpsilon) ||
            segment2d.P1.Equals2D(somePoint, StandardGeometryEpsilon))
            return true;
        var dirToP0 = somePoint.DirectionTo(segment2d.P0);
        var dirToP1 = somePoint.DirectionTo(segment2d.P1);
        var pointBetweenThem = dirToP0.EpsilonEquals(-dirToP1);
        return pointBetweenThem;
    }

    public static Vector2D DirectionTo(this Coordinate coordinate1, Coordinate coordinate2)
    {
        return (new Vector2D(coordinate2) - new Vector2D(coordinate1)).Normalize();
    }
    public static Vector2D Direction(this LineSegment segment)
    {
        return segment.P1.DirectionTo(segment.P0);
    }
    public static bool EpsilonEquals(this Vector2D vector1, Vector2D vector2, double epsilon = StandardGeometryEpsilon)
    {
        return vector1.Distance(vector2) <= epsilon;
    }
    public static bool ClosedShapeSelfIntersects(this IEnumerable<LineSegment> segments)
    {
        var segment2ds = segments as LineSegment[] ?? segments.ToArray();
        for (var seg1Index = 0; seg1Index < segment2ds.Length; seg1Index++)
        {
            var seg1 = segment2ds[seg1Index];
            for (var seg2Index = seg1Index + 1; seg2Index < segment2ds.Length; seg2Index++)
            {
                var seg2 = segment2ds[seg2Index];
                if (seg2Index == seg1Index + 1)
                {
                    // If the next segment goes into the previousOne
                    if (seg2.Direction().EpsilonEquals(-seg1.Direction()))
                        return true;
                    continue;
                }

                if (seg1Index == 0 && seg2Index == segment2ds.Length - 1)
                {
                    // If the first segment goes into the last one
                    if (seg2.Direction().EpsilonEquals(-seg1.Direction(), StandardGeometryEpsilon))
                        return true;
                    continue;
                }

                if (seg1.Intersection(seg2) != null)
                    return true;
            }
        }

        return false;
    }
    // public record ByDistanceFromPoint(Vector2d OriginPoint) : IComparer<Vector2d>
    // {
    //     public int Compare(Vector2d point1, Vector2d point2)
    //     {
    //         return point1.EpsilonEqual(point2, StandardGeometryEpsilon) ? 0 
    //             : Comparer<double>.Default.Compare(OriginPoint.Distance(point1), OriginPoint.Distance(point2));
    //     }
    // }

    public static bool ContainsSomeOf(this Polygon polygon1, Polygon polygon2)
    {
        var intersectionShape = polygon1.Intersection(polygon2);
        return !intersectionShape.IsEmpty && intersectionShape.Dimension == Dimension.Surface;
    }

    // public static bool IntersectsNonBoundaryWise(this Polygon2d polygon1, Polygon2d polygon2)
    // {
    //     if (!polygon1.GetBounds().Intersects(polygon2.GetBounds()))
    //         return false;
    //     foreach (var seg1 in polygon1.SegmentItr())
    //     {
    //         SortedSet<Vector2d> segmentPointsSplitByIntersections = new(new ByDistanceFromPoint(seg1.P0))
    //         {
    //             seg1.P0,
    //             seg1.P1
    //         };
    //         foreach (var seg2 in polygon2.SegmentItr())
    //         {
    //             if (!seg1.Intersects(seg2)) continue;
    //             if (seg1.AsLine().IntersectionPointTotal(seg2.AsLine()) is { } someValidIntersectionPoint)
    //             {
    //                 segmentPointsSplitByIntersections.Add(someValidIntersectionPoint);
    //             }
    //         }
    //
    //         List<Vector2d> splitSegmentCenters = new(segmentPointsSplitByIntersections.Count - 1);
    //         Vector2d previousPoint = segmentPointsSplitByIntersections.First();
    //         foreach (var nextPoint in segmentPointsSplitByIntersections.Skip(1))
    //         {
    //             splitSegmentCenters.Add((nextPoint + previousPoint)/2);
    //
    //             previousPoint = nextPoint;
    //         }
    //
    //         foreach (var splitSegmentCenter in splitSegmentCenters)
    //         {
    //             if (polygon2.SegmentItr().Any(segment => segment.Includes(splitSegmentCenter)))
    //                 continue;
    //             if (polygon2.Contains(splitSegmentCenter))
    //                 return true;
    //         }
    //
    //         // RelativePosition? polygon2PointsRelativeToPolygon1 = null;
    //         // foreach (var seg2 in polygon2.SegmentItr())
    //         // {
    //         //     if (!seg1.Intersects(seg2)) continue;
    //         //
    //         //     // TODO: fix overlooked problem when two segments one by one go through the polygon bounds
    //         //     // by intersecting "boundary-wise" from different sides
    //         //     var relativePositionOfPoints = new[] {seg2.P0, seg2.P1}
    //         //         .Select(seg2Point => seg1.RelativePositionOf(seg2Point))
    //         //         .ToArray();
    //         //     bool isToLeftSide = relativePositionOfPoints.Contains(RelativePosition.ToLeftOfSegment);
    //         //     bool isToRightSide = relativePositionOfPoints.Contains(RelativePosition.ToRightOfSegment);
    //         //     bool isOneTheLineOutside = relativePositionOfPoints.Contains(RelativePosition.OutsideOnTheLine);
    //         //
    //         //     switch (isToLeftSide,isToRightSide)
    //         //     {
    //         //         case (true,true):
    //         //             // Seg2 goes straight through seg1
    //         //             return true;
    //         //         
    //         //         case (true,false) when polygon2PointsRelativeToPolygon1 != null && polygon2PointsRelativeToPolygon1!= RelativePosition.ToLeftOfSegment:
    //         //             //
    //         //             return true;
    //         //         case (true,false):
    //         //             polygon2PointsRelativeToPolygon1 = RelativePosition.ToLeftOfSegment;
    //         //             break;
    //         //         
    //         //         case (false,true) when polygon2PointsRelativeToPolygon1 != null && polygon2PointsRelativeToPolygon1 != RelativePosition.ToRightOfSegment:
    //         //             return true;
    //         //         case (false,true):
    //         //             polygon2PointsRelativeToPolygon1 = RelativePosition.ToRightOfSegment;
    //         //             break;
    //         //         
    //         //         case (false,false):
    //         //             continue;
    //         //     }
    //         // }
    //     }
    //
    //     return false;
    // }

    public static Polygon AsPolygon(this IEnumerable<ISpatial> points)
    {
        return new Polygon(new LinearRing(points.Select(point => point.AsCoordinate()).ToArray()));
    }

    // public static bool EpsilonEqual(this Segment2d seg1, Segment2d seg2, double epsilon)
    // {
    //     return seg1.P0.EpsilonEqual(seg2.P0, epsilon) && seg1.P1.EpsilonEqual(seg2.P1, epsilon);
    // }

    public static bool AreOnOneLine(this IEnumerable<Coordinate> points)
    {
        using var enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
            return false;
        var firstPoint = enumerator.Current;
        Vector2D? firstNonZeroDirection = null;


        while (enumerator.MoveNext())
        {
            var somePoint = enumerator.Current;
            if (firstPoint.Equals2D(somePoint,StandardGeometryEpsilon))
                continue;
            var direction = somePoint.DirectionTo(firstPoint);
            if (firstNonZeroDirection is { } firstDirection)
            {
                if (!direction.EpsilonEquals(firstDirection) && !direction.EpsilonEquals(-firstDirection))
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
        return points.Select(point => point.AsCoordinate()).AreOnOneLine();
    }
}