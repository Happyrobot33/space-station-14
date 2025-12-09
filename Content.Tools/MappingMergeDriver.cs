using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Content.Tests;
using NUnit.Framework;

namespace Content.Tools
{
    internal static class MappingMergeDriver
    {
        /// %A: Our file
        /// %O: Origin (common, base) file
        /// %B: Other file
        /// %P: Actual filename of the resulting file
        public static void Main(string[] args)
        {
            //get a list of all methods with the test attribute
            if (args.Length != 3)
            {
                var assemblies = new List<Assembly>
                {
                    typeof(ContentUnitTest).Assembly,
                    typeof(Content.IntegrationTests.Tests.Access.AccessReaderTest).Assembly,
                    // typeof(Robust.UnitTesting.IIntegrationInstance).Assembly //bugged a bit
                };

                List<String> classNames = new();
                foreach (var assembly in assemblies)
                {
                    var methods = assembly.GetTypes()
                        .SelectMany(t => t.GetMethods())
                        .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Length > 0)
                        .ToArray();   
                    System.Console.WriteLine($"Assembly {assembly.GetName().Name} has {methods.Length} tests:");
                    
                    foreach (var method in methods)
                    {
                        if (!classNames.Contains(method.DeclaringType.FullName))
                            classNames.Add(method.DeclaringType.FullName);
                    }
                }
                System.Console.WriteLine($"{classNames.Count} Total Classes Gathered");

                List<List<String>> testLists = new(); 
                const int numLists = 3;

                //split evenly into numLists lists
                for (int i = 0; i < numLists; i++)
                {
                    testLists.Add(new List<string>());
                }
                for (int i = 0; i < classNames.Count; i++)
                {
                    testLists[i % numLists].Add(classNames[i]);
                }

                //make sure the total count matches
                int totalCount = 0;
                for (int i = 0; i < numLists; i++)
                {
                    totalCount += testLists[i].Count;
                }
                System.Console.WriteLine($"Total Count After Split: {totalCount}");
                if (totalCount != classNames.Count)
                {
                    throw new Exception("Total count after split does not match original count!");
                }

                //now construct into a filter command
                // FullyQualifiedName=XX | FullyQualifiedName=YY | ...
                List<String> filters = new();
                for (int i = 0; i < numLists; i++)
                {
                    var filter = string.Join(" | ", testLists[i].Select(x => $"FullyQualifiedName~{x}"));
                    filters.Add(filter);
                    //System.Console.WriteLine($"List {i + 1} Filter: {filter}");
                }

                // //write to files
                // for (int i = 0; i < numLists; i++)
                // {
                //     System.IO.File.WriteAllText($"TestList_{i + 1}.txt", filters[i]);
                // }

                //build up a mega command
                List<String> commands = new();
                for (int i = 0; i < numLists; i++)
                {
                    commands.Add($"dotnet test --no-build --logger trx --filter=\"{filters[i]}\"");
                }

                //write out as one command using &
                var megaCommand = string.Join(" & \n", commands);
                //write out to file
                System.IO.File.WriteAllText("RunAllTestsCommand.txt", megaCommand);
            }

            return;
            var ours = new Map(args[0]);
            var based = new Map(args[1]); // On what?
            var other = new Map(args[2]);

            if (ours.GridsNode.Children.Count != 1 || based.GridsNode.Children.Count != 1 || other.GridsNode.Children.Count != 1)
            {
                Console.WriteLine("one or more files had an amount of grids not equal to 1");
                Environment.Exit(1);
            }

            if (!(new Merger(ours, based, other).Merge()))
            {
                Console.WriteLine("unable to merge!");
                Environment.Exit(1);
            }

            ours.Save();
            Environment.Exit(0);
        }
    }
}
