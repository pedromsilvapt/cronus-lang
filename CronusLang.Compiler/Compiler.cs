using CronusLang.TypeSystem;
using CronusLang.TypeSystem.Types;
using CronusLang.ByteCode;
using AST = CronusLang.Parser.AST;
using static CronusLang.ByteCode.ByteCode;
using Sawmill;
using Sawmill.Expressions;
using CronusLang.Compiler.SST;
using CronusLang.Compiler.Definitions;
using System.Text;

namespace CronusLang.Compiler
{
    public class Compiler
    {
        protected int _functionIdCounter = 0;

        protected int _labelIdCounter = 0;

        public Dictionary<Symbol, FunctionDefinition> FunctionsBySymbol { get; set; } = new Dictionary<Symbol, FunctionDefinition>();

        public Dictionary<int, FunctionDefinition> Functions { get; set; } = new Dictionary<int, FunctionDefinition>();

        public GlobalDefinition Global { get; set; } = new GlobalDefinition();

        /// <summary>
        /// Create a function definition. Instructions can then be added to the function definition
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public FunctionDefinition CreateFunction(Symbol symbol, FunctionTypeDefinition type, string[] argNames, Dictionary<SymbolsScopeEntry, SymbolDefinition> variables)
        {
            var id = _functionIdCounter++;

            var definition = new FunctionDefinition(id, symbol, type, argNames, variables);

            Functions[id] = definition;

            FunctionsBySymbol[symbol] = definition;

            return definition;
        }

        #region Compile

        /// <summary>
        /// Compile the given Script AST Node. Can then call `Assemble` to generate the final byte code
        /// </summary>
        /// <param name="script"></param>
        public CompilationResult Compile(AST.Script scriptAST)
        {
            TypesLibrary library = new TypesLibrary();

            SymbolsScope rootScope = SymbolsScope.CreateRoot();

            #region Register Native Types

            rootScope.RegisterType(library.Register(new BoolType("Bool")));
            rootScope.RegisterType(library.Register(new IntType("Int")));
            rootScope.RegisterType(library.Register(new DecimalType("Decimal")));

            #endregion

            #region Convert AST into non-analyzed SST

            // Create the transformer, automatically mapping AST Node Types to SST Node Types
            // based on their names and namespaces
            SemanticTransformer transformer = SemanticTransformer.FromReflection();

            // Recursively create the empty SST nodes from the AST ones
            SST.Script script = transformer.ToSST<SST.Script>(rootScope, scriptAST);

            #endregion

            #region Build Initial Semantic Dependency Graph

            // Create a list of all semantic nodes in our tree. We start from the bottom, the leafs (nodes at the bottom of the tree)
            // are usually the most likely to not have dependencies (literals, etc...)
            List<SST.Node> semanticNodes = ((SST.Node)script).DescendantsAndSelf().ToList();

            // Creates the semantic analyzer responsible for tracking dependencies between
            // the semantic nodes and resolving them in the proper order
            SemanticAnalyzer analyzer = new SemanticAnalyzer(library, semanticNodes);

            rootScope.Analyzer = analyzer;

            // Perform the analysis of the code
            analyzer.Analyze();

            #endregion
            
            var result = new CompilationResult(script, analyzer.Diagnostics);

            #region Type Check

            if (result.IsSuccessfull)
            {
                // TODO
            }

            #endregion

            #region Generate Instructions

            if (result.IsSuccessfull)
            {
                //var mainBinding = script.Bindings.First(binding => binding.GetSyntaxNode<AST.Binding>().Identifier.Name == "main");
                CompileScript(script);

                Assemble(result.AssembledInstructions);
            }

            #endregion

            return result;
        }

        protected void CompileScript(SST.Script script)
        {
            // TODO Emit debug markers (node locations, symbols information, types information, etc...)

            var symbolsSize = CreateBlockSymbols(Global.Variables, script);

            Emit(Global, OpCode.PushN, symbolsSize);

            foreach (var binding in script.Bindings)
            {
                CompileBinding(binding);
            }

            Emit(Global, OpCode.Halt);
        }
        
        protected void CompileBinding(SST.Binding binding)
        {
            CompileBinding(null, Global, binding);
        }

