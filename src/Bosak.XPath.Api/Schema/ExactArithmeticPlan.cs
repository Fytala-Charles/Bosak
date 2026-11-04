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
//                      | Charles Korthout | 0.2   | 04-11-2026     | Compile typed scalar comparisons and top-level bounded string conversion (1.49.0).       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 04-11-2026     | Add separately identified idiv/mod profile (1.50.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 04-11-2026     | Select separately identified exact terminating division (1.51.0). |
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
    internal const string ScalarProfile="bosak-exact-scalars-v2";
    internal const string ScalarDescriptorFormat="bosak-exact-scalars-static-v2";
    internal const string QuotientProfile="bosak-exact-quotient-v3";
    internal const string QuotientDescriptorFormat="bosak-exact-quotient-static-v3";
    internal const string DivisionProfile="bosak-exact-division-v4";
    internal const string DivisionDescriptorFormat="bosak-exact-division-static-v4";
    private enum Op { Literal,Variable,Positive,Negative,Add,Subtract,Multiply,Divide,IntegerDivide,Remainder,Equal,NotEqual,Less,LessEqual,Greater,GreaterEqual,Open }
    private static bool Comparison(Op op)=>op is Op.Equal or Op.NotEqual or Op.Less or Op.LessEqual or Op.Greater or Op.GreaterEqual;
    private static Op? ComparisonToken(TokenKind token)=>token switch
    {
        TokenKind.Equal or TokenKind.ValueEq=>Op.Equal,TokenKind.NotEqual or TokenKind.ValueNe=>Op.NotEqual,
        TokenKind.LessThan or TokenKind.ValueLt=>Op.Less,TokenKind.LessThanOrEqual or TokenKind.ValueLe=>Op.LessEqual,
        TokenKind.GreaterThan or TokenKind.ValueGt=>Op.Greater,TokenKind.GreaterThanOrEqual or TokenKind.ValueGe=>Op.GreaterEqual,_=>null
    };
    private readonly record struct Instruction(Op Code,ExactNumber Number=default,string? Variable=null);
    private readonly Instruction[] _code;
    private readonly int _stackDepth;
    private readonly ExactArithmeticLimits _limits;
    private readonly bool _extended,_serialize,_quotient,_division;
    internal string ProfileName=>_division?DivisionProfile:_quotient?QuotientProfile:_extended?ScalarProfile:Profile;
    internal string DescriptorName=>_division?DivisionDescriptorFormat:_quotient?QuotientDescriptorFormat:_extended?ScalarDescriptorFormat:DescriptorFormat;
    internal int SerializationCapacity=>_limits.StringCapacity;
    private ExactArithmeticPlan(Instruction[] code,int depth,ExactArithmeticLimits limits,bool extended,bool serialize,bool quotient,bool division)
    { _code=code;_stackDepth=depth;_limits=limits;_extended=extended;_serialize=serialize;_quotient=quotient;_division=division; }
    private static int Priority(Op op)=>op is Op.Positive or Op.Negative ? 3 : op is Op.Multiply or Op.Divide or Op.IntegerDivide or Op.Remainder ? 2 : op is Op.Add or Op.Subtract ? 1 : Comparison(op)?0:-1;
    internal static bool TryCompile(string source,CompileOptions options,int digits,int scale,out ExactArithmeticPlan? plan,CancellationToken cancellation=default)
    {
        plan=null;
        if(options.Compatibility!=XPathCompatibility.XPath31 || options.BackwardsCompatible || options.Xml11LineEndings
            || options.EnableJit || options.StrictStaticTyping || options.EnumerationItemTypes is not null || options.ItemTypeAliases is not null
            || options.StaticVariableTypes is not null || options.NamedRecordTypeNames is not null)return false;
        ReadOnlySpan<char> body=source.AsSpan();bool serialize=UnwrapString(source,options,out int start,out int length,cancellation);
        if(serialize)body=body.Slice(start,length);
        var code=new List<Instruction>();var operators=new Stack<Op>();var kinds=new Stack<bool>();var lexer=new XPathLexer(body);
        bool operand=true,selected=serialize,extended=serialize,quotient=false,division=false;int depth=0,maximum=0;
        bool Emit(Op op)
        {
            if(depth<(op is Op.Positive or Op.Negative ? 1 : 2))return false;
            if(op is Op.Positive or Op.Negative){if(kinds.Peek())return false;}
            else
            {
                bool right=kinds.Pop(),left=kinds.Pop();if(left||right)return false;
                kinds.Push(Comparison(op));depth--;
            }
            code.Add(new(op));return true;
        }
        try
        {
            for(var token=lexer.NextToken();token.Kind!=TokenKind.Eof;token=lexer.NextToken())
            {
                cancellation.ThrowIfCancellationRequested();
                if(token.Kind is TokenKind.IntegerLiteral or TokenKind.DecimalLiteral)
                {
                    if(!operand)return false;
                    var value=ExactDecimalLexical.Parse(token.Text(body),digits,scale);
                    code.Add(new(Op.Literal,new(value.Coefficient,value.Scale,token.Kind==TokenKind.IntegerLiteral)));
                    maximum=Math.Max(maximum,++depth);kinds.Push(false);operand=false;selected=true;
                }
                else if(token.Kind==TokenKind.Dollar)
                {
                    if(!operand)return false;token=lexer.NextToken();if(token.Kind!=TokenKind.Name)return false;
                    var name=token.Text(body);if(name.IndexOfAny(':','{','}')>=0)return false;
                    code.Add(new(Op.Variable,Variable:name.ToString()));maximum=Math.Max(maximum,++depth);kinds.Push(false);operand=false;
                }
                else if(token.Kind==TokenKind.LParen) { if(!operand)return false;operators.Push(Op.Open); }
                else if(token.Kind==TokenKind.RParen)
                {
                    if(operand)return false;
                    while(operators.TryPeek(out var top)&&top!=Op.Open)if(!Emit(operators.Pop()))return false;
                    if(!operators.TryPop(out var open)||open!=Op.Open)return false;operand=false;
                }
                else if(token.Kind is TokenKind.Plus or TokenKind.Minus or TokenKind.Star or TokenKind.KeywordDiv or TokenKind.KeywordIdiv or TokenKind.KeywordMod)
                {
                    selected=true;
                    if(operand)
                    { if(token.Kind is TokenKind.Star or TokenKind.KeywordDiv or TokenKind.KeywordIdiv or TokenKind.KeywordMod)return false;operators.Push(token.Kind==TokenKind.Plus?Op.Positive:Op.Negative);continue; }
                    var op=token.Kind switch {TokenKind.Star=>Op.Multiply,TokenKind.KeywordDiv=>Op.Divide,TokenKind.KeywordIdiv=>Op.IntegerDivide,TokenKind.KeywordMod=>Op.Remainder,TokenKind.Plus=>Op.Add,_=>Op.Subtract};
                    if(op is Op.IntegerDivide or Op.Remainder){extended=true;quotient=true;}
                    if(op==Op.Divide){extended=true;division=true;}
                    while(operators.TryPeek(out var top)&&Priority(top)>=Priority(op))if(!Emit(operators.Pop()))return false;
                    operators.Push(op);operand=true;
                }
                else if(ComparisonToken(token.Kind) is Op comparison)
                {
                    if(operand)return false;selected=true;extended=true;
                    while(operators.TryPeek(out var top)&&Priority(top)>=0)
                    {if(Comparison(top)||!Emit(operators.Pop()))return false;}
                    operators.Push(comparison);operand=true;
                }
                else return false;
            }
            if(operand||!selected)return false;
            while(operators.TryPop(out var op))if(op==Op.Open||!Emit(op))return false;
            if(depth!=1)return false;
            plan=new(code.ToArray(),maximum,new(digits,scale),extended,serialize,quotient,division);return true;
        }
        catch(NotSupportedException){return false;}
        catch(FormatException){return false;}
    }
    private static bool UnwrapString(string source,CompileOptions options,out int start,out int length,CancellationToken cancellation)
    {
        start=0;length=source.Length;var lexer=new XPathLexer(source.AsSpan());var token=lexer.NextToken();
        if(token.Kind!=TokenKind.Name)return false;
        var name=token.Text(source.AsSpan());
        if(!name.SequenceEqual("string")&&!name.SequenceEqual("fn:string"))return false;
        if(name.SequenceEqual("fn:string")&&options.Namespaces?.TryGetValue("fn",out var ns)==true&&ns!="http://www.w3.org/2005/xpath-functions")return false;
        token=lexer.NextToken();if(token.Kind!=TokenKind.LParen)return false;
        start=token.Start+token.Length;int depth=1;
        while((token=lexer.NextToken()).Kind!=TokenKind.Eof)
        {
            cancellation.ThrowIfCancellationRequested();
            if(token.Kind==TokenKind.LParen)depth++;
            else if(token.Kind==TokenKind.RParen&&--depth==0)
            {length=token.Start-start;return lexer.NextToken().Kind==TokenKind.Eof;}
        }
        return false;
    }
    internal XdmValue Evaluate(EvaluationContext context,Action check)
        =>_extended?EvaluateScalars(context,check):EvaluateNumeric(context,check);
    private XdmValue EvaluateNumeric(EvaluationContext context,Action check)
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
    private readonly record struct Cell(ExactNumber Number,bool Boolean=false,bool IsBoolean=false);
    private XdmValue EvaluateScalars(EvaluationContext context,Action check)
    {
        var values=ArrayPool<Cell>.Shared.Rent(_stackDepth);int count=0;
        try
        {
            foreach(var instruction in _code)
            {
                check();
                if(instruction.Code==Op.Literal){values[count++]=new(instruction.Number);continue;}
                if(instruction.Code==Op.Variable)
                {
                    if(!context.TryGetVariable(instruction.Variable!,out var value))throw new InvalidOperationException($"XPST0008: Variable ${instruction.Variable} is not defined.");
                    values[count++]=new(_limits.Read(value));continue;
                }
                if(instruction.Code==Op.Positive)continue;
                if(instruction.Code==Op.Negative){values[count-1]=new(_limits.Negate(values[count-1].Number));continue;}
                var right=values[--count].Number;var left=values[count-1].Number;
                if(Comparison(instruction.Code))
                {
                    int cmp=_limits.Compare(left,right,check);bool result=instruction.Code switch
                    { Op.Equal=>cmp==0,Op.NotEqual=>cmp!=0,Op.Less=>cmp<0,Op.LessEqual=>cmp<=0,Op.Greater=>cmp>0,_=>cmp>=0 };
                    values[count-1]=new(default,result,true);
                }
                else values[count-1]=new(instruction.Code==Op.Divide?_limits.Divide(left,right,check):instruction.Code is Op.IntegerDivide or Op.Remainder?_limits.QuotientOrRemainder(left,right,instruction.Code==Op.Remainder,check):instruction.Code==Op.Multiply?_limits.Multiply(left,right,check)
                    :_limits.Add(left,instruction.Code==Op.Subtract?_limits.Negate(right):right,check));
            }
            check();var final=values[0];XdmValue output;
            if(_serialize)output=XdmValue.FromString(final.IsBoolean?(final.Boolean?"true":"false"):_limits.Format(final.Number,check));
            else output=final.IsBoolean?XdmValue.FromBoolean(final.Boolean):_limits.ToValue(final.Number);
            check();return output;
        }
        finally{ArrayPool<Cell>.Shared.Return(values,clearArray:true);}
    }

}
