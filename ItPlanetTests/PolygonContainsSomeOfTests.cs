
using ItPlanetAPI.Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace ItPlanetTests;

public class PolygonContainsSomeOfTests
{
    private Polygon _polygon;

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
        _polygon = new Polygon(new LinearRing(new Coordinate[]
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
            new (0,0),
        }));
    }

    [Test]
    public void ContainsSomeOfItself()
    {
        Assert.That(_polygon.ContainsSomeOf(_polygon), Is.True);
    }
    [Test]
    public void ContainsSomeOfPolygonCompletelyInside()
    {
        var polygonCompletelyInside = new Polygon(new LinearRing(new Coordinate[]
        {
            new(10,7),
            new(20,7),
            new(20,3),
            new(10,3),
            new(10,7),
        }));
        Assert.That(_polygon.ContainsSomeOf(polygonCompletelyInside), Is.True);
    }
    [Test]
    public void ContainsSomeOfPolygonInsideSharingBoundaries()
    {
        var polygonInsideSharingBoundaries = new Polygon(new LinearRing(new Coordinate[]
        {
            new(10,10),
            new(20,10),
            new(20,0),
            new(10,0),
            new(10,10),
        }));
        Assert.That(_polygon.ContainsSomeOf(polygonInsideSharingBoundaries), Is.True);
    }
    [Test]
    public void ContainsSomeOfPolygonPartiallyInside()
    {
        var polygonInsidePartially = new Polygon(new LinearRing(new Coordinate[]
        {
            new(10,100),
            new(20,100),
            new(20,0),
            new(10,0),
            new(10,100),
        }));
        Assert.That(_polygon.ContainsSomeOf(polygonInsidePartially), Is.True);
    }
    [Test]
    public void DoesNotContainSomeOfPolygonTouchingBoundaries()
    {
        var polygonTouchingBoundaries = new Polygon(new LinearRing(new Coordinate[]
        {
            new(10,-100),
            new(20,-100),
            new(20,0),
            new(10,0),
            new(10,-100),
        }));
        Assert.That(_polygon.ContainsSomeOf(polygonTouchingBoundaries), Is.False);
    }
    [Test]
    public void DoesNotContainSomeOfFarawayPolygon()
    {
        var polygonFarAway = new Polygon(new LinearRing(new Coordinate[]
        {
            new(10,-100),
            new(20,-100),
            new(20,-50),
            new(10,-50),
            new(10,-100),
        }));
        Assert.That(_polygon.ContainsSomeOf(polygonFarAway), Is.False);
    }
    [Test]
    public void ContainsPolygonWithTwoOverlappingAreas()
    {
        var polygonOverlappingTwice = new Polygon(new LinearRing(new Coordinate[]
        {
            new(0,10),
            new(10,10),
            new(10,-50),
            new(30,-50),
            new(30,10),
            new(40,10),
            new(40,-60),
            new(0,-60),
            new(0,10),
        }));
        Assert.That(_polygon.ContainsSomeOf(polygonOverlappingTwice), Is.True);
    }
}