        protected void CompileBinding(FunctionDefinition? frame, InstructionsDefinition body, SST.Binding binding)
        {
            // Get the registered the symbol
            var identifier = binding.Identifier.GetSyntaxNode<AST.Identifier>().Name;

            var symbol = body.GetVariable(binding.Scope.ParentScope!.Lookup(identifier));

            if (binding.Type.Value is FunctionTypeDefinition functionType && binding.Signature?.Parameters?.Count() > 0)
            {
                var argNames = binding
                    .Signature!
                    .GetSyntaxNode<AST.BindingType>()
                    .Parameters
                    .Select(param => param.Identifier.Name)
                    .ToArray();

                var bindingNames = binding
                    .Block
                    .Bindings
                    .Select(bind => bind.GetSyntaxNode<AST.Binding>().Identifier.Name)
                    .ToArray();

                var symbols = CreateFunctionSymbols(functionType, binding, argNames, bindingNames, out int returnOffset, out int stackSymbolsSize);

                // When the symbol is a function, like in this case, the symbol itself will just be a pointer to the function id
                // The metadata regarding the function, such as the position of the bytecode, it's type and so on, will be stored in
                // a header structure
                var function = CreateFunction(binding.Scope.FullName!, functionType, argNames, symbols);
                function.StackPointerOffset = stackSymbolsSize;

                // Create the return variable
                function.CreateVariable("@return", functionType.ReturnType, returnOffset);

                // Function Id
                Emit(body, OpCode.PushInt, function.Id);
                // Context pointer (global means no context)
                Emit(body, OpCode.PushInt, 0);
                // Store the two integers on the assigned slot for this symbol
                Emit(body, body.StoreOperation, symbol.Index, symbol.Type.GetSize());

                // Reserve the symbols stack space for this function
                Emit(function, OpCode.PushN, function.StackPointerOffset);

                // Compile the block bindings and expression
                CompileBlock(function, function, binding.Block);

                var returnSymbol = function.GetVariable("@return");
                // Perform an implicit cast(if needed) from the block actual type and the binding return type
                CompileImplicitCast(function, binding.Block.Type.Value, returnSymbol.Type);
                // Save the value in the correct location
                Emit(function, returnSymbol.StoreOperation, returnSymbol.Index, returnSymbol.Type.GetSize());

                Emit(function, OpCode.Return);
            }
            else
            {
                CompileBlock(frame, body, binding.Block);
                // Perform an implicit cast(if needed) from the block actual type and the binding type
                CompileImplicitCast(body, binding.Block.Type.Value, symbol.Type);
                // Store the result of the block expression on the assigned slot for this symbol
                Emit(body, body.StoreOperation, symbol.Index, symbol.Type.GetSize());
            }
        }

        protected void CompileBlock(FunctionDefinition? frame, InstructionsDefinition body, SST.Expressions.Block block)
        {
            foreach (var childBinding in block.Bindings)
            {
                CompileBinding(frame, body, childBinding);
            }

            CompileExpression(frame, body, block.Expression);
        }

