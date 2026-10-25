from pathlib import Path
import json
r=Path('D:/Development/Bosak');t=r/'.guard-tmp';s=t/'req126-array-atomization-audit-source';e=t/'req126-array-atomization-audit-evidence';p=e/'probe';p.mkdir(parents=True,exist_ok=True)
(p/'Program.cs').write_bytes((t/'req126-modulo-evidence/probe/Program.cs').read_bytes())
(p/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><ProjectReference Include="../../req126-array-atomization-audit-source/src/Bosak.XPath.Api/Bosak.XPath.Api.csproj" /></ItemGroup></Project>\n',encoding='utf-8')
shapes=[('empty',[], '[]'),('empty-member',[None],'[()]'),('two-empty-arrays',[[],[]],None),('empty-first',[[],2],None),('empty-last',[2,[]],None),('singleton-between-empty',[[],[2],[]],None),('two-empty-before-singleton',[[],[],[2]],None),('nested-singleton',[[2]],None),('empty-sequence-before-singleton',[None,2],'[(),2]'),('nested-multiple',[[1,2]],None),('two-singleton-members',[[1],[2]],None),('multiple-between-empty',[[],[1,2],[]],None),('flat-multiple',[1,2],None),('three-empty-arrays',[[],[],[]],None),('nested-empty-first',[[[]],2],None),('flat-singleton',[2],None)]
def flatten(v):
 if v is None:return []
 if isinstance(v,list):return [x for part in v for x in flatten(part)]
 return [v]
def xp(v):return '()' if v is None else '['+','.join(xp(x) for x in v)+']' if isinstance(v,list) else str(v)
contexts=[('data',lambda a:"string-join(data("+a+") ! string(.), ',')"),('unary-minus',lambda a:'-('+a+')'),('unary-plus',lambda a:'+('+a+')'),('add',lambda a:'('+a+') + 0'),('multiply',lambda a:'('+a+') * 1'),('idiv',lambda a:'('+a+') idiv 1'),('range',lambda a:'('+a+') to 3'),('value-eq',lambda a:'('+a+') eq 2'),('general-eq',lambda a:'('+a+') = 2'),('cast',lambda a:'('+a+') cast as xs:integer?'),('castable',lambda a:'('+a+') castable as xs:integer?'),('function-argument',lambda a:'abs('+a+')')]
# Serialize result sequences explicitly; ToString on a multi-item XDM sequence is only a display marker.
def report(expr):return "string-join(("+expr+") ! concat(string(.),'|',if (. instance of xs:integer) then 'integer' else if (. instance of xs:boolean) then 'boolean' else if (. instance of xs:string) then 'string' else 'other'),';')"
rows=[];expressions=[]
for name,v,explicit in shapes:
 array=explicit or xp(v);flat=flatten(v);reference='('+','.join(map(str,flat))+')'
 for context,apply in contexts:
  expression=report(apply(array));referenceExpression=report(apply(reference))
  rows.append({'shape':name,'array':array,'flatIntegers':flat,'context':context,'expression':expression,'referenceExpression':referenceExpression})
  expressions.extend([expression,referenceExpression])
(e/'inputs.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8');(e/'expressions.json').write_text(json.dumps(expressions,indent=2)+'\n',encoding='utf-8')
header=(s/'tests/Bosak.XPath.Standard.Tests/UnaryCoercionTests.cs').read_text(encoding='utf-8').split('using Bosak.')[0].replace('Verifies repeated unary operators preserve conversion, atomization and errors.','Verifies supported recursive-array atomization and singleton-consumer controls.')
controls=[("string-join(data([[],2,[]]) ! string(.), ',')",'2',False),("string-join(data([[1,2],[],[3]]) ! string(.), ',')",'1,2,3',False),("count(data([[],[]]))",'0',False),("([[],2]) cast as xs:integer",'2',False),("([2,[]]) cast as xs:integer",'2',False),("count(([[],[]]) cast as xs:integer?)",'0',False),("([[],2]) castable as xs:integer",'true',False),("([[1,2]]) castable as xs:integer",'false',False),("-[[2]]",'-2',False),("+[[2]]",'2',False),("[2] + 1",'3',False),("[[2]] * 2",'4',False),("[[2]] idiv 1",'2',False),("[[2]] eq 2",'true',False),("[1,2] = 2",'true',False),("count(-[[]])",'0',False),("[1,2] + 0",'XPTY0004',True),("[[1,2]] eq 2",'XPTY0004',True),("([[1,2]]) cast as xs:integer",'XPTY0004',True),("data([map{}])",'FOTY0013',True),("data([function(){1}])",'FOTY0013',True),("[map{}] cast as xs:integer?",'FOTY0013',True),("[xs:untypedAtomic('bad')] cast as xs:integer",'FORG0001',True),("(function($x){-$x})([[2]])",'-2',False)]
body='''using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies supported recursive-array atomization and singleton-consumer controls.</summary>
public sealed class ArrayAtomizationAuditTests
{
    /// <summary>Provides supported behaviors without treating diagnostic failures as desired outcomes.</summary>
    /// <returns>Profile, expression, expected value/error and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Expression, string Expected, bool Error)[]
        {
'''
body+='\n'.join('            ('+json.dumps(a)+', '+json.dumps(b)+', '+str(c).lower()+'),' for a,b,c in controls)
body+='''
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Expression, row.Expected, row.Error };
    }

    /// <summary>Preserves supported values and required errors across public profiles.</summary>
    /// <param name="profile">The modern compatibility profile.</param>
    /// <param name="expression">The supported control expression.</param>
    /// <param name="expected">The expected value or error code.</param>
    /// <param name="error">Whether evaluation must fail.</param>
    [Theory, MemberData(nameof(Cases))]
    public void SupportedControls(XPathCompatibility profile, string expression, string expected, bool error)
    {
        string Evaluate() => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile })
            .Evaluate(new EvaluationContext()).ToString();
        if (error)
            Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
        else
            Assert.Equal(expected, Evaluate());
    }
}
'''
(s/'tests/Bosak.XPath.Standard.Tests/ArrayAtomizationAuditTests.cs').write_bytes((header+body).encode('utf-8'))
print(str(len(controls)*3)+' supported cases; '+str(len(rows)*3)+' array/reference pairs ('+str(len(expressions)*3)+' raw observations).')
