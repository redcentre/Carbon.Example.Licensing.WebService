using System;
using System.Threading.Tasks;

namespace RCS.Licensing.Example.WebService.UnitTests;

public class Program
{
	[STAThread]
	static async Task Main(string[] args)
	{
		// This class can be used for running test from the command line.
		await Task.CompletedTask;
		Console.WriteLine("PAUSE...");
		Console.ReadLine();
	}
}
