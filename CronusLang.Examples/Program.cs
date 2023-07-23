using CronusLang;
using CronusLang.ByteCode;
using CronusLang.Compiler;
using CronusLang.Parser;
using System.Diagnostics;

var parser = new CronusParser();
string code = @"
    let fib :: Int n -> Int {
	    let n1 = n - 1;
	    let n2 = n - 2;

	    if n <= 2 then 1 else {
		    let f1 = fib n1;
		    let f2 = fib n2;
		
		    f1 + f2
	    }
    }

    let math :: Int i -> Decimal d -> Bool {
        let n = i + d;
        let cond = n > 0.5 and n < 5;

        not cond
    }

    let main = math 0 2;
    ";

var ast = parser.Parse(code);

var compiler = new Compiler();

var result = compiler.Compile(ast);

if (result.IsSuccessfull)
{
    // Print human-friendly bytecode assembly
    Console.WriteLine("\n\n// Assembled Code");
    Console.Write(compiler.AssembleText());

    var vm = new CronusVM(result.AssembledInstructions);
    var clock = new Stopwatch();
    clock.Start();
    vm.Execute();
    clock.Stop();

    vm.Stack.Compact();
    //vm.Stack.Cursor = 0;
    //while (!vm.Stack.EOF)
    //{
    //    Console.WriteLine(vm.Stack.Cursor.ToString("X8") + ": " + vm.Stack.ReadInt());
    //}
    Console.WriteLine("Result: {0}", vm.Stack.PopBool());
    Console.WriteLine("Elapsed time: {0}", clock.Elapsed);
}
else
{
    foreach (var msg in result.Diagnostics)
    {
        Console.WriteLine(String.Format("{0}: {1}", msg.Level, msg.Message));
    }
}
