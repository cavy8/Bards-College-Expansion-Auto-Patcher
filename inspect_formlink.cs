using System;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda.Plugins;

var asm = typeof(FormKey).Assembly;
var types = asm.GetTypes().Where(t => t.Name.Contains("FormLink")).Take(30).ToList();
foreach (var type in types)
{
    Console.WriteLine(type.FullName);
}