        protected void CompileExpression(FunctionDefinition? frame, InstructionsDefinition body, SST.Expression expression)
        {
            #region Literals

            if (expression is SST.Literals.IntLiteral intLit)
            {
                Emit(body, OpCode.PushInt, intLit.GetSyntaxNode<AST.Literals.IntLiteral>().Value);
            }
            else if (expression is SST.Literals.DecimalLiteral decLit)
            {
                Emit(body, OpCode.PushDec, decLit.GetSyntaxNode<AST.Literals.DecimalLiteral>().Value);
            }
            else if (expression is SST.Literals.BoolLiteral boolLit)
            {
                Emit(body, boolLit.GetSyntaxNode<AST.Literals.BoolLiteral>().Value 
                    ? OpCode.PushTrue
                    : OpCode.PushFalse);
            }
            // TODO string literals

            #endregion

            // TODO Add type inference, auto-cast to all operators
            // TODO Implement decimal operators compilation

            #region Arithmetic Operators

            else if (expression is SST.Operators.Arithmetic.AddOp addOp)
            {
                CompileExpression(frame, body, addOp.Left);
                CompileImplicitCast(body, addOp.Left.Type.Value, addOp.Type.Value);
                CompileExpression(frame, body, addOp.Right);
                CompileImplicitCast(body, addOp.Right.Type.Value, addOp.Type.Value);

                var addCode = addOp.Type.Value switch
                {
                    IntType => OpCode.AddInt,
                    DecimalType => OpCode.AddDec,
                    _ => throw new Exception($"No OpCode valid to perform addition of {addOp.Type.Value.Symbol.FullPath}"),
                };

                Emit(body, addCode);
            }
            else if (expression is SST.Operators.Arithmetic.SubOp subOp)
            {
                CompileExpression(frame, body, subOp.Left);
                CompileImplicitCast(body, subOp.Left.Type.Value, subOp.Type.Value);
                CompileExpression(frame, body, subOp.Right);
                CompileImplicitCast(body, subOp.Right.Type.Value, subOp.Type.Value);

                var subCode = subOp.Type.Value switch
                {
                    IntType => OpCode.SubInt,
                    DecimalType => OpCode.SubDec,
                    _ => throw new Exception($"No OpCode valid to perform subtraction of {subOp.Type.Value.Symbol.FullPath}"),
                };

                Emit(body, subCode);
            }
            else if (expression is SST.Operators.Arithmetic.DivOp divOp)
            {
                CompileExpression(frame, body, divOp.Left);
                CompileImplicitCast(body, divOp.Left.Type.Value, divOp.Type.Value);
                CompileExpression(frame, body, divOp.Right);
                CompileImplicitCast(body, divOp.Right.Type.Value, divOp.Type.Value);

                var divCode = divOp.Type.Value switch
                {
                    IntType => OpCode.DivInt,
                    DecimalType => OpCode.DivInt,
                    _ => throw new Exception($"No OpCode valid to perform division of {divOp.Type.Value.Symbol.FullPath}"),
                };

                Emit(body, divCode);
            }
            else if (expression is SST.Operators.Arithmetic.MulOp mulOp)
            {
                CompileExpression(frame, body, mulOp.Left);
                CompileImplicitCast(body, mulOp.Left.Type.Value, mulOp.Type.Value);
                CompileExpression(frame, body, mulOp.Right);
                CompileImplicitCast(body, mulOp.Right.Type.Value, mulOp.Type.Value);

                var mulCode = mulOp.Type.Value switch
                {
                    IntType => OpCode.MulInt,
                    DecimalType => OpCode.MulDec,
                    _ => throw new Exception($"No OpCode valid to perform multiplication of {mulOp.Type.Value.Symbol.FullPath}"),
                };

                Emit(body, mulCode);
            }
            else if (expression is SST.Operators.Arithmetic.NegOp negOp)
            {
                CompileExpression(frame, body, negOp.Right);
                CompileImplicitCast(body, negOp.Right.Type.Value, negOp.Type.Value);

                var negCode = negOp.Type.Value switch
                {
                    IntType => OpCode.NegInt,
                    DecimalType => OpCode.NegInt,
                    _ => throw new Exception($"No OpCode valid to perform negation of {negOp.Type.Value.Symbol.FullPath}"),
                };

                Emit(body, negCode);
            }
            else if (expression is SST.Operators.Arithmetic.PowOp powOp)
            {
                CompileExpression(frame, body, powOp.Left);
                CompileImplicitCast(body, powOp.Left.Type.Value, powOp.Type.Value);
                CompileExpression(frame, body, powOp.Right);
                CompileImplicitCast(body, powOp.Right.Type.Value, powOp.Type.Value);

                var powCode = powOp.Type.Value switch
                {
                    IntType => OpCode.PowInt,
                    DecimalType => OpCode.PowDec,
                    _ => throw new Exception($"No OpCode valid to perform power of {powOp.Type.Value.Symbol.FullPath}"),
                };

                Emit(body, powCode);
            }

            #endregion

            #region Comparison Operators

            else if (expression is SST.Operators.Comparison.EqOp eqOp)
            {
                CompileExpression(frame, body, eqOp.Left);
                CompileImplicitCast(body, eqOp.Left.Type.Value, eqOp.OperationType.Value);
                CompileExpression(frame, body, eqOp.Right);
                CompileImplicitCast(body, eqOp.Right.Type.Value, eqOp.OperationType.Value);

                var eqCode = eqOp.OperationType.Value switch
                {
                    IntType => OpCode.EqInt,
                    DecimalType => OpCode.EqDec,
                    BoolType => OpCode.EqBool,
                    _ => throw new Exception($"No OpCode valid to perform equality of {eqOp.OperationType.Value.Symbol.FullPath}"),
                };

                Emit(body, eqCode);
            }
            else if (expression is SST.Operators.Comparison.NeqOp neqOp)
            {
                CompileExpression(frame, body, neqOp.Left);
                CompileImplicitCast(body, neqOp.Left.Type.Value, neqOp.OperationType.Value);
                CompileExpression(frame, body, neqOp.Right);
                CompileImplicitCast(body, neqOp.Right.Type.Value, neqOp.OperationType.Value);

                var neqCode = neqOp.OperationType.Value switch
                {
                    IntType => OpCode.NeqInt,
                    DecimalType => OpCode.NeqDec,
                    BoolType => OpCode.NeqBool,
                    _ => throw new Exception($"No OpCode valid to perform inequality of {neqOp.OperationType.Value.Symbol.FullPath}"),
                };

                Emit(body, neqCode);
            }
            else if (expression is SST.Operators.Comparison.LtOp ltOp)
            {
                CompileExpression(frame, body, ltOp.Left);
                CompileImplicitCast(body, ltOp.Left.Type.Value, ltOp.OperationType.Value);
                CompileExpression(frame, body, ltOp.Right);
                CompileImplicitCast(body, ltOp.Right.Type.Value, ltOp.OperationType.Value);

                var neqCode = ltOp.OperationType.Value switch
                {
                    IntType => OpCode.LtInt,
                    DecimalType => OpCode.LtDec,
                    _ => throw new Exception($"No OpCode valid to perform less than of {ltOp.OperationType.Value.Symbol.FullPath}"),
                };

                Emit(body, neqCode);
            }
            else if (expression is SST.Operators.Comparison.LteOp lteOp)
            {
                CompileExpression(frame, body, lteOp.Left);
                CompileImplicitCast(body, lteOp.Left.Type.Value, lteOp.OperationType.Value);
                CompileExpression(frame, body, lteOp.Right);
                CompileImplicitCast(body, lteOp.Right.Type.Value, lteOp.OperationType.Value);

                var lteCode = lteOp.OperationType.Value switch
                {
                    IntType => OpCode.LteInt,
                    DecimalType => OpCode.LteDec,
                    _ => throw new Exception($"No OpCode valid to perform less than or equal of {lteOp.OperationType.Value.Symbol.FullPath}"),
                };

                Emit(body, lteCode);
            }
            else if (expression is SST.Operators.Comparison.GtOp gtOp)
            {
                CompileExpression(frame, body, gtOp.Left);
                CompileImplicitCast(body, gtOp.Left.Type.Value, gtOp.OperationType.Value);
                CompileExpression(frame, body, gtOp.Right);
                CompileImplicitCast(body, gtOp.Right.Type.Value, gtOp.OperationType.Value);

                var gtCode = gtOp.OperationType.Value switch
                {
                    IntType => OpCode.GtInt,
                    DecimalType => OpCode.GtDec,
                    _ => throw new Exception($"No OpCode valid to perform greater than of {gtOp.OperationType.Value.Symbol.FullPath}"),
                };

                Emit(body, gtCode);
            }
            else if (expression is SST.Operators.Comparison.GteOp gteOp)
            {
                CompileExpression(frame, body, gteOp.Left);
                CompileImplicitCast(body, gteOp.Left.Type.Value, gteOp.OperationType.Value);
                CompileExpression(frame, body, gteOp.Right);
                CompileImplicitCast(body, gteOp.Right.Type.Value, gteOp.OperationType.Value);

                var gteCode = gteOp.OperationType.Value switch
                {
                    IntType => OpCode.GteInt,
                    DecimalType => OpCode.GteDec,
                    _ => throw new Exception($"No OpCode valid to perform greater than or equal of {gteOp.OperationType.Value.Symbol.FullPath}"),
                };

                Emit(body, gteCode);
            }

            #endregion

            #region Logic Operators

            else if (expression is SST.Operators.Logic.AndOp andOp)
            {
                var falseLabel = CreateLabelPlaceholder(body);
                var endLabel = CreateLabelPlaceholder(body);

                CompileExpression(frame, body, andOp.Left);
                // If this value is false, short circuit it
                Emit(body, OpCode.JumpCond, falseLabel);
                // If we reach this instruction, then the first operand was true, so now
                // the result of the operation is actually whatever the result of the second operand is
                CompileExpression(frame, body, andOp.Right);
                // Hence after the expression we can jump right away to the end of the operation
                Emit(body, OpCode.Jump, endLabel);
                // This code is only reached if the first operand is false
                AssignLabel(body, falseLabel);
                Emit(body, OpCode.PushFalse);
                AssignLabel(body, endLabel);
            }
            else if (expression is SST.Operators.Logic.OrOp orOp)
            {
                var trueLabel = CreateLabelPlaceholder(body);
                var endLabel = CreateLabelPlaceholder(body);

                CompileExpression(frame, body, orOp.Left);
                // Since we want to jump when this is true, and JumpCond jumps when the value is false, we have to negate it first
                Emit(body, OpCode.Not);
                // If this value is true, short circuit it
                Emit(body, OpCode.JumpCond, trueLabel);
                // If we reach this instruction, then the first operand was false, so now
                // the result of the operation is actually whatever the result of the second operand is
                CompileExpression(frame, body, orOp.Right);
                // Hence after the expression we can jump right away to the end of the operation
                Emit(body, OpCode.Jump, endLabel);
                // This code is only reached if the first operand is true
                AssignLabel(body, trueLabel);
                Emit(body, OpCode.PushTrue);
                AssignLabel(body, endLabel);
            }
            else if (expression is SST.Operators.Logic.NotOp notOp)
            {
                CompileExpression(frame, body, notOp.Right);
                Emit(body, OpCode.Not);
            }

            #endregion

            else if (expression is SST.Expressions.Application application)
            {
                var functionType = (FunctionTypeDefinition)application.Func.Type.Value;

                // Reserve space for the return value
                var returnSize = functionType.ReturnType.GetSize();

                if (returnSize > 0)
                {
                    Emit(body, OpCode.PushN, returnSize);
                }

                // TODO Emit and build the captures context object

                int argsSize = 0;
                int argIndex = 0;

                foreach (var arg in application.Args)
                {
                    CompileExpression(frame, body, arg);
                    CompileImplicitCast(body, arg.Type.Value, functionType.Arguments[argIndex].Type);

                    argsSize += functionType.Arguments[argIndex].Type.GetSize();
                    argIndex += 1;
                }

                CompileExpression(frame, body, application.Func);
                Emit(body, OpCode.Call);

                if (argsSize > 0)
                {
                    Emit(body, OpCode.PopN, argsSize);
                }
            }
            else if (expression is SST.Expressions.Block block)
            {
                CompileBlock(frame, body, block);
            }
            else if (expression is SST.Expressions.IfNode ifNode)
            {
                var elseLabel = CreateLabelPlaceholder(body);
                var endLabel = CreateLabelPlaceholder(body);
                
                // Compile the condition expression
                CompileExpression(frame, body, ifNode.Condition);

                // If the condition is false, conditionally jump to "Else" label
                // NOTE JumpCond only performs the jump if the value at the top of the stack is false
                Emit(body, OpCode.JumpCond, elseLabel);
                // Otherwise we will execute the Then expression
                CompileExpression(frame, body, ifNode.ThenExpression);
                // After the end of the Then expression, always jump to the "End" label
                // To avoid executing both the Then and Else expressions at the same time
                Emit(body, OpCode.Jump, endLabel);
                AssignLabel(body, elseLabel);
                CompileExpression(frame, body, ifNode.ElseExpression);
                AssignLabel(body, endLabel);
            }
            else if (expression is SST.Identifier identifier)
            {
                CompileSymbol(frame, body, identifier.Symbol.Value);
            }
            else
            {
                throw new Exception($"Compile not yet implemented for {expression.GetType().Name}");
            }
        }

