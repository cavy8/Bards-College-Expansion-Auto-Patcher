using System;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda.Skyrim;

var t = typeof(IPlacedObject);
var p = t.GetProperty("Base");
Console.WriteLine($"IPlacedObject.Base type: {p?.PropertyType.FullName}");
if (p != null)
{
    var pt = p.PropertyType;
    Console.WriteLine("Constructors:");
    foreach (var c in pt.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
        Console.WriteLine("  " + c);

    Console.WriteLine("Public static methods:");
    foreach (var m in pt.GetMethods(BindingFlags.Public|BindingFlags.Static))
        Console.WriteLine("  " + m);

    Console.WriteLine("Writable properties:");
    foreach (var wp in pt.GetProperties(BindingFlags.Public|BindingFlags.Instance).Where(x=>x.CanWrite))
        Console.WriteLine($"  {wp.Name}: {wp.PropertyType.FullName}");
}
