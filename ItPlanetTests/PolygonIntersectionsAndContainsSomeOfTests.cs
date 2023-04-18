using g3;
using ItPlanetAPI.Extensions;

namespace ItPlanetTests;

public class PolygonIntersectionsAndContainsSomeOfTests
{
    private Polygon2d _polygon;

    [SetUp]
    public void SetupPolygon()
    {
        /*
         * Rough shape estimation:
         * 
         *  .  __  __
         *  |\/  \/  \
         *  |_________\
         * 
         */
        _polygon = new Polygon2d(new Vector2d[]
        {
            new (0,0),
            new (0,10),
            new (5,5),
            new (10,10),
            new (20,10),
            new (25,5),
            new (30,10),
            new (40,10),
            new (45,5),
            new (50,0),
        });
    }

    [Test]
    public void IntersectsWithSelfOnlyBoundaryWise()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_polygon.Intersects(_polygon), Is.True);
            Assert.That(_polygon.ContainsSomeOf(_polygon), Is.False);
        });
    }
    [Test]
    public void IntersectsBothWaysWhenInside()
    {
        var polygonCompletelyInside = new Polygon2d(new Vector2d[]
        {
            new(10,7),
            new(20,7),
            new(20,3),
            new(10,3),
        });
        Assert.Multiple(() =>
        {
            Assert.That(_polygon.Intersects(_polygon), Is.True);
            Assert.That(_polygon.ContainsSomeOf(_polygon), Is.True);
        });
    }
    [Test]
    public void IntersectsBothWaysWhenInsideBoundaryWise()
    {
        var polygonInsideTouchingBoundaries = new Polygon2d(new Vector2d[]
        {
            new(10,10),
            new(20,10),
            new(20,0),
            new(10,0),
        });
        Assert.Multiple(() =>
        {
            Assert.That(_polygon.Intersects(_polygon), Is.True);
            Assert.That(_polygon.ContainsSomeOf(_polygon), Is.True);
        });
    }
}