        protected void CompileSymbol(FunctionDefinition? frame, InstructionsDefinition body, SymbolsScopeEntry symbol)
        {
            if (symbol.IsParameter)
            {
                if (frame == null)
                {
                    throw new Exception("Cannot compile a parameter symbol in a frame-less environment!");
                }

                var parameter = frame.GetVariable(symbol);

                Emit(body, parameter.LoadOperation, parameter.Index, parameter.Type.GetSize());
            }
            else if (symbol.IsCapture)
            {
                throw new NotImplementedException();
            }
            else if (symbol.IsBinding)
            {
                SymbolDefinition variable;

                if (symbol.BindingGlobal!.Value)
                {
                    variable = Global.GetVariable(symbol);
                }
                else
                {
                    variable = body.GetVariable(symbol);
                }

                Emit(body, variable.LoadOperation, variable.Index, variable.Type.GetSize());
            }
            else if (symbol.IsType)
            {
                Emit(body, OpCode.PushInt, symbol.Type.Id);
            }
        }

        protected void CompileImplicitCast(InstructionsDefinition body, TypeDefinition sourceType, TypeDefinition targetType)
        {
            // If the types are exactly the same, nothing needs to be done
            if (sourceType == targetType)
            {
                return;
            }

            if (sourceType is IntType && targetType is DecimalType)
            {
                Emit(body, OpCode.IntToDec);
            }
            else
            {
                throw new Exception($"No implicit cast found from type {sourceType.Symbol.FullPath} and {targetType.Symbol.FullPath}");
            }
        }

