// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies guarded calendar query contracts and authority callback boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Q = Bosak.XPath.Runtime.Tests.CalendarQuerySupport;
using S = Bosak.XPath.Runtime.Tests.CalendarTransportSupport;
using Session = Bosak.XPath.Api.Schema.SchemaQuerySession;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies the explicit guarded two-calendar query profile through public contracts.</summary>
public sealed class GuardedCalendarQueryTests
{
    private static SchemaQueryLimits Limits => new(3,32,8,256,256);

    /// <summary>Checks C01 the six closed operators and XML whitespace compile.</summary>
    /// <param name="operation">Supported comparison operator.</param>
    [Theory] [InlineData("eq")] [InlineData("ne")] [InlineData("lt")] [InlineData("le")] [InlineData("gt")] [InlineData("ge")]
    public void C01_ClosedGrammar(string operation)
    {
        using var f=new Q.Fixture(" \t$left\r\n"+operation+"\t$right \n");
        Assert.Equal("bosak-typed-calendar-comparison-v1",f.Session.Environment.BehaviorProfile);
        Assert.Equal(1,f.Assessor.Binds);Assert.Equal(0,f.Input.Roots);
    }

    /// <summary>Checks C02 unsupported syntax refuses before a poisoned assessor can bind.</summary>
    /// <param name="expression">Unsupported source text.</param>
    [Theory] [InlineData("$left = $right")] [InlineData("$right eq $left")] [InlineData("$left eq $right + 1")]
    [InlineData("$left eq xs:dateTime('2000-01-01T00:00:00Z')")] [InlineData("$left\u00a0eq $right")]
    [InlineData("$left(:x:)eq $right")] [InlineData("$left eq $right\0")] [InlineData("$left eq $other")]
    public void C02_SyntaxRefusesBeforeBind(string expression)
    {
        var assessor=new S.Assessor { Hook=_=>throw new Exception("bind poison") };
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileCalendarComparison(expression,Limits,assessor,0).Kind);
        Assert.Equal(0,assessor.Binds);
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileCalendarComparison(new string(' ',129)+"$left eq $right",Limits,assessor,0).Kind);
    }

    /// <summary>Checks C02 unsupported semantic options refuse before issuer access.</summary>
    /// <param name="mode">Unsupported option mode.</param>
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
    public void C02_OptionsRefuseBeforeBind(int mode)
    {
        var options=mode switch {
            0=>new CompileOptions { BackwardsCompatible=true },1=>new CompileOptions { StrictStaticTyping=true },
            2=>new CompileOptions { Xml11LineEndings=true },3=>new CompileOptions { EnableJit=true },
            4=>new CompileOptions { StaticVariableTypes=new Dictionary<string,string>() },
            5=>new CompileOptions { SchemaSet=new System.Xml.Schema.XmlSchemaSet() },
            _=>new CompileOptions { Namespaces=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) }};
        var assessor=new S.Assessor { Hook=_=>throw new Exception("bind poison") };
        Assert.Equal(SchemaResultKind.Unsupported,Session.CompileCalendarComparison("$left eq $right",Limits,assessor,0,options).Kind);Assert.Equal(0,assessor.Binds);
    }

    /// <summary>Checks C03 invalid fixed timezones refuse before binding.</summary>
    /// <param name="timezone">Invalid context offset.</param>
    [Theory] [InlineData(-841)] [InlineData(841)] [InlineData(int.MinValue)] [InlineData(int.MaxValue)]
    public void C03_TimezoneBounds(int timezone)
    {
        var assessor=new S.Assessor();Assert.Throws<ArgumentOutOfRangeException>(()=>Session.CompileCalendarComparison("$left eq $right",Limits,assessor,timezone));Assert.Equal(0,assessor.Binds);
    }

    /// <summary>Checks C04 descriptors freeze operator, timezone, capacities and copied static options.</summary>
    [Fact] public void C04_ImmutableDescriptors()
    {
        var namespaces=new Dictionary<string,string>(StringComparer.Ordinal){{"customer","urn:old"}};
        using var f=new Q.Fixture(options:new CompileOptions { Namespaces=namespaces });
        string fingerprint=f.Session.Environment.StaticContextFingerprint;namespaces["customer"]="urn:new";
        Assert.Equal("urn:old",f.Session.Environment.StaticContext.Namespaces["customer"]);Assert.Equal(fingerprint,f.Session.Environment.StaticContextFingerprint);
        using var same=new Q.Fixture(options:new CompileOptions { Namespaces=new Dictionary<string,string>(StringComparer.Ordinal){{"customer","urn:old"}} });
        using var baseline=new Q.Fixture();using var zone=new Q.Fixture(timezone:60);using var op=new Q.Fixture("$left ne $right");using var capacity=new Q.Fixture(digits:64);
        string standard=baseline.Session.Environment.StaticContextFingerprint;
        Assert.Equal(fingerprint,same.Session.Environment.StaticContextFingerprint);Assert.NotEqual(standard,fingerprint);
        Assert.NotEqual(standard,zone.Session.Environment.StaticContextFingerprint);Assert.NotEqual(standard,op.Session.Environment.StaticContextFingerprint);Assert.NotEqual(standard,capacity.Session.Environment.StaticContextFingerprint);
        Assert.Equal("bosak-typed-calendar-comparison-descriptor-v1",f.Session.Environment.DescriptorFormatVersion);
    }

    /// <summary>Checks C05 populated focus, alternate bindings and ambient services refuse before type lookup.</summary>
    /// <param name="mode">Forbidden runtime state.</param>
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void C05_ClosedContext(int mode)
    {
        using var f=new Q.Fixture();var pair=f.Admit(f.Value(Q.D()),f.Value(Q.D()));var c=Q.Fixture.Context(pair.Left,pair.Right);
        switch(mode)
        {
            case 0:c=c.WithVariable("extra",XdmValue.FromInteger(1));break;
            case 1:c=new EvaluationContext().WithVariable("left",pair.Left);break;
            case 2:c=c.WithFocus(XdmValue.FromString("focus"),1,1);break;
            case 3:c.ImplicitTimezoneOffsetMinutes=0;break;
            case 4:_=c.CurrentDateTimeSnapshot;break;
            case 5:c.DocumentLoader=_=>throw new Exception("loader poison");break;
            case 6:c=c.WithNamespace("customer","urn:changed");break;
            case 7:c.BackwardsCompatible=true;break;
        }
        int lookups=f.Authority.Resolutions;f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.Evaluate(c));return 0;});Assert.Equal(lookups,f.Authority.Resolutions);
        Assert.True(f.Compare(pair.Left,pair.Right));
    }

    /// <summary>Checks C06 singleton admission, unsupported complete containers and no node traversal.</summary>
    [Fact] public void C06_SingletonBoundary()
    {
        using var f=new Q.Fixture();var one=f.Value(Q.D());var two=f.Value(Q.D(),count:2);var empty=f.Value(Q.D(),count:0);
        f.Run(op=>{Assert.Equal(SchemaResultKind.Supported,op.ReadTypedValue(f.Snapshot,one).Kind);Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedValue(f.Snapshot,two).Kind);Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedValue(f.Snapshot,empty).Kind);Assert.Equal(SchemaResultKind.Unsupported,op.ReadTypedContainer(f.Snapshot,one).Kind);Assert.Throws<NotSupportedException>(()=>op.GetRoot(f.Snapshot));return 0;});Assert.Equal(0,f.Input.Roots);
    }

    /// <summary>Checks C07 actual dateTime and owned user-derived dateTime are authorized.</summary>
    /// <param name="derived">Use a user-derived actual type.</param>
    [Theory] [InlineData(false)] [InlineData(true)]
    public void C07_DateTimeAuthority(bool derived)
    {
        using var f=new Q.Fixture();var type=derived?f.Authority.Derived("dateTime"):f.Authority.Builtins["dateTime"];
        var pair=f.Admit(f.Value(Q.D(),type),f.Value(Q.D()));Assert.True(f.Compare(pair.Left,pair.Right));Assert.True(f.Authority.Relations>0);Assert.True(f.Authority.Resolutions>0);
    }

    /// <summary>Checks C08 owned time types compare using clock fields alone.</summary>
    /// <param name="derived">Use a user-derived time type.</param>
    [Theory] [InlineData(false)] [InlineData(true)]
    public void C08_TimeAuthority(bool derived)
    {
        using var f=new Q.Fixture();var type=derived?f.Authority.Derived("time"):f.Authority.Builtins["time"];
        var pair=f.Admit(f.Value(Q.T(),type),f.Value(Q.T("04:04:05.123456789+01:00")));Assert.True(f.Compare(pair.Left,pair.Right));
        var legacy=XdmValue.FromTime(new XPathDateTime(2026,11,25,3,4,5,123,0,true),true);var p=f.Admit(f.Value(legacy,type),f.Value(Q.T("03:04:05.123Z")));Assert.True(f.Compare(p.Left,p.Right));
    }

    /// <summary>Checks C09 dateTimeStamp constraints follow actual type rather than descriptive hints.</summary>
    [Fact] public void C09_StampTypeProof()
    {
        using var f=new Q.Fixture();var stamp=f.Authority.Derived("dateTimeStamp");
        var valid=f.Admit(f.Value(Q.D(stamp:true),stamp),f.Value(Q.D()));Assert.True(f.Compare(valid.Left,valid.Right));
        var absent=f.Admit(f.Value(Q.D("2000-01-02T03:04:05.123456789"),stamp),f.Value(Q.D()));Assert.Throws<ArgumentException>(()=>f.Compare(absent.Left,absent.Right));
        var hint=f.Admit(f.Value(Q.D(stamp:true),f.Authority.Builtins["dateTime"]),f.Value(Q.D()));Assert.Throws<ArgumentException>(()=>f.Compare(hint.Left,hint.Right));
    }

    /// <summary>Checks C10 malformed legacy calendars refuse only after actual family is proved.</summary>
    /// <param name="mode">Malformed representation.</param>
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void C10_LegacyValidation(int mode)
    {
        using var f=new Q.Fixture();var raw=new XPathDateTime(2000,1,2,mode==0?25:3,4,mode==1?60:5,mode==2?1000:123,mode==3?60:0,mode!=3);
        var bad=XdmValue.FromDateTime(raw,mode==4?false:raw.HasTimezone);var pair=f.Admit(f.Value(bad),f.Value(Q.D()));
        Assert.ThrowsAny<ArgumentException>(()=>f.Compare(pair.Left,pair.Right));Assert.True(f.Authority.Resolutions>0);
        var good=f.Admit(f.Value(XdmValue.FromDateTime(new DateTimeOffset(2000,1,2,3,4,5,123,TimeSpan.Zero))),f.Value(Q.D("2000-01-02T03:04:05.123Z")));Assert.True(f.Compare(good.Left,good.Right));
    }

    /// <summary>Checks C11 exact precision survives comparison while receiving capacities refuse.</summary>
    [Fact] public void C11_ExactPrecisionAndCaps()
    {
        using var f=new Q.Fixture("$left lt $right");var pair=f.Admit(f.Value(Q.D()),f.Value(Q.D("2000-01-02T03:04:05.123456790Z")));Assert.True(f.Compare(pair.Left,pair.Right));
        var high=f.Admit(f.Value(Q.D("2000-01-02T03:04:00."+new string('0',255)+"1Z")),f.Value(Q.D("2000-01-02T03:04:00."+new string('0',255)+"2Z")));Assert.True(f.Compare(high.Left,high.Right));
        using var small=new Q.Fixture(digits:5,scale:3);var large=small.Value(Q.D());small.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedValue(small.Snapshot,large));return 0;});
        var ok=small.Admit(small.Value(Q.D("2000-01-02T03:04:05.123Z")),small.Value(Q.D("2000-01-02T03:04:05.123Z")));Assert.True(small.Compare(ok.Left,ok.Right));
        using var whole=new Q.Fixture(digits:5,scale:0);var legacy=XdmValue.FromDateTime(new DateTimeOffset(2000,1,2,3,4,5,123,TimeSpan.Zero));
        var legacyPair=whole.Admit(whole.Value(legacy),whole.Value(Q.D("2000-01-02T03:04:05Z")));Assert.Throws<NotSupportedException>(()=>whole.Compare(legacyPair.Left,legacyPair.Right));
        var wholePair=whole.Admit(whole.Value(XdmValue.FromDateTime(new DateTimeOffset(2000,1,2,3,4,5,TimeSpan.Zero))),whole.Value(Q.D("2000-01-02T03:04:05Z")));Assert.True(whole.Compare(wholePair.Left,wholePair.Right));
    }

    /// <summary>Checks C12 all six operators produce plain booleans for less/equal/greater vectors.</summary>
    /// <param name="operation">Comparison operator.</param>
    [Theory] [InlineData("eq")] [InlineData("ne")] [InlineData("lt")] [InlineData("le")] [InlineData("gt")] [InlineData("ge")]
    public void C12_OperatorResults(string operation)
    {
        using var f=new Q.Fixture("$left "+operation+" $right");
        foreach(int order in new[]{-1,0,1})
        {
            var pair=f.Admit(f.Value(Q.D("2000-01-02T03:04:0"+(5+order)+"Z")),f.Value(Q.D("2000-01-02T03:04:05Z")));
            var result=f.Run(op=>op.Evaluate(Q.Fixture.Context(pair.Left,pair.Right)));
            bool expected=operation switch {"eq"=>order==0,"ne"=>order!=0,"lt"=>order<0,"le"=>order<=0,"gt"=>order>0,_=>order>=0};
            Assert.Equal(expected,result.BooleanValue);Assert.Equal(XdmValueKind.Boolean,result.Kind);Assert.False(SchemaAtomics.TryGetProvenance(result,out _));Assert.Null(result.UserSchemaTypeName);
        }
    }

    /// <summary>Checks C13 the frozen implicit timezone affects absent offsets only.</summary>
    /// <param name="timezone">Valid boundary or central context offset.</param>
    [Theory] [InlineData(-840)] [InlineData(0)] [InlineData(840)]
    public void C13_ImplicitTimezone(int timezone)
    {
        using var f=new Q.Fixture(timezone:timezone);var a=Q.D("2000-01-02T03:04:05");var dto=new DateTimeOffset(2000,1,2,3,4,5,TimeSpan.FromMinutes(timezone));
        var pair=f.Admit(f.Value(a),f.Value(XdmValue.FromDateTime(dto)));Assert.True(f.Compare(pair.Left,pair.Right));
        var aware=f.Admit(f.Value(Q.D()),f.Value(Q.D("2000-01-02T04:04:05.123456789+01:00")));Assert.True(f.Compare(aware.Left,aware.Right));
    }

    /// <summary>Checks C14 time retains normalized UTC day displacement and wide Gregorian years work.</summary>
    [Fact] public void C14_ReferenceDayAndWideYears()
    {
        using var f=new Q.Fixture("$left gt $right");var time=f.Admit(f.Value(Q.T("23:00:00-03:00")),f.Value(Q.T("02:00:00Z")));Assert.True(f.Compare(time.Left,time.Right));
        var years=f.Admit(f.Value(Q.D("10000-01-01T00:00:00Z")),f.Value(Q.D("0000-01-01T00:00:00Z")));Assert.True(f.Compare(years.Left,years.Right));
        var bce=f.Admit(f.Value(Q.D("0000-01-01T00:00:00Z")),f.Value(Q.D("-0400-02-29T00:00:00Z")));Assert.True(f.Compare(bce.Left,bce.Right));
    }

    /// <summary>Checks C15 actual types and physical families cannot be authorized by labels.</summary>
    [Fact] public void C15_TypeAndFamilyMismatch()
    {
        using var f=new Q.Fixture();var mislabeled=f.Admit(f.Value(Q.D(),f.Authority.Builtins["string"]),f.Value(Q.D()));Assert.Throws<NotSupportedException>(()=>f.Compare(mislabeled.Left,mislabeled.Right));
        var wrongPayload=f.Admit(f.Value(XdmValue.FromString("2000-01-02T03:04:05Z","dateTime"),f.Authority.Builtins["dateTime"]),f.Value(Q.D()));Assert.Throws<ArgumentException>(()=>f.Compare(wrongPayload.Left,wrongPayload.Right));
        var mixed=f.Admit(f.Value(Q.D()),f.Value(Q.T()));Assert.Throws<ArgumentException>(()=>f.Compare(mixed.Left,mixed.Right));
        var badLabel=XdmValue.FromDateTime(new XPathDateTime(2000,1,2,3,4,5,0,0,true),true,"gYear");var label=f.Admit(f.Value(badLabel),f.Value(Q.D()));Assert.Throws<NotSupportedException>(()=>f.Compare(label.Left,label.Right));
    }

    /// <summary>Checks C16 plain, foreign-session and nonmember carriers refuse.</summary>
    [Fact] public void C16_CarrierMembership()
    {
        using var f=new Q.Fixture();using var other=new Q.Fixture();var a=f.Admit(f.Value(Q.D()),f.Value(Q.D()));var b=other.Admit(other.Value(Q.D()),other.Value(Q.D()));
        Assert.Throws<ArgumentException>(()=>f.Compare(Q.D(),a.Right));Assert.Throws<ArgumentException>(()=>f.Compare(a.Left,b.Right));
        Assert.Throws<ArgumentException>(()=>f.Session.Run([],op=>op.Evaluate(Q.Fixture.Context(a.Left,a.Right))));
        Assert.Throws<ArgumentException>(()=>f.Session.Run([other.Snapshot],_=>0));Assert.True(f.Compare(a.Left,a.Right));
    }

    /// <summary>Checks C16 both operands may belong to different admitted snapshots in the same session.</summary>
    [Fact] public void C16_TwoAdmittedSources()
    {
        using var f=new Q.Fixture();var second=new S.Input();var origin=new S.Origin();f.Advisor.Origins.Add(origin);
        using var snapshot=f.Session.OpenSnapshot(second,second,origin).Value;
        var left=f.Value(Q.D());var right=f.Value(Q.D());f.Advisor.Values[right]=(second,origin);
        right.Items=[new(Q.D(),new S.Provenance(right,f.Governing,f.Authority.Builtins["dateTime"],origin))];
        Assert.True(f.Session.Run([f.Snapshot,snapshot],op=>
        {
            var a=op.ReadTypedValue(f.Snapshot,left).Value;var b=op.ReadTypedValue(snapshot,right).Value;
            return op.Evaluate(Q.Fixture.Context(a,b)).BooleanValue;
        }));
        Assert.Equal(0,second.Roots);
    }

    /// <summary>Checks C17 revocation and unsupported or foreign authority answers refuse at callback boundaries.</summary>
    /// <param name="mode">Hostile provider behavior.</param>
    [Theory] [InlineData("source")] [InlineData("advisor")] [InlineData("type")] [InlineData("foreign")] [InlineData("unsupported")] [InlineData("unresolved")] [InlineData("late")]
    public void C17_HostileCallbacks(string mode)
    {
        using var f=new Q.Fixture();var actual=f.Authority.Derived("dateTime");var pair=f.Admit(f.Value(Q.D(),actual),f.Value(Q.D()));
        int callback=0;var stamp=f.Authority.Builtins["dateTimeStamp"];
        if(mode=="unresolved")f.Authority.Builtins.Remove("dateTimeStamp");
        else if(mode=="foreign")f.Authority.ForeignResolution=true;
        else if(mode=="unsupported")f.Authority.UnsupportedRelation=true;
        else f.Authority.Hook=stage=>{if(stage!=(mode=="advisor"?"derived":"resolve:dateTime"))return;if(mode=="late"&&++callback<5)return;f.Authority.Hook=null;if(mode is "source" or "late")f.Input.Available=false;else if(mode=="advisor")f.Advisor.Available=false;else f.Authority.Types.Remove(actual);};
        if(mode is "source" or "advisor" or "late")Assert.Throws<ObjectDisposedException>(()=>f.Compare(pair.Left,pair.Right));
        else if(mode is "unsupported" or "unresolved")Assert.Throws<NotSupportedException>(()=>f.Compare(pair.Left,pair.Right));
        else Assert.Throws<ArgumentException>(()=>f.Compare(pair.Left,pair.Right));
        f.Authority.Hook=null;f.Input.Available=true;f.Advisor.Available=true;f.Authority.Types.Add(actual);f.Authority.ForeignResolution=false;f.Authority.UnsupportedRelation=false;f.Authority.Builtins["dateTimeStamp"]=stamp;Assert.True(f.Compare(pair.Left,pair.Right));
    }

    /// <summary>Checks C18 cancellation is observed before binding and after authority callbacks.</summary>
    /// <param name="stage">Callback stage that cancels raw work.</param>
    [Theory] [InlineData("resolve:dateTime")] [InlineData("same")] [InlineData("derived")] [InlineData("after-compute")]
    public void C18_Cancellation(string stage)
    {
        using var canceled=new CancellationTokenSource();canceled.Cancel();var assessor=new S.Assessor();Assert.Throws<OperationCanceledException>(()=>Session.CompileCalendarComparison("$left eq $right",Limits,assessor,0,cancellationToken:canceled.Token));Assert.Equal(0,assessor.Binds);
        using var f=new Q.Fixture();using var raw=new CancellationTokenSource();var pair=f.Admit(f.Value(Q.D(),f.Authority.Derived("dateTime")),f.Value(Q.D()));
        int calls=0;f.Authority.Hook=at=>{if(stage=="after-compute"?at=="resolve:dateTime"&&++calls==5:at==stage)raw.Cancel();};Assert.Throws<OperationCanceledException>(()=>f.Run(op=>op.Evaluate(Q.Fixture.Context(pair.Left,pair.Right)),raw.Token));f.Authority.Hook=null;Assert.True(f.Compare(pair.Left,pair.Right));
    }

    /// <summary>Checks C19 asynchronous raw work retains resources while disposal revokes access.</summary>
    /// <returns>Completion after raw resources and expired access have been checked.</returns>
    [Fact] public async Task C19_AsyncLifetime()
    {
        using var f=new Q.Fixture();var pair=f.Admit(f.Value(Q.D()),f.Value(Q.D()));
        Assert.True(await f.Session.RunAsync([f.Snapshot],op=>Task.FromResult(op.Evaluate(Q.Fixture.Context(pair.Left,pair.Right)).BooleanValue)));
        var started=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var finish=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);SchemaQueryOperation? escaped=null;
        var task=f.Session.RunAsync([f.Snapshot],async op=>{escaped=op;started.SetResult();await finish.Task;return op.Evaluate(Q.Fixture.Context(pair.Left,pair.Right));});
        await started.Task;f.Snapshot.Dispose();Assert.Equal(0,f.Input.Releases);finish.SetResult();await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await task);Assert.Equal(1,f.Input.Releases);
        Assert.Throws<ObjectDisposedException>(()=>escaped!.Evaluate(Q.Fixture.Context(pair.Left,pair.Right)));Assert.False(SchemaAtomics.TryGetProvenance(pair.Left,out _));
    }

    /// <summary>Checks C20 existing typed identity, exact getter and map boundaries remain intact.</summary>
    [Fact] public void C20_ExistingProfileBoundaries()
    {
        using var identity=new S.Fixture();var plain=Q.D();var c=identity.Value(plain);var atom=identity.Run(op=>op.ReadTypedValue(identity.Snapshot,c).Value);
        var returned=identity.Run(op=>op.Evaluate(new EvaluationContext().WithVariable("value",atom)));Assert.True(SchemaAtomics.TryGetProvenance(returned,out _));
        Assert.Throws<NotSupportedException>(()=>plain.DateTimeXPathValue);Assert.Throws<NotSupportedException>(()=>XdmValueEqualityComparer.Instance.GetHashCode(plain));
        Assert.Throws<NotSupportedException>(()=>XdmCalendarInterop.Read(atom));
        var assessor=new S.Assessor();Assert.Equal(SchemaResultKind.Unsupported,Session.CompileXPath("$left eq $right",Limits,assessor:assessor).Kind);Assert.Equal(0,assessor.Binds);
    }
}
