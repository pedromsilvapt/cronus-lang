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
        public FunctionDefinition CreateFunction(Symbol symbol, FunctionTypeDefinition type, string[] argNames, string[] bindingNames)
        {
            var id = _functionIdCounter++;

            var definition = new FunctionDefinition(id, symbol, type, argNames, bindingNames);

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
            Global.BindingNames = script
                .Bindings
                .Select(bind => bind.GetSyntaxNode<AST.Binding>().Identifier.Name)
                .ToArray();

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
            // Register the symbol
            var identifier = binding.Identifier.GetSyntaxNode<AST.Identifier>().Name;

            var symbol = body.CreateVariable(identifier, binding.Type.Value);

            if (binding.Type.Value is FunctionTypeDefinition functionType && binding.Signature?.Parameters?.Count() > 0)
            {
                var argNames = binding
                    .Signature!
                    .GetSyntaxNode<AST.BindingType>()
                    .Parameters
                    .Select(param => param.Identifier.Name)
                    .ToArray();

                var bindingNames = binding
                    .Bindings
                    .Select(bind => bind.GetSyntaxNode<AST.Binding>().Identifier.Name)
                    .ToArray();

                // When the symbol is a function, like in this case, the symbol itself will just be a pointer to the function id
                // The metadata regarding the function, such as the position of the bytecode, it's type and so on, will be stored in
                // a header structure
                var function = CreateFunction(binding.Scope.FullName!, functionType, argNames, bindingNames);

                // Function Id
                Emit(body, OpCode.PushInt, function.Id);
                // Context pointer (global means no context)
                Emit(body, OpCode.PushInt, 0);

                CompileBlock(function, function, binding.Bindings, binding.Expression);

                var returnSymbol = function.GetVariable("@return");

                Emit(function, returnSymbol.StoreOperation, returnSymbol.Index, binding.Expression.Type.Value.GetSize());
                Emit(function, OpCode.Return);
            }
            else
            {
                CompileBlock(frame, body, binding.Bindings, binding.Expression);
            }
        }

        protected void CompileBlock(FunctionDefinition? frame, InstructionsDefinition body, IList<SST.Binding> childBindings, SST.Expression expression)
        {
            // If there are no bindings on this block, we can assume it's result will already be on the correct stack position
            // So no need to reserve space for it and move it afterwards, we can just compile it
            if (childBindings.Count == 0)
            {
                CompileExpression(frame, body, expression);
            }
            else
            {
                // Reserve space for the result of the expression
                Emit(body, OpCode.PushN, expression.Type.Value.GetSize());

                // The value of this offset will be equal to the Expr.TypeSize + Sum(Bindings.TypeSize) + Expr.TypeSize
                int resultOffset = expression.Type.Value.GetSize();

                foreach (var childBinding in childBindings)
                {
                    CompileBinding(frame, body, childBinding);

                    resultOffset += childBinding.Type.Value.GetSize();
                }

                CompileExpression(frame, body, expression);

                resultOffset += expression.Type.Value.GetSize();

                // Move the result of the expression to the place we reserved for it on the stack
                Emit(body, OpCode.StoreStackN, resultOffset * -1, expression.Type.Value.GetSize());

                foreach (var childBinding in childBindings.Reverse())
                {
                    Emit(body, OpCode.PopN, childBinding.Type.Value.GetSize());
                }
            }
        }

        protected void CompileExpression(FunctionDefinition? frame, InstructionsDefinition body, SST.Expression expression)
        {
            #region Literals

            if (expression is SST.Literals.IntLiteral intLit)
            {
                Emit(body, OpCode.PushInt, intLit.GetSyntaxNode<AST.Literals.IntLiteral>().Value);
            }
            // TODO Bool, decimal, string literals

            #endregion

            // TODO Add type inference, auto-cast to all operators
            // TODO Implement decimal operators compilation

            #region Arithmetic Operators

            else if (expression is SST.Operators.Arithmetic.AddOp addOp)
            {
                CompileExpression(frame, body, addOp.Left);
                CompileExpression(frame, body, addOp.Right);
                Emit(body, OpCode.AddInt);
            }
            else if (expression is SST.Operators.Arithmetic.SubOp subOp)
            {
                CompileExpression(frame, body, subOp.Left);
                CompileExpression(frame, body, subOp.Right);
                Emit(body, OpCode.SubInt);
            }
            else if (expression is SST.Operators.Arithmetic.DivOp divOp)
            {
                CompileExpression(frame, body, divOp.Left);
                CompileExpression(frame, body, divOp.Right);
                Emit(body, OpCode.DivInt);
            }
            else if (expression is SST.Operators.Arithmetic.MulOp mulOp)
            {
                CompileExpression(frame, body, mulOp.Left);
                CompileExpression(frame, body, mulOp.Right);
                Emit(body, OpCode.MulInt);
            }
            else if (expression is SST.Operators.Arithmetic.NegOp negOp)
            {
                CompileExpression(frame, body, negOp.Right);
                Emit(body, OpCode.NegInt);
            }
            else if (expression is SST.Operators.Arithmetic.PowOp powOp)
            {
                CompileExpression(frame, body, powOp.Left);
                CompileExpression(frame, body, powOp.Right);
                Emit(body, OpCode.PowInt);
            }

            #endregion

            #region Comparison Operators

            else if (expression is SST.Operators.Comparison.EqOp eqOp)
            {
                CompileExpression(frame, body, eqOp.Left);
                CompileExpression(frame, body, eqOp.Right);
                Emit(body, OpCode.Eq);
            }
            else if (expression is SST.Operators.Comparison.NeqOp neqOp)
            {
                CompileExpression(frame, body, neqOp.Left);
                CompileExpression(frame, body, neqOp.Right);
                Emit(body, OpCode.Neq);
            }
            else if (expression is SST.Operators.Comparison.LtOp ltOp)
            {
                CompileExpression(frame, body, ltOp.Left);
                CompileExpression(frame, body, ltOp.Right);
                Emit(body, OpCode.LtInt);
            }
            else if (expression is SST.Operators.Comparison.LteOp lteOp)
            {
                CompileExpression(frame, body, lteOp.Left);
                CompileExpression(frame, body, lteOp.Right);
                Emit(body, OpCode.LteInt);
            }
            else if (expression is SST.Operators.Comparison.GtOp gtOp)
            {
                CompileExpression(frame, body, gtOp.Left);
                CompileExpression(frame, body, gtOp.Right);
                Emit(body, OpCode.GtInt);
            }
            else if (expression is SST.Operators.Comparison.GteOp gteOp)
            {
                CompileExpression(frame, body, gteOp.Left);
                CompileExpression(frame, body, gteOp.Right);
                Emit(body, OpCode.GteInt);
            }

            #endregion

            else if (expression is SST.Expressions.Application application)
            {
                // Reserve space for the return value
                var returnSize = ((FunctionTypeDefinition)application.Func.Type.Value).ReturnType.GetSize();

                if (returnSize > 0)
                {
                    Emit(body, OpCode.PushN, returnSize);
                }

                // TODO Emit and build the captures context object

                foreach (var arg in application.Args)
                {
                    CompileExpression(frame, body, arg);
                }

                CompileExpression(frame, body, application.Func);
                Emit(body, OpCode.Call);
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
        }

        public void CompileSymbol(FunctionDefinition? frame, InstructionsDefinition body, SymbolsScopeEntry symbol)
        {
            if (symbol.IsParameter)
            {
                if (frame == null)
                {
                    throw new Exception("Cannot compile a parameter symbol in a frame-less environment!");
                }

                var parameterName = frame.ArgNames[symbol.ParameterIndex!.Value];

                var parameter = frame.GetVariable(parameterName);

                Emit(body, parameter.LoadOperation, parameter.Index, parameter.Type.GetSize());
            }
            else if (symbol.IsCapture)
            {
                throw new NotImplementedException();
            }
            else if (symbol.IsBinding)
            {
                if (symbol.BindingGlobal!.Value)
                {
                    var variableName = Global.BindingNames[symbol.BindingIndex!.Value];

                    var variable = Global.GetVariable(variableName);
                
                    Emit(body, variable.LoadOperation, variable.Index, variable.Type.GetSize());
                }
                else
                {
                    var variableName = body.BindingNames[symbol.BindingIndex!.Value];

                    var variable = body.GetVariable(variableName);

                    Emit(body, variable.LoadOperation, variable.Index, variable.Type.GetSize());
                }
            }
            else if (symbol.IsType)
            {
                Emit(body, OpCode.PushInt, symbol.Type.Id);
            }
        }

        public LabelPlaceholder CreateLabelPlaceholder(InstructionsDefinition body)
        {
            return new LabelPlaceholder(_labelIdCounter++);
        }

        public void AssignLabel(InstructionsDefinition body, LabelPlaceholder label)
        {
            body.Labels[label.LabelId] = body.Instructions.Count();
        }

        public void Emit(InstructionsDefinition body, OpCode opcode, params object[] args)
        {
            Emit(body, new Instruction(opcode, args));
        }

        public void Emit(InstructionsDefinition body, Instruction instruction)
        {
            body.Instructions.Add(instruction);
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