        protected void Emit(InstructionsDefinition body, OpCode opcode, params object[] args)
        {
            Emit(body, new Instruction(opcode, args));
        }

        protected void Emit(InstructionsDefinition body, Instruction instruction)
        {
            body.Instructions.Add(instruction);
        }

        /// <summary>
        /// Build the Dictionary of variables and their byte offset positions relative to the Frame Pointer.
        /// 
        /// Sample of a stack where each row is one byte. LocalVar1 and LocalVar3 have 1 byte of size, while LocalVar2 has 2 bytes.
        /// Similarly, Arg1, Arg2 and Arg3 have 1 byte of size as well. The return value, has 3 bytes.
        /// 
        /// ┌────────────────┐
        /// │        .       │
        /// │        .       │
        /// │        .       │
        /// │    LocalVar3   │+3
        /// │                │
        /// │    LocalVar2   │+1
        /// │    LocalVar1   │+0
        /// ├────────────────┤FP
        /// │      Arg3      │-1
        /// │      Arg2      │-2
        /// │      Arg1      │-3
        /// │                │
        /// │                │
        /// │     Return     │-6
        /// └────────────────┘
        /// </summary>
        /// <param name="type"></param>
        /// <param name="argNames"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Dictionary<SymbolsScopeEntry, SymbolDefinition> CreateFunctionSymbols(FunctionTypeDefinition type, SST.Binding binding, string[] argNames, string[] bindingNames, out int returnOffset, out int stackSymbolsSize)
        {
            if (argNames.Length != type.Arguments.Count)
            {
                throw new Exception("Invalid number of argument names: does not match function's type argument's number");
            }

            var symbols = new Dictionary<SymbolsScopeEntry, SymbolDefinition>();

            var rootScope = binding.Scope;

            // Leave space for the frame pointer and instruction pointer values
            int offset = sizeof(int) * -2;

            for (int i = argNames.Length; i > 0; i--)
            {
                // Get the size of the argument
                offset -= type.Arguments[i - 1].Type.GetSize();

                symbols[rootScope.Lookup(argNames[i - 1])] = new SymbolDefinition(
                    isGlobal: false, OpCode.LoadFrameN, OpCode.StoreFrameN, offset, type.Arguments[i - 1].Type
                );
            }

            offset -= type.ReturnType.GetSize();

            returnOffset = offset;

            // TODO Create @captures variable
            // TODO Create @framePointer variables
            stackSymbolsSize = CreateBlockSymbols(symbols, binding.Block);

            return symbols;
        }

