using g3;
using ItPlanetAPI.Extensions;

namespace ItPlanetTests;

public class SegmentRelativePositionAnalysisTests
{
    private Segment2d _segment2d;

    [SetUp]
    public void SetupSegment()
    {
        _segment2d = new Segment2d(new Vector2d(1, 2), new Vector2d(4, 10));
    }

    [Test]
    public void InsideSegment()
    {
        var pointSomewhereInBetween = _segment2d.P0 + (_segment2d.P1 - _segment2d.P0).Normalized * 2;
        Assert.That(_segment2d.RelativePositionOf(pointSomewhereInBetween), Is.EqualTo(RelativePosition.OnSegment));
    }

    [Test]
    public void OnSegmentPoints()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_segment2d.RelativePositionOf(_segment2d.P0), Is.EqualTo(RelativePosition.OnSegment));
            Assert.That(_segment2d.RelativePositionOf(_segment2d.P1), Is.EqualTo(RelativePosition.OnSegment));
        });
    }

    [Test]
    public void OnTheLineOutsideSegment()
    {
        var pointOnLineOutside = _segment2d.P0 + (_segment2d.P1 - _segment2d.P0).Normalized * 200;
        Assert.That(_segment2d.RelativePositionOf(pointOnLineOutside), Is.EqualTo(RelativePosition.OutsideOnTheLine));
    }

    [Test]
    public void DifferentSides()
    {
        var pointSomewhereInBetween = _segment2d.P0 + (_segment2d.P1 - _segment2d.P0).Normalized * 2;
        var includeResultsToSides = new[] {new Vector2d(-5, 0), new Vector2d(5, 0)}
            .Select(sideOffset => pointSomewhereInBetween + sideOffset)
            .Select(pointToTheSide => _segment2d.RelativePositionOf(pointToTheSide))
            .ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(includeResultsToSides, Contains.Item(RelativePosition.ToLeftOfSegment));
            Assert.That(includeResultsToSides, Contains.Item(RelativePosition.ToRightOfSegment));
        });
    }
}