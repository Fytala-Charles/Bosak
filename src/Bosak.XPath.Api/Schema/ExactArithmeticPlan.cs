// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Compiles and evaluates the bounded exact integer/decimal arithmetic profile.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; bounded exact arithmetic profile (1.48.0).                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Buffers;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Lexer;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Api.Schema;

/// <summary>Compiles the selected exact arithmetic grammar without legacy numeric literal conversion.</summary>
internal sealed class ExactArithmeticPlan
{
    internal const string Profile="bosak-exact-arithmetic-v1";
    internal const string DescriptorFormat="bosak-exact-arithmetic-static-v1";
    private enum Op { Literal,Variable,Positive,Negative,Add,Subtract,Multiply,Open }
    private readonly record struct Instruction(Op Code,ExactNumber Number=default,string? Variable=null);
    private readonly Instruction[] _code;
    private readonly int _stackDepth;
    private readonly ExactArithmeticLimits _limits;
    private ExactArithmeticPlan(Instruction[] code,int depth,ExactArithmeticLimits limits) { _code=code;_stackDepth=depth;_limits=limits; }
    private static int Priority(Op op)=>op is Op.Positive or Op.Negative ? 3 : op==Op.Multiply ? 2 : op is Op.Add or Op.Subtract ? 1 : 0;
    internal static bool TryCompile(string source,CompileOptions options,int digits,int scale,out ExactArithmeticPlan? plan,CancellationToken cancellation=default)
    {
        plan=null;
        if(options.Compatibility!=XPathCompatibility.XPath31 || options.BackwardsCompatible || options.Xml11LineEndings
            || options.EnableJit || options.StrictStaticTyping || options.EnumerationItemTypes is not null || options.ItemTypeAliases is not null
            || options.StaticVariableTypes is not null || options.NamedRecordTypeNames is not null)return false;
        var code=new List<Instruction>();var operators=new Stack<Op>();var lexer=new XPathLexer(source.AsSpan());
        bool operand=true,selected=false;int depth=0,maximum=0;
        bool Emit(Op op)
        {
            if(depth<(op is Op.Positive or Op.Negative ? 1 : 2))return false;
            if(op is not(Op.Positive or Op.Negative))depth--;code.Add(new(op));return true;
        }
        try
        {
            for(var token=lexer.NextToken();token.Kind!=TokenKind.Eof;token=lexer.NextToken())
            {
                cancellation.ThrowIfCancellationRequested();
                if(token.Kind is TokenKind.IntegerLiteral or TokenKind.DecimalLiteral)
                {
                    if(!operand)return false;
                    var value=ExactDecimalLexical.Parse(token.Text(source.AsSpan()),digits,scale);
                    code.Add(new(Op.Literal,new(value.Coefficient,value.Scale,token.Kind==TokenKind.IntegerLiteral)));
                    maximum=Math.Max(maximum,++depth);operand=false;selected=true;
                }
                else if(token.Kind==TokenKind.Dollar)
                {
                    if(!operand)return false;token=lexer.NextToken();if(token.Kind!=TokenKind.Name)return false;
                    var name=token.Text(source.AsSpan());if(name.IndexOfAny(':','{','}')>=0)return false;
                    code.Add(new(Op.Variable,Variable:name.ToString()));maximum=Math.Max(maximum,++depth);operand=false;
                }
                else if(token.Kind==TokenKind.LParen) { if(!operand)return false;operators.Push(Op.Open); }
                else if(token.Kind==TokenKind.RParen)
                {
                    if(operand)return false;
                    while(operators.TryPeek(out var top)&&top!=Op.Open)if(!Emit(operators.Pop()))return false;
                    if(!operators.TryPop(out var open)||open!=Op.Open)return false;operand=false;
                }
                else if(token.Kind is TokenKind.Plus or TokenKind.Minus or TokenKind.Star)
                {
                    selected=true;
                    if(operand)
                    { if(token.Kind==TokenKind.Star)return false;operators.Push(token.Kind==TokenKind.Plus?Op.Positive:Op.Negative);continue; }
                    var op=token.Kind==TokenKind.Star?Op.Multiply:token.Kind==TokenKind.Plus?Op.Add:Op.Subtract;
                    while(operators.TryPeek(out var top)&&Priority(top)>=Priority(op))if(!Emit(operators.Pop()))return false;
                    operators.Push(op);operand=true;
                }
                else return false;
            }
            if(operand||!selected)return false;
            while(operators.TryPop(out var op))if(op==Op.Open||!Emit(op))return false;
            if(depth!=1)return false;
            plan=new(code.ToArray(),maximum,new(digits,scale));return true;
        }
        catch(NotSupportedException){return false;}
        catch(FormatException){return false;}
    }
    internal XdmValue Evaluate(EvaluationContext context,Action check)
    {
        var values=ArrayPool<ExactNumber>.Shared.Rent(_stackDepth);int count=0;
        try
        {
            foreach(var instruction in _code)
            {
                check();
                if(instruction.Code==Op.Literal){values[count++]=instruction.Number;continue;}
                if(instruction.Code==Op.Variable)
                {
                    if(!context.TryGetVariable(instruction.Variable!,out var value))throw new InvalidOperationException($"XPST0008: Variable ${instruction.Variable} is not defined.");
                    values[count++]=_limits.Read(value);continue;
                }
                if(instruction.Code==Op.Positive)continue;
                if(instruction.Code==Op.Negative){values[count-1]=_limits.Negate(values[count-1]);continue;}
                var right=values[--count];var left=values[count-1];
                values[count-1]=instruction.Code==Op.Multiply?_limits.Multiply(left,right,check)
                    :_limits.Add(left,instruction.Code==Op.Subtract?_limits.Negate(right):right,check);
            }
            check();var result=_limits.ToValue(values[0]);check();return result;
        }
        finally { ArrayPool<ExactNumber>.Shared.Return(values,clearArray:true); }
    }
}