        private int CreateBlockSymbols(Dictionary<SymbolsScopeEntry, SymbolDefinition> symbols, SST.Node root, int offset = 0)
        {
            int maximumOffset = offset;

            var offsetByScope = new Dictionary<SymbolsScope, int>();

            var nodesToVisit = new Queue<SST.Node>();
            nodesToVisit.Enqueue(root);

            while (nodesToVisit.Count > 0)
            {
                var node = nodesToVisit.Dequeue();

                #region Queue Child Nodes

                // We do not want to queue the children of bindings that are functions
                // Their symbols will not have offsets relative to this frame pointer anyway, so no use registering them
                if (!(node is SST.Binding) || (node as SST.Binding)!.Signature == null || (node as SST.Binding)!.Signature!.Parameters.Count == 0)
                {
                    foreach (var childNode in node.GetChildren())
                    {
                        nodesToVisit.Enqueue(childNode);
                    }
                }

                #endregion

                if (node is SST.Binding binding)
                {
                    // Represents the byte offset from the frame pointer
                    int scopeOffset = offset;

                    // Bindings will be registered on their parent scope (since bindings create a child scope for themselves)
                    SymbolsScope scopeToRegister = binding.Scope.ParentScope!;

                    // We want to know the byte offset on that scope
                    SymbolsScope? scopeToSearch = scopeToRegister;

                    // We may not have registered any binding in some ascendant scopes of this one
                    // And as such, we may have to loop until we find one parent scope that had bindings registered.
                    // Whatever offset was registered in that scope will be our starting point
                    while (scopeToSearch != null)
                    {
                        if (offsetByScope.TryGetValue(scopeToSearch, out int offsetFound))
                        {
                            scopeOffset = offsetFound;

                            break;
                        }
                        // We do not want to search further than the root scope 
                        else if (scopeToSearch == root.Scope)
                        {
                            break;
                        }
                        else
                        {
                            scopeToSearch = scopeToSearch.ParentScope;
                        }
                    }

                    var identifier = binding.GetSyntaxNode<AST.Binding>().Identifier.Name;

                    var symbol = new SymbolDefinition(
                        isGlobal: scopeToRegister.Global,
                        loadOp: scopeToRegister.Global
                            ? OpCode.LoadGlobalN
                            : OpCode.LoadFrameN,
                        storeOp: scopeToRegister.Global
                            ? OpCode.StoreGlobalN
                            : OpCode.StoreFrameN,
                        index: scopeOffset,
                        type: binding.Type.Value
                    );

                    // Register the symbol
                    symbols[scopeToRegister.Lookup(identifier)] = symbol;

                    // Increase the offset with the space taken by this symbol
                    scopeOffset += binding.Type.Value.GetSize();

                    // Update the offset of this scope so any bindings registered on it in the future
                    // will have the updated scope
                    offsetByScope[scopeToRegister] = scopeOffset;

                    // The total required size for all scopes will be the maximum value of the sizes required for each individual scope
                    // So, any time that any scope's offset overpasses the current maximum, we increase the maximum to it
                    if (scopeOffset > maximumOffset)
                    {
                        maximumOffset = scopeOffset;
                    }
                }
            }

            return maximumOffset;
        }

        protected LabelPlaceholder CreateLabelPlaceholder(InstructionsDefinition body)
        {
            return new LabelPlaceholder(_labelIdCounter++);
        }

