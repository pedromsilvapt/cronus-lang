using CronusLang;
using CronusLang.ByteCode;
using CronusLang.Compiler;
using CronusLang.Parser;

var parser = new CronusParser();
string code = @"
let fib :: Int n -> Int {
	let n1 = fib (n - 1);
	let n2 = fib (n - 2);

	if n <= 2 then 1 else n1 + n2
}

let main = fib 5;
";
var ast = parser.Parse(code);

var compiler = new Compiler();

var result = compiler.Compile(ast);

if (result.IsSuccessfull)
{
    // Print human-friendly bytecode assembly
    //Console.WriteLine("\n\n// Assembled Code");
    //Console.Write(compiler.AssembleText());

    var vm = new CronusVM(result.AssembledInstructions);
    vm.Execute();

    Console.WriteLine(vm.Stack.PopInt());
}
else
{
    foreach (var msg in result.Diagnostics)
    {
        Console.WriteLine(String.Format("{0}: {1}", msg.Level, msg.Message));
    }
}