        protected void AssignLabel(InstructionsDefinition body, LabelPlaceholder label)
        {
            body.Labels[label.LabelId] = body.Instructions.Count();
        }

        #endregion

        #region Assemble (Generate final ByteCode Sequence)

        public void Assemble(ByteCode.ByteCode byteCode)
        {
            AssemblePlaceholderHeader(byteCode, out HeaderStruct header);

            AssembleGlobalInstructions(byteCode, Global, out int instructionsStart);

            AssembleFunctionsInstructions(byteCode, out var functionsPosById, out int instructionsEnd);

            AssembleFunctionsStructs(byteCode, functionsPosById, out int functionsStructPos);

            header.CodeStartIndex = instructionsStart;
            header.CodeEndIndex = instructionsEnd;
            header.FunctionsIndex = functionsStructPos;
            // TODO Write down type metadata too
            //header.TypesIndex = typesStructPos;

            AssembleHeader(byteCode, ref header);
        }

        protected void AssemblePlaceholderHeader(ByteCode.ByteCode byteCode, out HeaderStruct headerStruct)
        {
            headerStruct = new HeaderStruct() { HeaderIndex = byteCode.Cursor };

            byteCode.Write(headerStruct);
        }

        protected void AssembleHeader(ByteCode.ByteCode byteCode, ref HeaderStruct headerStruct)
        {
            // Save the end position of the section covered by this header
            headerStruct.TotalLength = byteCode.Count - headerStruct.HeaderIndex;

            // Move the cursor back to the beginning of the header
            byteCode.Cursor = headerStruct.HeaderIndex;

            // Write the header structure again
            byteCode.Write(headerStruct);

            // Finally seek back the cursor to the end of the byte code
            byteCode.Cursor = headerStruct.HeaderIndex + headerStruct.TotalLength;
        }

        /// <summary>
        /// Writes the bytecode of the relative to the structs of the functions
        /// </summary>
        /// <param name="functionsStructPos">The bytecode position where the first function was written to</param>
        /// <param name="functionsPosById">Dictionary where the key represents the Function Id, and the value is the Byte Position in the bytecode where that Function Structure was written</param>
        protected void AssembleFunctionsStructs(ByteCode.ByteCode byteCode, Dictionary<int, int> functionsPosById, out int functionsStructPos)
        {
            functionsStructPos = byteCode.Cursor;

            byteCode.Write(functionsPosById.Count);
            foreach (var function in Functions.Values)
            {
                byteCode.Write(new FunctionStruct
                {
                    FunctionId = function.Id,
                    Position = functionsPosById[function.Id],
                    TypeId = function.Type.Id,
                    ArgNames = function.ArgNames,
                    Symbol = function.Symbol.FullPath
                });
            }
        }

        protected void AssembleGlobalInstructions(ByteCode.ByteCode byteCode, GlobalDefinition global, out int instructionsStartPos)
        {
            instructionsStartPos = byteCode.Cursor;

            // Write the instructions
            AssembleInstructionsList(byteCode, global.Labels, global.Instructions);
        }

        protected void AssembleFunctionsInstructions(ByteCode.ByteCode byteCode, out Dictionary<int, int> functionsPosById, out int instructionsEndPos)
        {
            functionsPosById = new Dictionary<int, int>();

            foreach (var function in Functions.Values)
            {
                // Note down this function's location in the bytecode
                functionsPosById[function.Id] = byteCode.Cursor;

                // Write the instructions
                AssembleInstructionsList(byteCode, function.Labels, function.Instructions);
            }

            instructionsEndPos = byteCode.Cursor;
        }

        protected void AssembleInstructionsList(ByteCode.ByteCode byteCode, Dictionary<int, int> labels, IEnumerable<Instruction> instructions)
        {
            // Dictionary mapping a label Id with the Index (bytecode) where it is defined
            Dictionary<int, int> labelDefinitions = new Dictionary<int, int>();
            // Dictionary mapping a label Id with the 
            Dictionary<int, List<int>> labelReferences = new Dictionary<int, List<int>>();

            // By default, the labels dictionary stores the label ids as keys and the instruction indexes as values
            // Here we will revert that to store for each index, the list of labels on that index (usually should only be one,
            // but to prevent bugs, we support multiple labels on the same instruction)
            var labelsByIndex = labels
                // Group by the value (instruction index)
                .GroupBy(kv => kv.Value)
                // And convert to a dictionary where the key is the instruction index
                // And the value is the list of labels attached to that position
                .ToDictionary(group => group.Key, group => group.Select(kv => kv.Key).ToList());


            var instIndex = 0;

            foreach (var inst in instructions)
            {
                if (labelsByIndex.ContainsKey(instIndex))
                {
                    // Get the id of the label pointing to this instruction index
                    foreach (var labelId in labelsByIndex[instIndex])
                    {
                        var labelIndex = byteCode.Cursor;

                        labelDefinitions[labelId] = labelIndex;

                        // Go back to any label reference to this label we may have already written and write it down
                        if (labelReferences.ContainsKey(labelId))
                        {
                            foreach (var refIndex in labelReferences[labelId])
                            {
                                // Seek back in the stream and write down the index of the label
                                byteCode.Cursor = refIndex;
                                byteCode.Write(labelIndex);
                            }

                            // Remove the references for this label. From now on, any reference to it can refer directly to the label
                            labelReferences.Remove(labelId);

                            // Make sure to put back the cursor
                            byteCode.Cursor = labelDefinitions[labelId];
                        }
                    }
                }

                AssembleInstruction(byteCode, labelDefinitions, labelReferences, inst);

                instIndex += 1;
            }

            // TODO What if we have, for example, 4 instructions, and we have a label pointing to index 4 (right after last instruction)?

            // If we have any labels left, it means we have not 
            if (labelReferences.Any())
            {
                throw new Exception($"Compiler Bug: Label references with ids {string.Join(", ", labelReferences.Keys)} were not resolved during compilation.");
            }
        }

        protected void AssembleInstruction(ByteCode.ByteCode byteCode, Dictionary<int, int> labelDefinitions, Dictionary<int, List<int>> labelReferences, Instruction inst)
        {
            byteCode.Write(inst.OpCode);
            foreach (var arg in inst.Args)
            {
                if (arg is int argInt)
                {
                    byteCode.Write(argInt);
                }
                else if (arg is uint argUint)
                {
                    byteCode.Write(argUint);
                }
                else if (arg is decimal argDecimal)
                {
                    byteCode.Write(argDecimal);
                }
                else if (arg is bool argBool)
                {
                    byteCode.Write(argBool);
                }
                else if (arg is string argString)
                {
                    byteCode.Write(argString);
                }
                else if (arg is LabelPlaceholder argLabel)
                {
                    // If we already know where the position is of this label, we can write it down already
                    if (labelDefinitions.TryGetValue(argLabel.LabelId, out int labelIndex))
                    {
                        byteCode.Write(labelIndex);
                    }
                    else if (labelReferences.TryGetValue(argLabel.LabelId, out List<int>? referencesList))
                    {
                        // Save this byte position and associate it to this 
                        referencesList.Add(byteCode.Cursor);
                        // Write down a placeholder value
                        byteCode.Write(0);
                    }
                    else
                    {
                        // Save this byte position and associate it to this 
                        labelReferences.Add(argLabel.LabelId, new List<int> { byteCode.Cursor });
                        // Write down a placeholder value
                        byteCode.Write(0);
                    }
                    
                }
                else
                {
                    throw new Exception("Invalid instruction argument type");
                }
            }
        }

        #endregion

        #region Assemble Text (Generate human-friendly instructions code)

        public string AssembleText()
        {
            StringBuilder stringBuilder = new StringBuilder();

            AssembleTextInstructionsList(stringBuilder, Global.Instructions);
            stringBuilder.AppendLine();

            foreach (var function in Functions)
            {
                stringBuilder.AppendLine("// " + function.Value.Symbol.FullPath);

                AssembleTextInstructionsList(stringBuilder, function.Value.Instructions, indent: "\t");

                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        protected void AssembleTextInstructionsList(StringBuilder builder, IEnumerable<Instruction> instructions, string indent = "")
        {
            foreach (var instruction in instructions)
            {
                if (instruction.Args.Length > 0)
                {
                    builder.AppendLine(string.Format("{0}{1} {2}", indent, instruction.OpCode, string.Join(" ", instruction.Args)));
                } 
                else
                {
                    builder.AppendLine(string.Format("{0}{1}", indent, instruction.OpCode));
                }
            }
        }

        #endregion
    }

    public class CompilationResult
    {
        public SST.Script SemanticNodes { get; set; }

        public IReadOnlyList<DiagnosticMessage> Diagnostics { get; set; }

        public ByteCode.ByteCode AssembledInstructions { get; set; }

        public bool IsSuccessfull { get; protected set; }

        public CompilationResult(Script semanticNodes, IReadOnlyList<DiagnosticMessage>? diagnostics)
        {
            SemanticNodes = semanticNodes;
            Diagnostics = diagnostics ?? new List<DiagnosticMessage>();
            IsSuccessfull = Diagnostics.All(msg => msg.Level != DiagnosticLevel.Error);
            AssembledInstructions = new ByteCode.ByteCode();
        }
    }
}