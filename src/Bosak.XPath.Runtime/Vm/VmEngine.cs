// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : A register-based virtual machine that interprets <see cref="IrModule"/> instructions
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Implemented string, sequence, and aggregate VM opcodes                                 |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added Intersect, Except, and SimpleMap VM handlers                                     |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Added Map, Array, and Lookup VM handlers                                               |
//                      | Charles Korthout | 0.5   | 19-05-2026     | Added occurrence indicator support for InstanceOf, Cast, Castable, TreatAs             |
//                      | Charles Korthout | 0.6   | 22-05-2026     | Expanded TryCast with validation for xs: types, whitespace, Unicode, extended years      |
//                      | Charles Korthout | 0.6   | 19-05-2026     | Optimized Subscript, First, Last VM handlers to avoid full sequence materialization    |
//                      | Charles Korthout | 0.7   | 21-05-2026     | Divide opcode returns decimal for integer operands (XPath div semantics)               |
//                      | Charles Korthout | 0.8   | 21-05-2026     | MapAdd uses XdmValue keys with numeric promotion; fixed xs:boolean string cast         |
//                      | Charles Korthout | 0.9   | 22-05-2026     | ItemInstanceOf recognizes duration, dayTimeDuration, yearMonthDuration                 |
//                      | Charles Korthout | 1.0   | 23-05-2026     | Added TryCast support for many xs: types, duration normalization, boolean→numeric       |
//                      | Charles Korthout | 1.1   | 24-05-2026     | Range opcode uses lazy IntegerRangeSequence to avoid OOM on huge ranges                 |
//                      | Charles Korthout | 1.2   | 24-05-2026     | Added date/time value comparison type checking (XPTY0004 for cross-subtype)            |
//                      | Charles Korthout | 1.3   | 27-05-2026     | Added DocumentRoot VM handler for absolute XPath paths                                 |
//                      | Charles Korthout | 1.4   | 29-05-2026     | Fixed TryCast to return empty sequence for empty input (xs:type(()) semantics)         |
//                      | Charles Korthout | 1.5   | 30-05-2026     | Fixed Compare/CompareGeneral to return empty sequence for empty operands; added backwards-compatible coercion |
//                      | Charles Korthout | 1.6   | 30-05-2026     | Added PathStepMap opcode for per-context-item predicate evaluation on path steps        |
//                      | Charles Korthout | 1.7   | 30-05-2026     | Filter opcode treats double/decimal/float predicates as numeric position (fixes path-007/008) |
//                      | Charles Korthout | 1.8   | 30-05-2026     | IsSameNode unwraps singleton sequences; returns empty for empty-seq operand (fixes boolean-074/075) |
//                      | Charles Korthout | 1.9   | 31-05-2026     | Implemented PrecedesNode/FollowsNode (<< / >>) using DocumentOrder                          |
//                      | Charles Korthout | 2.0   | 01-06-2026     | Include XPST0017 error code in function-not-found exceptions                             |
//                      | Charles Korthout | 2.1   | 02-06-2026     | Numeric predicate uses exact equality, not Math.Round (XPath 2.0 §3.2.4)                 |
//                      | Charles Korthout | 2.2   | 03-06-2026     | MultiplyOrAddInteger: detect overflow, promote to decimal (fixes number-0111)            |
//                      | Charles Korthout | 2.3   | 01-06-2026     | Use module.MaxRegisterCount instead of hardcoded 256 for register array sizing           |
//                      | Charles Korthout | 2.4   | 05-06-2026     | Inline function sequence param validation; numeric promotion; node()/anyAtomicType matching |
//                      | Charles Korthout | 2.5   | 05-06-2026     | Removed global NormalizeSequence from Execute; path/union already normalize via opcodes     |
//                      | Charles Korthout | 2.6   | 05-06-2026     | Added function(*)/map(*)/array(*) support to ValueMatchesType for instance-of checks      |
//                      | Charles Korthout | 2.7   | 05-06-2026     | Added typed function signature matching (function(T...) as R) with contravariant params   |
//                      | Charles Korthout | 2.8   | 05-06-2026     | Node comparison operators raise XPTY0004 for non-node operands; ParseException XPST0003  |
//                      | Charles Korthout | 2.9   | 05-06-2026     | ResolveVariableName handles Q{uri}local; inline function params bind by expanded QName     |
//                      | Charles Korthout | 2.10  | 11-06-2026     | Apply opcode invokes map/array functions; date comparison casts untypedAtomic operands    |
//                      | Charles Korthout | 2.11  | 13-06-2026     | Empty-URI EQName support in ResolveVariableName (Q{}local)                              |
//                      | Charles Korthout | 2.12  | 13-06-2026     | Parameterized map(K,V) and array(T) matching in ValueMatchesType                         |
//                      | Charles Korthout | 2.13  | 13-06-2026     | Date/time comparison uses implicit timezone; time constructor avoids DateTimeOffset       |
//                      | Charles Korthout | 2.14  | 25-06-2026     | QName equality compares namespace URI + local name, ignoring prefix (fixes type-0129)   |
//                      | Charles Korthout | 2.15  | 25-06-2026     | Value comparison casts xs:untypedAtomic operands to xs:string before comparing (fixes type-0165)            |
//                      | Charles Korthout | 2.16  | 25-06-2026     | LoadContextItem raises XPDY0002 when the XPath context item is absent                      |
//                      | Charles Korthout | 2.17  | 25-06-2026     | InstanceOf applies default element namespace and reports unknown types (XPST0051)        |
//                      | Charles Korthout | 2.18  | 26-06-2026     | Subscript/First/Last return empty sequence instead of Undefined for out-of-range/missing items |
//                      | Charles Korthout | 2.29  | 09-07-2026     | StringConcat atomizes operands instead of using XdmValue.ToString()                    |
//                      | Charles Korthout | 2.18  | 26-06-2026     | InstanceOf recognises parameterised sequence type names and avoids spurious XPST0051   |
//                      | Charles Korthout | 2.19  | 26-06-2026     | CompareGeneral returns false (not empty sequence) for empty general-comparison operands |
//                      | Charles Korthout | 2.20  | 28-06-2026     | NormalizeSequence uses HashSet for duplicate removal; restores catalog self-test speed   |
//                      | Charles Korthout | 2.21  | 26-06-2026     | Integer/decimal division and modulo by zero raise FOAR0001 DynamicException            |
//                      | Charles Korthout | 2.22  | 30-06-2026     | Cast to xs:float parses via float.TryParse to preserve single-precision lexical form  |
//                      | Charles Korthout | 2.23  | 02-07-2026     | Root opcode handles parentless nodes and raises XPDY0050; Range atomizes operands       |
//                      | Charles Korthout | 2.24  | 03-07-2026     | Trim whitespace when casting strings to xs:integer (TVT function results)              |
//                      | Charles Korthout | 2.26  | 19-07-2026     | cbcl fixes: gDay/gMonthDay timezone-aware equality; duration*NaN raises FOCA0005          |
//                      | Charles Korthout | 2.26  | 26-06-2026     | LookupWildcard flattens map values and array members                                   |
//                      | Charles Korthout | 2.27  | 26-06-2026     | NormalizeSequence places document-rooted nodes before parentless nodes                 |
//                      | Charles Korthout | 2.28  | 26-06-2026     | Removed leftover debug output from CompareCore                                         |
//                      | Charles Korthout | 2.29  | 13-07-2026     | Honor FunctionSignature.DynamicImplementation for dynamic named-function calls         |
//                      | Charles Korthout | 2.30  | 13-07-2026     | HOF: closure capture, dynamic-call arity/conversion, coerced items, instance-of types  |
//                      | Charles Korthout | 2.31  | 14-07-2026     | Dynamic-call String conversion back to spec (untypedAtomic cast + URI promotion only)  |
//                      | Charles Korthout | 2.32   | 14-07-2026     | NamedFunctionItem carries defining context; fallback resolution across contexts        |
//                      | Charles Korthout | 2.33  | 14-07-2026     | CompareGeneral integer-set fast path (cached HashSet) for = / != on large sequences    |
//                      | Charles Korthout | 2.34  | 15-07-2026     | OverflowException during execution is surfaced as FOAR0002 (numeric range error)       |
//                      | Charles Korthout | 2.35  | 15-07-2026     | ConvertArgToKind passes empty sequences through for optional parameters (xs:T?/xs:T*)  |
//                      | Charles Korthout | 2.36  | 15-07-2026     | Lookup semantics: container-major multi-key, single-result unwrap, strict xs:integer array keys (FOAY0001 bounds/XPTY0004 type), array-as-function via shared ArrayLookup, CompareGeneral atomizes arrays |
//                      | Charles Korthout | 2.37  | 15-07-2026     | MapAdd raises XQDY0137 on duplicate keys in map constructors (serialize-xml-119/124/125) |
//                      | Charles Korthout | 2.38  | 15-07-2026     | Tier-2i: strict singleton map keys (XPTY0004); Exists/Empty opcodes count-based; parameterized map(K,V)/array(T)/function(A)-as-R instance-of routing; maps/arrays match function(*); map/array-as-function value rule; named-fn signature checks with context; structural function-family subtyping (MapTest-050..054); XPST0003/XPST0051 map-type validation |
//                      | Charles Korthout | 2.39  | 15-07-2026     | Tier-2j: For opcode binds optional positional variable (1-based); Atomize raises XPTY0004 on multi-item sequences; arithmetic operands validated numeric/untypedAtomic (XPTY0004) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.40  | 15-07-2026     | Tier-2k: annotation-aware instance-of (integer/string/duration supertype walks, nmtoken); 'to' operand validation (XPTY0004); value-comparison string-vs-numeric XPTY0004; general-comparison untypedAtomic rule-b casting via primitive base type; for/some/every bind namespace-qualified variables; duration casts keep subtype annotation |
//                      | Charles Korthout | 2.41  | 18-07-2026     | NamedFunctionItem dynamic calls use defining-context focus (fn:function-lookup base-uri) |
//                      | Charles Korthout | 2.42  | 19-07-2026     | Tier-2u: xs:numeric cast and xs:numeric#1 constructor                                  |
//                      | Charles Korthout | 2.43  | 19-07-2026     | Tier-2v: idiv NaN/INF and numeric-literal+keyword boundary checks                       |
//                      | Charles Korthout | 2.44  | 19-07-2026     | Tier-2x: floating-point mod by zero returns NaN instead of FOAR0001                     |
//                      | Charles Korthout | 2.45  | 19-07-2026     | Castable opcode catches overflow/cast errors; empty sequence only castable for ?/*    |
//                      | Charles Korthout | 2.46  | 19-07-2026     | xs:unsignedLong values above long.MaxValue stored as xs:decimal with subtype annotation; instance-of accepts decimal-backed integer subtypes |
//                      | Charles Korthout | 2.47  | 19-07-2026     | RangeExpr supports xs:integer operands that exceed long range via DecimalRangeSequence |
//                      | Charles Korthout | 2.48  | 19-07-2026     | CompareGeneral enumerates operands lazily to avoid materializing huge ranges |
//                      | Charles Korthout | 2.49  | 19-07-2026     | NormalizeSequence stable-sorts namespace nodes by owner element so namespace axis is document-ordered |
//                      | Charles Korthout | 2.50  | 19-07-2026     | Duration multiply/divide uses round-half-up for yearMonth and overflow-safe decimal for dayTime |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.51  | 19-07-2026     | Tier-2z: union/intersect/except require node sequences; added LoadNode VM opcode           |
//                      | Charles Korthout | 2.52  | 20-07-2026     | Cast/Castable pass EvaluationContext; xs:QName cast resolves prefixes and default namespace |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.52  | 19-07-2026     | Tier-2z: date/dayTime arithmetic zeroes time for xs:date; time +/- yearMonth raises XPTY0004 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.53  | 19-07-2026     | Tier-2z: unary plus validates numeric operand and raises XPTY0004 for non-numeric        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.54  | 19-07-2026     | Tier-2z: arithmetic with xs:untypedAtomic now casts the result to xs:double                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.55  | 19-07-2026     | Tier-2z: duration div by NaN/0 checked before zero-duration short-circuit               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.56  | 20-07-2026     | PathStepMap raises XPTY0019 when context item is not a node (K2-Axes-50/53)            |
//                      | Charles Korthout | 2.57  | 20-07-2026     | Cast opcode raises XPTY0004 for empty input with occurrence One (K-SeqExprCast-67)     |
//                      | Charles Korthout | 2.58  | 20-07-2026     | Inline functions apply XPath function conversion rules to arguments (FunctionCall-010/011/025/026) |
//                      | Charles Korthout | 2.60  | 21-07-2026     | Added xs:dateTimeStamp cast, instance-of, and type hierarchy support                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.61  | 21-07-2026     | xs:NOTATION instance-of returns false; xs:QName name is case-sensitive (xs:qname raises XPST0051) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.62  | 21-07-2026     | ParseGDateTime uses regex to avoid IndexOutOfRangeException on gDay/gMonth/gMonthDay/gYearMonth |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.63  | 22-07-2026     | Added OrderBy and TupleBind VM handlers for XQuery FLWOR order by                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.59  | 21-07-2026     | Static function calls apply ParameterTypeNames conversion; URI promotion detects xs:anyURI annotation |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.64  | 25-07-2026     | Added GroupBy VM handler and grouping-key equality for XQuery FLWOR group by            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.65  | 25-07-2026     | Added Window VM handler for XQuery FLWOR tumbling/sliding windows                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.66  | 25-07-2026     | Window end-pos/no-end fixes; EnforceType; NaN ordering; XQST0076; dateTime group keys   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.67  | 25-07-2026     | Named-ref arity validation; group-by collation + g-date key ordering fixes             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.68  | 25-07-2026     | Constructor opcodes; default-elem-ns tags; group-key atomization; predicate EBV; scoping |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.69  | 25-07-2026     | Attributes in content (XQTY0024); ctor-local ns; type-prefix resolution; array content  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.70  | 25-07-2026     | Computed constructor execution with content accumulator and name resolution; window variable namespace binding |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.71  | 25-07-2026     | Literal-only attribute whitespace normalization; xml:id collapse normalization                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.72  | 26-07-2026     | User-function semantics: absent focus; full variable-scope snapshot per call; attribute atomization; function coercion; order-by type families |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.73  | 27-07-2026     | TryCatch: code-pattern clause matching, err:* binding, static-error bypass; FORG0001/XPDY0050/XQDY0074 codes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.74  | 28-07-2026     | NameTest XPST0081 for unbound prefixes; KindTestType opcode; instance-of prefixed-name ns check; attr type compat supertypes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.75  | 29-07-2026     | EnforceType atomization for typed variable initializers; XPST0081 for unbound function/variable prefixes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.76  | 29-07-2026     | Function-test annotation assertions stripped in InstanceOf; XQST0045 for reserved annotation namespaces |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.77  | 29-07-2026     | ApplyAxis raises XPTY0019 for atomic items in a path step's input sequence |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.78  | 29-07-2026     | Singleton-sequence unwrap for map/array/function-typed call parameters |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.79  | 29-07-2026     | Computed namespace prefix type check (XPTY0004); ns decls not XQTY0024 content; ns nodes atomize to xs:string |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.80  | 29-07-2026     | HOF: FOTY0013/XQTY0105 function items; Curry arity check; absent-focus named refs; base-URI capture; paren types |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.81  | 29-07-2026     | Plain xs:duration rejected in date/time arithmetic (XPTY0004, cbcl-plus/minus) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.82  | 29-07-2026     | Residual sweep: stable order-by; array atomize/flatten; default-ns computed names; div duration rule |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.83  | 01-08-2026     | XPath 1.0 BC: numeric fn args truncate to first item and convert via fn:number       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.84  | 01-08-2026     | BC first-item truncation for all singleton params; pre-atomization type pass-through |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.85  | 03-08-2026     | element()/attribute() kind tests honor Q{uri}local and default-element/no-namespace rules |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.86  | 03-08-2026     | Document-order sort computes keys in sequence order (detached-tree sequence stability) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.87  | 07-08-2026     | Dropped VM-level constructor attribute normalization: the parser already normalizes raw whitespace and exempts character references (K2-Serialization-6, xml-to-json-051) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.88  | 07-08-2026     | Comparison strictness sweep: same-type binary (hex/base64) comparisons order by decoded octets, mixed binary/non-binary XPTY0004; duration ordering honors the dynamic subtype annotation (plain xs:duration XPTY0004); general comparisons flatten nested arrays; unary minus on untypedAtomic yields xs:double; idiv raises FOAR0002 when the result exceeds xs:long range |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.89  | 07-08-2026     | NamespaceTest Q{}* sentinel (never the default element ns); EQName xs type names in ValueMatchesType/ApplyFunctionConversion; element/attribute(*, prefixed-T) kind tests validate the schema type (XPST0008/XPST0081) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.90  | 07-08-2026     | General comparisons on function items raise FOTY0013; named function items subtype-check against coarse kind-derived signatures (instanceof134); ApplyFunctionConversion (now public) atomizes array arguments into their members (FunctionCall-022) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.91  | 07-08-2026     | Coarse named-function return type: Undefined means empty-sequence() (xs-error-006/007) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.92  | 14-08-2026     | Order-by string comparisons use per-spec collation (default or explicit) instead of ordinal |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.93  | 14-08-2026     | Direct attribute constructors include comment/PI string values in attribute values (K2-DirectConElemAttr-42/43); raise XQDY0092 for invalid xml:space (K2-DirectConOther-65) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.94  | 15-08-2026     | ValueMatchesType preserves case for nested kind tests in document-node(element(...)) (NodeTest004) |
//                      | Charles Korthout | 2.95  | 15-08-2026     | Map dynamic calls return empty sequence for missing keys; map/array coercion to function types (UseCaseR31) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.96  | 18-08-2026     | ApplyAxis/PathStepMap treat empty-sequence input as empty instead of XPDY0002 (Catalog004) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.97  | 18-08-2026     | XQDY0101 for namespace constructor bound to XMLNS namespace (nscons-020)                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.98  | 18-08-2026     | Clamped function arity int.MaxValue raises FOAR0002 (fn-function-arity-017/fn-function-name-018) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.99  | 19-08-2026     | Schema-aware: user-defined schema types in ValueMatchesType/ApplyFunctionConversion; pass context to atomic type match |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.100 | 19-08-2026     | InstanceOf accepts prefixed user-defined schema types; ConvertSchemaValue keeps integer kind for integer subtypes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.99  | 18-08-2026     | Unprefixed computed attribute names do not use the default element namespace (currencysvg) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;
using Bosak.XPath.Runtime.Functions;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// A register-based virtual machine that interprets <see cref="IrModule"/> instructions.
/// </summary>
public static class VmEngine
{
    /// <summary>
    /// Executes a compiled IR module against the given evaluation context.
    /// </summary>
    public static XdmValue Execute(IrModule module, EvaluationContext context)
    {
        try
        {
            // The lowerer uses monotonic register allocation; size is determined at compile time.
            var registers = new XdmValue[module.MaxRegisterCount];
            var (result, _) = ExecuteBlock(module, context, registers, 0);
            return result;
        }
        catch (OverflowException ex)
        {
            // XPath surfaces numeric range failures as FOAR0002 (arithmetic overflow),
            // not as raw CLR conversion/negation exceptions.
            throw new InvalidOperationException($"FOAR0002: {ex.Message}", ex);
        }
    }

    private static (XdmValue Result, int NextIp) ExecuteBlock(
        IrModule module,
        EvaluationContext context,
        XdmValue[] registers,
        int startIp)
    {
        var instructions = module.Instructions;
        var literalPool = module.LiteralPool;
        int ip = startIp;
        while (ip < instructions.Length)
        {
            var instr = instructions[ip];

            switch (instr.OpCode)
            {
                // ------------------------------------------------------------------
                // Control flow
                // ------------------------------------------------------------------
                case IrOpCode.Nop:
                    ip++;
                    break;

                case IrOpCode.Return:
                    return (registers[instr.RegisterA], ip + 1);

                case IrOpCode.Jump:
                    ip = instr.Operand;
                    break;

                case IrOpCode.JumpIfTrue:
                    ip = registers[instr.RegisterA].EffectiveBooleanValue() ? instr.Operand : ip + 1;
                    break;

                case IrOpCode.JumpIfFalse:
                    ip = !registers[instr.RegisterA].EffectiveBooleanValue() ? instr.Operand : ip + 1;
                    break;

                case IrOpCode.JumpIfEmpty:
                    {
                        var seq = registers[instr.RegisterA];
                        bool isEmpty = seq.IsUndefined ||
                            (seq.IsSequence && seq.SequenceValue is not null &&
                             TryGetSequenceLength(seq.SequenceValue, out var len) && len == 0);
                        ip = isEmpty ? instr.Operand : ip + 1;
                        break;
                    }

                case IrOpCode.Call:
                    {
                        int argCount = instr.RegisterC;
                        int firstArgReg = instr.RegisterB;

                        string localName;
                        string nsUri;
                        var funcLiteral = literalPool[instr.Operand]!;
                        if (funcLiteral is ValueTuple<string, string> resolved)
                        {
                            localName = resolved.Item1;
                            nsUri = resolved.Item2;
                        }
                        else
                        {
                            string funcName = (string)funcLiteral;
                            (localName, nsUri) = ResolveFunctionName(funcName, context);
                        }

                        bool found = context.TryResolveFunction(nsUri, localName, argCount, out var sig);
                        if (!found)
                            throw new InvalidOperationException(
                                $"XPST0017: Function {{{nsUri}}}{localName}#{argCount} not found.");

                        // Build argument span
                        XdmValue[] args = new XdmValue[argCount];
                        for (int i = 0; i < argCount; i++)
                            args[i] = registers[firstArgReg + i];

                        // A singleton sequence unwraps to its item when the parameter is
                        // declared as a map, array, or function item (the function
                        // conversion rules applied to kind-typed parameters — covers
                        // map:size($ctx ! map{...}) and path-position map constructors).
                        for (int i = 0; i < argCount && i < sig.ParameterTypes.Count; i++)
                        {
                            if (sig.ParameterTypes[i] is XdmValueKind.Map or XdmValueKind.Array or XdmValueKind.Function)
                                args[i] = UnwrapSingletonItem(args[i], sig.ParameterTypes[i]);
                        }

                        // Apply XPath 3.1 function conversion rules when the function signature
                        // declares precise parameter sequence types. This covers untypedAtomic casting,
                        // numeric promotion, and URI promotion for static calls (FunctionCall-011).
                        if (sig.ParameterTypeNames != null)
                        {
                            for (int i = 0; i < argCount && i < sig.ParameterTypeNames.Count; i++)
                            {
                                var paramType = sig.ParameterTypeNames[i];
                                if (!string.IsNullOrEmpty(paramType))
                                    args[i] = ApplyFunctionConversion(args[i], paramType!, context);
                            }
                        }

                        registers[instr.RegisterA] = sig.Implementation(context, args);
                        ip++;
                        break;
                    }

                case IrOpCode.TailCall:
                    throw new NotImplementedException("TailCall is not yet implemented.");

                // ------------------------------------------------------------------
                // Context
                // ------------------------------------------------------------------
                case IrOpCode.LoadContextItem:
                    if (context.ContextItem.IsUndefined)
                        throw new InvalidOperationException("XPDY0002: The context item is absent.");
                    registers[instr.RegisterA] = context.ContextItem;
                    ip++;
                    break;

                case IrOpCode.LoadContextPosition:
                    registers[instr.RegisterA] = XdmValue.FromInteger(context.ContextPosition);
                    ip++;
                    break;

                case IrOpCode.LoadContextSize:
                    registers[instr.RegisterA] = XdmValue.FromInteger(context.ContextSize);
                    ip++;
                    break;

                case IrOpCode.SetContext:
                    // Not used by current lowerer; placeholder for future use.
                    throw new NotImplementedException("SetContext is not yet implemented.");

                // ------------------------------------------------------------------
                // Variables
                // ------------------------------------------------------------------
                case IrOpCode.LoadVariable:
                    {
                        string localName;
                        string nsUri;
                        var varLiteral = literalPool[instr.Operand]!;
                        if (varLiteral is ValueTuple<string, string> resolvedVar)
                        {
                            localName = resolvedVar.Item1;
                            nsUri = resolvedVar.Item2;
                        }
                        else
                        {
                            string varName = (string)varLiteral;
                            (localName, nsUri) = ResolveVariableName(varName, context);
                        }

                        if (!context.TryGetVariable(localName, out var value, nsUri))
                        {
                            if (context.BackwardsCompatible)
                            {
                                // XPath 1.0 compatibility: an undefined variable is treated
                                // as an empty sequence.
                                registers[instr.RegisterA] = XdmValue.Undefined;
                            }
                            else
                            {
                                string displayName = string.IsNullOrEmpty(nsUri) ? localName : $"Q{{{nsUri}}}{localName}";
                                throw new InvalidOperationException($"XPST0008: Variable ${displayName} is not defined.");
                            }
                        }
                        else
                        {
                            registers[instr.RegisterA] = value;
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.StoreVariable:
                    {
                        string localName;
                        string nsUri;
                        var varLiteral = literalPool[instr.Operand]!;
                        if (varLiteral is ValueTuple<string, string> resolvedVar)
                        {
                            localName = resolvedVar.Item1;
                            nsUri = resolvedVar.Item2;
                        }
                        else
                        {
                            string varName = (string)varLiteral;
                            (localName, nsUri) = ResolveVariableName(varName, context);
                        }
                        context.WithVariable(localName, registers[instr.RegisterB], nsUri);
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Literals
                // ------------------------------------------------------------------
                case IrOpCode.LoadString:
                    registers[instr.RegisterA] = XdmValue.FromString((string)literalPool[instr.Operand]!);
                    ip++;
                    break;

                case IrOpCode.LoadInteger:
                    registers[instr.RegisterA] = XdmValue.FromInteger((long)literalPool[instr.Operand]!);
                    ip++;
                    break;

                case IrOpCode.LoadDecimal:
                    registers[instr.RegisterA] = XdmValue.FromDecimal((decimal)literalPool[instr.Operand]!);
                    ip++;
                    break;

                case IrOpCode.LoadDouble:
                    registers[instr.RegisterA] = XdmValue.FromDouble((double)literalPool[instr.Operand]!);
                    ip++;
                    break;

                case IrOpCode.LoadBoolean:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(instr.Operand != 0);
                    ip++;
                    break;

                case IrOpCode.LoadNode:
                    registers[instr.RegisterA] = (XdmValue)literalPool[instr.Operand]!;
                    ip++;
                    break;

                case IrOpCode.LoadEmptySequence:
                    registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                    ip++;
                    break;

                case IrOpCode.Move:
                    registers[instr.RegisterA] = registers[instr.RegisterB];
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // Sequences
                // ------------------------------------------------------------------
                case IrOpCode.SequenceStart:
                    // Store a mutable list as an external value during construction.
                    registers[instr.RegisterA] = XdmValue.FromExternal(new List<XdmValue>());
                    ip++;
                    break;

                case IrOpCode.SequenceAdd:
                    {
                        var list = (List<XdmValue>)registers[instr.RegisterA].ExternalValue!;
                        var item = registers[instr.RegisterB];
                        // Flatten empty sequences (e.g., () in (a, (), b))
                        if (item.IsUndefined)
                        {
                            ip++;
                            break;
                        }
                        if (item.IsSequence && item.SequenceValue is not null)
                        {
                            if (item.SequenceValue.TryGetLength(out var len) && len == 0)
                            {
                                ip++;
                                break;
                            }
                            // Sequences are always flat in XPath; flatten nested sequences
                            foreach (var seqItem in item.SequenceValue)
                                list.Add(seqItem);
                        }
                        else
                        {
                            list.Add(item);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.SequenceEnd:
                    {
                        var list = (List<XdmValue>)registers[instr.RegisterA].ExternalValue!;
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(list));
                        ip++;
                        break;
                    }

                case IrOpCode.Singleton:
                    registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Singleton(registers[instr.RegisterB]));
                    ip++;
                    break;

                case IrOpCode.Range:
                    {
                        var left = registers[instr.RegisterB];
                        var right = registers[instr.RegisterC];

                        // XPath 1.0 backwards compatibility: the operands of "to" are
                        // converted to integers by taking the first item of a sequence.
                        if (context.BackwardsCompatible)
                        {
                            left = FirstItemOrUndefined(left);
                            right = FirstItemOrUndefined(right);
                        }

                        if (left.IsUndefined || IsEmptySeq(left) || right.IsUndefined || IsEmptySeq(right))
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                            ip++;
                            break;
                        }
                        if (context.BackwardsCompatible)
                        {
                            long from = ToInteger(left);
                            long to = ToInteger(right);
                            if (from > to)
                            {
                                registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                                ip++;
                                break;
                            }
                            registers[instr.RegisterA] = XdmValue.FromSequence(
                                XdmSequence.FromSource(new IntegerRangeSequence(from, to)));
                            ip++;
                            break;
                        }

                        if (!TryGetRangeOperand(left, out var fromDecimal) || !TryGetRangeOperand(right, out var toDecimal))
                            throw new InvalidOperationException("XPTY0004: The operands of 'to' must be xs:integer");

                        if (fromDecimal > toDecimal)
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                            ip++;
                            break;
                        }
                        if (fromDecimal >= long.MinValue && fromDecimal <= long.MaxValue
                            && toDecimal >= long.MinValue && toDecimal <= long.MaxValue)
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(
                                XdmSequence.FromSource(new IntegerRangeSequence((long)fromDecimal, (long)toDecimal)));
                        }
                        else
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(
                                XdmSequence.FromSource(new DecimalRangeSequence(fromDecimal, toDecimal)));
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Concatenate:
                    {
                        var left = MaterializeSequence(registers[instr.RegisterB]);
                        var right = MaterializeSequence(registers[instr.RegisterC]);
                        RequireNodeSequence(left);
                        RequireNodeSequence(right);
                        var combined = new List<XdmValue>(left.Length + right.Length);
                        combined.AddRange(left);
                        combined.AddRange(right);
                        registers[instr.RegisterA] = NormalizeSequence(
                            XdmValue.FromSequence(MaterializedSequence.FromList(combined)));
                        ip++;
                        break;
                    }

                case IrOpCode.Intersect:
                    {
                        var left = MaterializeSequence(registers[instr.RegisterB]);
                        var right = MaterializeSequence(registers[instr.RegisterC]);
                        RequireNodeSequence(left);
                        RequireNodeSequence(right);
                        var rightNodes = new List<IXdmNode>();
                        foreach (var item in right)
                            if (item.IsNode)
                                rightNodes.Add(item.NodeValue);

                        var result = new List<XdmValue>();
                        foreach (var item in left)
                        {
                            if (!item.IsNode) continue;
                            foreach (var rn in rightNodes)
                            {
                                if (rn.IsSameNode(item.NodeValue))
                                {
                                    result.Add(item);
                                    break;
                                }
                            }
                        }
                        registers[instr.RegisterA] = NormalizeSequence(
                            XdmValue.FromSequence(MaterializedSequence.FromList(result)));
                        ip++;
                        break;
                    }

                case IrOpCode.Except:
                    {
                        var left = MaterializeSequence(registers[instr.RegisterB]);
                        var right = MaterializeSequence(registers[instr.RegisterC]);
                        RequireNodeSequence(left);
                        RequireNodeSequence(right);
                        var rightNodes = new List<IXdmNode>();
                        foreach (var item in right)
                            if (item.IsNode)
                                rightNodes.Add(item.NodeValue);

                        var result = new List<XdmValue>();
                        foreach (var item in left)
                        {
                            if (!item.IsNode) continue;
                            bool inRight = false;
                            foreach (var rn in rightNodes)
                            {
                                if (rn.IsSameNode(item.NodeValue))
                                {
                                    inRight = true;
                                    break;
                                }
                            }
                            if (!inRight)
                                result.Add(item);
                        }
                        registers[instr.RegisterA] = NormalizeSequence(
                            XdmValue.FromSequence(MaterializedSequence.FromList(result)));
                        ip++;
                        break;
                    }

                case IrOpCode.SimpleMap:
                    {
                        var sequence = registers[instr.RegisterB];
                        int rhsEntry = instr.Operand;
                        bool enforceNodeResult = instr.RegisterC != 0;

                        var items = MaterializeSequence(sequence);
                        var results = new List<XdmValue>();

                        // XPath path steps require every context item to be a node (XPTY0019).
                        // SimpleMap with ! allows non-node items, so only enforce in path mode.
                        if (enforceNodeResult)
                        {
                            foreach (var item in items)
                            {
                                if (!item.IsNode)
                                    throw new InvalidOperationException("XPTY0019: An axis step requires a node as context item.");
                            }
                        }

                        // Save context
                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;

                        for (int i = 0; i < items.Length; i++)
                        {
                            context.WithFocus(items[i], i + 1, items.Length);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, rhsEntry);

                            if (rhsResult.IsSequence && rhsResult.SequenceValue is not null)
                            {
                                foreach (var r in XdmSequence.FromSource(rhsResult.SequenceValue))
                                    results.Add(r);
                            }
                            else if (!rhsResult.IsUndefined)
                            {
                                results.Add(rhsResult);
                            }
                        }

                        // Restore context
                        context.WithFocus(savedItem, savedPos, savedSize);

                        // XPath 2.0/3.0: path expression result must not contain both nodes and non-nodes
                        if (enforceNodeResult)
                        {
                            bool hasNode = results.Any(r => r.IsNode);
                            bool hasNonNode = results.Any(r => !r.IsNode);
                            if (hasNode && hasNonNode)
                                throw new InvalidOperationException("XPTY0018: result of a path expression step contains both nodes and non-nodes");
                        }

                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            MaterializedSequence.FromList(results));
                        ip++;
                        break;
                    }

                case IrOpCode.PathStepMap:
                    {
                        var sequence = registers[instr.RegisterB];
                        if (sequence.IsUndefined)
                        {
                            // Empty sequence input to a path step simply produces an empty
                            // sequence; the real "no context item" case is caught earlier by
                            // LoadContextItem.
                            registers[instr.RegisterA] = XdmValue.Undefined;
                            ip++;
                            break;
                        }
                        int rhsEntry = instr.Operand;

                        var items = MaterializeSequence(sequence);
                        var results = new List<XdmValue>();

                        // Save context
                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;

                        for (int i = 0; i < items.Length; i++)
                        {
                            // A path step requires every context item to be a node (XPTY0019).
                            if (!items[i].IsNode)
                                throw new InvalidOperationException("XPTY0019: An axis step requires a node as context item.");

                            // Path-step predicates must see position=1, size=1
                            // for each context item (predicate is relative to the
                            // step result, not the outer sequence).
                            context.WithFocus(items[i], 1, 1);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, rhsEntry);

                            if (rhsResult.IsSequence && rhsResult.SequenceValue is not null)
                            {
                                foreach (var r in XdmSequence.FromSource(rhsResult.SequenceValue))
                                    results.Add(r);
                            }
                            else if (!rhsResult.IsUndefined)
                            {
                                results.Add(rhsResult);
                            }
                        }

                        // Restore context
                        context.WithFocus(savedItem, savedPos, savedSize);

                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            MaterializedSequence.FromList(results));
                        ip++;
                        break;
                    }

                case IrOpCode.Normalize:
                    {
                        registers[instr.RegisterA] = NormalizeSequence(registers[instr.RegisterB]);
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // FLWOR / Quantified
                // ------------------------------------------------------------------
                case IrOpCode.For:
                    {
                        var info = (QuantifiedLoopInfo)literalPool[instr.Operand]!;
                        var sequence = registers[instr.RegisterB];
                        var items = MaterializeSequence(sequence);
                        var results = new List<XdmValue>();

                        var (bindLocal, bindNs) = ResolveLoopVariableKey(info, context);

                        // Save the bindings of the loop variable and an optional positional
                        // variable. Let-bound (scoped) variables are captured separately and
                        // restored after EACH iteration: their bindings are scoped to the
                        // tuple and must not persist across iterations or leak out.
                        var savedBindings = new List<(string Name, string Ns, bool Had, XdmValue Value)>();
                        void Save(string name, string ns)
                        {
                            savedBindings.Add((name, ns, context.TryGetVariable(name, out var saved, ns), saved));
                        }
                        Save(bindLocal, bindNs);
                        if (info.PositionalVariableName is not null)
                            Save(info.PositionalVariableName, "");
                        var scopedSaved = new List<(string Name, bool Had, XdmValue Value)>();
                        if (info.ScopedVariableNames is not null)
                        {
                            foreach (var scoped in info.ScopedVariableNames)
                                scopedSaved.Add((scoped, context.TryGetVariable(scoped, out var saved, ""), saved));
                        }
                        void RestoreScoped()
                        {
                            foreach (var (name, had, value) in scopedSaved)
                            {
                                if (had)
                                    context.WithVariable(name, value, "");
                                else
                                    context.RemoveVariable(name, "");
                            }
                        }

                        int position = 0;
                        if (items.Length == 0 && info.AllowingEmpty)
                        {
                            // XQuery "allowing empty": one iteration with the variable bound to
                            // the empty sequence (and a declared positional variable bound to 0).
                            context.WithVariable(bindLocal, XdmValue.FromSequence(XdmSequence.Empty), bindNs);
                            if (info.PositionalVariableName is not null)
                                context.WithVariable(info.PositionalVariableName, XdmValue.FromInteger(0));
                            var (emptyResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);
                            if (emptyResult.IsSequence && emptyResult.SequenceValue is not null)
                            {
                                foreach (var r in XdmSequence.FromSource(emptyResult.SequenceValue))
                                    results.Add(r);
                            }
                            else if (!emptyResult.IsUndefined)
                            {
                                results.Add(emptyResult);
                            }
                        }

                        foreach (var item in items)
                        {
                            position++;
                            // FLWOR for-expression does NOT change the focus;
                            // it only binds the variable (and optional positional variable).
                            context.WithVariable(bindLocal, item, bindNs);
                            if (info.PositionalVariableName is not null)
                                context.WithVariable(info.PositionalVariableName, XdmValue.FromInteger(position));
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);
                            RestoreScoped();

                            if (rhsResult.IsSequence && rhsResult.SequenceValue is not null)
                            {
                                foreach (var r in XdmSequence.FromSource(rhsResult.SequenceValue))
                                    results.Add(r);
                            }
                            else if (!rhsResult.IsUndefined)
                            {
                                results.Add(rhsResult);
                            }
                        }

                        foreach (var (name, ns, had, value) in savedBindings)
                        {
                            if (had)
                                context.WithVariable(name, value, ns);
                            else
                                context.RemoveVariable(name, ns);
                        }

                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            MaterializedSequence.FromList(results));
                        ip++;
                        break;
                    }

                case IrOpCode.Some:
                    {
                        var info = (QuantifiedLoopInfo)literalPool[instr.Operand]!;
                        var sequence = registers[instr.RegisterB];
                        var items = MaterializeSequence(sequence);

                        var (bindLocal, bindNs) = ResolveLoopVariableKey(info, context);
                        bool hadVariable = context.TryGetVariable(bindLocal, out var savedVar, bindNs);

                        bool result = false;
                        foreach (var item in items)
                        {
                            // Quantified expression does NOT change the focus;
                            // it only binds the variable.
                            context.WithVariable(bindLocal, item, bindNs);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);

                            if (rhsResult.EffectiveBooleanValue())
                            {
                                result = true;
                                break;
                            }
                        }

                        if (hadVariable)
                            context.WithVariable(bindLocal, savedVar, bindNs);
                        else
                            context.RemoveVariable(bindLocal, bindNs);

                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.Every:
                    {
                        var info = (QuantifiedLoopInfo)literalPool[instr.Operand]!;
                        var sequence = registers[instr.RegisterB];
                        var items = MaterializeSequence(sequence);

                        var (bindLocal, bindNs) = ResolveLoopVariableKey(info, context);
                        bool hadVariable = context.TryGetVariable(bindLocal, out var savedVar, bindNs);

                        bool result = true;
                        foreach (var item in items)
                        {
                            // Quantified expression does NOT change the focus;
                            // it only binds the variable.
                            context.WithVariable(bindLocal, item, bindNs);
                            var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);

                            if (!rhsResult.EffectiveBooleanValue())
                            {
                                result = false;
                                break;
                            }
                        }

                        if (hadVariable)
                            context.WithVariable(bindLocal, savedVar, bindNs);
                        else
                            context.RemoveVariable(bindLocal, bindNs);

                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.OrderBy:
                    {
                        var orderInfo = (OrderByInfo)literalPool[instr.Operand]!;
                        var tupleSequence = registers[instr.RegisterB];

                        // XQST0076: an unknown collation in an order by clause is a static error.
                        // Relative collation URIs resolve against the static base URI.
                        foreach (var collation in orderInfo.CollationUri)
                        {
                            if (collation is not null && !IsSupportedOrderByCollation(ResolveCollationUri(collation, context.BaseUri)))
                                throw new InvalidOperationException($"XQST0076: Collation '{collation}' is not supported.");
                        }

                        var tuples = MaterializeSequence(tupleSequence);
                        // List<T>.Sort is introsort and NOT stable: decorate with the
                        // original position so equal keys keep their input order
                        // ('stable order by' semantics, fn-doc-33).
                        var materializedTuples = new List<(XdmValue[] Items, int Index)>();
                        int tupleIndex = 0;
                        foreach (var tuple in tuples)
                        {
                            if (tuple.IsArray && tuple.ArrayValue is not null)
                            {
                                materializedTuples.Add((tuple.ArrayValue.Values.ToArray(), tupleIndex));
                            }
                            else if (tuple.IsSequence && tuple.SequenceValue is not null)
                            {
                                materializedTuples.Add((MaterializeSequence(tuple), tupleIndex));
                            }
                            else
                            {
                                materializedTuples.Add((new[] { tuple }, tupleIndex));
                            }
                            tupleIndex++;
                        }

                        materializedTuples.Sort((x, y) =>
                        {
                            int cmp = CompareTuples(x.Items, y.Items, orderInfo, context);
                            return cmp != 0 ? cmp : x.Index.CompareTo(y.Index);
                        });

                        var sorted = new List<XdmValue>(materializedTuples.Count);
                        foreach (var (tupleItems, _) in materializedTuples)
                        {
                            sorted.Add(XdmValue.FromArray(new XdmArray(tupleItems)));
                        }

                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(sorted));
                        ip++;
                        break;
                    }

                case IrOpCode.TupleBind:
                    {
                        var bindInfo = (TupleBindInfo)literalPool[instr.Operand]!;
                        var tuple = registers[instr.RegisterA];
                        XdmValue[] items;
                        if (tuple.IsArray && tuple.ArrayValue is not null)
                        {
                            items = tuple.ArrayValue.Values.ToArray();
                        }
                        else if (tuple.IsSequence && tuple.SequenceValue is not null)
                        {
                            items = MaterializeSequence(tuple);
                        }
                        else
                        {
                            items = new[] { tuple };
                        }

                        for (int i = 0; i < bindInfo.Variables.Count; i++)
                        {
                            var (localName, prefix, nsUri) = bindInfo.Variables[i];
                            var item = i < items.Length ? items[i] : XdmValue.Undefined;
                            string bindNs = nsUri ?? "";
                            if (nsUri is null && prefix is not null)
                            {
                                if (!context.TryResolveNamespace(prefix, out var resolvedPrefixNs))
                                    throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
                                bindNs = resolvedPrefixNs;
                            }
                            context.WithVariable(localName, item, bindNs);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.GroupBy:
                    {
                        var groupInfo = (GroupByInfo)literalPool[instr.Operand]!;
                        var tupleSequence = registers[instr.RegisterB];
                        var tuples = MaterializeSequence(tupleSequence);
                        var materializedTuples = new List<XdmValue[]>(tuples.Length);
                        foreach (var tuple in tuples)
                        {
                            if (tuple.IsArray && tuple.ArrayValue is not null)
                            {
                                materializedTuples.Add(tuple.ArrayValue.Values.ToArray());
                            }
                            else if (tuple.IsSequence && tuple.SequenceValue is not null)
                            {
                                materializedTuples.Add(MaterializeSequence(tuple));
                            }
                            else
                            {
                                materializedTuples.Add(new[] { tuple });
                            }
                        }

                        // Group tuples by their grouping keys, preserving first-appearance order.
                        var groups = new List<List<XdmValue[]>>();
                        foreach (var tupleItems in materializedTuples)
                        {
                            // XPTY0004: enforce optional 'as SequenceType' declarations on grouping keys.
                            // The check applies to the atomized key value.
                            for (int k = 0; k < groupInfo.KeyIndices.Count; k++)
                            {
                                var declaredType = groupInfo.DeclaredTypeNames[k];
                                if (declaredType is null)
                                    continue;
                                int keyIndex = groupInfo.KeyIndices[k];
                                var keyValue = keyIndex < tupleItems.Length ? tupleItems[keyIndex] : XdmValue.Undefined;
                                if (!InstanceOf(Atomize(keyValue), declaredType, groupInfo.DeclaredTypeOccurrences[k], null, context))
                                {
                                    throw new InvalidOperationException(
                                        $"XPTY0004: Grouping key does not match the declared type '{declaredType}'.");
                                }
                            }

                            List<XdmValue[]>? match = null;
                            foreach (var group in groups)
                            {
                                if (GroupKeysEqual(group[0], tupleItems, groupInfo, context))
                                {
                                    match = group;
                                    break;
                                }
                            }
                            if (match is null)
                            {
                                match = new List<XdmValue[]>();
                                groups.Add(match);
                            }
                            match.Add(tupleItems);
                        }

                        // Merge each group into a single tuple: grouping variables keep their
                        // (shared) key value atomized (per the XQuery grouping rules); all other
                        // variables are bound to the concatenation of their per-tuple values.
                        var merged = new List<XdmValue>(groups.Count);
                        foreach (var group in groups)
                        {
                            int arity = group[0].Length;
                            var mergedTuple = new XdmValue[arity];
                            for (int slot = 0; slot < arity; slot++)
                            {
                                if (IsGroupKeySlot(slot, groupInfo))
                                {
                                    var keyValue = group[0][slot];
                                    mergedTuple[slot] = SingleGroupKeyItem(Atomize(keyValue));
                                    continue;
                                }

                                var combined = new List<XdmValue>();
                                foreach (var member in group)
                                {
                                    var slotValue = slot < member.Length ? member[slot] : XdmValue.Undefined;
                                    if (slotValue.IsSequence && slotValue.SequenceValue is not null)
                                    {
                                        foreach (var item in XdmSequence.FromSource(slotValue.SequenceValue))
                                            combined.Add(item);
                                    }
                                    else if (!slotValue.IsUndefined)
                                    {
                                        combined.Add(slotValue);
                                    }
                                }
                                mergedTuple[slot] = combined.Count switch
                                {
                                    0 => XdmValue.FromSequence(MaterializedSequence.FromList(combined)),
                                    1 => combined[0],
                                    _ => XdmValue.FromSequence(MaterializedSequence.FromList(combined))
                                };
                            }
                            merged.Add(XdmValue.FromArray(new XdmArray(mergedTuple)));
                        }

                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(merged));
                        ip++;
                        break;
                    }

                case IrOpCode.EnforceType:
                    {
                        // XQuery 'as SequenceType' enforcement for variable bindings: the
                        // value is atomized (unless the target is a node-kind test) and then
                        // instance-checked — no casts or promotions (XQuery 3.1 §4.16/§4.10).
                        var enforceInfo = (EnforceTypeInfo)literalPool[instr.Operand]!;
                        var value = registers[instr.RegisterA];
                        if (!IsNodeKindTest(enforceInfo.TypeName))
                            value = AtomizeItems(value);
                        if (!InstanceOf(value, enforceInfo.TypeName, enforceInfo.Occurrence, null, context))
                        {
                            throw new InvalidOperationException(
                                $"{enforceInfo.ErrorCode}: Value does not match the declared type '{enforceInfo.TypeName}'.");
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.ConstructComputed:
                    {
                        var computedInfo = (ComputedConstructorInfo)literalPool[instr.Operand]!;
                        switch (computedInfo.Kind)
                        {
                            case ComputedConstructorKind.Element:
                            {
                                var (local, prefix, ns) = ResolveComputedName(computedInfo, registers[instr.RegisterB], context, "element");
                                var accumulator = new ComputedContentAccumulator(elementNamespaceUri: ns ?? "");
                                foreach (var item in MaterializeSequence(registers[instr.RegisterC]))
                                    accumulator.Add(item, context);
                                accumulator.Flush();
                                // An element in no namespace under a non-empty default namespace
                                // undeclares it: xmlns="" is materialized so in-scope-prefixes and
                                // the namespace axis see it (K2-InScopePrefixesFunc-12/29).
                                if (ns is null && !string.IsNullOrEmpty(context.DefaultElementNamespace))
                                    accumulator.Content.Insert(0, new XdmContentItem(XdmContentKind.Namespace, "", null, ""));
                                AppendConstructorLocalNamespaces(accumulator.Content, accumulator.Attributes, context, prefix);
                                if (context.ElementConstructorHook is null)
                                    throw new InvalidOperationException("Node construction is not available: no element-constructor provider is registered (EvaluationContext.ElementConstructorHook).");
                                var spec = new XdmElementSpec(local, prefix, ns, accumulator.Attributes, accumulator.Content, context.BaseUri, context.Xml11Mode);
                                registers[instr.RegisterA] = XdmValue.FromNode(context.ElementConstructorHook(spec));
                                break;
                            }
                            case ComputedConstructorKind.Attribute:
                            {
                                var (local, prefix, ns) = ResolveComputedName(computedInfo, registers[instr.RegisterB], context, "attribute");
                                // Attribute prefix rules: a name in the XML namespace coerces to the
                                // 'xml' prefix; any other namespace without a prefix gets a generated one.
                                if (prefix is null && ns is not null)
                                    prefix = ns == "http://www.w3.org/XML/1998/namespace" ? "xml" : "ns1";
                                // XQDY0044: an attribute must not be named xmlns, use the xmlns
                                // prefix, or be in the xmlns namespace.
                                if (local == "xmlns" || prefix == "xmlns" || ns == "http://www.w3.org/2000/xmlns/")
                                    throw new InvalidOperationException("XQDY0044: An attribute must not be named 'xmlns', use the 'xmlns' prefix, or be in the xmlns namespace.");
                                var value = JoinAtomizedItems(registers[instr.RegisterC], " ");
                                // XQDY0091: xml:id attribute values must not have leading/trailing whitespace.
                                if (local == "id" && prefix == "xml" && value != value.Trim())
                                    throw new InvalidOperationException("XQDY0091: An xml:id attribute value must not have leading or trailing whitespace.");
                                if (context.AttributeConstructorHook is null)
                                    throw new InvalidOperationException("Node construction is not available: no attribute-constructor provider is registered (EvaluationContext.AttributeConstructorHook).");
                                registers[instr.RegisterA] = XdmValue.FromNode(context.AttributeConstructorHook(new XdmAttributeValue(local, prefix, ns, value)));
                                break;
                            }
                            case ComputedConstructorKind.Document:
                            {
                                var accumulator = new ComputedContentAccumulator(allowAttributes: false);
                                foreach (var item in MaterializeSequence(registers[instr.RegisterC]))
                                    accumulator.Add(item, context);
                                accumulator.Flush();
                                if (context.DocumentConstructorHook is null)
                                    throw new InvalidOperationException("Node construction is not available: no document-constructor provider is registered (EvaluationContext.DocumentConstructorHook).");
                                registers[instr.RegisterA] = XdmValue.FromNode(context.DocumentConstructorHook(accumulator.Content));
                                break;
                            }
                            case ComputedConstructorKind.Text:
                            {
                                // An empty content sequence produces no text node at all
                                // (a zero-length string still constructs a text node).
                                if (MaterializeSequence(registers[instr.RegisterC]).Length == 0)
                                {
                                    registers[instr.RegisterA] = XdmValue.Undefined;
                                    break;
                                }
                                var value = JoinAtomizedItems(registers[instr.RegisterC], " ");
                                if (context.ContentNodeConstructorHook is null)
                                    throw new InvalidOperationException("Node construction is not available: no content-node provider is registered (EvaluationContext.ContentNodeConstructorHook).");
                                registers[instr.RegisterA] = XdmValue.FromNode(context.ContentNodeConstructorHook(new XdmContentItem(XdmContentKind.Text, value)));
                                break;
                            }
                            case ComputedConstructorKind.Comment:
                            {
                                var value = JoinAtomizedItems(registers[instr.RegisterC], " ");
                                // XQDY0072: comment content must not contain '--' or end with '-'.
                                if (value.Contains("--", StringComparison.Ordinal) || value.EndsWith('-'))
                                    throw new InvalidOperationException("XQDY0072: A comment must not contain '--' or end with '-'.");
                                if (context.ContentNodeConstructorHook is null)
                                    throw new InvalidOperationException("Node construction is not available: no content-node provider is registered (EvaluationContext.ContentNodeConstructorHook).");
                                registers[instr.RegisterA] = XdmValue.FromNode(context.ContentNodeConstructorHook(new XdmContentItem(XdmContentKind.Comment, value)));
                                break;
                            }
                            case ComputedConstructorKind.ProcessingInstruction:
                            {
                                string target;
                                if (computedInfo.HasNameExpression)
                                {
                                    // XPTY0004: a computed PI target must atomize to a single
                                    // xs:string / xs:untypedAtomic / xs:NCName value.
                                    var nameAtom = Atomize(registers[instr.RegisterB]);
                                    if (nameAtom.IsUndefined || !IsValidPiTargetType(nameAtom))
                                        throw new InvalidOperationException("XPTY0004: The computed processing-instruction target must be a single xs:string, xs:untypedAtomic, or xs:NCName value.");
                                    target = nameAtom.ToString().Trim();
                                }
                                else
                                {
                                    target = computedInfo.LocalName!;
                                }
                                // XQDY0041: a target with a colon is not a valid computed PI name;
                                // XQDY0064: the target must be a valid NCName other than 'xml'.
                                if (target.Contains(':'))
                                    throw new InvalidOperationException($"XQDY0041: Invalid processing instruction target '{target}'.");
                                if (!IsValidNcName(target) || target.Equals("xml", StringComparison.OrdinalIgnoreCase))
                                    throw new InvalidOperationException($"XQDY0064: Invalid processing instruction target '{target}'.");
                                var data = JoinAtomizedItems(registers[instr.RegisterC], " ").TrimStart();
                                // XQDY0026: PI data must not contain '?>'.
                                if (data.Contains("?>", StringComparison.Ordinal))
                                    throw new InvalidOperationException("XQDY0026: Processing instruction data must not contain '?>'.");
                                if (context.ContentNodeConstructorHook is null)
                                    throw new InvalidOperationException("Node construction is not available: no content-node provider is registered (EvaluationContext.ContentNodeConstructorHook).");
                                registers[instr.RegisterA] = XdmValue.FromNode(context.ContentNodeConstructorHook(new XdmContentItem(XdmContentKind.ProcessingInstruction, data, null, target)));
                                break;
                            }
                            case ComputedConstructorKind.Namespace:
                            {
                                string nsPrefix;
                                if (computedInfo.HasNameExpression)
                                {
                                    var prefixAtom = Atomize(registers[instr.RegisterB]);
                                    if (prefixAtom.IsUndefined)
                                    {
                                        // An empty prefix expression yields a default
                                        // namespace declaration (nscons-015).
                                        nsPrefix = string.Empty;
                                    }
                                    else
                                    {
                                        // XPTY0004: the prefix expression must atomize to a single
                                        // xs:string / xs:untypedAtomic / xs:NCName value
                                        // (nscons-043/044: xs:anyURI and xs:duration are rejected).
                                        if (prefixAtom.Kind != XdmValueKind.String
                                            || prefixAtom.SchemaTypeName is not (null or "untypedAtomic" or "NCName"))
                                        {
                                            throw new InvalidOperationException("XPTY0004: The computed namespace prefix must be a single xs:string, xs:untypedAtomic, or xs:NCName value.");
                                        }
                                        nsPrefix = prefixAtom.ToString().Trim();
                                    }
                                }
                                else
                                {
                                    nsPrefix = computedInfo.LocalName!;
                                }
                                var uri = JoinAtomizedItems(registers[instr.RegisterC], " ");
                                // XQDY0101: reserved/invalid namespace-node forms.
                                if (nsPrefix == "xmlns")
                                    throw new InvalidOperationException("XQDY0101: The 'xmlns' prefix must not be used in a namespace constructor.");
                                if (nsPrefix == "xml" && uri != "http://www.w3.org/XML/1998/namespace")
                                    throw new InvalidOperationException("XQDY0101: The 'xml' prefix must only be bound to the XML namespace URI.");
                                if (nsPrefix != "xml" && uri == "http://www.w3.org/XML/1998/namespace")
                                    throw new InvalidOperationException("XQDY0101: The XML namespace URI must only be bound to the 'xml' prefix.");
                                if (uri == "http://www.w3.org/2000/xmlns/")
                                    throw new InvalidOperationException("XQDY0101: A namespace constructor must not bind a prefix to the XMLNS namespace URI.");
                                if (uri.Length == 0 && nsPrefix.Length > 0)
                                    throw new InvalidOperationException("XQDY0101: A namespace constructor with a non-empty prefix must not have an empty URI.");
                                if (nsPrefix.Length > 0 && !IsValidNcName(nsPrefix))
                                    throw new InvalidOperationException($"XQDY0101: Invalid namespace prefix '{nsPrefix}'.");
                                if (context.ContentNodeConstructorHook is null)
                                    throw new InvalidOperationException("Node construction is not available: no content-node provider is registered (EvaluationContext.ContentNodeConstructorHook).");
                                registers[instr.RegisterA] = XdmValue.FromNode(context.ContentNodeConstructorHook(new XdmContentItem(XdmContentKind.Namespace, uri, null, nsPrefix)));
                                break;
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.SaveNamespaces:
                    // Snapshot namespace bindings and the default element namespace for
                    // constructor-local namespace scoping (restored by RestoreNamespaces).
                    registers[instr.RegisterA] = XdmValue.FromExternal(
                        (context.SnapshotNamespaces(), context.DefaultElementNamespace, context.ConstructorLocalNamespaceCount));
                    ip++;
                    break;

                case IrOpCode.DeclareNamespace:
                    {
                        // Constructor-local namespace declaration (prefix operand in the pool,
                        // URI value in RegisterB). Empty prefix sets the default element namespace.
                        string prefix = (string)literalPool[instr.Operand]!;
                        var nsValue = registers[instr.RegisterB];
                        string uri = nsValue.IsUndefined ? string.Empty : Atomize(nsValue).ToString();
                        if (prefix.Length > 0)
                            context.AddConstructorLocalNamespace(prefix, uri);
                        if (prefix.Length == 0)
                        {
                            // An empty URI undeclares the default namespace.
                            context.DefaultElementNamespace = uri.Length == 0 ? null : uri;
                        }
                        else if (uri.Length == 0)
                        {
                            // An empty URI undeclares the prefix.
                            context.RemoveNamespace(prefix);
                        }
                        else
                        {
                            context.WithNamespace(prefix, uri);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.RestoreNamespaces:
                    {
                        var snapshot = ((Dictionary<string, string> Namespaces, string? DefaultNs, int LocalCount))registers[instr.RegisterB].ExternalValue!;
                        context.RestoreNamespaces(snapshot.Namespaces);
                        context.DefaultElementNamespace = snapshot.DefaultNs;
                        context.TruncateConstructorLocalNamespaces(snapshot.LocalCount);
                        ip++;
                        break;
                    }

                case IrOpCode.ConstructElement:
                    {
                        var ctorInfo = (ConstructElementInfo)literalPool[instr.Operand]!;

                        // Evaluate all attribute values first.
                        var evaluatedAttrs = new List<(string LocalName, string? Prefix, string Value)>(ctorInfo.Attributes.Length);
                        foreach (var attr in ctorInfo.Attributes)
                        {
                            var valueParts = new List<string>();
                            for (int i = attr.FirstPart; i < attr.FirstPart + attr.PartCount; i++)
                            {
                                var part = ctorInfo.Parts[i];
                                // The parser normalizes raw whitespace (tab/CR/LF → space) in
                                // literal parts at scan time; characters introduced by character
                                // references (e.g. &#x9;) must survive verbatim (XML 1.0 §3.3.3;
                                // K2-Serialization-6), so no further normalization happens here.
                                // Enclosed-expression values are never normalized.
                                switch (part.Kind)
                                {
                                    case ConstructPartKind.Literal:
                                    case ConstructPartKind.Comment:
                                    case ConstructPartKind.ProcessingInstruction:
                                        valueParts.Add((string)literalPool[part.Index]!);
                                        break;
                                    default:
                                        valueParts.Add(JoinAtomizedItems(registers[instr.RegisterB + part.Index], " "));
                                        break;
                                }
                            }
                            var value = string.Concat(valueParts);

                            // xml:id additionally uses whiteSpace="collapse" normalization
                            // (the xml:id specification; K2-DirectConElem-51).
                            if (attr.LocalName == "id" && attr.Prefix == "xml")
                                value = string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

                            // XQDY0092: xml:space must be "default" or "preserve" (XQ 3.1 §3.9.2).
                            if (attr.LocalName == "space" && attr.Prefix == "xml" && value != "default" && value != "preserve")
                                throw new InvalidOperationException("XQDY0092: The value of xml:space must be 'default' or 'preserve'.");

                            evaluatedAttrs.Add((attr.LocalName, attr.Prefix, value));
                        }

                        // Tag prefix resolves against the static context (constructor-local
                        // declarations were applied by DeclareNamespace); an unprefixed tag
                        // uses the default element namespace (attribute names never do).
                        string? tagNs = null;
                        if (ctorInfo.Prefix is not null)
                        {
                            if (!context.TryResolveNamespace(ctorInfo.Prefix, out var resolvedTagNs))
                                throw new InvalidOperationException($"XPST0081: Prefix '{ctorInfo.Prefix}' is not declared.");
                            tagNs = resolvedTagNs;
                        }
                        else if (!string.IsNullOrEmpty(context.DefaultElementNamespace))
                        {
                            tagNs = context.DefaultElementNamespace;
                        }

                        var attributes = new List<XdmAttributeValue>(evaluatedAttrs.Count);
                        var seenAttrs = new HashSet<(string, string)>();
                        foreach (var (attrLocalName, attrPrefix, attrValue) in evaluatedAttrs)
                        {
                            string? attrNs = null;
                            if (attrPrefix is not null && attrPrefix != "xmlns")
                            {
                                if (!context.TryResolveNamespace(attrPrefix, out var resolvedAttrNs))
                                    throw new InvalidOperationException($"XPST0081: Prefix '{attrPrefix}' is not declared.");
                                attrNs = resolvedAttrNs;
                            }

                            // XQDY0025: duplicate expanded attribute names are an error
                            // (xmlns declarations are not ordinary attributes).
                            if (attrLocalName != "xmlns" && attrPrefix != "xmlns" &&
                                !seenAttrs.Add((attrLocalName, attrNs ?? "")))
                            {
                                throw new InvalidOperationException($"XQDY0025: Duplicate attribute '{attrLocalName}'.");
                            }
                            attributes.Add(new XdmAttributeValue(attrLocalName, attrPrefix, attrNs, attrValue));
                        }

                        var content = new List<XdmContentItem>();
                        string? pendingAtomic = null;
                        bool lastWasAtomic = false;
                        bool seenNonAttributeContent = false;
                        void FlushAtomic()
                        {
                            if (pendingAtomic is not null)
                            {
                                content.Add(new XdmContentItem(XdmContentKind.Text, pendingAtomic));
                                pendingAtomic = null;
                                lastWasAtomic = false;
                                seenNonAttributeContent = true;
                            }
                        }

                        // One array member (or sequence item) as content: flattened recursively
                        // (nested arrays and sequence members included, ArrayTest-047/051).
                        void AddArrayMember(XdmValue member)
                        {
                            if (member.IsSequence && member.SequenceValue is not null)
                            {
                                foreach (var inner in XdmSequence.FromSource(member.SequenceValue))
                                    AddArrayMember(inner);
                                return;
                            }
                            if (member.IsArray && member.ArrayValue is not null)
                            {
                                foreach (var nested in member.ArrayValue.Values)
                                    AddArrayMember(nested);
                                return;
                            }
                            if (member.IsNode && member.NodeValue.NodeKind == XdmNodeKind.Attribute)
                            {
                                if (seenNonAttributeContent)
                                    throw new InvalidOperationException("XQTY0024: An attribute node in element content must not follow other content.");
                                var attrNode = member.NodeValue;
                                string? itemNs = string.IsNullOrEmpty(attrNode.NamespaceUri) ? null : attrNode.NamespaceUri;
                                if (attrNode.LocalName != "xmlns" && attrNode.Prefix != "xmlns" &&
                                    !seenAttrs.Add((attrNode.LocalName, itemNs ?? "")))
                                {
                                    throw new InvalidOperationException($"XQDY0025: Duplicate attribute '{attrNode.LocalName}'.");
                                }
                                attributes.Add(new XdmAttributeValue(attrNode.LocalName, attrNode.Prefix, itemNs, attrNode.StringValue));
                                return;
                            }
                            if (member.IsNode)
                            {
                                FlushAtomic();
                                content.Add(new XdmContentItem(XdmContentKind.Node, null, member));
                                seenNonAttributeContent = true;
                                return;
                            }
                            var memberText = member.ToString();
                            pendingAtomic = pendingAtomic is null ? memberText : pendingAtomic + (lastWasAtomic ? " " : "") + memberText;
                            lastWasAtomic = true;
                            seenNonAttributeContent = true;
                        }

                        for (int i = ctorInfo.FirstContentPart; i < ctorInfo.FirstContentPart + ctorInfo.ContentPartCount; i++)
                        {
                            var part = ctorInfo.Parts[i];
                            switch (part.Kind)
                            {
                                case ConstructPartKind.Literal:
                                    FlushAtomic();
                                    content.Add(new XdmContentItem(XdmContentKind.Text, (string)literalPool[part.Index]!));
                                    seenNonAttributeContent = true;
                                    break;
                                case ConstructPartKind.Comment:
                                    FlushAtomic();
                                    content.Add(new XdmContentItem(XdmContentKind.Comment, (string)literalPool[part.Index]!));
                                    seenNonAttributeContent = true;
                                    break;
                                case ConstructPartKind.ProcessingInstruction:
                                    FlushAtomic();
                                    content.Add(new XdmContentItem(XdmContentKind.ProcessingInstruction,
                                        (string)literalPool[part.Index]!, null, (string)literalPool[part.Index2]!));
                                    seenNonAttributeContent = true;
                                    break;
                                default:
                                    foreach (var item in MaterializeSequence(registers[instr.RegisterB + part.Index]))
                                    {
                                        // XQuery content rules: attribute nodes in content become
                                        // attributes of the element, but only before any other
                                        // content (XQTY0024 after non-attribute content).
                                        if (item.IsNode && item.NodeValue.NodeKind == XdmNodeKind.Attribute)
                                        {
                                            if (seenNonAttributeContent)
                                                throw new InvalidOperationException("XQTY0024: An attribute node in element content must not follow other content.");
                                            var attrNode = item.NodeValue;
                                            // The attribute keeps the namespace it had in the source.
                                            string? itemNs = string.IsNullOrEmpty(attrNode.NamespaceUri) ? null : attrNode.NamespaceUri;
                                            if (attrNode.LocalName != "xmlns" && attrNode.Prefix != "xmlns" &&
                                                !seenAttrs.Add((attrNode.LocalName, itemNs ?? "")))
                                            {
                                                throw new InvalidOperationException($"XQDY0025: Duplicate attribute '{attrNode.LocalName}'.");
                                            }
                                            attributes.Add(new XdmAttributeValue(attrNode.LocalName, attrNode.Prefix, itemNs, attrNode.StringValue));
                                            continue;
                                        }
                                        // A namespace node in content becomes a namespace declaration
                                        // (XQDY0102 on conflicting redeclaration of the same prefix).
                                        // The node's name is the bound prefix (empty for a default
                                        // declaration); its string value is the namespace URI.
                                        if (item.IsNode && item.NodeValue.NodeKind == XdmNodeKind.Namespace)
                                        {
                                            var nsNode = item.NodeValue;
                                            string declPrefix = nsNode.LocalName;
                                            string declUri = nsNode.StringValue;
                                            if (declPrefix.Length == 0)
                                            {
                                                // Spec bug 22032: a default namespace declaration
                                                // conflicts with an element name in no namespace, and an
                                                // empty-URI undeclaration with an element name in one.
                                                if (string.IsNullOrEmpty(tagNs) && declUri.Length > 0)
                                                    throw new InvalidOperationException("XQDY0102: A default namespace declaration must not be added to an element in no namespace.");
                                                if (!string.IsNullOrEmpty(tagNs) && declUri.Length == 0)
                                                    throw new InvalidOperationException("XQDY0102: The default namespace must not be undeclared on an element in a namespace.");
                                                if (!string.IsNullOrEmpty(context.DefaultElementNamespace) && context.DefaultElementNamespace != declUri)
                                                    throw new InvalidOperationException("XQDY0102: The default namespace is redeclared with a different URI.");
                                                context.DefaultElementNamespace = declUri.Length == 0 ? null : declUri;
                                            }
                                            else
                                            {
                                                if (context.TryResolveNamespace(declPrefix, out var existing) && existing != declUri)
                                                    throw new InvalidOperationException($"XQDY0102: The namespace prefix '{declPrefix}' is redeclared with a different URI.");
                                                context.WithNamespace(declPrefix, declUri);
                                            }
                                            content.Add(new XdmContentItem(XdmContentKind.Namespace, declUri, null, declPrefix));
                                            // Namespace declarations are not "other content": they may
                                            // interleave freely with attributes at the start of the
                                            // content (nscons-001 — no XQTY0024 between them).
                                            continue;
                                        }
                                        if (item.IsNode && item.NodeValue.NodeKind is XdmNodeKind.Text)
                                        {
                                            // Text nodes merge with adjacent atomic text rather than
                                            // being copied; no separator at a text-node boundary.
                                            var textNodeValue = item.NodeValue.StringValue;
                                            pendingAtomic = pendingAtomic is null ? textNodeValue : pendingAtomic + textNodeValue;
                                            lastWasAtomic = false;
                                            seenNonAttributeContent = true;
                                            continue;
                                        }
                                        if (item.IsNode)
                                        {
                                            FlushAtomic();
                                            content.Add(new XdmContentItem(XdmContentKind.Node, null, item));
                                            seenNonAttributeContent = true;
                                        }
                                        else if (item.IsFunction)
                                        {
                                            // XQTY0105: element content must not contain function items
                                            // (function-item-5: element a { avg#1 }).
                                            throw new InvalidOperationException("XQTY0105: Element content must not contain a function item.");
                                        }
                                        else if (item.IsArray && item.ArrayValue is not null)
                                        {
                                            // Arrays in content are flattened: their members are
                                            // processed recursively as content (ArrayTest-047/051).
                                            foreach (var member in item.ArrayValue.Values)
                                                AddArrayMember(member);
                                        }
                                        else
                                        {
                                            // Adjacent atomic values WITHIN one enclosed expression join
                                            // with single spaces; separate expressions concatenate
                                            // without a separator. A text node adjacent to an atomic
                                            // value takes no separator either.
                                            var text = item.ToString();
                                            pendingAtomic = pendingAtomic is null ? text : pendingAtomic + (lastWasAtomic ? " " : "") + text;
                                            lastWasAtomic = true;
                                            seenNonAttributeContent = true;
                                        }
                                    }
                                    // End of one enclosed expression: flush so the next part starts fresh.
                                    FlushAtomic();
                                    break;
                            }
                        }
                        FlushAtomic();
                        AppendConstructorLocalNamespaces(content, attributes, context, ctorInfo.Prefix);

                        if (context.ElementConstructorHook is null)
                        {
                            throw new InvalidOperationException(
                                "Node construction is not available: no element-constructor provider is registered (EvaluationContext.ElementConstructorHook).");
                        }

                        var spec = new XdmElementSpec(ctorInfo.LocalName, ctorInfo.Prefix, tagNs, attributes, content, context.BaseUri, context.Xml11Mode);
                        registers[instr.RegisterA] = XdmValue.FromNode(context.ElementConstructorHook(spec));
                        ip++;
                        break;
                    }

                case IrOpCode.ConstructContentNode:
                    {
                        var item = (XdmContentItem)literalPool[instr.Operand]!;
                        if (context.ContentNodeConstructorHook is null)
                        {
                            throw new InvalidOperationException(
                                "Node construction is not available: no content-node provider is registered (EvaluationContext.ContentNodeConstructorHook).");
                        }
                        registers[instr.RegisterA] = XdmValue.FromNode(context.ContentNodeConstructorHook(item));
                        ip++;
                        break;
                    }

                case IrOpCode.Window:
                    {
                        var windowInfo = (WindowInfo)literalPool[instr.Operand]!;
                        var input = registers[instr.RegisterB];
                        var items = MaterializeSequence(input);
                        var results = new List<XdmValue>();

                        // Resolve every variable name this clause binds (including lexical
                        // prefix:local and Q{uri}local forms) to its (local, namespace) pair,
                        // so the bindings match the way references to the variables resolve.
                        var boundVars = new List<(string Local, string Ns)>
                        {
                            ResolveWindowVariableName(windowInfo.VariableName, context)
                        };
                        foreach (var name in new[]
                        {
                            windowInfo.StartCurrent, windowInfo.StartPos, windowInfo.StartPrev, windowInfo.StartNext,
                            windowInfo.EndCurrent, windowInfo.EndPos, windowInfo.EndPrev, windowInfo.EndNext
                        })
                        {
                            if (name is null)
                                continue;
                            var resolved = ResolveWindowVariableName(name, context);
                            if (!boundVars.Contains(resolved))
                                boundVars.Add(resolved);
                        }

                        // Save the previous bindings of every variable this clause binds.
                        var savedBindings = new List<((string Local, string Ns) Var, bool Had, XdmValue Value)>(boundVars.Count);
                        foreach (var boundVar in boundVars)
                        {
                            bool had = context.TryGetVariable(boundVar.Local, out var saved, boundVar.Ns);
                            savedBindings.Add((boundVar, had, saved));
                        }

                        bool hasEndCondition = windowInfo.EndEntryPoint >= 0;

                        if (windowInfo.Sliding)
                        {
                            // Sliding: every item matching the start condition opens a new
                            // (possibly overlapping) window. Without an end condition each
                            // window extends to the end of the input sequence.
                            for (int i = 0; i < items.Length; i++)
                            {
                                if (!EvaluateWindowCondition(module, context, registers, windowInfo.StartEntryPoint,
                                        items, i, i + 1,
                                        windowInfo.StartCurrent, windowInfo.StartPos, windowInfo.StartPrev, windowInfo.StartNext))
                                    continue;

                                var startBindings = CaptureWindowBindings(items, i, i + 1,
                                    windowInfo.StartCurrent, windowInfo.StartPos, windowInfo.StartPrev, windowInfo.StartNext);

                                var windowItems = new List<XdmValue>();
                                bool closed = false;
                                if (hasEndCondition)
                                {
                                    for (int j = i; j < items.Length; j++)
                                    {
                                        windowItems.Add(items[j]);
                                        if (EvaluateWindowCondition(module, context, registers, windowInfo.EndEntryPoint,
                                                items, j, j + 1,
                                                windowInfo.EndCurrent, windowInfo.EndPos, windowInfo.EndPrev, windowInfo.EndNext))
                                        {
                                            EmitFlworWindow(module, context, registers, windowInfo, results, windowItems,
                                                startBindings, items, j, j + 1);
                                            closed = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    windowItems.AddRange(items.Skip(i));
                                }
                                if (!closed && !windowInfo.OnlyEnd)
                                {
                                    EmitFlworWindow(module, context, registers, windowInfo, results, windowItems,
                                        startBindings, items, items.Length - 1, items.Length);
                                }
                            }
                        }
                        else
                        {
                            // Tumbling: a new window starts only when no window is open.
                            // Without an end condition a window closes when the next one
                            // starts (exclusive) or at the end of the input sequence.
                            var windowItems = new List<XdmValue>();
                            List<(string Name, XdmValue Value)>? startBindings = null;
                            bool open = false;
                            for (int i = 0; i < items.Length; i++)
                            {
                                if (!open)
                                {
                                    if (!EvaluateWindowCondition(module, context, registers, windowInfo.StartEntryPoint,
                                            items, i, i + 1,
                                            windowInfo.StartCurrent, windowInfo.StartPos, windowInfo.StartPrev, windowInfo.StartNext))
                                        continue;
                                    open = true;
                                    startBindings = CaptureWindowBindings(items, i, i + 1,
                                        windowInfo.StartCurrent, windowInfo.StartPos, windowInfo.StartPrev, windowInfo.StartNext);
                                    windowItems.Add(items[i]);
                                }
                                else if (!hasEndCondition &&
                                         EvaluateWindowCondition(module, context, registers, windowInfo.StartEntryPoint,
                                             items, i, i + 1,
                                             windowInfo.StartCurrent, windowInfo.StartPos, windowInfo.StartPrev, windowInfo.StartNext))
                                {
                                    // No end condition: the current window closes before a
                                    // new start; the new window opens at this item.
                                    EmitFlworWindow(module, context, registers, windowInfo, results, windowItems,
                                        startBindings!, items, i - 1, i);
                                    startBindings = CaptureWindowBindings(items, i, i + 1,
                                        windowInfo.StartCurrent, windowInfo.StartPos, windowInfo.StartPrev, windowInfo.StartNext);
                                    windowItems = new List<XdmValue> { items[i] };
                                }
                                else
                                {
                                    windowItems.Add(items[i]);
                                }

                                if (open && hasEndCondition &&
                                    EvaluateWindowCondition(module, context, registers, windowInfo.EndEntryPoint,
                                        items, i, i + 1,
                                        windowInfo.EndCurrent, windowInfo.EndPos, windowInfo.EndPrev, windowInfo.EndNext))
                                {
                                    EmitFlworWindow(module, context, registers, windowInfo, results, windowItems,
                                        startBindings!, items, i, i + 1);
                                    open = false;
                                    windowItems = new List<XdmValue>();
                                }
                            }
                            if (open && !windowInfo.OnlyEnd)
                            {
                                EmitFlworWindow(module, context, registers, windowInfo, results, windowItems,
                                    startBindings!, items, items.Length - 1, items.Length);
                            }
                        }

                        // Restore the previous variable bindings.
                        foreach (var (boundVar, had, value) in savedBindings)
                        {
                            if (had)
                                context.WithVariable(boundVar.Local, value, boundVar.Ns);
                            else
                                context.RemoveVariable(boundVar.Local, boundVar.Ns);
                        }

                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(results));
                        ip++;
                        break;
                    }

                case IrOpCode.TryCatch:
                    {
                        var info = (TryCatchInfo)literalPool[instr.Operand]!;
                        try
                        {
                            var (result, _) = ExecuteBlock(module, context, registers, info.TryEntryPoint);
                            registers[instr.RegisterA] = result;
                        }
                        // Errors raised by lazy global variable initializers are not caught
                        // by try/catch (XQuery try-006/007).
                        catch (Exception ex) when (ex is not GlobalVariableEvaluationException)
                        {
                            var details = XPathError.GetErrorDetails(ex);
                            // try/catch catches dynamic errors only: static (XPST/XQST)
                            // errors propagate even when a pattern would match them.
                            if (XPathError.IsUncatchableStaticError(ex, details))
                                throw;
                            CatchClauseInfo? matched = null;
                            foreach (var clause in info.Clauses)
                            {
                                bool clauseMatches = false;
                                foreach (var pattern in clause.Patterns)
                                {
                                    if (XPathError.CatchPatternMatches(pattern, details, context))
                                    {
                                        clauseMatches = true;
                                        break;
                                    }
                                }
                                if (clauseMatches)
                                {
                                    matched = clause;
                                    break;
                                }
                            }
                            // No catch clause matches the error: it propagates unchanged.
                            if (matched is null)
                                throw;
                            var previousErrorVars = XPathError.BindCatchErrorVariables(context, details);
                            try
                            {
                                var (catchResult, _) = ExecuteBlock(module, context, registers, matched.EntryPoint);
                                registers[instr.RegisterA] = catchResult;
                            }
                            finally
                            {
                                XPathError.RestoreCatchErrorVariables(context, previousErrorVars);
                            }
                        }
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Axes
                // ------------------------------------------------------------------
                case IrOpCode.Child:
                case IrOpCode.Descendant:
                case IrOpCode.DescendantOrSelf:
                case IrOpCode.Ancestor:
                case IrOpCode.AncestorOrSelf:
                case IrOpCode.Attribute:
                case IrOpCode.Parent:
                case IrOpCode.Self:
                case IrOpCode.Following:
                case IrOpCode.FollowingSibling:
                case IrOpCode.Preceding:
                case IrOpCode.PrecedingSibling:
                case IrOpCode.Namespace:
                    {
                        var input = registers[instr.RegisterB];
                        var axis = ToXdmAxis(instr.OpCode);
                        registers[instr.RegisterA] = ApplyAxis(input, axis);
                        ip++;
                        break;
                    }

                case IrOpCode.DocumentRoot:
                    {
                        var input = registers[instr.RegisterB];
                        if (input.IsNode && input.NodeValue != null)
                        {
                            var node = input.NodeValue;
                            var root = node.Document;
                            if (root == null)
                            {
                                // Parentless node: the root of its tree is the node itself.
                                root = node;
                            }
                            if (root.NodeKind != XdmNodeKind.Document)
                            {
                                throw new InvalidOperationException("XPDY0050: The root of the tree containing the context item is not a document node.");
                            }
                            registers[instr.RegisterA] = XdmValue.FromNode(root);
                        }
                        else
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Node tests
                // ------------------------------------------------------------------
                case IrOpCode.NameTest:
                    {
                        string name = (string)literalPool[instr.Operand]!;
                        var input = registers[instr.RegisterB];
                        var filtered = FilterNodes(input, n =>
                        {
                            // Wildcard: match any name (kind test already restricted node kind).
                            if (name == "*")
                                return true;

                            // Namespace wildcard prefix:* — match any local name in the namespace.
                            if (name.EndsWith(":*", StringComparison.Ordinal))
                            {
                                var wildcardPrefix = name[..^2];
                                if (context.TryResolveNamespace(wildcardPrefix, out var wildcardNsUri))
                                    return n.NamespaceUri == wildcardNsUri;
                                return false;
                            }

                            if (n.LocalName != name && !(name.Contains(':') && n.LocalName == name.Split(':')[1]))
                                return false;
                            // Unprefixed attribute names always match no namespace
                            if (n.NodeKind == XdmNodeKind.Attribute && !name.Contains(':'))
                                return n.NamespaceUri == "";
                            return true;
                        });
                        registers[instr.RegisterA] = filtered;
                        ip++;
                        break;
                    }

                case IrOpCode.KindTest:
                    {
                        string kindName = (string)literalPool[instr.Operand]!;
                        var input = registers[instr.RegisterB];
                        var filtered = FilterNodes(input, n => MatchesKindTest(n, kindName));
                        registers[instr.RegisterA] = filtered;
                        ip++;
                        break;
                    }

                case IrOpCode.KindTestType:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var input = registers[instr.RegisterB];
                        ValidateKindTestTypeName(typeName, context);
                        var filtered = FilterNodes(input, n => n.NodeKind == XdmNodeKind.Attribute
                            ? IsAttributeTypeCompatible(typeName, context, n)
                            : IsElementTypeCompatible(typeName, context, n));
                        registers[instr.RegisterA] = filtered;
                        ip++;
                        break;
                    }

                case IrOpCode.NamespaceTest:
                    {
                        string prefix = (string)literalPool[instr.Operand]!;
                        var input = registers[instr.RegisterB];
                        XdmValue filtered;
                        if (prefix == "Q{}")
                        {
                            // Sentinel from a Q{}* wildcard: match the empty namespace
                            // unconditionally (never the default element namespace).
                            filtered = FilterNodes(input, n => n.NamespaceUri == "");
                        }
                        else if (context.TryResolveNamespace(prefix, out var nsUri))
                        {
                            filtered = FilterNodes(input, n => n.NamespaceUri == nsUri);
                        }
                        else if (prefix.Contains('/') || prefix.Contains(':'))
                        {
                            // Operand is a URI (e.g. from Q{uri}local syntax) — use directly
                            filtered = FilterNodes(input, n => n.NamespaceUri == prefix);
                        }
                        else if (prefix.Length == 0)
                        {
                            // Empty prefix stands for the default element namespace:
                            // none is declared, so match the empty namespace.
                            filtered = FilterNodes(input, n => n.NamespaceUri == "");
                        }
                        else
                        {
                            // XPST0081: a name test with an unresolvable prefix is a static error.
                            throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
                        }
                        registers[instr.RegisterA] = filtered;
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Predicates / Filtering
                // ------------------------------------------------------------------
                case IrOpCode.Filter:
                    {
                        var sequence = registers[instr.RegisterB];
                        int predicateEntry = instr.Operand;

                        var items = MaterializeSequence(sequence);
                        var kept = new List<XdmValue>();

                        // Save context
                        var savedItem = context.ContextItem;
                        var savedPos = context.ContextPosition;
                        var savedSize = context.ContextSize;

                        for (int i = 0; i < items.Length; i++)
                        {
                            context.WithFocus(items[i], i + 1, items.Length);
                            var (predResult, _) = ExecuteBlock(module, context, registers, predicateEntry);

                            // Predicate semantics (XPath §2.4): a singleton whose single item
                            // is numeric is a positional predicate ([n] means position() = n);
                            // everything else uses effective boolean value — a node is always
                            // true, so the result must NOT be atomized before the EBV check.
                            bool numericPredicate = false;
                            double numericValue = 0;
                            if (!predResult.IsUndefined)
                            {
                                if (IsNumeric(predResult))
                                {
                                    numericPredicate = true;
                                    numericValue = ToDouble(predResult);
                                }
                                else if (predResult.IsSequence && predResult.SequenceValue is not null)
                                {
                                    var predItems = MaterializeSequence(predResult);
                                    if (predItems.Length == 1 && !predItems[0].IsNode)
                                    {
                                        var atomizedItem = Atomize(predItems[0]);
                                        if (IsNumeric(atomizedItem))
                                        {
                                            numericPredicate = true;
                                            numericValue = ToDouble(atomizedItem);
                                        }
                                    }
                                }
                            }

                            if (numericPredicate)
                            {
                                if (numericValue == i + 1)
                                    kept.Add(items[i]);
                            }
                            else if (predResult.EffectiveBooleanValue())
                            {
                                kept.Add(items[i]);
                            }
                        }

                        // Restore context
                        context.WithFocus(savedItem, savedPos, savedSize);

                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(kept));
                        ip++;
                        break;
                    }

                case IrOpCode.Subscript:
                    {
                        var sequence = registers[instr.RegisterB];
                        int index = instr.Operand; // 1-based

                        if (sequence.IsUndefined || index < 1)
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                        }
                        else if (!sequence.IsSequence)
                        {
                            registers[instr.RegisterA] = index == 1 ? sequence : XdmValue.FromSequence(XdmSequence.Empty);
                        }
                        else
                        {
                            var seq = sequence.SequenceValue;
                            if (seq is null)
                            {
                                registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                            }
                            else
                            {
                                var en = XdmSequence.FromSource(seq).GetEnumerator();
                                XdmValue? result = null;
                                for (int i = 0; i < index; i++)
                                {
                                    if (!en.MoveNext())
                                    {
                                        result = null;
                                        break;
                                    }
                                    result = en.Current;
                                }
                                registers[instr.RegisterA] = result ?? XdmValue.FromSequence(XdmSequence.Empty);
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.First:
                    {
                        var sequence = registers[instr.RegisterB];
                        if (sequence.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                        }
                        else if (!sequence.IsSequence)
                        {
                            registers[instr.RegisterA] = sequence;
                        }
                        else
                        {
                            var seq = sequence.SequenceValue;
                            if (seq is null)
                                registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                            else
                            {
                                var en = XdmSequence.FromSource(seq).GetEnumerator();
                                registers[instr.RegisterA] = en.MoveNext() ? en.Current : XdmValue.FromSequence(XdmSequence.Empty);
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Last:
                    {
                        var sequence = registers[instr.RegisterB];
                        if (sequence.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                        }
                        else if (!sequence.IsSequence)
                        {
                            registers[instr.RegisterA] = sequence;
                        }
                        else
                        {
                            var seq = sequence.SequenceValue;
                            if (seq is null)
                                registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                            else
                            {
                                var en = XdmSequence.FromSource(seq).GetEnumerator();
                                XdmValue? last = null;
                                while (en.MoveNext())
                                    last = en.Current;
                                registers[instr.RegisterA] = last ?? XdmValue.FromSequence(XdmSequence.Empty);
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Position:
                    registers[instr.RegisterA] = XdmValue.FromInteger(context.ContextPosition);
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // Comparisons (value comparisons for atomics)
                // ------------------------------------------------------------------
                case IrOpCode.Equal:
                case IrOpCode.NotEqual:
                case IrOpCode.LessThan:
                case IrOpCode.LessThanOrEqual:
                case IrOpCode.GreaterThan:
                case IrOpCode.GreaterThanOrEqual:
                case IrOpCode.ValueEqual:
                case IrOpCode.ValueNotEqual:
                case IrOpCode.ValueLessThan:
                case IrOpCode.ValueLessThanOrEqual:
                case IrOpCode.ValueGreaterThan:
                case IrOpCode.ValueGreaterThanOrEqual:
                    {
                        var cmpResult = Compare(instr.OpCode, registers[instr.RegisterB], registers[instr.RegisterC], context);
                        registers[instr.RegisterA] = cmpResult;
                        ip++;
                        break;
                    }

                case IrOpCode.GeneralEqual:
                case IrOpCode.GeneralNotEqual:
                case IrOpCode.GeneralLessThan:
                case IrOpCode.GeneralLessThanOrEqual:
                case IrOpCode.GeneralGreaterThan:
                case IrOpCode.GeneralGreaterThanOrEqual:
                    // General comparisons have existential semantics over sequences.
                    {
                        var cmpResult = CompareGeneral(instr.OpCode, registers[instr.RegisterB], registers[instr.RegisterC], context);
                        registers[instr.RegisterA] = cmpResult;
                        ip++;
                        break;
                    }

                case IrOpCode.IsSameNode:
                    {
                        var left = UnwrapSingleton(registers[instr.RegisterB]);
                        var right = UnwrapSingleton(registers[instr.RegisterC]);
                        // Empty sequence operand -> empty sequence result
                        if (left.IsUndefined || right.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (!left.IsNode || !right.IsNode)
                        {
                            throw new InvalidOperationException("XPTY0004: Node comparison operator 'is' requires single node operands.");
                        }
                        else
                        {
                            bool result = left.NodeValue.IsSameNode(right.NodeValue);
                            registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.PrecedesNode:
                case IrOpCode.FollowsNode:
                    {
                        var left = UnwrapSingleton(registers[instr.RegisterB]);
                        var right = UnwrapSingleton(registers[instr.RegisterC]);
                        if (left.IsUndefined || right.IsUndefined)
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else if (!left.IsNode || !right.IsNode)
                        {
                            throw new InvalidOperationException("XPTY0004: Node comparison operators '<<' and '>>' require single node operands.");
                        }
                        else
                        {
                            bool result = instr.OpCode == IrOpCode.PrecedesNode
                                ? left.NodeValue.DocumentOrder < right.NodeValue.DocumentOrder
                                : left.NodeValue.DocumentOrder > right.NodeValue.DocumentOrder;
                            registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        }
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Arithmetic
                // ------------------------------------------------------------------
                case IrOpCode.Add:
                    registers[instr.RegisterA] = Add(registers[instr.RegisterB], registers[instr.RegisterC], context);
                    ip++;
                    break;

                case IrOpCode.Subtract:
                    registers[instr.RegisterA] = Subtract(registers[instr.RegisterB], registers[instr.RegisterC], context);
                    ip++;
                    break;

                case IrOpCode.Multiply:
                    registers[instr.RegisterA] = Multiply(registers[instr.RegisterB], registers[instr.RegisterC], context);
                    ip++;
                    break;

                case IrOpCode.Divide:
                    registers[instr.RegisterA] = Divide(registers[instr.RegisterB], registers[instr.RegisterC], context);
                    ip++;
                    break;

                case IrOpCode.IntegerDivide:
                    registers[instr.RegisterA] = IntegerDivide(registers[instr.RegisterB], registers[instr.RegisterC], context);
                    ip++;
                    break;

                case IrOpCode.Modulo:
                    registers[instr.RegisterA] = Modulo(registers[instr.RegisterB], registers[instr.RegisterC], context);
                    ip++;
                    break;

                case IrOpCode.UnaryPlus:
                    registers[instr.RegisterA] = UnaryPlus(registers[instr.RegisterB], context);
                    ip++;
                    break;

                case IrOpCode.UnaryMinus:
                    registers[instr.RegisterA] = Negate(registers[instr.RegisterB], context);
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // Boolean logic
                // ------------------------------------------------------------------
                case IrOpCode.And:
                    {
                        bool result = registers[instr.RegisterB].EffectiveBooleanValue() &&
                                      registers[instr.RegisterC].EffectiveBooleanValue();
                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.Or:
                    {
                        bool result = registers[instr.RegisterB].EffectiveBooleanValue() ||
                                      registers[instr.RegisterC].EffectiveBooleanValue();
                        registers[instr.RegisterA] = XdmValue.FromBoolean(result);
                        ip++;
                        break;
                    }

                case IrOpCode.Not:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(!registers[instr.RegisterB].EffectiveBooleanValue());
                    ip++;
                    break;

                // ------------------------------------------------------------------
                // String
                // ------------------------------------------------------------------
                case IrOpCode.StringConcat:
                    registers[instr.RegisterA] = XdmValue.FromString(
                        AtomizedStringValue(registers[instr.RegisterB]) + AtomizedStringValue(registers[instr.RegisterC]));
                    ip++;
                    break;

                case IrOpCode.StringLength:
                    {
                        string s = AtomizedString(registers[instr.RegisterB]);
                        registers[instr.RegisterA] = XdmValue.FromInteger(s.Length);
                        ip++;
                        break;
                    }

                case IrOpCode.Substring:
                    {
                        string s = AtomizedString(registers[instr.RegisterB]);
                        double startD = ToDouble(Atomize(registers[instr.RegisterC]));
                        if (double.IsNaN(startD))
                        {
                            registers[instr.RegisterA] = XdmValue.FromString(string.Empty);
                        }
                        else
                        {
                            int start = (int)Math.Round(startD);
                            if (start <= 0) start = 1;
                            if (start > s.Length)
                                registers[instr.RegisterA] = XdmValue.FromString(string.Empty);
                            else if (instr.Operand != 0)
                            {
                                double lenD = ToDouble(Atomize(registers[instr.Operand]));
                                if (double.IsNaN(lenD) || lenD <= 0)
                                    registers[instr.RegisterA] = XdmValue.FromString(string.Empty);
                                else
                                {
                                    int len = (int)Math.Round(lenD);
                                    int end = Math.Min(start - 1 + len, s.Length);
                                    registers[instr.RegisterA] = XdmValue.FromString(s[(start - 1)..end]);
                                }
                            }
                            else
                            {
                                registers[instr.RegisterA] = XdmValue.FromString(s[(start - 1)..]);
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Contains:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(
                        AtomizedString(registers[instr.RegisterB]).Contains(AtomizedString(registers[instr.RegisterC])));
                    ip++;
                    break;

                case IrOpCode.StartsWith:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(
                        AtomizedString(registers[instr.RegisterB]).StartsWith(AtomizedString(registers[instr.RegisterC])));
                    ip++;
                    break;

                case IrOpCode.EndsWith:
                    registers[instr.RegisterA] = XdmValue.FromBoolean(
                        AtomizedString(registers[instr.RegisterB]).EndsWith(AtomizedString(registers[instr.RegisterC])));
                    ip++;
                    break;

                case IrOpCode.NormalizeSpace:
                    {
                        string s = AtomizedString(registers[instr.RegisterB]);
                        registers[instr.RegisterA] = XdmValue.FromString(NormalizeSpaceString(s));
                        ip++;
                        break;
                    }

                case IrOpCode.Translate:
                    {
                        string arg = AtomizedString(registers[instr.RegisterB]);
                        string map = AtomizedString(registers[instr.RegisterC]);
                        string trans = AtomizedString(registers[instr.RegisterB + 1]);
                        var sb = new System.Text.StringBuilder(arg.Length);
                        foreach (char c in arg)
                        {
                            int idx = map.IndexOf(c);
                            if (idx >= 0)
                            {
                                if (idx < trans.Length)
                                    sb.Append(trans[idx]);
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        registers[instr.RegisterA] = XdmValue.FromString(sb.ToString());
                        ip++;
                        break;
                    }

                case IrOpCode.UpperCase:
                    registers[instr.RegisterA] = XdmValue.FromString(AtomizedString(registers[instr.RegisterB]).ToUpperInvariant());
                    ip++;
                    break;

                case IrOpCode.LowerCase:
                    registers[instr.RegisterA] = XdmValue.FromString(AtomizedString(registers[instr.RegisterB]).ToLowerInvariant());
                    ip++;
                    break;

                case IrOpCode.MatchesRegex:
                    {
                        string input = AtomizedString(registers[instr.RegisterB]);
                        string pattern = AtomizedString(registers[instr.RegisterC]);
                        var options = instr.Operand != 0
                            ? ParseRegexFlags(AtomizedString(registers[instr.Operand]))
                            : System.Text.RegularExpressions.RegexOptions.None;
                        registers[instr.RegisterA] = XdmValue.FromBoolean(
                            System.Text.RegularExpressions.Regex.IsMatch(input, pattern, options));
                        ip++;
                        break;
                    }

                case IrOpCode.ReplaceRegex:
                    {
                        string input = AtomizedString(registers[instr.RegisterB]);
                        string pattern = AtomizedString(registers[instr.RegisterC]);
                        string replacement = AtomizedString(registers[instr.RegisterB + 1]);
                        var options = instr.Operand != 0
                            ? ParseRegexFlags(AtomizedString(registers[instr.Operand]))
                            : System.Text.RegularExpressions.RegexOptions.None;
                        registers[instr.RegisterA] = XdmValue.FromString(
                            System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement, options));
                        ip++;
                        break;
                    }

                case IrOpCode.TokenizeRegex:
                    {
                        string input = AtomizedString(registers[instr.RegisterB]);
                        string pattern = AtomizedString(registers[instr.RegisterC]);
                        var options = instr.Operand != 0
                            ? ParseRegexFlags(AtomizedString(registers[instr.Operand]))
                            : System.Text.RegularExpressions.RegexOptions.None;
                        var tokens = System.Text.RegularExpressions.Regex.Split(input, pattern, options)
                            .Where(t => !string.IsNullOrEmpty(t))
                            .Select(XdmValue.FromString)
                            .ToList();
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(tokens));
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Type operations
                // ------------------------------------------------------------------
                case IrOpCode.Cast:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        var value = registers[instr.RegisterB];
                        bool isEmpty = value.IsUndefined || (value.IsSequence && TryGetSequenceLength(value.SequenceValue, out var len) && len == 0);
                        if (isEmpty)
                        {
                            if (occurrence == OccurrenceIndicator.ZeroOrOne)
                            {
                                registers[instr.RegisterA] = XdmValue.Undefined;
                            }
                            else if (occurrence is OccurrenceIndicator.ZeroOrMore or OccurrenceIndicator.OneOrMore)
                            {
                                throw new InvalidOperationException("Cannot cast to a sequence type with * or + occurrence indicator.");
                            }
                            else
                            {
                                throw new InvalidOperationException("XPTY0004: Cast expression requires a singleton input sequence.");
                            }
                        }
                        else
                        {
                            registers[instr.RegisterA] = Cast(value, typeName, context);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Castable:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        var value = registers[instr.RegisterB];
                        bool isEmpty = value.IsUndefined || (value.IsSequence && TryGetSequenceLength(value.SequenceValue, out var len) && len == 0);
                        bool castable;
                        if (isEmpty)
                        {
                            castable = occurrence is OccurrenceIndicator.ZeroOrOne or OccurrenceIndicator.ZeroOrMore;
                        }
                        else if (occurrence is OccurrenceIndicator.ZeroOrMore or OccurrenceIndicator.OneOrMore)
                        {
                            castable = false;
                        }
                        else
                        {
                            try
                            {
                                castable = TryCast(value, typeName, context, out _);
                            }
                            catch (InvalidOperationException)
                            {
                                // castable as returns false for any dynamic error that the cast would raise
                                // (e.g. FOCA0003, FOAR0002), rather than propagating the exception.
                                castable = false;
                            }
                            catch (OverflowException)
                            {
                                castable = false;
                            }
                        }
                        registers[instr.RegisterA] = XdmValue.FromBoolean(castable);
                        ip++;
                        break;
                    }

                case IrOpCode.InstanceOf:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        var instanceValue = registers[instr.RegisterB];
                        bool instance;
                        if (typeName.TrimStart().StartsWith("function(", StringComparison.OrdinalIgnoreCase))
                        {
                            // Function type tests on named function items need the
                            // registered signature, which requires the evaluation context.
                            if (instanceValue.IsUndefined)
                                instance = occurrence is OccurrenceIndicator.ZeroOrOne or OccurrenceIndicator.ZeroOrMore;
                            else
                                instance = FunctionItemInstanceOf(instanceValue, typeName, context);
                        }
                        else
                        {
                            instance = InstanceOf(instanceValue, typeName, occurrence, context.DefaultElementNamespace, context);
                        }
                        registers[instr.RegisterA] = XdmValue.FromBoolean(instance);
                        ip++;
                        break;
                    }

                case IrOpCode.TreatAs:
                    {
                        string typeName = (string)literalPool[instr.Operand]!;
                        var occurrence = (OccurrenceIndicator)instr.RegisterC;
                        var value = registers[instr.RegisterB];
                        if (!InstanceOf(value, typeName, occurrence, context.DefaultElementNamespace, context))
                            throw new InvalidOperationException($"XPDY0050: Treat as assertion failed for type {typeName} with occurrence {occurrence}.");
                        registers[instr.RegisterA] = value;
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Sequence functions
                // ------------------------------------------------------------------
                case IrOpCode.Count:
                    {
                        var seq = registers[instr.RegisterB];
                        if (!seq.IsSequence)
                            registers[instr.RegisterA] = XdmValue.FromInteger(1);
                        else if (seq.SequenceValue!.TryGetLength(out var len))
                            registers[instr.RegisterA] = XdmValue.FromInteger(len);
                        else
                        {
                            long count = 0;
                            foreach (var _ in XdmSequence.FromSource(seq.SequenceValue!))
                                count++;
                            registers[instr.RegisterA] = XdmValue.FromInteger(count);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Exists:
                    {
                        // fn:exists is purely existential: any item (including 0, "",
                        // false, maps and arrays) counts; no effective boolean value
                        // is computed.
                        var seq = registers[instr.RegisterB];
                        registers[instr.RegisterA] = XdmValue.FromBoolean(SequenceHasAnyItem(seq));
                        ip++;
                        break;
                    }

                case IrOpCode.Empty:
                    {
                        var seq = registers[instr.RegisterB];
                        registers[instr.RegisterA] = XdmValue.FromBoolean(!SequenceHasAnyItem(seq));
                        ip++;
                        break;
                    }

                case IrOpCode.Head:
                    {
                        var seq = registers[instr.RegisterB];
                        if (!seq.IsSequence)
                            registers[instr.RegisterA] = seq;
                        else
                        {
                            XdmValue? first = null;
                            foreach (var item in XdmSequence.FromSource(seq.SequenceValue!))
                            {
                                first = item;
                                break;
                            }
                            registers[instr.RegisterA] = first ?? XdmValue.FromSequence(XdmSequence.Empty);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Tail:
                    {
                        var seq = registers[instr.RegisterB];
                        if (!seq.IsSequence)
                        {
                            registers[instr.RegisterA] = XdmValue.FromSequence(XdmSequence.Empty);
                        }
                        else
                        {
                            var list = new List<XdmValue>();
                            bool first = true;
                            foreach (var item in XdmSequence.FromSource(seq.SequenceValue!))
                            {
                                if (first) { first = false; continue; }
                                list.Add(item);
                            }
                            registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(list));
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.InsertBefore:
                    {
                        var target = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        long pos = ToInteger(Atomize(registers[instr.RegisterC]));
                        var inserts = MaterializeSequence(registers[instr.RegisterB + 1]).ToList();
                        if (pos < 1) pos = 1;
                        if (pos > target.Count + 1) pos = target.Count + 1;
                        target.InsertRange((int)pos - 1, inserts);
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(target));
                        ip++;
                        break;
                    }

                case IrOpCode.Remove:
                    {
                        var target = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        long pos = ToInteger(Atomize(registers[instr.RegisterC]));
                        if (pos >= 1 && pos <= target.Count)
                            target.RemoveAt((int)pos - 1);
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(target));
                        ip++;
                        break;
                    }

                case IrOpCode.Reverse:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        items.Reverse();
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(items));
                        ip++;
                        break;
                    }

                case IrOpCode.Subsequence:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        double startD = ToDouble(Atomize(registers[instr.RegisterC]));
                        if (double.IsNaN(startD))
                        {
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        }
                        else
                        {
                            int start = (int)Math.Round(startD);
                            if (start < 1) start = 1;
                            if (start > items.Count)
                            {
                                registers[instr.RegisterA] = XdmValue.Undefined;
                            }
                            else if (instr.Operand != 0)
                            {
                                double lenD = ToDouble(Atomize(registers[instr.Operand]));
                                if (double.IsNaN(lenD) || lenD <= 0)
                                    registers[instr.RegisterA] = XdmValue.Undefined;
                                else
                                {
                                    int len = (int)Math.Round(lenD);
                                    int count = Math.Min(len, items.Count - start + 1);
                                    registers[instr.RegisterA] = XdmValue.FromSequence(
                                        MaterializedSequence.FromList(items.Skip(start - 1).Take(count).ToList()));
                                }
                            }
                            else
                            {
                                registers[instr.RegisterA] = XdmValue.FromSequence(
                                    MaterializedSequence.FromList(items.Skip(start - 1).ToList()));
                            }
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.DistinctValues:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        var seen = new HashSet<string>();
                        var result = new List<XdmValue>();
                        foreach (var item in items)
                        {
                            string key = AtomizedString(item);
                            if (seen.Add(key))
                                result.Add(item);
                        }
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(result));
                        ip++;
                        break;
                    }

                case IrOpCode.IndexOf:
                    {
                        var seq = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        string search = AtomizedString(registers[instr.RegisterC]);
                        var result = new List<XdmValue>();
                        for (int i = 0; i < seq.Count; i++)
                        {
                            if (AtomizedString(seq[i]) == search)
                                result.Add(XdmValue.FromInteger(i + 1));
                        }
                        registers[instr.RegisterA] = XdmValue.FromSequence(MaterializedSequence.FromList(result));
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Aggregation
                // ------------------------------------------------------------------
                case IrOpCode.Sum:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        if (items.Count == 0)
                            registers[instr.RegisterA] = instr.Operand != 0 ? registers[instr.Operand] : XdmValue.FromInteger(0);
                        else
                            registers[instr.RegisterA] = Sum(items);
                        ip++;
                        break;
                    }

                case IrOpCode.Avg:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        if (items.Count == 0)
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        else
                        {
                            var total = Sum(items);
                            if (total.Kind == XdmValueKind.Decimal)
                                registers[instr.RegisterA] = XdmValue.FromDecimal(total.DecimalValue / items.Count);
                            else
                                registers[instr.RegisterA] = XdmValue.FromDouble(ToDouble(total) / items.Count);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Min:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        if (items.Count == 0)
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        else
                            registers[instr.RegisterA] = MinMax(items, true);
                        ip++;
                        break;
                    }

                case IrOpCode.Max:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        if (items.Count == 0)
                            registers[instr.RegisterA] = XdmValue.Undefined;
                        else
                            registers[instr.RegisterA] = MinMax(items, false);
                        ip++;
                        break;
                    }

                case IrOpCode.StringJoin:
                    {
                        var items = MaterializeSequence(registers[instr.RegisterB]).ToList();
                        string sep = instr.Operand != 0 ? AtomizedString(registers[instr.Operand]) : string.Empty;
                        var strings = new List<string>(items.Count);
                        foreach (var item in items)
                            strings.Add(AtomizedString(item));
                        registers[instr.RegisterA] = XdmValue.FromString(string.Join(sep, strings));
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Higher-order (XPath 3.1)
                // ------------------------------------------------------------------
                case IrOpCode.Map:
                    registers[instr.RegisterA] = XdmValue.FromMap(new XdmMap());
                    ip++;
                    break;

                case IrOpCode.MapAdd:
                    {
                        var map = registers[instr.RegisterA].MapValue;
                        var key = AtomizeMapKey(registers[instr.RegisterB]);
                        // XPath 3.1 §3.11.4: duplicate keys in a map constructor are a dynamic error.
                        if (map.ContainsKey(key))
                            throw new InvalidOperationException($"XQDY0137: Duplicate key '{key}' in map constructor.");
                        map.Add(key, registers[instr.RegisterC]);
                        ip++;
                        break;
                    }

                case IrOpCode.Array:
                    registers[instr.RegisterA] = XdmValue.FromArray(new XdmArray());
                    ip++;
                    break;

                case IrOpCode.ArrayAdd:
                    {
                        var arr = registers[instr.RegisterA].ArrayValue;
                        arr.Add(registers[instr.RegisterB]);
                        ip++;
                        break;
                    }

                case IrOpCode.ArrayAddAll:
                    {
                        var arr = registers[instr.RegisterA].ArrayValue;
                        var seq = registers[instr.RegisterB];
                        if (seq.IsSequence && seq.SequenceValue is not null)
                        {
                            foreach (var item in seq.SequenceValue)
                                arr.Add(item);
                        }
                        else if (!seq.IsUndefined)
                        {
                            arr.Add(seq);
                        }
                        ip++;
                        break;
                    }

                case IrOpCode.Lookup:
                    {
                        var container = registers[instr.RegisterB];
                        var key = registers[instr.RegisterC];

                        registers[instr.RegisterA] = LookupValue(container, key);
                        ip++;
                        break;
                    }

                case IrOpCode.LookupWildcard:
                    {
                        var container = registers[instr.RegisterB];
                        var result = new List<XdmValue>();

                        void AddFlattened(XdmValue v)
                        {
                            if (v.IsSequence && v.SequenceValue is not null)
                            {
                                foreach (var item in XdmSequence.FromSource(v.SequenceValue))
                                    result.Add(item);
                            }
                            else if (!v.IsUndefined)
                            {
                                result.Add(v);
                            }
                        }

                        if (container.Kind == XdmValueKind.Map)
                        {
                            foreach (var v in container.MapValue.Values)
                                AddFlattened(v);
                        }
                        else if (container.Kind == XdmValueKind.Array)
                        {
                            foreach (var v in container.ArrayValue.Values)
                                AddFlattened(v);
                        }
                        else if (container.IsSequence && container.SequenceValue is not null)
                        {
                            foreach (var item in XdmSequence.FromSource(container.SequenceValue))
                            {
                                if (item.Kind == XdmValueKind.Map)
                                {
                                    foreach (var v in item.MapValue.Values)
                                        AddFlattened(v);
                                }
                                else if (item.Kind == XdmValueKind.Array)
                                {
                                    foreach (var v in item.ArrayValue.Values)
                                        AddFlattened(v);
                                }
                                else
                                {
                                    throw new InvalidOperationException($"XPTY0004: Wildcard lookup requires a map or an array, got {item.Kind}.");
                                }
                            }
                        }
                        else if (!container.IsUndefined)
                        {
                            throw new InvalidOperationException($"XPTY0004: Wildcard lookup requires a map or an array, got {container.Kind}.");
                        }

                        registers[instr.RegisterA] = XdmValue.FromSequence(
                            MaterializedSequence.FromList(result));
                        ip++;
                        break;
                    }

                case IrOpCode.LoadFunction:
                    {
                        var raw = literalPool[instr.Operand]!;
                        FunctionItem funcItem = raw switch
                        {
                            // Capture the defining context so the function item can still be
                            // resolved if it crosses into another evaluation context (e.g. a
                            // function returned by fn:transform delivery-format="raw").
                            NamedFunctionItem named => ResolveNamedFunctionItem(named, context),
                            CurriedFunctionItem curried => curried,
                            InlineFunctionItem inline => inline,
                            CompilerInlineFunction cif => new InlineFunctionItem(cif.Parameters, cif.Body, cif.ParameterTypes, cif.ReturnType)
                            {
                                // Capture the defining variable environment for closure semantics.
                                CapturedVariables = context.SnapshotVariables()
                            },
                            ValueTuple<string, int> namedTuple => ResolveNamedFunctionTuple(namedTuple, context),
                            _ => throw new InvalidOperationException($"Unknown function item type: {raw.GetType().Name}")
                        };
                        registers[instr.RegisterA] = XdmValue.FromFunction(funcItem);
                        ip++;
                        break;
                    }

                case IrOpCode.Curry:
                    {
                        var baseFunc = (FunctionItem)registers[instr.RegisterB].FunctionValue;
                        var descriptor = (int[])literalPool[instr.Operand]!;
                        // XPTY0004: a partial application must supply exactly the function's
                        // arity in argument positions, placeholders included (xqhof8/9).
                        if (descriptor.Length != baseFunc.Arity)
                            throw new InvalidOperationException($"XPTY0004: Partial application of a function of arity {baseFunc.Arity} with {descriptor.Length} argument positions.");
                        var fixedArgs = new XdmValue?[descriptor.Length];
                        for (int i = 0; i < descriptor.Length; i++)
                        {
                            fixedArgs[i] = descriptor[i] >= 0 ? registers[descriptor[i]] : null;
                        }
                        registers[instr.RegisterA] = XdmValue.FromFunction(new CurriedFunctionItem(baseFunc, fixedArgs));
                        ip++;
                        break;
                    }

                case IrOpCode.Apply:
                    {
                        var funcValue = registers[instr.RegisterB];
                        int argCount = instr.RegisterC;
                        int firstArgReg = instr.Operand;
                        XdmValue[] args = new XdmValue[argCount];
                        for (int i = 0; i < argCount; i++)
                            args[i] = registers[firstArgReg + i];
                        registers[instr.RegisterA] = InvokeFunctionItem(funcValue, context, args);
                        ip++;
                        break;
                    }

                // ------------------------------------------------------------------
                // Constructors
                // ------------------------------------------------------------------
                case IrOpCode.ElementConstructor:
                case IrOpCode.AttributeConstructor:
                case IrOpCode.TextConstructor:
                case IrOpCode.DocumentConstructor:
                    throw new NotImplementedException($"{instr.OpCode} is not yet implemented.");

                // ------------------------------------------------------------------
                // Error
                // ------------------------------------------------------------------
                case IrOpCode.Error:
                    throw new InvalidOperationException("Runtime error instruction encountered.");

                default:
                    throw new NotSupportedException($"Unsupported opcode: {instr.OpCode}");
            }
        }

        throw new InvalidOperationException("VM reached end of instructions without Return.");
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static XdmValue[] MaterializeSequence(XdmValue sequence)
    {
        if (sequence.IsUndefined)
            return Array.Empty<XdmValue>();

        if (sequence.IsSequence)
        {
            var seq = sequence.SequenceValue;
            if (seq is null)
                return Array.Empty<XdmValue>();

            var list = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(seq))
                list.Add(item);
            return list.ToArray();
        }

        return new[] { sequence };
    }

    /// <summary>
    /// Expands arrays into their member items for general-comparison operands
    /// (XDM 3.1 atomization: an array atomizes to the atomized values of its members).
    /// Returns the input array unchanged when no item is an array.
    /// </summary>
    private static XdmValue[] ExpandArraysForComparison(XdmValue[] items)
    {
        bool hasArray = false;
        foreach (var item in items)
        {
            if (item.IsArray) { hasArray = true; break; }
        }
        if (!hasArray)
            return items;

        var list = new List<XdmValue>();
        foreach (var item in items)
            FlattenArrayItem(item, list);
        return list.ToArray();
    }

    /// <summary>
    /// Lazily enumerates the items of a value for general comparison, expanding arrays.
    /// Unlike <see cref="MaterializeSequence"/>, this avoids allocating huge lists for
    /// large lazy sequences such as integer ranges.
    /// </summary>
    private static IEnumerable<XdmValue> EnumerateItemsForComparison(XdmValue value)
    {
        if (value.IsUndefined) yield break;

        if (!value.IsSequence)
        {
            if (value.IsArray)
            {
                // General-comparison atomization flattens array members recursively,
                // so a nested array contributes its own atomized members (GenCompEq-8).
                foreach (var member in value.ArrayValue.Values)
                {
                    foreach (var flattened in EnumerateItemsForComparison(member))
                        yield return flattened;
                }
            }
            else
            {
                yield return value;
            }
            yield break;
        }

        var seq = value.SequenceValue;
        if (seq is null) yield break;

        foreach (var item in XdmSequence.FromSource(seq))
        {
            if (item.IsArray)
            {
                foreach (var flattened in EnumerateItemsForComparison(item))
                    yield return flattened;
            }
            else
            {
                yield return item;
            }
        }
    }

    private static bool SequenceContainsBooleanItem(XdmValue value)
    {
        foreach (var item in EnumerateItemsForComparison(value))
        {
            var atomized = Atomize(item);
            if (!atomized.IsUndefined && atomized.Kind == XdmValueKind.Boolean)
                return true;
        }
        return false;
    }

    private static void FlattenArrayItem(XdmValue value, List<XdmValue> list)
    {
        if (value.IsArray)
        {
            foreach (var member in value.ArrayValue.Values)
                FlattenArrayItem(member, list);
            return;
        }
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                FlattenArrayItem(item, list);
            return;
        }
        if (!value.IsUndefined)
            list.Add(value);
    }

    private static bool TryGetSequenceLength(IXdmSequence? seq, out int length)
    {
        if (seq is null)
        {
            length = 0;
            return true;
        }
        return seq.TryGetLength(out length);
    }

    private static (string LocalName, string NamespaceUri) ResolveFunctionName(string funcName, EvaluationContext? context)
    {
        string localName = funcName;
        string? prefix = null;
        int colon = funcName.IndexOf(':');
        if (colon >= 0)
        {
            prefix = funcName[..colon];
            localName = funcName[(colon + 1)..];
        }

        string nsUri;
        if (prefix is not null)
        {
            if (context is null || !context.TryResolveNamespace(prefix, out nsUri))
                throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
        }
        else
        {
            nsUri = "http://www.w3.org/2005/xpath-functions"; // default function namespace
        }

        return (localName, nsUri);
    }

    private static (string LocalName, string NamespaceUri) ResolveVariableName(string varName, EvaluationContext context)
    {
        // Braced URI literal: Q{uri}localname or Q{uri}prefix:local
        // The empty URI form Q{}local is permitted and means "no namespace".
        if (varName.Length > 2 && varName[0] == 'Q' && varName[1] == '{')
        {
            int closeBrace = varName.IndexOf('}');
            if (closeBrace >= 2)
            {
                string uri = varName[2..closeBrace];
                string local = varName[(closeBrace + 1)..];
                return (local, uri);
            }
        }

        string localName = varName;
        string? prefix = null;
        int colon = varName.IndexOf(':');
        if (colon >= 0)
        {
            prefix = varName[..colon];
            localName = varName[(colon + 1)..];
        }

        string nsUri = "";
        if (prefix is not null)
        {
            if (!context.TryResolveNamespace(prefix, out var resolvedNs))
                throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
            nsUri = resolvedNs;
        }

        return (localName, nsUri);
    }

    private static XdmAxis ToXdmAxis(IrOpCode opcode) => opcode switch
    {
        IrOpCode.Child => XdmAxis.Child,
        IrOpCode.Descendant => XdmAxis.Descendant,
        IrOpCode.DescendantOrSelf => XdmAxis.DescendantOrSelf,
        IrOpCode.Ancestor => XdmAxis.Ancestor,
        IrOpCode.AncestorOrSelf => XdmAxis.AncestorOrSelf,
        IrOpCode.Attribute => XdmAxis.Attribute,
        IrOpCode.Parent => XdmAxis.Parent,
        IrOpCode.Self => XdmAxis.Self,
        IrOpCode.Following => XdmAxis.Following,
        IrOpCode.FollowingSibling => XdmAxis.FollowingSibling,
        IrOpCode.Preceding => XdmAxis.Preceding,
        IrOpCode.PrecedingSibling => XdmAxis.PrecedingSibling,
        IrOpCode.Namespace => XdmAxis.Namespace,
        _ => throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null)
    };

    private static XdmValue ApplyAxis(XdmValue input, XdmAxis axis)
    {
        if (input.IsUndefined)
        {
            // An empty-sequence input to a path step (e.g. doc(())/*) produces an empty
            // sequence; the real "no context item" case is already caught by LoadContextItem.
            return XdmValue.Undefined;
        }
        if (input.IsAtomic)
            throw new InvalidOperationException("XPTY0020: An axis step requires a context item that is a node.");

        if (input.IsNode)
            return XdmValue.FromSequence(input.NodeValue.Axis(axis));

        if (input.IsSequence)
        {
            var items = MaterializeSequence(input);
            var result = new List<XdmValue>();
            foreach (var item in items)
            {
                // A path step whose input contains atomic values is a type error
                // (XPTY0019 — this covers both intermediate steps and axis steps
                // applied to a mixed sequence, XPTY0019_1/2).
                if (!item.IsNode)
                    throw new InvalidOperationException("XPTY0019: A path step requires nodes, but the step input contains an atomic value.");
                var seq = item.NodeValue.Axis(axis);
                foreach (var node in seq)
                    result.Add(node);
            }
            return XdmValue.FromSequence(MaterializedSequence.FromList(result));
        }

        throw new InvalidOperationException(
            $"Axis {axis} requires a node or sequence of nodes, but got {input.Kind}.");
    }

    /// <summary>
    /// Atomizes an XDM value for comparison: nodes become their string value,
    /// singleton sequences are unpacked, and other values pass through.
    /// </summary>
    private static FunctionItem ResolveNamedFunctionTuple(ValueTuple<string, int> tuple, EvaluationContext context)
    {
        var (localName, nsUri) = ResolveFunctionName(tuple.Item1, context);
        // An arity clamped to int.MaxValue in the parser indicates an out-of-range literal
        // (e.g. fn:concat#340282366920938463463374607431768211456); surface FOAR0002 instead
        // of resolving against a variadic fallback and returning an impossible arity.
        if (tuple.Item2 == int.MaxValue)
            throw new InvalidOperationException($"FOAR0002: Function {{{nsUri}}}{localName}#{tuple.Item2} arity exceeds the implementation limit.");
        if (!context.TryResolveFunction(nsUri, localName, tuple.Item2, out _))
            throw new InvalidOperationException($"XPST0017: Function {{{nsUri}}}{localName}#{tuple.Item2} not found.");
        return new NamedFunctionItem(nsUri, localName, tuple.Item2)
        {
            DefiningContext = context,
            CapturedContextItem = context.ContextItem,
            CapturedContextPosition = context.ContextPosition,
            CapturedContextSize = context.ContextSize,
            CapturedBaseUri = context.BaseUri
        };
    }

    /// <summary>
    /// Validates the arity of a compile-time-resolved named function reference and captures
    /// the defining context (XPST0017 when the function/arity pair is not registered).
    /// </summary>
    private static NamedFunctionItem ResolveNamedFunctionItem(NamedFunctionItem named, EvaluationContext context)
    {
        if (named.DefiningContext != null)
            return named;
        // An arity clamped to int.MaxValue in the parser indicates an out-of-range literal;
        // surface FOAR0002 instead of resolving against a variadic fallback.
        if (named.Arity == int.MaxValue)
            throw new InvalidOperationException($"FOAR0002: Function {{{named.NamespaceUri}}}{named.LocalName}#{named.Arity} arity exceeds the implementation limit.");
        if (!context.TryResolveFunction(named.NamespaceUri, named.LocalName, named.Arity, out _))
            throw new InvalidOperationException($"XPST0017: Function {{{named.NamespaceUri}}}{named.LocalName}#{named.Arity} not found.");
        return named with
        {
            DefiningContext = context,
            CapturedContextItem = context.ContextItem,
            CapturedContextPosition = context.ContextPosition,
            CapturedContextSize = context.ContextSize,
            CapturedBaseUri = context.BaseUri
        };
    }

    public static XdmValue InvokeFunctionItem(FunctionItem func, EvaluationContext context, ReadOnlySpan<XdmValue> args)
    {
        // XSLT 3.0 §5.3.4: dynamic function calls clear the current captured substrings
        // (regex groups) and the current output URI for the duration of the call.
        var savedRegexGroups = context.RegexGroups;
        var savedOutputUri = context.CurrentOutputUri;
        context.RegexGroups = null;
        context.CurrentOutputUri = null;
        try
        {
            return InvokeFunctionItemCore(func, context, args);
        }
        finally
        {
            context.RegexGroups = savedRegexGroups;
            context.CurrentOutputUri = savedOutputUri;
        }
    }

    private static XdmValue InvokeFunctionItemCore(FunctionItem func, EvaluationContext context, ReadOnlySpan<XdmValue> args)
    {
        switch (func)
        {
            case NamedFunctionItem named:
                // The function item has a fixed arity; a dynamic call must supply exactly
                // that many arguments (higher-order-functions-049/050).
                if (args.Length != named.ArityValue)
                    throw new InvalidOperationException($"XPST0017: Function {{{named.NamespaceUri}}}{named.LocalName}#{named.ArityValue} cannot be called with {args.Length} argument(s).");
                if (!context.TryResolveFunction(named.NamespaceUri, named.LocalName, args.Length, out var sig))
                {
                    // Fall back to the context in which the function item was created
                    // (function items returned by fn:transform delivery-format="raw").
                    if (named.DefiningContext is EvaluationContext definingContext
                        && definingContext.TryResolveFunction(named.NamespaceUri, named.LocalName, args.Length, out sig))
                    {
                        // Resolved against the defining context.
                    }
                    else
                    {
                        throw new InvalidOperationException($"XPST0017: Function {{{named.NamespaceUri}}}{named.LocalName}#{args.Length} not found.");
                    }
                }
                // Named function references and function-lookup results capture the dynamic
                // context in which they are created. Context-dependent functions such as
                // fn:base-uri#0 and fn:document-uri#0 therefore use the creator's focus,
                // not the call-site focus (fn-function-lookup-018/022).
                XdmValue savedItem = XdmValue.Undefined;
                int savedPosition = 0;
                int savedSize = 0;
                bool restoreFocus = false;
                if (!named.CapturedContextItem.IsUndefined)
                {
                    savedItem = context.ContextItem;
                    savedPosition = context.ContextPosition;
                    savedSize = context.ContextSize;
                    context.WithFocus(named.CapturedContextItem, named.CapturedContextPosition, named.CapturedContextSize);
                    restoreFocus = true;
                }
                else if (!context.ContextItem.IsUndefined)
                {
                    // No focus was captured at creation: the function item's focus is
                    // absent. Context-dependent calls see XPDY0002 rather than the
                    // call-site focus (xqhof14: <a/>/(name#0)()).
                    savedItem = context.ContextItem;
                    savedPosition = context.ContextPosition;
                    savedSize = context.ContextSize;
                    context.WithFocus(XdmValue.Undefined, 0, 0);
                    restoreFocus = true;
                }
                // Static base URI: context-dependent functions resolve against the base URI
                // of the module that materialized the function item (xqhof16/18).
                string? savedBaseUri = null;
                bool restoreBaseUri = false;
                if (named.CapturedBaseUri is not null && named.CapturedBaseUri != context.BaseUri)
                {
                    savedBaseUri = context.BaseUri;
                    context.BaseUri = named.CapturedBaseUri;
                    restoreBaseUri = true;
                }
                try
                {
                    // XSLT context-dependent functions (e.g. current-group) supply a separate
                    // implementation for dynamic invocation through a function item.
                    return (sig.DynamicImplementation ?? sig.Implementation)(context, ConvertDynamicCallArgs(sig, args, context));
                }
                finally
                {
                    if (restoreFocus)
                        context.WithFocus(savedItem, savedPosition, savedSize);
                    if (restoreBaseUri)
                        context.BaseUri = savedBaseUri;
                }

            case DelegateFunctionItem del:
                if (args.Length != del.ArityValue)
                    throw new InvalidOperationException("XPTY0004: Wrong number of arguments for dynamic function call.");
                return del.Implementation(context, args);

            case CoercedFunctionItem coerced:
                {
                    // XPath 3.1 function conversion rules: convert each argument to the
                    // declared parameter type, invoke the inner function, and validate the
                    // result against the declared return type (higher-order-functions-038/060).
                    if (args.Length != coerced.ParamTypes.Count)
                        throw new InvalidOperationException("XPTY0004: Wrong number of arguments for dynamic function call.");
                    var convertedArgs = new XdmValue[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        var paramType = coerced.ParamTypes[i];
                        convertedArgs[i] = string.IsNullOrEmpty(paramType)
                            ? args[i]
                            : ApplyFunctionConversion(args[i], paramType!, context);
                    }
                    var coercedResult = InvokeFunctionItem(coerced.Inner, context, convertedArgs);
                    return string.IsNullOrEmpty(coerced.ReturnType)
                        ? coercedResult
                        : ApplyFunctionConversion(coercedResult, coerced.ReturnType!, context);
                }

            case InlineFunctionItem inline:
                {
                    if (args.Length != inline.Parameters.Count)
                        throw new InvalidOperationException("XPTY0004: Wrong number of arguments for dynamic call of an inline function.");

                    // Snapshot the entire variable scope: the function body is a new scope.
                    // Parameters, captured closure variables, and any let/for bindings created
                    // during execution must not leak back to the caller, and recursive calls
                    // must not clobber the caller's same-named bindings (functx dynamic-path).
                    var variableSnapshot = context.SnapshotVariables();

                    // Restore captured closure variables (the defining environment) so the
                    // body can reference outer variables after the defining frame exited.
                    if (inline.CapturedVariables is { Count: > 0 } captured)
                    {
                        foreach (var (key, capturedValue) in captured)
                            context.WithVariable(key.LocalName, capturedValue, key.NamespaceUri);
                    }

                    // Apply XPath 3.1 function conversion rules to each argument: atomization,
                    // untypedAtomic casting, numeric promotion, and URI promotion. Validation
                    // against the declared type is performed on the converted value using the
                    // static context for context-sensitive types (e.g., xs:QName).
                    var convertedArgs = new XdmValue[args.Length];
                    for (int i = 0; i < inline.Parameters.Count; i++)
                    {
                        var expectedType = i < inline.ParameterTypes.Count ? inline.ParameterTypes[i] : null;
                        if (!string.IsNullOrEmpty(expectedType))
                        {
                            convertedArgs[i] = ApplyFunctionConversion(args[i], expectedType!, context);
                            // Function tests: ApplyFunctionConversion already enforced arity
                            // and wrapped the items in CoercedFunctionItems, which carry no
                            // declared signature for ValueMatchesType to match — skip the
                            // redundant re-validation for function items (hof-040/045).
                            var trimmedType = expectedType!.TrimStart();
                            bool functionTest = trimmedType.StartsWith("function", StringComparison.OrdinalIgnoreCase)
                                || trimmedType.StartsWith("(function", StringComparison.OrdinalIgnoreCase);
                            var converted = convertedArgs[i];
                            if (!converted.IsUndefined && !functionTest)
                            {
                                if (converted.IsSequence)
                                {
                                    foreach (var item in XdmSequence.FromSource(converted.SequenceValue!))
                                    {
                                        if (!ValueMatchesType(item, expectedType, context))
                                            throw new InvalidOperationException("XPTY0004");
                                    }
                                }
                                else
                                {
                                    if (!ValueMatchesType(converted, expectedType, context))
                                        throw new InvalidOperationException("XPTY0004");
                                }
                            }
                        }
                        else
                        {
                            convertedArgs[i] = args[i];
                        }
                    }

                    for (int i = 0; i < inline.Parameters.Count; i++)
                    {
                        var (localName, nsUri) = ResolveVariableName(inline.Parameters[i], context);
                        context.WithVariable(localName, i < convertedArgs.Length ? convertedArgs[i] : XdmValue.Undefined, nsUri);
                    }
                    var savedFocusItem = context.ContextItem;
                    var savedFocusPosition = context.ContextPosition;
                    var savedFocusSize = context.ContextSize;
                    try
                    {
                        // XPath 3.1 §3.1.5 / XQuery: a user-defined function body (declared or
                        // inline) is evaluated with the context item, position, and size
                        // absent — the caller's focus must not propagate into the body
                        // (K2-FunctionProlog-14); references to it raise XPDY0002.
                        context.WithFocus(XdmValue.Undefined, 0, 0);
                        var result = Execute(inline.Body, context);
                        // XPath 3.1 function conversion (atomization, untypedAtomic and URI
                        // casts, numeric promotion) applies to the result before return-type
                        // validation; cardinality and type mismatches raise XPTY0004.
                        if (!string.IsNullOrEmpty(inline.ReturnType))
                            result = ApplyFunctionConversion(result, inline.ReturnType, context);
                        return result;
                    }
                    finally
                    {
                        context.WithFocus(savedFocusItem, savedFocusPosition, savedFocusSize);
                        context.RestoreVariables(variableSnapshot);
                    }
                }

            case CurriedFunctionItem curried:
                {
                    int placeholderCount = curried.FixedArgs.Count(a => a is null);
                    if (args.Length != placeholderCount)
                        throw new InvalidOperationException("XPTY0004: Wrong number of arguments for dynamic call of a partially applied function.");
                    var merged = new XdmValue[curried.FixedArgs.Length];
                    int argIdx = 0;
                    for (int i = 0; i < curried.FixedArgs.Length; i++)
                    {
                        if (curried.FixedArgs[i] is { } boundArg)
                            merged[i] = boundArg;
                        else
                            merged[i] = argIdx < args.Length ? args[argIdx++] : XdmValue.Undefined;
                    }
                    return InvokeFunctionItem(curried.BaseFunction, context, merged);
                }

            default:
                throw new InvalidOperationException($"Unknown function item type: {func.GetType().Name}");
        }
    }

    public static XdmValue InvokeFunctionItem(XdmValue funcValue, EvaluationContext context, ReadOnlySpan<XdmValue> args)
    {
        // A dynamic call target may arrive as a single-item sequence wrapping a
        // function item (e.g. the result of a let expression); unwrap it.
        if (funcValue.IsSequence)
        {
            var items = MaterializeSequence(funcValue);
            if (items.Length == 1)
                funcValue = items[0];
            else
                throw new InvalidOperationException("XPTY0004");
        }

        if (funcValue.IsFunction)
            return InvokeFunctionItem((FunctionItem)funcValue.FunctionValue, context, args);

        if (funcValue.IsMap)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("XPTY0004");
            var key = AtomizeMapKey(args[0]);
            var map = funcValue.MapValue;
            if (map.TryGetValue(key, out var value))
                return value;
            return XdmValue.FromSequence(XdmSequence.Empty);
        }

        if (funcValue.IsArray)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("XPTY0004");
            return ArrayLookup(funcValue.ArrayValue, args[0]);
        }

        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue Atomize(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;

        if (value.IsNode)
        {
            var node = value.NodeValue;
            // XDM §2.7.2: comments and PIs atomize to xs:string; namespace nodes too.
            // Untyped nodes atomize to xs:untypedAtomic. Schema-validated nodes return
            // their PSVI typed value; element-only/empty complex types raise FOTY0012.
            if (node.NodeKind is XdmNodeKind.ProcessingInstruction or XdmNodeKind.Comment)
                return XdmValue.FromString(node.StringValue);
            if (node.NodeKind == XdmNodeKind.Namespace)
                return XdmValue.FromString(node.StringValue);
            if (node.SchemaTypeAnnotation is null)
                return XdmValue.FromString(node.StringValue, "untypedAtomic");
            if (node.HasNoTypedValue)
                throw new InvalidOperationException("FOTY0012: Cannot atomize a node that has no typed value.");
            return node.TypedValue;
        }

        if (value.IsArray && value.ArrayValue is not null)
        {
            // fn:data semantics: atomizing an array atomizes its members (ArrayTest-028).
            var members = new List<XdmValue>();
            foreach (var member in value.ArrayValue.Values)
            {
                if (member.IsSequence && member.SequenceValue is not null)
                {
                    foreach (var inner in XdmSequence.FromSource(member.SequenceValue))
                        members.Add(inner);
                }
                else if (!member.IsUndefined)
                {
                    members.Add(member);
                }
            }
            if (members.Count == 0)
                return XdmValue.Undefined;
            if (members.Count == 1)
                return Atomize(members[0]);
            // Singleton requirement (arithmetic, value comparisons): atomization of a
            // multi-item array is a type error, as with multi-item sequences.
            throw new InvalidOperationException(
                "XPTY0004: Atomization requires a singleton or empty sequence, but got an array with " + members.Count + " items");
        }

        if (value.IsSequence)
        {
            var items = MaterializeSequence(value);
            if (items.Length == 1)
                return Atomize(items[0]);
            if (items.Length == 0)
                return XdmValue.Undefined;
            // Singleton requirement (fn:data, arithmetic, value comparisons):
            // atomization of a multi-item sequence is a type error.
            throw new InvalidOperationException(
                "XPTY0004: Atomization requires a singleton or empty sequence, but got a sequence of length " + items.Length);
        }

        return value;
    }

    // Atomizes node items in a value (one level; sequence items are mapped individually)
    // for variable-declaration type checking (XQuery 3.1 §4.16).
    private static XdmValue AtomizeItems(XdmValue value)
    {
        if (value.IsNode)
            return Atomize(value);
        if (!value.IsSequence || value.SequenceValue is null)
            return value;
        var items = MaterializeSequence(value);
        bool anyNode = false;
        foreach (var item in items)
        {
            if (item.IsNode)
            {
                anyNode = true;
                break;
            }
        }
        if (!anyNode)
            return value;
        var atomized = new List<XdmValue>(items.Length);
        foreach (var item in items)
            atomized.Add(item.IsNode ? Atomize(item) : item);
        return XdmValue.FromSequence(MaterializedSequence.FromList(atomized));
    }

    // True when a sequence-type text is a node kind test (element/attribute/node/...),
    // whose matching applies to the nodes themselves without atomization.
    private static bool IsNodeKindTest(string typeName)
    {
        var t = typeName.TrimStart();
        return t.StartsWith("element(", StringComparison.Ordinal)
            || t.StartsWith("attribute(", StringComparison.Ordinal)
            || t.StartsWith("document-node(", StringComparison.Ordinal)
            || t.StartsWith("comment(", StringComparison.Ordinal)
            || t.StartsWith("text(", StringComparison.Ordinal)
            || t.StartsWith("processing-instruction(", StringComparison.Ordinal)
            || t.StartsWith("namespace-node(", StringComparison.Ordinal)
            || t.StartsWith("schema-element(", StringComparison.Ordinal)
            || t.StartsWith("schema-attribute(", StringComparison.Ordinal)
            || t.StartsWith("node(", StringComparison.Ordinal);
    }

    private static string AtomizedStringValue(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013: Argument to string concatenation is a function, map, or array");

        var atomized = Atomize(value);
        if (atomized.IsUndefined)
            return string.Empty;

        return atomized.ToString();
    }

    /// <summary>
    /// Unwraps a singleton sequence to its single item, or returns the value as-is.
    /// Returns Undefined for an empty sequence.
    /// </summary>
    private static XdmValue UnwrapSingleton(XdmValue value)
    {
        if (value.IsUndefined || !value.IsSequence)
            return value;

        var items = MaterializeSequence(value);
        if (items.Length == 0)
            return XdmValue.Undefined;
        if (items.Length == 1)
            return items[0];
        return value;
    }

    /// <summary>
    /// Returns true if the value is a node or a singleton sequence containing a node.
    /// Used to determine whether atomization produces an untyped atomic value.
    /// </summary>
    private static bool IsNodeOrigin(XdmValue value)
    {
        if (value.IsUndefined)
            return false;
        if (value.IsNode)
            return true;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            var items = MaterializeSequence(value);
            if (items.Length == 1)
                return IsNodeOrigin(items[0]);
        }
        return false;
    }

    /// <summary>
    /// Removes duplicate nodes and sorts the remaining nodes into document order.
    /// Non-node items are preserved in their original relative order after all nodes.
    /// </summary>
    private static XdmValue NormalizeSequence(XdmValue value)
    {
        if (value.IsUndefined || !value.IsSequence)
            return value;

        var items = MaterializeSequence(value);
        if (items.Length <= 1)
            return value;

        // Separate nodes from non-node items and remove duplicate nodes.
        var nodes = new List<XdmValue>(items.Length);
        var nonNodes = new List<XdmValue>();
        var seen = new HashSet<IXdmNode>();
        bool hasNodes = false;
        foreach (var item in items)
        {
            if (!item.IsNode)
            {
                nonNodes.Add(item);
                continue;
            }

            hasNodes = true;
            var node = item.NodeValue;
            if (seen.Add(node))
                nodes.Add(item);
        }

        if (!hasNodes)
            return XdmValue.FromSequence(MaterializedSequence.FromList(nonNodes));

        if (nodes.Count > 1)
        {
            // Use a stable sort by document order. Namespace nodes of the same element
            // share the same DocumentOrder (they are ordered by the owner element), so
            // preserving their original sequence order keeps the namespace axis order
            // (xml first, then root-to-current) intact. Document-order keys are computed
            // eagerly in sequence order first: parentless trees receive their tree
            // sequence on first access, and computing keys inside the comparator would
            // assign them in comparison order, scrambling detached copies (square-array-014).
            var indexed = nodes.Select((n, i) => (Node: n, Index: i, OrderKey: n.NodeValue!.DocumentOrder)).ToList();
            indexed.Sort((a, b) =>
            {
                bool aDoc = a.Node.NodeValue!.Document is not null;
                bool bDoc = b.Node.NodeValue!.Document is not null;
                if (aDoc != bDoc)
                    return aDoc ? -1 : 1;
                int cmp = a.OrderKey.CompareTo(b.OrderKey);
                if (cmp != 0)
                    return cmp;
                return a.Index.CompareTo(b.Index);
            });
            nodes = indexed.Select(x => x.Node).ToList();
        }

        if (nonNodes.Count == 0)
            return XdmValue.FromSequence(MaterializedSequence.FromList(nodes));

        var combined = new List<XdmValue>(nodes.Count + nonNodes.Count);
        combined.AddRange(nodes);
        combined.AddRange(nonNodes);
        return XdmValue.FromSequence(MaterializedSequence.FromList(combined));
    }

    /// <summary>
    /// Validates that every item in a sequence is a node, raising XPTY0004 otherwise.
    /// Used by union, intersect, and except operators.
    /// </summary>
    private static void RequireNodeSequence(XdmValue[] sequence)
    {
        foreach (var item in sequence)
        {
            if (!item.IsNode)
                throw new InvalidOperationException("XPTY0004");
        }
    }

    private static XdmValue FilterNodes(XdmValue input, Func<IXdmNode, bool> predicate)
    {
        var items = MaterializeSequence(input);
        var filtered = new List<XdmValue>();
        foreach (var item in items)
        {
            if (item.IsNode && predicate(item.NodeValue))
                filtered.Add(item);
        }
        return XdmValue.FromSequence(MaterializedSequence.FromList(filtered));
    }

    private static bool MatchesKindTest(IXdmNode node, string kindName)
    {
        return kindName.ToLowerInvariant() switch
        {
            "node" => true,
            "text" => node.NodeKind == XdmNodeKind.Text,
            "comment" => node.NodeKind == XdmNodeKind.Comment,
            "processing-instruction" => node.NodeKind == XdmNodeKind.ProcessingInstruction,
            "element" => node.NodeKind == XdmNodeKind.Element,
            "attribute" => node.NodeKind == XdmNodeKind.Attribute,
            "document-node" => node.NodeKind == XdmNodeKind.Document,
            "namespace-node" => node.NodeKind == XdmNodeKind.Namespace,
            // Schema-aware kind tests are XPST0008 (no schema awareness); a prefixed
            // name argument has already been namespace-checked by a preceding
            // NamespaceTest opcode (XPST0081 for unbound prefixes, K2-NameTest-35/36).
            "schema-element" or "schema-attribute" =>
                throw new InvalidOperationException("XPST0008: Schema-aware kind tests are not supported (no schema awareness)."),
            _ => true // permissive fallback
        };
    }

    // ------------------------------------------------------------------
    // Arithmetic
    // ------------------------------------------------------------------

    private static bool IsEmptySequence(XdmValue value)
    {
        if (value.IsUndefined)
            return true;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var _ in XdmSequence.FromSource(value.SequenceValue))
                return false;
            return true;
        }
        return false;
    }

    private static XdmValue Add(XdmValue left, XdmValue right, EvaluationContext context)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
        {
            if (context.BackwardsCompatible)
                return XdmValue.FromDouble(double.NaN);
            return XdmValue.Undefined;
        }

        // XPath 1.0 backwards compatibility: arithmetic always returns an xs:double.
        if (context.BackwardsCompatible)
            return XdmValue.FromDouble(ToDoubleOrNaN(left) + ToDoubleOrNaN(right));

        // Date/Time + Duration
        if ((left.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time) && (right.Kind == XdmValueKind.String || right.Kind == XdmValueKind.Duration))
        {
            RequireProperDurationSubtype(right);
            return AddDuration(left, right.ToString());
        }
        if ((right.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time) && (left.Kind == XdmValueKind.String || left.Kind == XdmValueKind.Duration))
        {
            RequireProperDurationSubtype(left);
            return AddDuration(right, left.ToString());
        }

        // Duration + Duration
        if (left.Kind == XdmValueKind.Duration && right.Kind == XdmValueKind.Duration)
        {
            RequireProperDurationSubtype(left);
            RequireProperDurationSubtype(right);
            return AddDurations(left, right);
        }

        ValidateNumericOperand(left);
        ValidateNumericOperand(right);

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
            return XdmValue.FromDouble(ToDouble(left) + ToDouble(right));

        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) + ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) + ToFloat(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) + ToDecimal(right));

        return MultiplyOrAddInteger(ToInteger(left), ToInteger(right), false);
    }

    // A duration operand in date/time arithmetic must be an xs:dayTimeDuration or
    // xs:yearMonthDuration — a plain xs:duration is a type error (cbcl-plus/minus-*).
    private static void RequireProperDurationSubtype(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Duration && GetDurationSubtype(value) == DurationSubtype.Duration)
            throw new InvalidOperationException("XPTY0004: A plain xs:duration value is not allowed in date/time arithmetic (xs:dayTimeDuration or xs:yearMonthDuration required).");
    }

    private static XdmValue AddDuration(XdmValue dateTimeValue, string duration)
    {
        bool hasTz = dateTimeValue.HasTimezone;
        var xdt = dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => dateTimeValue.DateTimeXPathValue,
            XdmValueKind.Date => dateTimeValue.DateXPathValue,
            XdmValueKind.Time => dateTimeValue.TimeXPathValue,
            _ => throw new InvalidOperationException("Expected date/time value")
        };
        int tzMinutes = xdt.TimezoneOffsetMinutes;
        bool isTime = dateTimeValue.Kind == XdmValueKind.Time;

        XPathDateTime result;
        if (IsYearMonthDurationString(duration))
        {
            if (isTime)
                throw new InvalidOperationException("XPTY0004: xs:time values do not support year-month duration arithmetic");

            var (years, months, _, _, _, _) = ParseDuration(duration);
            var (ny, nm, nd) = XPathDateTimeHelper.AddMonths(xdt.Year, xdt.Month, xdt.Day, years * 12 + months);
            result = new XPathDateTime(ny, nm, nd, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, tzMinutes, hasTz);
        }
        else if (IsDayTimeDurationString(duration))
        {
            result = AddDayTimeDuration(xdt, duration, isTime, tzMinutes, hasTz);
            if (dateTimeValue.Kind == XdmValueKind.Date)
                result = new XPathDateTime(result.Year, result.Month, result.Day, 0, 0, 0, 0, tzMinutes, hasTz);
        }
        else
        {
            throw new InvalidOperationException("Invalid duration format");
        }

        return dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => XdmValue.FromDateTime(result, hasTz),
            XdmValueKind.Date => XdmValue.FromDate(result, hasTz),
            XdmValueKind.Time => XdmValue.FromTime(result, hasTz),
            _ => throw new InvalidOperationException("Unexpected kind")
        };
    }

    private static XPathDateTime AddDayTimeDuration(XPathDateTime xdt, string duration, bool isTime, int tzMinutes, bool hasTz)
    {
        var (_, _, days, hours, minutes, seconds) = ParseDuration(duration);
        long deltaMs = ((days * 24L + hours) * 3600L + minutes * 60L) * 1000L + (long)(seconds * 1000m);
        long msOfDay = (xdt.Hour * 3600L + xdt.Minute * 60L + xdt.Second) * 1000L + xdt.Millisecond;
        if (isTime)
        {
            long totalMs = (msOfDay + deltaMs) % 86400000L;
            if (totalMs < 0) totalMs += 86400000L;
            return MsToTime(totalMs, tzMinutes, hasTz);
        }
        long totalMsFull = msOfDay + deltaMs;
        long dayOffset = totalMsFull / 86400000L;
        long newMsOfDay = totalMsFull % 86400000L;
        if (newMsOfDay < 0) { newMsOfDay += 86400000L; dayOffset--; }
        var (ny, nm, nd) = XPathDateTimeHelper.CivilFromDays(XPathDateTimeHelper.DaysFromCivil(xdt.Year, xdt.Month, xdt.Day) + dayOffset);
        return MsToDateTime(ny, nm, nd, newMsOfDay, tzMinutes, hasTz);
    }

    private static XPathDateTime MsToTime(long totalMs, int tzMinutes, bool hasTz)
    {
        int hour = (int)(totalMs / 3600000L); totalMs %= 3600000L;
        int minute = (int)(totalMs / 60000L); totalMs %= 60000L;
        int second = (int)(totalMs / 1000L);
        int ms = (int)(totalMs % 1000L);
        return new XPathDateTime(1, 1, 1, hour, minute, second, ms, tzMinutes, hasTz);
    }

    private static XPathDateTime MsToDateTime(long year, int month, int day, long totalMs, int tzMinutes, bool hasTz)
    {
        int hour = (int)(totalMs / 3600000L); totalMs %= 3600000L;
        int minute = (int)(totalMs / 60000L); totalMs %= 60000L;
        int second = (int)(totalMs / 1000L);
        int ms = (int)(totalMs % 1000L);
        return new XPathDateTime(year, month, day, hour, minute, second, ms, tzMinutes, hasTz);
    }

    private static XdmValue Subtract(XdmValue left, XdmValue right, EvaluationContext context)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
        {
            if (context.BackwardsCompatible)
                return XdmValue.FromDouble(double.NaN);
            return XdmValue.Undefined;
        }

        // XPath 1.0 backwards compatibility: arithmetic always returns an xs:double.
        if (context.BackwardsCompatible)
            return XdmValue.FromDouble(ToDoubleOrNaN(left) - ToDoubleOrNaN(right));

        if (left.Kind == XdmValueKind.Date && right.Kind == XdmValueKind.Date)
            return XdmValue.FromDuration(FormatDurationFromDateTimeDiff(left.DateXPathValue, right.DateXPathValue));
        if (left.Kind == XdmValueKind.DateTime && right.Kind == XdmValueKind.DateTime)
            return XdmValue.FromDuration(FormatDurationFromDateTimeDiff(left.DateTimeXPathValue, right.DateTimeXPathValue));
        if (left.Kind == XdmValueKind.Time && right.Kind == XdmValueKind.Time)
        {
            var leftDt = left.TimeXPathValue;
            var rightDt = right.TimeXPathValue;
            var leftRef = new XPathDateTime(1972, 12, 31, leftDt.Hour, leftDt.Minute, leftDt.Second, leftDt.Millisecond, leftDt.TimezoneOffsetMinutes, left.HasTimezone);
            var rightRef = new XPathDateTime(1972, 12, 31, rightDt.Hour, rightDt.Minute, rightDt.Second, rightDt.Millisecond, rightDt.TimezoneOffsetMinutes, right.HasTimezone);
            return XdmValue.FromDuration(FormatDurationFromDateTimeDiff(leftRef, rightRef));
        }
        if (left.Kind is XdmValueKind.DateTime or XdmValueKind.Date or XdmValueKind.Time && (right.Kind == XdmValueKind.String || right.Kind == XdmValueKind.Duration))
        {
            RequireProperDurationSubtype(right);
            return SubtractDuration(left, right.ToString());
        }

        // Duration - Duration
        if (left.Kind == XdmValueKind.Duration && right.Kind == XdmValueKind.Duration)
        {
            RequireProperDurationSubtype(left);
            RequireProperDurationSubtype(right);
            return SubtractDurations(left, right);
        }

        ValidateNumericOperand(left);
        ValidateNumericOperand(right);

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
            return XdmValue.FromDouble(ToDouble(left) - ToDouble(right));

        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) - ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) - ToFloat(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) - ToDecimal(right));

        return MultiplyOrAddInteger(ToInteger(left), -ToInteger(right), false);
    }

    private static string FormatDurationFromDateTimeDiff(XPathDateTime left, XPathDateTime right)
    {
        var ul = XPathDateTimeHelper.NormalizeToUtc(left);
        var ur = XPathDateTimeHelper.NormalizeToUtc(right);
        decimal msL = (decimal)XPathDateTimeHelper.DaysFromCivil(ul.Year, ul.Month, ul.Day) * 86400000m
            + ((ul.Hour * 3600m + ul.Minute * 60m + ul.Second) * 1000m + ul.Millisecond);
        decimal msR = (decimal)XPathDateTimeHelper.DaysFromCivil(ur.Year, ur.Month, ur.Day) * 86400000m
            + ((ur.Hour * 3600m + ur.Minute * 60m + ur.Second) * 1000m + ur.Millisecond);
        return FormatDurationFromMilliseconds(msL - msR);
    }

    private static XdmValue SubtractDuration(XdmValue dateTimeValue, string duration)
    {
        bool hasTz = dateTimeValue.HasTimezone;
        var xdt = dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => dateTimeValue.DateTimeXPathValue,
            XdmValueKind.Date => dateTimeValue.DateXPathValue,
            XdmValueKind.Time => dateTimeValue.TimeXPathValue,
            _ => throw new InvalidOperationException("Expected date/time value")
        };
        int tzMinutes = xdt.TimezoneOffsetMinutes;
        bool isTime = dateTimeValue.Kind == XdmValueKind.Time;

        XPathDateTime result;
        if (IsYearMonthDurationString(duration))
        {
            if (isTime)
                throw new InvalidOperationException("XPTY0004: xs:time values do not support year-month duration arithmetic");

            var (years, months, _, _, _, _) = ParseDuration(duration);
            var (ny, nm, nd) = XPathDateTimeHelper.AddMonths(xdt.Year, xdt.Month, xdt.Day, -(years * 12 + months));
            result = new XPathDateTime(ny, nm, nd, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, tzMinutes, hasTz);
        }
        else if (IsDayTimeDurationString(duration))
        {
            var (_, _, days, hours, minutes, seconds) = ParseDuration(duration);
            long deltaMs = ((days * 24L + hours) * 3600L + minutes * 60L) * 1000L + (long)(seconds * 1000m);
            result = AddDayTimeDuration(xdt, $"-P{days}DT{hours}H{minutes}M{seconds}S", isTime, tzMinutes, hasTz);
            if (dateTimeValue.Kind == XdmValueKind.Date)
                result = new XPathDateTime(result.Year, result.Month, result.Day, 0, 0, 0, 0, tzMinutes, hasTz);
        }
        else
        {
            throw new InvalidOperationException("Invalid duration format");
        }

        return dateTimeValue.Kind switch
        {
            XdmValueKind.DateTime => XdmValue.FromDateTime(result, hasTz),
            XdmValueKind.Date => XdmValue.FromDate(result, hasTz),
            XdmValueKind.Time => XdmValue.FromTime(result, hasTz),
            _ => throw new InvalidOperationException("Unexpected kind")
        };
    }

    private static string FormatDuration(TimeSpan ts) => FormatDurationFromMilliseconds((decimal)ts.TotalMilliseconds);

    private static string FormatDurationFromMilliseconds(decimal totalMs)
    {
        if (totalMs == 0) return "PT0S";
        bool negative = totalMs < 0;
        decimal remaining = negative ? -totalMs : totalMs;
        long days = (long)(remaining / 86400000m);
        remaining -= (decimal)days * 86400000m;
        int hours = (int)(remaining / 3600000m);
        remaining -= hours * 3600000m;
        int minutes = (int)(remaining / 60000m);
        remaining -= minutes * 60000m;
        int seconds = (int)(remaining / 1000m);
        decimal frac = remaining - seconds * 1000m;
        decimal sec = seconds + frac / 1000m;
        return FormatDayTimeDurationParts(negative, days, hours, minutes, sec);
    }

    private static string FormatDayTimeDurationParts(bool negative, long days, int hours, int minutes, decimal seconds)
    {
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append('P');
        if (days > 0) sb.Append(days).Append('D');
        if (hours > 0 || minutes > 0 || seconds > 0)
        {
            sb.Append('T');
            if (hours > 0) sb.Append(hours).Append('H');
            if (minutes > 0) sb.Append(minutes).Append('M');
            if (seconds > 0 || (hours == 0 && minutes == 0))
            {
                sb.Append(FormatDecimalTrim(seconds)).Append('S');
            }
        }
        if (sb.Length == (negative ? 2 : 1)) sb.Append("T0S");
        return sb.ToString();
    }

    private static string FormatDecimalTrim(decimal value)
    {
        string s = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (s.Contains('.')) s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    private static (long Years, long Months, long Days, long Hours, long Minutes, decimal Seconds) ParseDuration(string s)
    {
        bool negative = s.StartsWith('-');
        s = negative ? s[1..] : s;
        if (!s.StartsWith('P')) return (0, 0, 0, 0, 0, 0m);
        s = s[1..];

        long years = 0, months = 0, days = 0, hours = 0, minutes = 0;
        decimal seconds = 0m;

        int tIndex = s.IndexOf('T');
        string datePart = tIndex >= 0 ? s[..tIndex] : s;
        string timePart = tIndex >= 0 ? s[(tIndex + 1)..] : string.Empty;

        years = ParseDurationNumber(ref datePart, 'Y');
        months = ParseDurationNumber(ref datePart, 'M');
        days = ParseDurationNumber(ref datePart, 'D');

        hours = ParseDurationNumber(ref timePart, 'H');
        minutes = ParseDurationNumber(ref timePart, 'M');
        seconds = ParseDurationDecimal(ref timePart, 'S');

        if (negative)
        {
            years = -years;
            months = -months;
            days = -days;
            hours = -hours;
            minutes = -minutes;
            seconds = -seconds;
        }

        return (years, months, days, hours, minutes, seconds);
    }

    private static long ParseDurationNumber(ref string s, char suffix)
    {
        int idx = s.IndexOf(suffix);
        if (idx < 0) return 0;
        var numStr = s[..idx];
        s = s[(idx + 1)..];
        return long.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    private static decimal ParseDurationDecimal(ref string s, char suffix)
    {
        int idx = s.IndexOf(suffix);
        if (idx < 0) return 0m;
        var numStr = s[..idx];
        s = s[(idx + 1)..];
        return decimal.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0m;
    }

    private static bool IsYearMonthDurationString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (s.StartsWith('-')) s = s[1..];
        return s.StartsWith('P') && !s.Contains('D') && !s.Contains('T');
    }

    private static bool IsDayTimeDurationString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (s.StartsWith('-')) s = s[1..];
        return s.StartsWith('P') && (s.Contains('D') || s.Contains('T'));
    }

    private static long RoundHalfUp(decimal value)
    {
        return (long)Math.Floor(value + 0.5m);
    }

    private enum DurationSubtype { YearMonthDuration, DayTimeDuration, Duration }

    private static DurationSubtype GetDurationSubtype(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return DurationSubtype.Duration;
        bool hasYm = m.Groups["Y"].Success || m.Groups["M"].Success;
        bool hasDt = m.Groups["D"].Success || m.Groups["T"].Success;
        if (hasYm && !hasDt) return DurationSubtype.YearMonthDuration;
        if (!hasYm && hasDt) return DurationSubtype.DayTimeDuration;
        return DurationSubtype.Duration;
    }

    private static DurationSubtype GetDurationSubtype(XdmValue value)
    {
        var schemaType = value.SchemaTypeName;
        if (schemaType is not null)
        {
            var normalized = schemaType.ToLowerInvariant().Replace("xs:", "");
            if (normalized == "yearmonthduration") return DurationSubtype.YearMonthDuration;
            if (normalized == "daytimeduration") return DurationSubtype.DayTimeDuration;
            if (normalized == "duration") return DurationSubtype.Duration;
        }
        return GetDurationSubtype(value.DurationValue);
    }

    private static string? GetDateTimeSubtype(XdmValue value)
    {
        return value.Kind switch
        {
            XdmValueKind.DateTime => "dateTime",
            XdmValueKind.Date => "date",
            XdmValueKind.Time => "time",
            XdmValueKind.String => value.SchemaTypeName?.ToLowerInvariant() switch
            {
                "gyear" => "gYear",
                "gyearmonth" => "gYearMonth",
                "gmonth" => "gMonth",
                "gmonthday" => "gMonthDay",
                "gday" => "gDay",
                _ => null
            },
            _ => null
        };
    }

    /// <summary>
    /// Compares two date/time values of the same subtype on the timeline.
    /// A value without a timezone is treated as having the supplied implicit timezone.
    /// Returns null when the comparison is indeterminate.
    /// </summary>
    public static int? CompareDateTimeValues(XdmValue left, XdmValue right, string subtype, int implicitTz)
    {
        var leftXdt = AsComparableDateTime(GetXPathDateTime(left, subtype), subtype);
        var rightXdt = AsComparableDateTime(GetXPathDateTime(right, subtype), subtype);

        bool leftHasTz = GetHasTimezone(left, subtype);
        bool rightHasTz = GetHasTimezone(right, subtype);

        // Neither has timezone: compare local components directly
        if (!leftHasTz && !rightHasTz)
        {
            return XPathDateTimeHelper.CompareComponents(leftXdt, rightXdt);
        }

        var leftEffective = leftHasTz
            ? leftXdt
            : new XPathDateTime(leftXdt.Year, leftXdt.Month, leftXdt.Day,
                leftXdt.Hour, leftXdt.Minute, leftXdt.Second, leftXdt.Millisecond,
                implicitTz, true);
        var rightEffective = rightHasTz
            ? rightXdt
            : new XPathDateTime(rightXdt.Year, rightXdt.Month, rightXdt.Day,
                rightXdt.Hour, rightXdt.Minute, rightXdt.Second, rightXdt.Millisecond,
                implicitTz, true);

        var leftUtc = XPathDateTimeHelper.NormalizeToUtc(leftEffective);
        var rightUtc = XPathDateTimeHelper.NormalizeToUtc(rightEffective);
        return XPathDateTimeHelper.CompareComponents(leftUtc, rightUtc);
    }

    private static int GetImplicitTimezoneOffsetMinutes(EvaluationContext context)
        => context.ImplicitTimezoneOffsetMinutes;

    private static XPathDateTime AsComparableDateTime(XPathDateTime xdt, string subtype)
    {
        if (subtype == "time")
        {
            // xs:time comparisons use the reference date 1972-12-31 (per XPath spec).
            return new XPathDateTime(1972, 12, 31, xdt.Hour, xdt.Minute, xdt.Second, xdt.Millisecond, xdt.TimezoneOffsetMinutes, xdt.HasTimezone);
        }
        if (subtype is "gYear" or "gYearMonth" or "gMonth" or "gMonthDay" or "gDay")
        {
            // ParseGDateTime already applies the correct reference components for each subtype.
            return xdt;
        }
        return xdt;
    }

    private static XPathDateTime GetXPathDateTime(XdmValue value, string subtype)
    {
        return subtype switch
        {
            "dateTime" => value.DateTimeXPathValue,
            "date" => value.DateXPathValue,
            "time" => value.TimeXPathValue,
            "gYear" or "gYearMonth" or "gMonth" or "gMonthDay" or "gDay" => ParseGDateTime(value.ToString(), subtype).Xdt,
            _ => throw new InvalidOperationException($"Unsupported date/time subtype: {subtype}")
        };
    }

    private static bool GetHasTimezone(XdmValue value, string subtype)
    {
        return subtype switch
        {
            "dateTime" => value.HasTimezone,
            "date" => value.HasTimezone,
            "time" => value.HasTimezone,
            "gYear" or "gYearMonth" or "gMonth" or "gMonthDay" or "gDay" => ParseGDateTime(value.ToString(), subtype).HasTz,
            _ => throw new InvalidOperationException($"Unsupported date/time subtype: {subtype}")
        };
    }

    private static (XPathDateTime Xdt, bool HasTz) ParseGDateTime(string s, string subtype)
    {
        int year = 1972, month = 1, day = 1;
        int tz = 0;
        bool hasTz = false;

        s = s.Trim();
        string tzStr = "";

        switch (subtype)
        {
            case "gYear":
                var gYearMatch = Regex.Match(s, @"^(-?\d{4,})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                if (gYearMatch.Success)
                {
                    year = int.Parse(gYearMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    tzStr = NormalizeTimezone(gYearMatch.Groups[2].Value) ?? "";
                }
                break;
            case "gYearMonth":
                var gYearMonthMatch = Regex.Match(s, @"^(-?\d{4,})-(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                if (gYearMonthMatch.Success)
                {
                    year = int.Parse(gYearMonthMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    month = int.Parse(gYearMonthMatch.Groups[2].Value, CultureInfo.InvariantCulture);
                    tzStr = NormalizeTimezone(gYearMonthMatch.Groups[3].Value) ?? "";
                }
                break;
            case "gMonth":
                var gMonthMatch = Regex.Match(s, @"^--(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                if (gMonthMatch.Success)
                {
                    month = int.Parse(gMonthMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    tzStr = NormalizeTimezone(gMonthMatch.Groups[2].Value) ?? "";
                }
                break;
            case "gMonthDay":
                var gMonthDayMatch = Regex.Match(s, @"^--(\d{2})-(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                if (gMonthDayMatch.Success)
                {
                    month = int.Parse(gMonthDayMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    day = int.Parse(gMonthDayMatch.Groups[2].Value, CultureInfo.InvariantCulture);
                    tzStr = NormalizeTimezone(gMonthDayMatch.Groups[3].Value) ?? "";
                }
                break;
            case "gDay":
                var gDayMatch = Regex.Match(s, @"^---(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                if (gDayMatch.Success)
                {
                    day = int.Parse(gDayMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    tzStr = NormalizeTimezone(gDayMatch.Groups[2].Value) ?? "";
                }
                break;
        }

        hasTz = !string.IsNullOrEmpty(tzStr);
        if (hasTz)
            tz = tzStr == "Z" ? 0 : ParseTimezoneOffset(tzStr);

        return (new XPathDateTime(year, month, day, 0, 0, 0, 0, tz, hasTz), hasTz);
    }

    private static (long TotalMonths, decimal TotalSeconds) NormalizeDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return (0, 0);
        bool negative = m.Groups["sign"].Value == "-";

        long years = m.Groups["Y"].Success ? long.Parse(m.Groups["Y"].Value.TrimEnd('Y'), CultureInfo.InvariantCulture) : 0;
        long months = m.Groups["M"].Success ? long.Parse(m.Groups["M"].Value.TrimEnd('M'), CultureInfo.InvariantCulture) : 0;
        long days = m.Groups["D"].Success ? long.Parse(m.Groups["D"].Value.TrimEnd('D'), CultureInfo.InvariantCulture) : 0;
        long hours = m.Groups["H"].Success ? long.Parse(m.Groups["H"].Value.TrimEnd('H'), CultureInfo.InvariantCulture) : 0;
        long minutes = m.Groups["Tm"].Success ? long.Parse(m.Groups["Tm"].Value.TrimEnd('M'), CultureInfo.InvariantCulture) : 0;
        decimal seconds = m.Groups["S"].Success ? decimal.Parse(m.Groups["S"].Value.TrimEnd('S'), CultureInfo.InvariantCulture) : 0;

        long totalMonths = years * 12 + months;
        decimal totalSeconds = days * 86400m + hours * 3600m + minutes * 60m + seconds;

        if (negative)
        {
            totalMonths = -totalMonths;
            totalSeconds = -totalSeconds;
        }

        return (totalMonths, totalSeconds);
    }

    private static XdmValue AddDurations(XdmValue left, XdmValue right)
    {
        var l = left.DurationValue;
        var r = right.DurationValue;
        if (IsYearMonthDurationString(l) && IsYearMonthDurationString(r))
        {
            var (y1, m1, _, _, _, _) = ParseDuration(l);
            var (y2, m2, _, _, _, _) = ParseDuration(r);
            long totalMonths = y1 * 12 + m1 + y2 * 12 + m2;
            return XdmValue.FromDuration(FormatYearMonthDuration(totalMonths));
        }
        if (IsDayTimeDurationString(l) && IsDayTimeDurationString(r))
        {
            var (_, _, d1, h1, min1, s1) = ParseDuration(l);
            var (_, _, d2, h2, min2, s2) = ParseDuration(r);
            long totalTicks = (d1 + d2) * TimeSpan.TicksPerDay
                + (h1 + h2) * TimeSpan.TicksPerHour
                + (min1 + min2) * TimeSpan.TicksPerMinute
                + (long)((s1 + s2) * TimeSpan.TicksPerSecond);
            return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue SubtractDurations(XdmValue left, XdmValue right)
    {
        var l = left.DurationValue;
        var r = right.DurationValue;
        if (IsYearMonthDurationString(l) && IsYearMonthDurationString(r))
        {
            var (y1, m1, _, _, _, _) = ParseDuration(l);
            var (y2, m2, _, _, _, _) = ParseDuration(r);
            long totalMonths = y1 * 12 + m1 - (y2 * 12 + m2);
            return XdmValue.FromDuration(FormatYearMonthDuration(totalMonths));
        }
        if (IsDayTimeDurationString(l) && IsDayTimeDurationString(r))
        {
            var (_, _, d1, h1, min1, s1) = ParseDuration(l);
            var (_, _, d2, h2, min2, s2) = ParseDuration(r);
            long totalTicks = (d1 - d2) * TimeSpan.TicksPerDay
                + (h1 - h2) * TimeSpan.TicksPerHour
                + (min1 - min2) * TimeSpan.TicksPerMinute
                + (long)((s1 - s2) * TimeSpan.TicksPerSecond);
            return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue MultiplyDuration(XdmValue duration, XdmValue factor)
    {
        double f = ToDouble(factor);
        if (double.IsNaN(f) || double.IsInfinity(f))
            throw new InvalidOperationException("FOCA0005");

        var subtype = GetDurationSubtype(duration);
        if (subtype == DurationSubtype.YearMonthDuration)
        {
            var d = duration.DurationValue;
            var (y, m, _, _, _, _) = ParseDuration(d);
            decimal baseMonths = y * 12m + m;
            if (baseMonths == 0m)
                return XdmValue.FromDuration("P0M");
            decimal totalMonths = baseMonths * (decimal)f;
            long roundedMonths;
            try
            {
                roundedMonths = RoundHalfUp(totalMonths);
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException("FODT0002");
            }
            return XdmValue.FromDuration(FormatYearMonthDuration(roundedMonths));
        }
        if (subtype == DurationSubtype.DayTimeDuration)
        {
            var d = duration.DurationValue;
            var (_, _, days, hours, minutes, seconds) = ParseDuration(d);
            decimal totalSeconds = days * 86400m + hours * 3600m + minutes * 60m + seconds;
            if (totalSeconds == 0m)
                return XdmValue.FromDuration("PT0S");
            try
            {
                decimal resultSeconds = totalSeconds * (decimal)f;
                long totalTicks = (long)(resultSeconds * TimeSpan.TicksPerSecond);
                return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException("FODT0002");
            }
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue DivideDuration(XdmValue duration, XdmValue divisor)
    {
        double div = ToDouble(divisor);
        var subtype = GetDurationSubtype(duration);
        if (subtype == DurationSubtype.YearMonthDuration)
        {
            if (double.IsNaN(div))
                throw new InvalidOperationException("FOCA0005");
            if (div == 0.0)
                throw new InvalidOperationException("FODT0002");
            if (double.IsInfinity(div))
                return XdmValue.FromDuration("P0M");
            var d = duration.DurationValue;
            var (y, m, _, _, _, _) = ParseDuration(d);
            decimal baseMonths = y * 12m + m;
            if (baseMonths == 0m)
                return XdmValue.FromDuration("P0M");
            long roundedMonths;
            try
            {
                decimal totalMonths = baseMonths / (decimal)div;
                roundedMonths = RoundHalfUp(totalMonths);
            }
            catch (OverflowException)
            {
                double resultMonthsD = (double)baseMonths / div;
                if (Math.Abs(resultMonthsD) < 0.5)
                    return XdmValue.FromDuration("P0M");
                throw new InvalidOperationException("FODT0002");
            }
            catch (DivideByZeroException)
            {
                double resultMonthsD = (double)baseMonths / div;
                if (Math.Abs(resultMonthsD) < 0.5)
                    return XdmValue.FromDuration("P0M");
                if (Math.Abs(resultMonthsD) > (double)decimal.MaxValue || double.IsInfinity(resultMonthsD))
                    throw new InvalidOperationException("FODT0002");
                roundedMonths = RoundHalfUp((decimal)resultMonthsD);
            }
            return XdmValue.FromDuration(FormatYearMonthDuration(roundedMonths));
        }
        if (subtype == DurationSubtype.DayTimeDuration)
        {
            if (double.IsNaN(div))
                throw new InvalidOperationException("FOCA0005");
            if (div == 0.0)
                throw new InvalidOperationException("FODT0002");
            if (double.IsInfinity(div))
                return XdmValue.FromDuration("PT0S");
            var d = duration.DurationValue;
            var (_, _, days, hours, minutes, seconds) = ParseDuration(d);
            decimal totalSeconds = days * 86400m + hours * 3600m + minutes * 60m + seconds;
            if (totalSeconds == 0m)
                return XdmValue.FromDuration("PT0S");
            try
            {
                decimal resultSeconds = totalSeconds / (decimal)div;
                long totalTicks = (long)(resultSeconds * TimeSpan.TicksPerSecond);
                return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
            }
            catch (OverflowException)
            {
                double resultSecondsD = (double)totalSeconds / div;
                if (Math.Abs(resultSecondsD) * TimeSpan.TicksPerSecond < 0.5)
                    return XdmValue.FromDuration("PT0S");
                throw new InvalidOperationException("FODT0002");
            }
            catch (DivideByZeroException)
            {
                double resultSecondsD = (double)totalSeconds / div;
                if (Math.Abs(resultSecondsD) * TimeSpan.TicksPerSecond < 0.5)
                    return XdmValue.FromDuration("PT0S");
                if (Math.Abs(resultSecondsD) > (double)decimal.MaxValue || double.IsInfinity(resultSecondsD))
                    throw new InvalidOperationException("FODT0002");
                long totalTicks = (long)((decimal)resultSecondsD * TimeSpan.TicksPerSecond);
                return XdmValue.FromDuration(FormatDuration(new TimeSpan(totalTicks)));
            }
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static string FormatYearMonthDuration(long totalMonths)
    {
        bool negative = totalMonths < 0;
        totalMonths = Math.Abs(totalMonths);
        long years = totalMonths / 12;
        long months = totalMonths % 12;
        var sb = new System.Text.StringBuilder();
        if (negative) sb.Append('-');
        sb.Append('P');
        if (years > 0) sb.Append($"{years}Y");
        if (months > 0) sb.Append($"{months}M");
        if (years == 0 && months == 0) sb.Append("0M");
        return sb.ToString();
    }

    private static XdmValue DivideDurationByDuration(XdmValue left, XdmValue right)
    {
        var l = left.DurationValue;
        var r = right.DurationValue;
        if (IsYearMonthDurationString(l) && IsYearMonthDurationString(r))
        {
            var (y1, m1, _, _, _, _) = ParseDuration(l);
            var (y2, m2, _, _, _, _) = ParseDuration(r);
            decimal totalMonths1 = y1 * 12m + m1;
            decimal totalMonths2 = y2 * 12m + m2;
            if (totalMonths2 == 0) throw new InvalidOperationException("FODT0002");
            return XdmValue.FromDecimal(totalMonths1 / totalMonths2);
        }
        if (IsDayTimeDurationString(l) && IsDayTimeDurationString(r))
        {
            var (_, _, d1, h1, min1, s1) = ParseDuration(l);
            var (_, _, d2, h2, min2, s2) = ParseDuration(r);
            decimal totalSeconds1 = d1 * 86400m + h1 * 3600m + min1 * 60m + s1;
            decimal totalSeconds2 = d2 * 86400m + h2 * 3600m + min2 * 60m + s2;
            if (totalSeconds2 == 0) throw new InvalidOperationException("FODT0002");
            return XdmValue.FromDecimal(totalSeconds1 / totalSeconds2);
        }
        throw new InvalidOperationException("XPTY0004");
    }

    private static XdmValue Multiply(XdmValue left, XdmValue right, EvaluationContext context)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
        {
            if (context.BackwardsCompatible)
                return XdmValue.FromDouble(double.NaN);
            return XdmValue.Undefined;
        }

        // XPath 1.0 backwards compatibility: arithmetic always returns an xs:double.
        if (context.BackwardsCompatible)
            return XdmValue.FromDouble(ToDoubleOrNaN(left) * ToDoubleOrNaN(right));

        // Duration * number or number * Duration
        if (left.Kind == XdmValueKind.Duration)
            return MultiplyDuration(left, right);
        if (right.Kind == XdmValueKind.Duration)
            return MultiplyDuration(right, left);

        ValidateNumericOperand(left);
        ValidateNumericOperand(right);

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
            return XdmValue.FromDouble(ToDouble(left) * ToDouble(right));

        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) * ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) * ToFloat(right));
        if (IsDecimal(left) || IsDecimal(right))
            return XdmValue.FromDecimal(ToDecimal(left) * ToDecimal(right));

        return MultiplyOrAddInteger(ToInteger(left), ToInteger(right), true);
    }

    private static XdmValue Divide(XdmValue left, XdmValue right, EvaluationContext context)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
        {
            if (context.BackwardsCompatible)
                return XdmValue.FromDouble(double.NaN);
            return XdmValue.Undefined;
        }

        // XPath 1.0 backwards compatibility: arithmetic always returns an xs:double.
        if (context.BackwardsCompatible)
            return XdmValue.FromDouble(ToDoubleOrNaN(left) / ToDoubleOrNaN(right));

        // Duration div number
        if (left.Kind == XdmValueKind.Duration && !IsDuration(right))
        {
            RequireProperDurationSubtype(left);
            return DivideDuration(left, right);
        }

        // Duration div Duration
        if (left.Kind == XdmValueKind.Duration && right.Kind == XdmValueKind.Duration)
        {
            RequireProperDurationSubtype(left);
            RequireProperDurationSubtype(right);
            return DivideDurationByDuration(left, right);
        }

        ValidateNumericOperand(left);
        ValidateNumericOperand(right);

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
            return XdmValue.FromDouble(ToDouble(left) / ToDouble(right));

        if (IsDouble(left) || IsDouble(right))
            return XdmValue.FromDouble(ToDouble(left) / ToDouble(right));
        if (IsFloat(left) || IsFloat(right))
            return XdmValue.FromFloat(ToFloat(left) / ToFloat(right));

        var divisor = ToDecimal(right);
        if (divisor == 0)
            throw new InvalidOperationException("FOAR0001: Division by zero.");
        return XdmValue.FromDecimal(ToDecimal(left) / divisor);
    }

    private static XdmValue IntegerDivide(XdmValue left, XdmValue right, EvaluationContext context)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
        {
            if (context.BackwardsCompatible)
                return XdmValue.FromDouble(double.NaN);
            return XdmValue.Undefined;
        }

        // XPath 1.0 backwards compatibility: integer division returns xs:integer.
        if (context.BackwardsCompatible)
            return XdmValue.FromInteger((long)(ToDoubleOrNaN(left) / ToDoubleOrNaN(right)));

        ValidateNumericOperand(left);
        ValidateNumericOperand(right);

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
        {
            double l = ToDouble(left);
            double r = ToDouble(right);
            if (double.IsNaN(l) || double.IsNaN(r) || double.IsInfinity(l))
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            if (double.IsInfinity(r))
                return XdmValue.FromInteger(0L);
            if (r == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            double result = l / r;
            if (double.IsNaN(result) || double.IsInfinity(result))
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            // The xs:integer result is backed by long; a quotient beyond its range is
            // an overflow (cbcl-numeric-idivide-002).
            if (result >= 9223372036854775808.0 || result < -9223372036854775808.0)
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            return XdmValue.FromInteger((long)result);
        }

        if (IsDouble(left) || IsDouble(right))
        {
            double l = ToDouble(left);
            double r = ToDouble(right);
            if (double.IsNaN(l) || double.IsNaN(r) || double.IsInfinity(l))
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            if (double.IsInfinity(r))
                return XdmValue.FromInteger(0L);
            if (r == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            double result = l / r;
            if (double.IsNaN(result) || double.IsInfinity(result))
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            // The xs:integer result is backed by long; a quotient beyond its range is
            // an overflow (cbcl-numeric-idivide-002).
            if (result >= 9223372036854775808.0 || result < -9223372036854775808.0)
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            return XdmValue.FromInteger((long)result);
        }

        if (IsFloat(left) || IsFloat(right))
        {
            float l = ToFloat(left);
            float r = ToFloat(right);
            if (float.IsNaN(l) || float.IsNaN(r) || float.IsInfinity(l))
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            if (float.IsInfinity(r))
                return XdmValue.FromInteger(0L);
            if (r == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            float result = l / r;
            if (float.IsNaN(result) || float.IsInfinity(result))
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            if (result >= 9223372036854775808.0 || result < -9223372036854775808.0)
                throw new InvalidOperationException("FOAR0002: Integer division overflow.");
            return XdmValue.FromInteger((long)result);
        }

        if (IsDecimal(left) || IsDecimal(right))
        {
            if (ToDecimal(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromInteger((long)(ToDecimal(left) / ToDecimal(right)));
        }

        if (ToInteger(right) == 0)
            throw new InvalidOperationException("FOAR0001: Division by zero.");
        return XdmValue.FromInteger(ToInteger(left) / ToInteger(right));
    }

    private static XdmValue Modulo(XdmValue left, XdmValue right, EvaluationContext context)
    {
        if (IsEmptySequence(left) || IsEmptySequence(right))
        {
            if (context.BackwardsCompatible)
                return XdmValue.FromDouble(double.NaN);
            return XdmValue.Undefined;
        }

        // XPath 1.0 backwards compatibility: arithmetic always returns an xs:double.
        if (context.BackwardsCompatible)
            return XdmValue.FromDouble(ToDoubleOrNaN(left) % ToDoubleOrNaN(right));

        ValidateNumericOperand(left);
        ValidateNumericOperand(right);

        left = Atomize(left);
        right = Atomize(right);
        if (IsUntypedAtomic(left) || IsUntypedAtomic(right))
        {
            if (ToDouble(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromDouble(ToDouble(left) % ToDouble(right));
        }

        if (IsDouble(left) || IsDouble(right))
        {
            // IEEE 754 semantics: floating-point mod by zero returns NaN, not FOAR0001.
            return XdmValue.FromDouble(ToDouble(left) % ToDouble(right));
        }
        if (IsFloat(left) || IsFloat(right))
        {
            // IEEE 754 semantics: floating-point mod by zero returns NaN, not FOAR0001.
            return XdmValue.FromFloat(ToFloat(left) % ToFloat(right));
        }
        if (IsDecimal(left) || IsDecimal(right))
        {
            if (ToDecimal(right) == 0)
                throw new InvalidOperationException("FOAR0001: Division by zero.");
            return XdmValue.FromDecimal(ToDecimal(left) % ToDecimal(right));
        }

        if (ToInteger(right) == 0)
            throw new InvalidOperationException("FOAR0001: Division by zero.");
        return XdmValue.FromInteger(ToInteger(left) % ToInteger(right));
    }

    private static XdmValue Negate(XdmValue value, EvaluationContext context)
    {
        if (IsEmptySequence(value))
        {
            if (context.BackwardsCompatible)
                return XdmValue.FromDouble(double.NaN);
            return XdmValue.Undefined;
        }

        if (context.BackwardsCompatible)
            return XdmValue.FromDouble(-ToDoubleOrNaN(value));

        // XPath 3.1 §3.1.5: an xs:untypedAtomic operand is converted to xs:double
        // before negation (op-numeric-unary-minus-1).
        if (IsUntypedAtomic(value))
            return XdmValue.FromDouble(-ToDouble(value));

        if (value.Kind == XdmValueKind.Duration)
        {
            var s = value.DurationValue;
            if (s.StartsWith('-'))
                return XdmValue.FromDuration(s[1..]);
            return XdmValue.FromDuration("-" + s);
        }
        ValidateNumericOperand(value);
        if (IsDouble(value))
            return XdmValue.FromDouble(-ToDouble(value));
        if (IsFloat(value))
            return XdmValue.FromFloat(-ToFloat(value));
        if (IsDecimal(value))
            return XdmValue.FromDecimal(-ToDecimal(value));
        return XdmValue.FromInteger(-ToInteger(value));
    }

    private static XdmValue UnaryPlus(XdmValue value, EvaluationContext context)
    {
        if (IsEmptySequence(value))
        {
            if (context.BackwardsCompatible)
                return XdmValue.FromDouble(double.NaN);
            return XdmValue.Undefined;
        }

        if (context.BackwardsCompatible)
            return XdmValue.FromDouble(ToDoubleOrNaN(value));

        if (IsUntypedAtomic(value))
            return XdmValue.FromDouble(ToDouble(value));

        ValidateNumericOperand(value);
        return value;
    }

    /// <summary>
    /// Performs integer multiplication or addition with overflow detection.
    /// If the result overflows <see cref="long"/>, promotes to <see cref="decimal"/>.
    /// </summary>
    private static XdmValue MultiplyOrAddInteger(long a, long b, bool multiply)
    {
        try
        {
            checked
            {
                long result = multiply ? a * b : a + b;
                return XdmValue.FromInteger(result);
            }
        }
        catch (OverflowException)
        {
            return XdmValue.FromDecimal(multiply ? (decimal)a * (decimal)b : (decimal)a + (decimal)b);
        }
    }

    // ------------------------------------------------------------------
    // Comparisons
    // ------------------------------------------------------------------

    private static XdmValue Compare(IrOpCode op, XdmValue left, XdmValue right, EvaluationContext context, bool strict = true)
    {
        bool leftFromNode = IsNodeOrigin(left);
        bool rightFromNode = IsNodeOrigin(right);

        // Value comparisons (eq/ne/lt/... and their value-comparison opcodes) require
        // each operand to be a singleton after atomization.
        if (strict && (SequenceLength(left) > 1 || SequenceLength(right) > 1))
            throw new InvalidOperationException("XPTY0004: Value comparison requires singleton operands");

        left = Atomize(left);
        right = Atomize(right);

        // Function items cannot be atomized: any comparison involving one is FOTY0013
        // (function-item-4 — string-join#1 eq string-join#1).
        if (left.IsFunction || right.IsFunction)
            throw new InvalidOperationException("FOTY0013: A comparison operand must not be a function item.");

        // XPath value comparisons with empty sequence operand return empty sequence
        if (left.IsUndefined || right.IsUndefined)
            return XdmValue.Undefined;

        // XPath 3.1 §17.2: in a value comparison, an xs:untypedAtomic operand is
        // cast to xs:string before the comparison proceeds, unless the other operand
        // is an xs:QName, in which case the untypedAtomic value is cast to xs:QName.
        if (strict)
        {
            if (IsUntypedAtomic(left) && right.Kind != XdmValueKind.QName)
            {
                left = XdmValue.FromString(left.StringValue);
                leftFromNode = false;
            }
            if (IsUntypedAtomic(right) && left.Kind != XdmValueKind.QName)
            {
                right = XdmValue.FromString(right.StringValue);
                rightFromNode = false;
            }
        }

        return XdmValue.FromBoolean(CompareCore(op, left, right, strict, leftFromNode, rightFromNode, context));
    }

    /// <summary>
    /// Returns the number of items in <paramref name="value"/> if it is a sequence,
    /// or 1 for any other defined value, or 0 for undefined.
    /// </summary>
    private static int SequenceLength(XdmValue value)
    {
        if (value.IsUndefined)
            return 0;
        if (!value.IsSequence)
            return 1;
        return MaterializeSequence(value).Length;
    }

    private static int CompareStrings(string left, string right, string collation, EvaluationContext context)
        => context.CollationComparer?.Invoke(left, right, collation)
           ?? string.CompareOrdinal(left, right);

    private static bool CompareCore(IrOpCode op, XdmValue left, XdmValue right, bool strict, bool leftFromNode, bool rightFromNode, EvaluationContext context)
    {
        // After untypedAtomic operands have been cast (xs:string for value comparisons,
        // per-type for general comparisons), a remaining xs:string operand has no valid
        // operator mapping against a numeric operand: type error XPTY0004.
        if (strict && ((left.Kind == XdmValueKind.String && IsNumeric(right))
            || (right.Kind == XdmValueKind.String && IsNumeric(left))))
            throw new InvalidOperationException(
                "XPTY0004: Comparison between xs:string and numeric operands is not defined");

        if (IsDouble(left) || IsDouble(right))
        {
            double l = ToDouble(left);
            double r = ToDouble(right);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => l < r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => l <= r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l > r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l >= r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        if (IsFloat(left) || IsFloat(right))
        {
            float l = ToFloat(left);
            float r = ToFloat(right);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => l < r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => l <= r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l > r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l >= r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        if (IsDecimal(left) || IsDecimal(right))
        {
            decimal l = ToDecimal(left);
            decimal r = ToDecimal(right);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => l < r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => l <= r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l > r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l >= r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        if (left.Kind == XdmValueKind.Integer && right.Kind == XdmValueKind.Integer)
        {
            long l = left.IntegerValue;
            long r = right.IntegerValue;
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => l < r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => l <= r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l > r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l >= r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        if (left.Kind == XdmValueKind.Boolean && right.Kind == XdmValueKind.Boolean)
        {
            bool l = left.BooleanValue;
            bool r = right.BooleanValue;
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => l == r,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => l != r,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => !l && r,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => !l || r,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => l && !r,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => l || !r,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        // QName comparison: prefix is ignored; only namespace URI and local name matter.
        // Ordering comparisons are not defined for QNames.
        if (left.Kind == XdmValueKind.QName && right.Kind == XdmValueKind.QName)
        {
            if (op is IrOpCode.LessThan or IrOpCode.ValueLessThan
                or IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual
                or IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan
                or IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual)
            {
                throw new InvalidOperationException("XPTY0004: Ordering comparison is not defined for xs:QName values.");
            }
            bool eq = left.QNameValue.Equals(right.QNameValue);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => eq,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => !eq,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        // General comparison promotion: an xs:untypedAtomic operand is cast to the
        // type of the other operand. When that type is xs:QName, resolve the lexical
        // QName using the static namespace context of the expression.
        if (left.Kind == XdmValueKind.QName && IsUntypedAtomic(right))
        {
            return CompareCore(op, left, CastUntypedAtomicToQName(right, context), strict, leftFromNode, false, context);
        }
        if (IsUntypedAtomic(left) && right.Kind == XdmValueKind.QName)
        {
            return CompareCore(op, CastUntypedAtomicToQName(left, context), right, strict, false, rightFromNode, context);
        }

        // Duration comparison: normalize to total months and total seconds
        if (left.Kind == XdmValueKind.Duration && right.Kind == XdmValueKind.Duration)
        {
            var (lMonths, lSeconds) = NormalizeDuration(left.DurationValue);
            var (rMonths, rSeconds) = NormalizeDuration(right.DurationValue);
            // The dynamic subtype (schema annotation) decides orderability: a plain
            // xs:duration is unordered even when its value carries only year/month or
            // only day/time parts (cbcl-value-greater-than-002/006/010).
            var lSub = GetDurationSubtype(left);
            var rSub = GetDurationSubtype(right);

            bool isEquality = op is IrOpCode.Equal or IrOpCode.ValueEqual
                              or IrOpCode.NotEqual or IrOpCode.ValueNotEqual;
            if (isEquality)
            {
                bool eq = lMonths == rMonths && lSeconds == rSeconds;
                return op switch
                {
                    IrOpCode.Equal or IrOpCode.ValueEqual => eq,
                    IrOpCode.NotEqual or IrOpCode.ValueNotEqual => !eq,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }

            // Ordering is only defined when both operands are the same subtype
            if (lSub == DurationSubtype.YearMonthDuration && rSub == DurationSubtype.YearMonthDuration)
            {
                int cmp = lMonths.CompareTo(rMonths);
                return op switch
                {
                    IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp < 0,
                    IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp <= 0,
                    IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp > 0,
                    IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp >= 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }
            if (lSub == DurationSubtype.DayTimeDuration && rSub == DurationSubtype.DayTimeDuration)
            {
                int cmp = lSeconds.CompareTo(rSeconds);
                return op switch
                {
                    IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp < 0,
                    IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp <= 0,
                    IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp > 0,
                    IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp >= 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }

            throw new InvalidOperationException("XPTY0004");
        }

        // Date/time comparison: only defined between operands of the same subtype
        string? leftDateSub = GetDateTimeSubtype(left);
        string? rightDateSub = GetDateTimeSubtype(right);
        if (leftDateSub is not null || rightDateSub is not null)
        {
            if (leftDateSub is null || rightDateSub is null || leftDateSub != rightDateSub)
            {
                // XPath general comparison promotion: an atomized untypedAtomic value
                // (typically from a node) is cast to the date/time subtype of the other
                // operand when the types would otherwise be incompatible.
                if (leftDateSub is not null && right.Kind == XdmValueKind.String && right.SchemaTypeName == "untypedAtomic" && TryCast(right, leftDateSub, out var castedRight))
                {
                    right = castedRight;
                    rightDateSub = leftDateSub;
                }
                else if (rightDateSub is not null && left.Kind == XdmValueKind.String && left.SchemaTypeName == "untypedAtomic" && TryCast(left, rightDateSub, out var castedLeft))
                {
                    left = castedLeft;
                    leftDateSub = rightDateSub;
                }
                else
                {
                    throw new InvalidOperationException("XPTY0004");
                }
            }
            // gYear/gYearMonth/gMonth/gMonthDay/gDay only support equality.
            if (leftDateSub is "gYear" or "gYearMonth" or "gMonth" or "gMonthDay" or "gDay")
            {
                if (op is IrOpCode.LessThan or IrOpCode.ValueLessThan or IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan
                    or IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual or IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual)
                    throw new InvalidOperationException("XPTY0004");

                var gCmp = CompareDateTimeValues(left, right, leftDateSub, GetImplicitTimezoneOffsetMinutes(context));
                if (gCmp.HasValue)
                {
                    return op switch
                    {
                        IrOpCode.Equal or IrOpCode.ValueEqual => gCmp.Value == 0,
                        IrOpCode.NotEqual or IrOpCode.ValueNotEqual => gCmp.Value != 0,
                        _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                    };
                }
                // Indeterminate: eq/ne fall back to string comparison
                int lexCmp = string.CompareOrdinal(left.ToString(), right.ToString());
                return op switch
                {
                    IrOpCode.Equal or IrOpCode.ValueEqual => lexCmp == 0,
                    IrOpCode.NotEqual or IrOpCode.ValueNotEqual => lexCmp != 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }

            var cmp = CompareDateTimeValues(left, right, leftDateSub, GetImplicitTimezoneOffsetMinutes(context));
            if (cmp.HasValue)
            {
                return op switch
                {
                    IrOpCode.Equal or IrOpCode.ValueEqual => cmp.Value == 0,
                    IrOpCode.NotEqual or IrOpCode.ValueNotEqual => cmp.Value != 0,
                    IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp.Value < 0,
                    IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp.Value <= 0,
                    IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp.Value > 0,
                    IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp.Value >= 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
            }
            // Indeterminate comparison: lt/gt/le/ge return false; eq/ne use string fallback
            return op is IrOpCode.Equal or IrOpCode.ValueEqual or IrOpCode.NotEqual or IrOpCode.ValueNotEqual
                ? CompareStrings(left.ToString(), right.ToString(), context.DefaultCollation, context) == (op is IrOpCode.Equal or IrOpCode.ValueEqual ? 0 : 1)
                : false;
        }

        // Binary comparison: xs:hexBinary / xs:base64Binary values are stored as
        // annotated strings. Comparison is defined only between two values of the same
        // binary type and orders by the decoded octets, not the lexical form
        // (op-base64Binary-less-than-17/25, op-hexBinary-greater-than-25).
        bool leftIsBinary = IsBinaryTypedString(left);
        bool rightIsBinary = IsBinaryTypedString(right);
        if (leftIsBinary || rightIsBinary)
        {
            if (!leftIsBinary || !rightIsBinary
                || !string.Equals(left.SchemaTypeName, right.SchemaTypeName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "XPTY0004: Comparison is only defined between two xs:hexBinary or two xs:base64Binary values.");
            }
            int bcmp = CompareBinaryValues(left, right);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => bcmp == 0,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => bcmp != 0,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => bcmp < 0,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => bcmp <= 0,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => bcmp > 0,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => bcmp >= 0,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        // Atomized nodes become strings; try numeric parsing for untyped values
        string lStr = left.ToString();
        string rStr = right.ToString();

        // If both are explicitly strings, compare as strings (don't parse as numbers)
        if (left.Kind == XdmValueKind.String && right.Kind == XdmValueKind.String)
        {
            int cmp = CompareStrings(lStr, rStr, context.DefaultCollation, context);
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => cmp == 0,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => cmp != 0,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp < 0,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp <= 0,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp > 0,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp >= 0,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        // In strict mode, a string may only be compared with a numeric if it originated
        // from a node (untyped atomic) or is explicitly typed as xs:untypedAtomic.
        // Typed string literals are not comparable with numbers.
        bool leftIsString = left.Kind == XdmValueKind.String;
        bool rightIsString = right.Kind == XdmValueKind.String;
        bool leftIsNumeric = IsDouble(left) || IsFloat(left) || IsDecimal(left) || left.Kind == XdmValueKind.Integer;
        bool rightIsNumeric = IsDouble(right) || IsFloat(right) || IsDecimal(right) || right.Kind == XdmValueKind.Integer;

        bool numericMismatch = (leftIsString && rightIsNumeric) || (leftIsNumeric && rightIsString);
        if (strict && numericMismatch)
        {
            bool stringIsCastable = (leftIsString && (leftFromNode || left.SchemaTypeName?.Equals("untypedAtomic", StringComparison.OrdinalIgnoreCase) == true))
                                 || (rightIsString && (rightFromNode || right.SchemaTypeName?.Equals("untypedAtomic", StringComparison.OrdinalIgnoreCase) == true));
            if (!stringIsCastable)
                throw new InvalidOperationException("XPTY0004");
        }

        if (double.TryParse(lStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lDbl) &&
            double.TryParse(rStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double rDbl))
        {
            return op switch
            {
                IrOpCode.Equal or IrOpCode.ValueEqual => lDbl == rDbl,
                IrOpCode.NotEqual or IrOpCode.ValueNotEqual => lDbl != rDbl,
                IrOpCode.LessThan or IrOpCode.ValueLessThan => lDbl < rDbl,
                IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => lDbl <= rDbl,
                IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => lDbl > rDbl,
                IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => lDbl >= rDbl,
                _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        }

        if (strict && numericMismatch)
        {
            // String came from a node but didn't parse as a number
            throw new InvalidOperationException("XPTY0004");
        }

        // In strict mode, only same-kind atomic values are comparable (cross-kind
        // mismatches such as boolean vs numeric should have been handled above).
        if (strict && left.Kind != right.Kind)
            throw new InvalidOperationException("XPTY0004");

        int cmp2 = CompareStrings(lStr, rStr, context.DefaultCollation, context);
        return op switch
        {
            IrOpCode.Equal or IrOpCode.ValueEqual => cmp2 == 0,
            IrOpCode.NotEqual or IrOpCode.ValueNotEqual => cmp2 != 0,
            IrOpCode.LessThan or IrOpCode.ValueLessThan => cmp2 < 0,
            IrOpCode.LessThanOrEqual or IrOpCode.ValueLessThanOrEqual => cmp2 <= 0,
            IrOpCode.GreaterThan or IrOpCode.ValueGreaterThan => cmp2 > 0,
            IrOpCode.GreaterThanOrEqual or IrOpCode.ValueGreaterThanOrEqual => cmp2 >= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }

    /// <summary>
    /// True when the value is an xs:hexBinary or xs:base64Binary atomic, which this
    /// engine stores as an <see cref="XdmValueKind.String"/> value carrying the binary
    /// schema-type annotation.
    /// </summary>
    private static bool IsBinaryTypedString(XdmValue value)
        => value.Kind == XdmValueKind.String
           && (value.SchemaTypeName?.Equals("hexBinary", StringComparison.OrdinalIgnoreCase) == true
               || value.SchemaTypeName?.Equals("base64Binary", StringComparison.OrdinalIgnoreCase) == true);

    /// <summary>
    /// Compares two same-type binary values by their decoded octets (unsigned,
    /// lexicographic). Callers must ensure both values share one binary schema type.
    /// </summary>
    private static int CompareBinaryValues(XdmValue left, XdmValue right)
    {
        bool isHex = left.SchemaTypeName!.Equals("hexBinary", StringComparison.OrdinalIgnoreCase);
        byte[] lBytes = isHex ? Convert.FromHexString(left.StringValue) : Convert.FromBase64String(left.StringValue);
        byte[] rBytes = isHex ? Convert.FromHexString(right.StringValue) : Convert.FromBase64String(right.StringValue);
        return ((ReadOnlySpan<byte>)lBytes).SequenceCompareTo(rBytes);
    }

    private static XdmValue CompareGeneral(IrOpCode op, XdmValue left, XdmValue right, EvaluationContext context)
    {
        // XPath 3.1 §17.3: if one operand is an empty sequence, the result is false.
        if (left.IsUndefined || right.IsUndefined)
            return XdmValue.FromBoolean(false);

        // Fast path: '=' / '!=' between a single xs:integer and a large all-integer
        // sequence (e.g. $validrange[not(. = $c)] with 1.1M x 2k comparisons): use a
        // cached hash set instead of O(n x m) pairwise comparison.
        if (!context.BackwardsCompatible && op is IrOpCode.GeneralEqual or IrOpCode.GeneralNotEqual &&
            TryIntegerSetComparison(op, left, right, out bool intSetResult))
        {
            return XdmValue.FromBoolean(intSetResult);
        }

        // General comparisons use existential semantics over sequences.
        // Enumerate lazily to avoid materializing huge ranges (e.g. 1e21 to 1e21+5e9).
        var leftItems = EnumerateItemsForComparison(left);
        var rightItems = EnumerateItemsForComparison(right);

        // XPath 1.0 backwards compatibility: when a node-set (or any sequence) is
        // compared to a boolean, both operands are converted to booleans using the
        // effective boolean value of the whole operand.
        if (context.BackwardsCompatible &&
            (SequenceContainsBooleanItem(left) || SequenceContainsBooleanItem(right) ||
             (left.IsUndefined && SequenceContainsBooleanItem(right)) ||
             (right.IsUndefined && SequenceContainsBooleanItem(left))))
        {
            int li = left.EffectiveBooleanValue() ? 1 : 0;
            int ri = right.EffectiveBooleanValue() ? 1 : 0;
            bool match = CompareCore(
                MapGeneralToStrictOp(op),
                XdmValue.FromInteger(li), XdmValue.FromInteger(ri), strict: false,
                false, false, context);
            return XdmValue.FromBoolean(match);
        }

        bool relational = IsRelationalGeneralComparison(op);

        foreach (var l in leftItems)
        {
            foreach (var r in rightItems)
            {
                // Atomize and check for empty sequence on each pair
                var atomizedL = Atomize(l);
                var atomizedR = Atomize(r);
                if (atomizedL.IsUndefined || atomizedR.IsUndefined)
                    continue;

                // Function items cannot be atomized: a comparison involving one is
                // FOTY0013 (inline-fn-031: comparing two inline functions with '=').
                if (atomizedL.IsFunction || atomizedR.IsFunction)
                    throw new InvalidOperationException("FOTY0013: A comparison operand must not be a function item.");

                // XPath 1.0 backwards compatibility coercion rules
                if (context.BackwardsCompatible)
                {
                    if (relational)
                    {
                        // Relational operators convert both operands to numbers.
                        if (atomizedL.Kind != XdmValueKind.Boolean)
                            atomizedL = XdmValue.FromDouble(ToDoubleOrNaN(atomizedL));
                        if (atomizedR.Kind != XdmValueKind.Boolean)
                            atomizedR = XdmValue.FromDouble(ToDoubleOrNaN(atomizedR));
                    }
                    else
                    {
                        ApplyBackwardsCompatibleCoercion(ref atomizedL, ref atomizedR);
                    }
                }

                // XPath 3.1 §3.5.3 general-comparison casting rules: when exactly one
                // value is xs:untypedAtomic, cast it to a type depending on the other
                // value's type (numeric -> xs:double, duration subtypes -> same subtype,
                // otherwise the primitive base type of T).
                if (!context.BackwardsCompatible)
                    CastUntypedForGeneralComparison(ref atomizedL, ref atomizedR, context);

                bool match = CompareCore(
                    MapGeneralToStrictOp(op),
                    atomizedL, atomizedR, strict: !context.BackwardsCompatible,
                    IsNodeOrigin(l), IsNodeOrigin(r), context);

                if (match)
                    return XdmValue.FromBoolean(true);
            }
        }

        return XdmValue.FromBoolean(false);
    }

    /// <summary>
    /// Applies the XPath 3.1 §3.5.3 general-comparison casting rules to one atomized
    /// value pair: both xs:untypedAtomic are cast to xs:string; a single xs:untypedAtomic
    /// is cast to xs:double (numeric other), the matching duration subtype, xs:QName
    /// (QName other), or the primitive base type of the other value.
    /// </summary>
    private static void CastUntypedForGeneralComparison(ref XdmValue left, ref XdmValue right, EvaluationContext context)
    {
        bool lu = IsUntypedAtomic(left);
        bool ru = IsUntypedAtomic(right);
        if (lu && ru)
        {
            left = XdmValue.FromString(left.StringValue);
            right = XdmValue.FromString(right.StringValue);
            return;
        }
        if (lu)
            left = CastUntypedToOtherType(left, right, context);
        else if (ru)
            right = CastUntypedToOtherType(right, left, context);
    }

    private static XdmValue CastUntypedToOtherType(XdmValue untyped, XdmValue other, EvaluationContext context)
    {
        string targetType;
        if (other.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float)
        {
            targetType = "xs:double";
        }
        else if (other.Kind == XdmValueKind.Duration)
        {
            targetType = other.SchemaTypeName?.Equals("yearMonthDuration", StringComparison.OrdinalIgnoreCase) == true
                ? "xs:yearMonthDuration"
                : "xs:dayTimeDuration";
        }
        else if (other.Kind == XdmValueKind.QName)
        {
            return CastUntypedAtomicToQName(untyped, context);
        }
        else
        {
            targetType = other.Kind switch
            {
                XdmValueKind.String => "xs:" + (other.SchemaTypeName is null
                    ? "string"
                    : PrimitiveBaseTypeName(other.SchemaTypeName)),
                XdmValueKind.Boolean => "xs:boolean",
                XdmValueKind.Uri => "xs:anyURI",
                XdmValueKind.Date => "xs:date",
                XdmValueKind.Time => "xs:time",
                XdmValueKind.DateTime => "xs:dateTime",
                _ => "xs:string"
            };
        }
        if (!TryCast(untyped, targetType, out var casted))
            throw new InvalidOperationException(
                $"FORG0001: Cannot cast xs:untypedAtomic '{untyped}' to {targetType}");
        return casted;
    }

    /// <summary>
    /// Walks the direct-supertype chain from a schema type annotation up to (but not
    /// including) xs:anyAtomicType, yielding the primitive base type — e.g. NCName →
    /// string, hexBinary → hexBinary. Used by the general-comparison casting rules.
    /// </summary>
    private static string PrimitiveBaseTypeName(string schemaTypeName)
    {
        string current = schemaTypeName;
        while (true)
        {
            var next = GetDirectSupertypes(current.ToLowerInvariant()).FirstOrDefault();
            if (next is null or "anyatomictype" or "item()")
                return current;
            current = next;
        }
    }

    private static IrOpCode MapGeneralToStrictOp(IrOpCode op)
        => op switch
        {
            IrOpCode.GeneralEqual => IrOpCode.Equal,
            IrOpCode.GeneralNotEqual => IrOpCode.NotEqual,
            IrOpCode.GeneralLessThan => IrOpCode.LessThan,
            IrOpCode.GeneralLessThanOrEqual => IrOpCode.LessThanOrEqual,
            IrOpCode.GeneralGreaterThan => IrOpCode.GreaterThan,
            IrOpCode.GeneralGreaterThanOrEqual => IrOpCode.GreaterThanOrEqual,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };

    private sealed class IntegerSetHolder
    {
        public HashSet<long>? Set;
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<IXdmSequence, IntegerSetHolder> IntegerSetCache = new();

    /// <summary>
    /// Evaluates <c>=</c>/<c>!=</c> between a single xs:integer item and a sequence that
    /// consists solely of xs:integer items, using a cached hash set for the sequence
    /// (built once per sequence identity). Returns false when the fast path does not apply.
    /// </summary>
    private static bool TryIntegerSetComparison(IrOpCode op, XdmValue left, XdmValue right, out bool result)
    {
        result = false;
        XdmValue single, multi;
        if (left.IsAtomic) { single = left; multi = right; }
        else if (right.IsAtomic) { single = right; multi = left; }
        else return false;

        var atom = Atomize(single);
        if (atom.Kind != XdmValueKind.Integer)
            return false;

        HashSet<long>? set;
        long singleOther = 0;
        if (multi.IsAtomic)
        {
            var other = Atomize(multi);
            if (other.Kind != XdmValueKind.Integer)
                return false;
            singleOther = other.IntegerValue;
            set = null;
        }
        else if (multi.IsSequence && multi.SequenceValue is not null)
        {
            set = GetOrBuildIntegerSet(multi.SequenceValue);
            if (set is null)
                return false;
        }
        else return false;

        result = op == IrOpCode.GeneralEqual
            ? (set is null ? atom.IntegerValue == singleOther : set.Contains(atom.IntegerValue))
            : (set is null ? atom.IntegerValue != singleOther : set.Count > 1 || !set.Contains(atom.IntegerValue));
        return true;
    }

    private static HashSet<long>? GetOrBuildIntegerSet(IXdmSequence seq)
    {
        var holder = IntegerSetCache.GetValue(seq, BuildIntegerSetHolder);
        return holder.Set;
    }

    private static IntegerSetHolder BuildIntegerSetHolder(IXdmSequence seq)
    {
        // Only larger, all-integer sequences qualify for the cached hash set.
        if (!seq.TryGetLength(out int len) || len < 8)
            return new IntegerSetHolder { Set = null };
        var set = new HashSet<long>(len);
        foreach (var item in XdmSequence.FromSource(seq))
        {
            var atom = Atomize(item);
            if (atom.Kind != XdmValueKind.Integer)
                return new IntegerSetHolder { Set = null };
            set.Add(atom.IntegerValue);
        }
        return new IntegerSetHolder { Set = set };
    }

    private static bool IsRelationalGeneralComparison(IrOpCode op)
        => op is IrOpCode.GeneralLessThan or IrOpCode.GeneralLessThanOrEqual
                or IrOpCode.GeneralGreaterThan or IrOpCode.GeneralGreaterThanOrEqual;

    private static bool HasBooleanItem(ReadOnlySpan<XdmValue> items)
    {
        foreach (var item in items)
        {
            var atomized = Atomize(item);
            if (!atomized.IsUndefined && atomized.Kind == XdmValueKind.Boolean)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Applies XPath 1.0 backwards-compatible coercion rules for general comparisons:
    /// 1. If either operand is boolean, convert the other to boolean.
    /// 2. If either operand is numeric, convert the other to numeric.
    /// 3. Otherwise, convert both to strings.
    /// </summary>
    private static void ApplyBackwardsCompatibleCoercion(ref XdmValue left, ref XdmValue right)
    {
        bool leftIsBool = left.Kind == XdmValueKind.Boolean;
        bool rightIsBool = right.Kind == XdmValueKind.Boolean;
        if (leftIsBool || rightIsBool)
        {
            if (!leftIsBool) left = XdmValue.FromBoolean(left.EffectiveBooleanValue());
            if (!rightIsBool) right = XdmValue.FromBoolean(right.EffectiveBooleanValue());
            return;
        }

        bool leftIsNum = IsNumeric(left);
        bool rightIsNum = IsNumeric(right);
        if (leftIsNum || rightIsNum)
        {
            if (!leftIsNum) left = XdmValue.FromDouble(ToDoubleOrNaN(left));
            if (!rightIsNum) right = XdmValue.FromDouble(ToDoubleOrNaN(right));
            return;
        }

        // Otherwise both become strings (they already are after atomization)
    }

    /// <summary>
    /// Converts a value to double, returning NaN for unparseable strings
    /// (XPath 1.0 semantics) instead of throwing.
    /// </summary>
    private static double ToDoubleOrNaN(XdmValue value)
    {
        // XPath 1.0 backwards compatibility: a multi-item sequence converts via its
        // first item (backwards-024: 1 + (6 to 10) = 7); the empty sequence gives NaN.
        if (value.IsSequence && value.SequenceValue is not null)
            value = FirstItemOrUndefined(value);
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0 : 0.0,
            _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : double.NaN
        };
    }

    private static bool IsNumeric(XdmValue value)
        => IsDouble(value) || IsFloat(value) || IsDecimal(value) || value.Kind == XdmValueKind.Integer;

    /// <summary>
    /// Resolves the (local, namespace) key under which a for/quantified loop variable is
    /// bound: resolved EQName URI if present, prefix resolved against the static context,
    /// otherwise the bare local name in no namespace.
    /// </summary>
    private static (string Local, string Ns) ResolveLoopVariableKey(QuantifiedLoopInfo info, EvaluationContext context)
    {
        if (info.VariableNamespaceUri is not null)
            return (info.VariableName, info.VariableNamespaceUri);
        if (info.VariablePrefix is not null)
            return ResolveVariableName($"{info.VariablePrefix}:{info.VariableName}", context);
        if (info.VariableName.Contains(':'))
            return ResolveVariableName(info.VariableName, context);
        return (info.VariableName, "");
    }

    /// <summary>
    /// Converts a 'to' (range) operand to an integer per function conversion rules:
    /// xs:integer is accepted directly, xs:untypedAtomic is cast (FORG0001 on failure),
    /// anything else (including xs:decimal/xs:double) is XPTY0004.
    /// </summary>
    private static bool TryGetRangeOperand(XdmValue value, out decimal result)
    {
        result = 0;
        var atomized = Atomize(value);
        if (atomized.Kind == XdmValueKind.Integer)
        {
            result = atomized.IntegerValue;
            return true;
        }
        if (atomized.Kind == XdmValueKind.Decimal)
        {
            decimal d = atomized.DecimalValue;
            if (d != decimal.Truncate(d))
                return false;
            result = d;
            return true;
        }
        if (IsUntypedAtomic(atomized))
        {
            string s = atomized.ToString().Trim();
            if (decimal.TryParse(s, out var dec) && dec == decimal.Truncate(dec))
            {
                result = dec;
                return true;
            }
            throw new InvalidOperationException(
                $"FORG0001: Cannot cast xs:untypedAtomic '{s}' to xs:integer");
        }
        return false;
    }

    /// <summary>
    /// Validates that an arithmetic operand is numeric or xs:untypedAtomic after atomization.
    /// Date/time and duration operands must be handled by the caller before this check.
    /// Throws XPTY0004 for xs:string, xs:boolean and other non-numeric atomic types.
    /// </summary>
    private static void ValidateNumericOperand(XdmValue value)
    {
        var atomized = Atomize(value);
        if (atomized.IsUndefined)
            return; // empty-sequence operands are handled by the caller
        if (IsNumeric(atomized) || IsUntypedAtomic(atomized))
            return;
        throw new InvalidOperationException(
            $"XPTY0004: Arithmetic operands must be numeric or xs:untypedAtomic values, but got {atomized.Kind}");
    }

    private static bool IsUntypedAtomic(XdmValue value)
        => value.Kind == XdmValueKind.String &&
           string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Casts an xs:untypedAtomic value to xs:QName, resolving any prefix against
    /// the static namespace context and using the default element namespace for
    /// unprefixed lexical QNames.
    /// </summary>
    private static XdmValue CastUntypedAtomicToQName(XdmValue value, EvaluationContext context)
    {
        string lexical = value.StringValue.Trim();
        if (string.IsNullOrEmpty(lexical))
            throw new InvalidOperationException("XPTY0004: Cannot cast empty xs:untypedAtomic value to xs:QName.");

        int colon = lexical.IndexOf(':');
        string prefix = colon >= 0 ? lexical[..colon] : string.Empty;
        string local = colon >= 0 ? lexical[(colon + 1)..] : lexical;

        if (!IsValidNcName(prefix) || !IsValidNcName(local))
            throw new InvalidOperationException("XPTY0004: Invalid lexical QName for cast to xs:QName.");

        string namespaceUri;
        if (string.IsNullOrEmpty(prefix))
        {
            namespaceUri = context.DefaultElementNamespace ?? string.Empty;
        }
        else
        {
            if (!context.TryResolveNamespace(prefix, out namespaceUri!))
                throw new InvalidOperationException($"XPTY0004: Namespace prefix '{prefix}' is not declared for xs:QName cast.");
        }

        return XdmValue.FromQName(new XsQName(local, namespaceUri, prefix));
    }

    private static bool IsValidNcName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return true; // empty prefix is allowed
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;
        for (int i = 1; i < name.Length; i++)
        {
            char c = name[i];
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_')
                return false;
        }
        return true;
    }

    // ------------------------------------------------------------------
    // Type operations
    // ------------------------------------------------------------------

    public static XdmValue Cast(XdmValue value, string typeName)
        => Cast(value, typeName, null);

    public static XdmValue Cast(XdmValue value, string typeName, EvaluationContext? context)
    {
        if (!TryCast(value, typeName, context, out var result))
            throw new InvalidOperationException($"FORG0001: Cannot cast '{value}' to {typeName}.");
        return result;
    }

    public static bool TryCast(XdmValue value, string typeName, out XdmValue result)
        => TryCast(value, typeName, null, out result);

    public static bool TryCast(XdmValue value, string typeName, EvaluationContext? context, out XdmValue result)
    {
        result = value;
        string normalized = typeName.ToLowerInvariant().Replace("xs:", "").Replace("xsd:", "");
        if (normalized.EndsWith('?') || normalized.EndsWith('*') || normalized.EndsWith('+'))
            normalized = normalized[..^1].TrimEnd();

        var (resolvedNs, resolvedLocal) = ResolveTypeQName(typeName, context);

        // A prefixed type name resolves via the in-scope namespaces (constructor-local
        // declarations included): the XML Schema namespace maps to the bare type.
        if (normalized.Contains(':') && !normalized.Contains('{'))
        {
            int colon = normalized.IndexOf(':');
            var typePrefix = normalized[..colon];
            if (context is not null && context.TryResolveNamespace(typePrefix, out var resolvedTypeNs))
            {
                if (resolvedTypeNs == "http://www.w3.org/2001/XMLSchema")
                    normalized = normalized[(colon + 1)..];
            }
            else
            {
                throw new InvalidOperationException($"XPST0081: Prefix '{typePrefix}' is not declared.");
            }
        }

        // Empty sequence casts to empty sequence for all types
        if (value.IsUndefined)
        {
            result = XdmValue.Undefined;
            return true;
        }

        // If value is a sequence, only allow single-item sequences for atomic casts
        if (value.IsSequence)
        {
            if (!TryGetSequenceLength(value.SequenceValue, out var seqLen))
                return false;
            if (seqLen == 0)
            {
                result = XdmValue.Undefined;
                return true;
            }
            if (seqLen != 1)
                return false;
            var enumerator = XdmSequence.FromSource(value.SequenceValue!).GetEnumerator();
            enumerator.MoveNext();
            value = enumerator.Current;
        }

        // Atomize nodes before casting (use PSVI typed value for schema-validated nodes).
        if (value.IsNode)
        {
            value = Atomize(value);
        }

        // Schema-imported simple types (not built-in xs:*): validate the lexical value and produce a typed value.
        if (resolvedNs != XmlSchema.Namespace && TryGetSchemaSimpleType(resolvedNs, resolvedLocal, context, out var schemaSimpleType))
        {
            try
            {
                string lexical = value.ToString() ?? string.Empty;
                var nsResolver = new XmlNamespaceManager(new NameTable());
                object parsed = schemaSimpleType.Datatype.ParseValue(lexical, new NameTable(), nsResolver);
                bool hasTz = LexicalHasTimezone(lexical);
                if (schemaSimpleType.Datatype.Variety == XmlSchemaDatatypeVariety.List && parsed is System.Collections.IEnumerable list && parsed is not string)
                {
                    var items = new List<XdmValue>();
                    foreach (object? item in list)
                    {
                        if (item is not null)
                            items.Add(ConvertSchemaValue(item, schemaSimpleType.Datatype, schemaSimpleType, hasTz));
                    }
                    result = XdmValue.FromSequence(MaterializedSequence.FromList(items));
                    return true;
                }
                result = ConvertSchemaValue(parsed, schemaSimpleType.Datatype, schemaSimpleType, hasTz);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // An unprefixed type name resolves against the default element namespace: when it
        // is set and is not the XML Schema namespace, the name is not a built-in type
        // (XQST0052, K2-DefaultNamespaceProlog-12a — '2 cast as byte' under the XSL namespace).
        if (!typeName.Contains(':') && !typeName.StartsWith("Q{", StringComparison.Ordinal)
            && context is not null
            && !string.IsNullOrEmpty(context.DefaultElementNamespace)
            && context.DefaultElementNamespace != "http://www.w3.org/2001/XMLSchema")
        {
            throw new InvalidOperationException($"XQST0052: The type '{typeName}' is not a known simple type in the namespace '{context.DefaultElementNamespace}'.");
        }

        // Prefixed name that is not xs:* and not a schema type is XPST0051.
        if (resolvedNs != "http://www.w3.org/2001/XMLSchema"
            && typeName.Contains(':')
            && !typeName.StartsWith("Q{", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("XPST0051");
        }

        // Schema type cast restrictions: some typed values can only cast to specific types
        if (value.SchemaTypeName is not null
            && normalized is not "string" and not "untypedatomic"
            && !IsCastAllowed(value.SchemaTypeName, normalized))
        {
            return false;
        }

        switch (normalized)
        {
            case "string":
                result = XdmValue.FromString(value.ToString());
                return true;

            case "integer":
            case "int":
            case "long":
            case "short":
            case "byte":
            case "unsignedshort":
            case "unsignedint":
            case "unsignedlong":
            case "unsignedbyte":
            case "positiveinteger":
            case "negativeinteger":
            case "nonpositiveinteger":
            case "nonnegativeinteger":
                // xs:unsignedLong values can exceed long.MaxValue; handle them with decimal backing.
                if (normalized == "unsignedlong")
                {
                    if (value.Kind == XdmValueKind.Integer)
                    {
                        if (value.IntegerValue < 0) return false;
                        result = XdmValue.FromInteger(value.IntegerValue, normalized);
                        return true;
                    }
                    if (value.Kind == XdmValueKind.Decimal)
                    {
                        decimal d = value.DecimalValue;
                        if (d < 0 || d > ulong.MaxValue) return false;
                        if (d <= long.MaxValue)
                            result = XdmValue.FromInteger((long)d, normalized);
                        else
                            result = XdmValue.FromDecimal(d, normalized);
                        return true;
                    }
                    if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
                    {
                        double d = value.DoubleValue;
                        if (double.IsNaN(d) || double.IsInfinity(d)) return false;
                        if (d < 0 || d > ulong.MaxValue) return false;
                        if (d <= long.MaxValue)
                            result = XdmValue.FromInteger((long)d, normalized);
                        else
                            result = XdmValue.FromDecimal((decimal)d, normalized);
                        return true;
                    }
                    if (value.Kind == XdmValueKind.Boolean)
                    {
                        result = XdmValue.FromInteger(value.BooleanValue ? 1 : 0, normalized);
                        return true;
                    }
                    if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                        or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                        return false;
                    string s = value.ToString().Trim();
                    if (s.StartsWith('+')) s = s[1..];
                    if (ulong.TryParse(s, out var uInt))
                    {
                        if (uInt <= (ulong)long.MaxValue)
                            result = XdmValue.FromInteger((long)uInt, normalized);
                        else
                            result = XdmValue.FromDecimal((decimal)uInt, normalized);
                        return true;
                    }
                    return false;
                }

                if (value.Kind == XdmValueKind.Integer)
                {
                    if (!IsIntegerInRange(value.IntegerValue, normalized))
                        return false;
                    result = XdmValue.FromInteger(value.IntegerValue, normalized);
                    return true;
                }
                if (value.Kind == XdmValueKind.Decimal)
                {
                    long lVal = (long)value.DecimalValue;
                    if (!IsIntegerInRange(lVal, normalized))
                        return false;
                    result = XdmValue.FromInteger(lVal, normalized);
                    return true;
                }
                if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
                {
                    double d = value.DoubleValue;
                    if (double.IsNaN(d) || double.IsInfinity(d))
                        return false;
                    if (d > long.MaxValue || d < long.MinValue)
                        throw new InvalidOperationException("FOCA0003");
                    long lDbl = (long)d;
                    if (!IsIntegerInRange(lDbl, normalized))
                        return false;
                    result = XdmValue.FromInteger(lDbl, normalized);
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    long lBool = value.BooleanValue ? 1 : 0;
                    if (!IsIntegerInRange(lBool, normalized))
                        return false;
                    result = XdmValue.FromInteger(lBool, normalized);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (long.TryParse(value.ToString().Trim(), out var lInt))
                {
                    if (!IsIntegerInRange(lInt, normalized))
                        return false;
                    result = XdmValue.FromInteger(lInt, normalized);
                    return true;
                }
                return false;

            case "decimal":
                if (value.Kind == XdmValueKind.Decimal)
                    return true;
                if (value.Kind == XdmValueKind.Integer)
                {
                    result = XdmValue.FromDecimal(value.IntegerValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
                {
                    double d = value.DoubleValue;
                    if (double.IsNaN(d) || double.IsInfinity(d))
                        return false;
                    result = XdmValue.FromDecimal((decimal)d);
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    result = XdmValue.FromDecimal(value.BooleanValue ? 1m : 0m);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                {
                    string sDec = value.ToString();
                    // xs:decimal does not allow exponent notation
                    if (sDec.Contains('e', StringComparison.OrdinalIgnoreCase))
                        return false;
                    if (decimal.TryParse(sDec, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    {
                        result = XdmValue.FromDecimal(dec);
                        return true;
                    }
                }
                return false;

            case "double":
                if (value.Kind == XdmValueKind.Double)
                    return true;
                if (value.Kind == XdmValueKind.Float)
                {
                    result = XdmValue.FromDouble(value.DoubleValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Integer)
                {
                    result = XdmValue.FromDouble(value.IntegerValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Decimal)
                {
                    result = XdmValue.FromDouble((double)value.DecimalValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    result = XdmValue.FromDouble(value.BooleanValue ? 1.0 : 0.0);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (TryParseDouble(value.ToString(), out var dbl))
                {
                    result = XdmValue.FromDouble(dbl);
                    return true;
                }
                return false;

            case "float":
                if (value.Kind == XdmValueKind.Float)
                    return true;
                if (value.Kind == XdmValueKind.Double)
                {
                    result = XdmValue.FromFloat((float)value.DoubleValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Integer)
                {
                    result = XdmValue.FromFloat(value.IntegerValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Decimal)
                {
                    result = XdmValue.FromFloat((float)value.DecimalValue);
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    result = XdmValue.FromFloat(value.BooleanValue ? 1.0f : 0.0f);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (TryParseFloat(value.ToString(), out var flt))
                {
                    result = XdmValue.FromFloat(flt);
                    return true;
                }
                return false;

            case "numeric":
                // xs:numeric is a union type of xs:double, xs:float, xs:decimal.
                // Casting from a member type preserves the source type; casting from
                // string, untypedAtomic, or boolean yields xs:double.
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float)
                {
                    result = value;
                    return true;
                }
                if (value.Kind == XdmValueKind.Boolean)
                {
                    result = XdmValue.FromDouble(value.BooleanValue ? 1.0 : 0.0);
                    return true;
                }
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (TryParseDouble(value.ToString(), out var numericDbl))
                {
                    result = XdmValue.FromDouble(numericDbl);
                    return true;
                }
                return false;

            case "boolean":
                if (value.Kind == XdmValueKind.Boolean)
                    return true;
                if (value.Kind == XdmValueKind.String)
                {
                    // xs:boolean lexical values are case-sensitive: true, false, 0, 1.
                    var s = CollapseWhitespace(value.StringValue);
                    if (s == "true" || s == "1")
                    {
                        result = XdmValue.True;
                        return true;
                    }
                    if (s == "false" || s == "0")
                    {
                        result = XdmValue.False;
                        return true;
                    }
                    return false;
                }
                if (value.Kind == XdmValueKind.Integer)
                {
                    result = XdmValue.FromBoolean(value.IntegerValue != 0);
                    return true;
                }
                if (value.Kind == XdmValueKind.Decimal)
                {
                    result = XdmValue.FromBoolean(value.DecimalValue != 0m);
                    return true;
                }
                if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
                {
                    double d = value.DoubleValue;
                    result = XdmValue.FromBoolean(d != 0.0 && !double.IsNaN(d));
                    return true;
                }
                return false;

            case "datetime":
                if (value.Kind == XdmValueKind.DateTime)
                    return true;
                if (value.Kind == XdmValueKind.Date)
                {
                    var xdtSrc = value.DateXPathValue;
                    result = XdmValue.FromDateTime(new XPathDateTime(xdtSrc.Year, xdtSrc.Month, xdtSrc.Day, 0, 0, 0, 0, xdtSrc.TimezoneOffsetMinutes, value.HasTimezone), value.HasTimezone);
                    return true;
                }
                if (value.Kind == XdmValueKind.Time)
                    return false;
                {
                    string sDt = NormalizeDateTimeString(value.ToString().Trim());
                    if (TryParseXPathDateTime(sDt, out var xdtDt, out var hasTzDt))
                    {
                        if (xdtDt.IsRepresentableAsDateTimeOffset && DateTimeOffset.TryParse(sDt, out var dtoDt))
                        {
                            result = XdmValue.FromDateTime(dtoDt, hasTzDt);
                        }
                        else
                        {
                            result = XdmValue.FromDateTime(xdtDt, hasTzDt);
                        }
                        return true;
                    }
                }
                return false;

            case "datetimestamp":
                if (value.Kind == XdmValueKind.DateTime && value.HasTimezone)
                {
                    result = XdmValue.FromDateTime(value.DateTimeXPathValue, hasTimezone: true, schemaTypeName: "dateTimeStamp");
                    return true;
                }
                if (value.Kind == XdmValueKind.Date)
                {
                    if (!value.HasTimezone) return false;
                    var xdtSrcStamp = value.DateXPathValue;
                    result = XdmValue.FromDateTime(
                        new XPathDateTime(xdtSrcStamp.Year, xdtSrcStamp.Month, xdtSrcStamp.Day, 0, 0, 0, 0, xdtSrcStamp.TimezoneOffsetMinutes, true),
                        hasTimezone: true,
                        schemaTypeName: "dateTimeStamp");
                    return true;
                }
                if (value.Kind == XdmValueKind.Time)
                    return false;
                {
                    string sStamp = NormalizeDateTimeString(value.ToString().Trim());
                    if (TryParseXPathDateTime(sStamp, out var xdtStamp, out var hasTzStamp))
                    {
                        if (!hasTzStamp) return false;
                        if (xdtStamp.IsRepresentableAsDateTimeOffset && DateTimeOffset.TryParse(sStamp, out var dtoStamp))
                        {
                            result = XdmValue.FromDateTime(dtoStamp, hasTimezone: true, schemaTypeName: "dateTimeStamp");
                        }
                        else
                        {
                            result = XdmValue.FromDateTime(xdtStamp, hasTimezone: true, schemaTypeName: "dateTimeStamp");
                        }
                        return true;
                    }
                }
                return false;

            case "date":
                if (value.Kind == XdmValueKind.Date)
                    return true;
                if (value.Kind == XdmValueKind.DateTime)
                {
                    bool hasTz = value.HasTimezone;
                    var xdtDt = value.DateTimeXPathValue;
                    result = XdmValue.FromDate(new XPathDateTime(xdtDt.Year, xdtDt.Month, xdtDt.Day, 0, 0, 0, 0, xdtDt.TimezoneOffsetMinutes, hasTz), hasTz);
                    return true;
                }
                if (value.Kind == XdmValueKind.Time)
                    return false;
                {
                    string sD = NormalizeDateTimeString(value.ToString().Trim());
                    if (TryParseXPathDate(sD, out var xdtD, out var hasTzD))
                    {
                        if (xdtD.IsRepresentableAsDateTimeOffset && DateTimeOffset.TryParse(sD, out var dtoD))
                        {
                            result = XdmValue.FromDate(dtoD, hasTzD);
                        }
                        else
                        {
                            result = XdmValue.FromDate(xdtD, hasTzD);
                        }
                        return true;
                    }
                    // Fallback for backward compatibility with dateTime-shaped strings cast to date
                    // (removed - DateTimeOffset.TryParse is too lenient and accepts invalid formats)
                }
                return false;

            case "time":
                if (value.Kind == XdmValueKind.Time)
                    return true;
                if (value.Kind == XdmValueKind.DateTime)
                {
                    var xdtDt = value.DateTimeXPathValue;
                    bool hasTz = value.HasTimezone;
                    result = XdmValue.FromTime(new XPathDateTime(1, 1, 1, xdtDt.Hour, xdtDt.Minute, xdtDt.Second, xdtDt.Millisecond, xdtDt.TimezoneOffsetMinutes, hasTz), hasTz);
                    return true;
                }
                if (value.Kind == XdmValueKind.Date)
                    return false;
                {
                    string sT = NormalizeDateTimeString(value.ToString().Trim());
                    if (TryParseXPathTime(sT, out var xdtT, out var hasTzT))
                    {
                        result = XdmValue.FromTime(xdtT, hasTzT);
                        return true;
                    }
                    // Fallback for backward compatibility with dateTime-shaped strings cast to time
                    // (removed - DateTimeOffset.TryParse is too lenient and accepts invalid formats)
                }
                return false;

            case "untypedatomic":
                result = XdmValue.FromString(value.ToString(), "untypedAtomic");
                return true;

            case "anyuri":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Date
                    or XdmValueKind.Time or XdmValueKind.DateTime or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                {
                    // XML Schema anyURI has whiteSpace="collapse"
                    string sUri = CollapseWhitespace(value.ToString());
                    // Reject invalid percent-encoding sequences
                    if (!IsValidAnyUri(sUri))
                        return false;
                    result = XdmValue.FromString(sUri, "anyURI");
                    return true;
                }

            case "base64binary":
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Integer
                    or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float
                    or XdmValueKind.Boolean or XdmValueKind.Node)
                    return false;
                {
                    string sB64 = value.ToString();
                    // Cross-cast from hexBinary: decode hex to bytes, encode as base64
                    if (value.SchemaTypeName is not null && value.SchemaTypeName.Equals("hexBinary", StringComparison.OrdinalIgnoreCase))
                    {
                        string hex = sB64.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                        try
                        {
                            byte[] bytes = Convert.FromHexString(hex);
                            result = XdmValue.FromString(Convert.ToBase64String(bytes), "base64Binary");
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    string normalizedB64 = sB64.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    if (!IsValidBase64(normalizedB64))
                        return false;
                    result = XdmValue.FromString(normalizedB64, "base64Binary");
                    return true;
                }

            case "hexbinary":
                if (value.Kind is XdmValueKind.Date or XdmValueKind.Time or XdmValueKind.DateTime
                    or XdmValueKind.Duration or XdmValueKind.QName or XdmValueKind.Integer
                    or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float
                    or XdmValueKind.Boolean or XdmValueKind.Node)
                    return false;
                {
                    string sHex = value.ToString().Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    // Cross-cast from base64Binary: decode base64 to bytes, encode as hex
                    if (value.SchemaTypeName is not null && value.SchemaTypeName.Equals("base64Binary", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            byte[] bytes = Convert.FromBase64String(sHex);
                            result = XdmValue.FromString(Convert.ToHexString(bytes), "hexBinary");
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    if (!Regex.IsMatch(sHex, @"^[0-9a-fA-F]*$"))
                        return false;
                    if (sHex.Length % 2 != 0)
                        return false;
                    result = XdmValue.FromString(sHex.ToUpperInvariant(), "hexBinary");
                    return true;
                }

            case "duration":
                if (value.Kind == XdmValueKind.Duration)
                {
                    // Casting to the generic xs:duration type ensures subsequent operator
                    // dispatch can distinguish it from the yearMonth/dayTime subtypes.
                    result = XdmValue.FromDuration(value.DurationValue, "duration");
                    return true;
                }
                {
                    string sDur = value.ToString().Trim();
                    if (IsValidDuration(sDur))
                    {
                        result = XdmValue.FromDuration(CanonicalizeDuration(sDur), "duration");
                        return true;
                    }
                }
                return false;

            case "yearmonthduration":
                if (value.Kind == XdmValueKind.Duration)
                {
                    result = XdmValue.FromDuration(ExtractYearMonthDuration(value.DurationValue), "yearMonthDuration");
                    return true;
                }
                {
                    string sYm = value.ToString().Trim();
                    if (IsValidYearMonthDuration(sYm))
                    {
                        result = XdmValue.FromDuration(ExtractYearMonthDuration(sYm), "yearMonthDuration");
                        return true;
                    }
                }
                return false;

            case "daytimeduration":
                if (value.Kind == XdmValueKind.Duration)
                {
                    result = XdmValue.FromDuration(ExtractDayTimeDuration(value.DurationValue), "dayTimeDuration");
                    return true;
                }
                {
                    string sDt = value.ToString().Trim();
                    if (IsValidDayTimeDuration(sDt))
                    {
                        result = XdmValue.FromDuration(ExtractDayTimeDuration(sDt), "dayTimeDuration");
                        return true;
                    }
                }
                return false;

            case "gyear":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtY = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtY.FormatTimezone();
                    result = XdmValue.FromString($"{xdtY.FormatYear()}{tz}", "gYear");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^(-?)(\d{4,})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        string sign = m.Groups[1].Value;
                        string yearStr = m.Groups[2].Value;
                        string tz = m.Groups[3].Value;
                        // Reject leading zeros for years longer than 4 digits
                        if (yearStr.Length > 4 && yearStr[0] == '0')
                            return false;
                        // Validate and normalize timezone
                        if (!string.IsNullOrEmpty(tz))
                        {
                            if (tz.Equals("Z", StringComparison.OrdinalIgnoreCase))
                            {
                                tz = "Z";
                            }
                            else
                            {
                                int tzHour = int.Parse(tz[1..3], CultureInfo.InvariantCulture);
                                int tzMin = int.Parse(tz[4..6], CultureInfo.InvariantCulture);
                                if (tzHour > 14 || (tzHour == 14 && tzMin > 0) || tzMin > 59)
                                    return false;
                                if (tzHour == 0 && tzMin == 0)
                                    tz = "Z";
                            }
                        }
                        // Reject years too large to fit in long (overflow)
                        if (yearStr.Length > 18)
                            return false;
                        // Normalize -0000 to 0000
                        if (sign == "-" && yearStr.TrimStart('0') == "")
                        {
                            sign = "";
                            yearStr = "0000";
                        }
                        result = XdmValue.FromString(sign + yearStr + tz, "gYear");
                        return true;
                    }
                }
                return false;

            case "gyearmonth":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtYm = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtYm.FormatTimezone();
                    result = XdmValue.FromString($"{xdtYm.FormatYear()}-{xdtYm.Month:00}{tz}", "gYearMonth");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^(-?)(\d{4,})-(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        string sign = m.Groups[1].Value;
                        string yearStr = m.Groups[2].Value;
                        string monthStr = m.Groups[3].Value;
                        if (!int.TryParse(monthStr, out int monthVal) || monthVal < 1 || monthVal > 12)
                            return false;
                        string rest = $"-{monthStr}{m.Groups[4].Value}";
                        // Reject leading zeros for years longer than 4 digits
                        if (yearStr.Length > 4 && yearStr[0] == '0')
                            return false;
                        // Validate and normalize timezone
                        string tz = rest[3..]; // after -MM
                        if (!string.IsNullOrEmpty(tz))
                        {
                            if (tz.Equals("Z", StringComparison.OrdinalIgnoreCase))
                            {
                                rest = rest[..3] + "Z";
                            }
                            else
                            {
                                int tzHour = int.Parse(tz[1..3], CultureInfo.InvariantCulture);
                                int tzMin = int.Parse(tz[4..6], CultureInfo.InvariantCulture);
                                if (tzHour > 14 || (tzHour == 14 && tzMin > 0) || tzMin > 59)
                                    return false;
                                if (tzHour == 0 && tzMin == 0)
                                    rest = rest[..3] + "Z";
                            }
                        }
                        // Reject years too large to fit in long (overflow)
                        if (yearStr.Length > 18)
                            return false;
                        // Normalize -0000 to 0000
                        if (sign == "-" && yearStr.TrimStart('0') == "")
                        {
                            sign = "";
                            yearStr = "0000";
                        }
                        result = XdmValue.FromString(sign + yearStr + rest, "gYearMonth");
                        return true;
                    }
                }
                return false;

            case "gmonthday":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtMd = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtMd.FormatTimezone();
                    result = XdmValue.FromString($"--{xdtMd.Month:00}-{xdtMd.Day:00}{tz}", "gMonthDay");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^--(\d{2})-(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        if (!int.TryParse(m.Groups[1].Value, out int month) || month < 1 || month > 12)
                            return false;
                        if (!int.TryParse(m.Groups[2].Value, out int day) || day < 1 || day > 31)
                            return false;
                        // Validate days per month (Feb max 29, Apr/Jun/Sep/Nov max 30)
                        int maxDay = month == 2 ? 29 : (month is 4 or 6 or 9 or 11 ? 30 : 31);
                        if (day > maxDay) return false;
                        string tz = m.Groups[3].Value;
                        string? normalizedTz = NormalizeTimezone(tz);
                        if (normalizedTz is null) return false;
                        result = XdmValue.FromString($"--{m.Groups[1].Value}-{m.Groups[2].Value}{normalizedTz}", "gMonthDay");
                        return true;
                    }
                }
                return false;

            case "gday":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtD = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtD.FormatTimezone();
                    result = XdmValue.FromString($"---{xdtD.Day:00}{tz}", "gDay");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^---(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        if (!int.TryParse(m.Groups[1].Value, out int day) || day < 1 || day > 31)
                            return false;
                        string tz = m.Groups[2].Value;
                        string? normalizedTz = NormalizeTimezone(tz);
                        if (normalizedTz is null) return false;
                        result = XdmValue.FromString($"---{m.Groups[1].Value}{normalizedTz}", "gDay");
                        return true;
                    }
                }
                return false;

            case "gmonth":
                if (value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double
                    or XdmValueKind.Float or XdmValueKind.Boolean or XdmValueKind.Duration
                    or XdmValueKind.QName or XdmValueKind.Node)
                    return false;
                if (value.Kind == XdmValueKind.DateTime || value.Kind == XdmValueKind.Date)
                {
                    var xdtM = value.Kind == XdmValueKind.DateTime ? value.DateTimeXPathValue : value.DateXPathValue;
                    string tz = xdtM.FormatTimezone();
                    result = XdmValue.FromString($"--{xdtM.Month:00}{tz}", "gMonth");
                    return true;
                }
                {
                    string s = value.ToString().Trim();
                    var m = Regex.Match(s, @"^--(\d{2})((?:[Zz]|[+\-]\d{2}:\d{2})?)$");
                    if (m.Success)
                    {
                        if (!int.TryParse(m.Groups[1].Value, out int month) || month < 1 || month > 12)
                            return false;
                        string tz = m.Groups[2].Value;
                        string? normalizedTz = NormalizeTimezone(tz);
                        if (normalizedTz is null) return false;
                        result = XdmValue.FromString($"--{m.Groups[1].Value}{normalizedTz}", "gMonth");
                        return true;
                    }
                }
                return false;

            case "ncname":
            case "id":
            case "idref":
            case "entity":
            {
                string s = CollapseWhitespace(value.ToString());
                if (Regex.IsMatch(s, @"^[\p{L}_][\w.\-]*$"))
                {
                    result = XdmValue.FromString(s, normalized);
                    return true;
                }
                return false;
            }

            case "name":
            {
                string s = CollapseWhitespace(value.ToString());
                if (Regex.IsMatch(s, @"^[\p{L}_:][\w.:\-]*$"))
                {
                    result = XdmValue.FromString(s, "Name");
                    return true;
                }
                return false;
            }

            case "nmtoken":
            {
                string s = CollapseWhitespace(value.ToString());
                if (Regex.IsMatch(s, @"^[\w.:\-]+$"))
                {
                    result = XdmValue.FromString(s, "NMTOKEN");
                    return true;
                }
                return false;
            }

            case "language":
            {
                string s = CollapseWhitespace(value.ToString());
                if (Regex.IsMatch(s, @"^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$"))
                {
                    result = XdmValue.FromString(s, "language");
                    return true;
                }
                return false;
            }

            case "normalizedstring":
            {
                string s = value.ToString();
                // XML Schema whiteSpace="replace": replace tab, CR, LF with space
                s = s.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                result = XdmValue.FromString(s, "normalizedString");
                return true;
            }
            case "token":
            {
                string s = value.ToString();
                // XML Schema whiteSpace="collapse": replace tab/CR/LF with space,
                // trim leading/trailing spaces, collapse internal runs of spaces
                s = s.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                s = s.Trim(' ');
                while (s.Contains("  "))
                    s = s.Replace("  ", " ");
                result = XdmValue.FromString(s, "token");
                return true;
            }

            case "idrefs":
            {
                string s = CollapseWhitespace(value.ToString());
                if (string.IsNullOrEmpty(s))
                    return false;
                var idrefTokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var idrefItems = new XdmValue[idrefTokens.Length];
                for (int i = 0; i < idrefTokens.Length; i++)
                {
                    if (!Regex.IsMatch(idrefTokens[i], @"^[\p{L}_][\w.\-]*$"))
                        return false;
                    idrefItems[i] = XdmValue.FromString(idrefTokens[i], "IDREF");
                }
                result = XdmValue.FromSequence(MaterializedSequence.FromArray(idrefItems));
                return true;
            }

            case "nmtokens":
            {
                string s = CollapseWhitespace(value.ToString());
                if (string.IsNullOrEmpty(s))
                    return false;
                var nmtokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var nmtokenItems = new XdmValue[nmtokens.Length];
                for (int i = 0; i < nmtokens.Length; i++)
                {
                    if (!Regex.IsMatch(nmtokens[i], @"^[\w.:\-]+$"))
                        return false;
                    nmtokenItems[i] = XdmValue.FromString(nmtokens[i], "NMTOKEN");
                }
                result = XdmValue.FromSequence(MaterializedSequence.FromArray(nmtokenItems));
                return true;
            }

            case "entities":
            {
                string s = CollapseWhitespace(value.ToString());
                if (string.IsNullOrEmpty(s))
                    return false;
                var entityTokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var entityItems = new XdmValue[entityTokens.Length];
                for (int i = 0; i < entityTokens.Length; i++)
                {
                    if (!Regex.IsMatch(entityTokens[i], @"^[\p{L}_][\w.\-]*$"))
                        return false;
                    entityItems[i] = XdmValue.FromString(entityTokens[i], "ENTITY");
                }
                result = XdmValue.FromSequence(MaterializedSequence.FromArray(entityItems));
                return true;
            }

            case "qname":
                if (value.Kind == XdmValueKind.QName)
                    return true;
                if (value.Kind == XdmValueKind.String)
                {
                    string sQName = value.StringValue.Trim();
                    if (string.IsNullOrEmpty(sQName))
                        return false;
                    // Validate lexical QName: prefix:local or local (no prefix)
                    int colon = sQName.IndexOf(':');
                    string prefix;
                    string local;
                    if (colon >= 0)
                    {
                        prefix = sQName[..colon];
                        local = sQName[(colon + 1)..];
                        if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(local))
                            return false;
                    }
                    else
                    {
                        prefix = string.Empty;
                        local = sQName;
                    }

                    if (!IsValidNcName(prefix) || !IsValidNcName(local))
                        return false;

                    // Resolve namespace using the static namespace context. Without a context,
                    // a prefixed QName cannot be resolved and the cast fails. An unprefixed
                    // QName always resolves to the default element namespace or the empty namespace.
                    string namespaceUri;
                    if (context is not null)
                    {
                        if (string.IsNullOrEmpty(prefix))
                        {
                            namespaceUri = context.DefaultElementNamespace ?? string.Empty;
                        }
                        else
                        {
                            if (!context.TryResolveNamespace(prefix, out namespaceUri!))
                                return false;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(prefix))
                            return false;
                        namespaceUri = string.Empty;
                    }

                    result = XdmValue.FromQName(new XsQName(local, namespaceUri, prefix));
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    private static readonly Regex DurationPartsRegex = new(
        @"^(?<sign>[-+]?)(?<P>P)(?<Y>\d+Y)?(?<M>\d+M)?(?<D>\d+D)?(?<T>T(?<H>\d+H)?(?<Tm>\d+M)?(?<S>\d+(?:\.\d+)?S)?)?$",
        RegexOptions.Compiled);

    private static bool IsValidAnyUri(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '%')
            {
                if (i + 2 >= s.Length)
                    return false;
                if (!IsHexDigit(s[i + 1]) || !IsHexDigit(s[i + 2]))
                    return false;
            }
        }
        return true;
    }

    private static bool IsHexDigit(char c)
        => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');

    private static string CollapseWhitespace(string s)
    {
        s = s.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
        s = s.Trim(' ');
        while (s.Contains("  "))
            s = s.Replace("  ", " ");
        return s;
    }

    private static bool IsMixedDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return false;
        bool hasYm = m.Groups["Y"].Success || m.Groups["M"].Success;
        bool hasDt = m.Groups["D"].Success || m.Groups["T"].Success;
        return hasYm && hasDt;
    }

    private static string? NormalizeTimezone(string tz)
    {
        if (string.IsNullOrEmpty(tz)) return "";
        if (tz.Equals("Z", StringComparison.OrdinalIgnoreCase)) return "Z";
        int tzHour = int.Parse(tz[1..3], CultureInfo.InvariantCulture);
        int tzMin = int.Parse(tz[4..6], CultureInfo.InvariantCulture);
        if (tzHour > 14 || (tzHour == 14 && tzMin > 0) || tzMin > 59)
            return null;
        if (tzHour == 0 && tzMin == 0)
            return "Z";
        return tz;
    }

    private static string CanonicalizeDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return s;
        string sign = m.Groups["sign"].Value;
        int years = 0, months = 0, days = 0, hours = 0, minutes = 0;
        double seconds = 0;
        if (m.Groups["Y"].Success) years = int.Parse(m.Groups["Y"].Value.TrimEnd('Y'), CultureInfo.InvariantCulture);
        if (m.Groups["M"].Success) months = int.Parse(m.Groups["M"].Value.TrimEnd('M'), CultureInfo.InvariantCulture);
        if (m.Groups["D"].Success) days = int.Parse(m.Groups["D"].Value.TrimEnd('D'), CultureInfo.InvariantCulture);
        if (m.Groups["H"].Success) hours = int.Parse(m.Groups["H"].Value.TrimEnd('H'), CultureInfo.InvariantCulture);
        if (m.Groups["Tm"].Success) minutes = int.Parse(m.Groups["Tm"].Value.TrimEnd('M'), CultureInfo.InvariantCulture);
        if (m.Groups["S"].Success) seconds = double.Parse(m.Groups["S"].Value.TrimEnd('S'), CultureInfo.InvariantCulture);

        years += months / 12;
        months %= 12;

        minutes += (int)(seconds / 60);
        seconds = seconds % 60;
        hours += minutes / 60;
        minutes %= 60;
        days += hours / 24;
        hours %= 24;

        if (years == 0 && months == 0 && days == 0 && hours == 0 && minutes == 0 && seconds == 0)
            sign = "";

        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(sign)) sb.Append('-');
        sb.Append('P');
        if (years > 0) sb.Append(years).Append('Y');
        if (months > 0) sb.Append(months).Append('M');
        if (days > 0) sb.Append(days).Append('D');
        bool hasTime = hours > 0 || minutes > 0 || seconds > 0;
        if (hasTime || (years == 0 && months == 0 && days == 0))
        {
            sb.Append('T');
            if (hours > 0) sb.Append(hours).Append('H');
            if (minutes > 0) sb.Append(minutes).Append('M');
            if (seconds > 0 || (hours == 0 && minutes == 0))
                sb.Append(FormatDurationSeconds(seconds)).Append('S');
        }
        return sb.ToString();
    }

    public static string ExtractYearMonthDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return s;
        string sign = m.Groups["sign"].Value;
        int years = 0, months = 0;
        if (m.Groups["Y"].Success) years = int.Parse(m.Groups["Y"].Value.TrimEnd('Y'), CultureInfo.InvariantCulture);
        if (m.Groups["M"].Success) months = int.Parse(m.Groups["M"].Value.TrimEnd('M'), CultureInfo.InvariantCulture);
        years += months / 12;
        months %= 12;
        if (years == 0 && months == 0) sign = "";
        string result = sign + "P";
        if (years > 0) result += years + "Y";
        if (months > 0 || years == 0) result += months + "M";
        return result;
    }

    public static string ExtractDayTimeDuration(string s)
    {
        var m = DurationPartsRegex.Match(s);
        if (!m.Success) return s;
        string sign = m.Groups["sign"].Value;
        int days = 0, hours = 0, minutes = 0;
        double seconds = 0;
        if (m.Groups["D"].Success) days = int.Parse(m.Groups["D"].Value.TrimEnd('D'), CultureInfo.InvariantCulture);
        if (m.Groups["H"].Success) hours = int.Parse(m.Groups["H"].Value.TrimEnd('H'), CultureInfo.InvariantCulture);
        if (m.Groups["Tm"].Success) minutes = int.Parse(m.Groups["Tm"].Value.TrimEnd('M'), CultureInfo.InvariantCulture);
        if (m.Groups["S"].Success) seconds = double.Parse(m.Groups["S"].Value.TrimEnd('S'), CultureInfo.InvariantCulture);

        minutes += (int)(seconds / 60);
        seconds = seconds % 60;
        hours += minutes / 60;
        minutes %= 60;
        days += hours / 24;
        hours %= 24;

        if (days == 0 && hours == 0 && minutes == 0 && seconds == 0) sign = "";

        string result = sign + "P";
        if (days > 0) result += days + "D";
        bool hasTime = hours > 0 || minutes > 0 || seconds > 0 || days == 0;
        if (hasTime)
        {
            result += "T";
            if (hours > 0) result += hours + "H";
            if (minutes > 0) result += minutes + "M";
            if (seconds > 0 || (hours == 0 && minutes == 0)) result += FormatDurationSeconds(seconds) + "S";
        }
        return result;
    }

    private static string FormatDurationSeconds(double seconds)
    {
        string s = seconds.ToString("0.0#########", CultureInfo.InvariantCulture);
        s = s.TrimEnd('0').TrimEnd('.');
        if (s == "0" || s == "-0") s = "0";
        return s;
    }

    private static readonly Regex DurationComponentRegex = new(@"(\d+)([YMDHST])", RegexOptions.Compiled);

    private static bool IsValidDuration(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        // Disallow leading '+'; 'T' must be followed by at least one time component
        if (!Regex.IsMatch(s, @"^-?P(\d+Y)?(\d+M)?(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S)?)?$"))
            return false;
        if (s == "P" || s == "-P") return false;
        // Reject 'T' without following components (e.g., P1DT, P1Y24MT)
        int tIdx = s.IndexOf('T');
        if (tIdx >= 0)
        {
            bool hasTimeComponent = s.IndexOf('H', tIdx) >= 0 || s.IndexOf('M', tIdx) >= 0 || s.IndexOf('S', tIdx) >= 0;
            if (!hasTimeComponent) return false;
        }
        // Reject absurdly large components
        foreach (Match m in DurationComponentRegex.Matches(s))
        {
            if (long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) > 999999999999L)
                return false;
        }
        return true;
    }

    private static bool IsValidYearMonthDuration(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (!Regex.IsMatch(s, @"^-?P(\d+Y)?(\d+M)?$"))
            return false;
        return s.Contains('Y') || s.Contains('M');
    }

    private static bool IsValidDayTimeDuration(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        // Disallow leading '+'; 'T' must be followed by at least one time component
        if (!Regex.IsMatch(s, @"^-?P(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S)?)?$"))
            return false;
        if (s.Contains('Y')) return false;
        // Reject 'M' before 'T' (months), but allow 'M' after 'T' (minutes)
        int tIdx = s.IndexOf('T');
        if (tIdx >= 0)
        {
            if (s.IndexOf('M') >= 0 && s.IndexOf('M') < tIdx) return false;
            bool hasTimeComponent = s.IndexOf('H', tIdx) >= 0 || s.IndexOf('M', tIdx) >= 0 || s.IndexOf('S', tIdx) >= 0;
            if (!hasTimeComponent) return false;
        }
        else
        {
            if (s.Contains('M')) return false;
        }
        if (s.Contains('D')) return true;
        if (tIdx < 0) return false;
        return s.IndexOf('H', tIdx) >= 0 || s.IndexOf('M', tIdx) >= 0 || s.IndexOf('S', tIdx) >= 0;
    }

    public static bool IsValidBase64(string s)
    {
        if (string.IsNullOrEmpty(s)) return true;
        s = s.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
        if (s.Length % 4 != 0) return false;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        foreach (char c in s)
            if (!chars.Contains(c) && c != '=')
                return false;
        int eq = s.IndexOf('=');
        if (eq >= 0)
        {
            for (int i = eq; i < s.Length; i++)
                if (s[i] != '=')
                    return false;
            if (s.Length - eq > 2)
                return false;
            // Validate padding rules:
            // '==' : preceding char must be one of AQgw (lower 4 bits == 0)
            // '='  : preceding char must be one of AEIMQUYcgkosw048 (lower 2 bits == 0)
            if (eq > 0)
            {
                char lastData = s[eq - 1];
                int padding = s.Length - eq;
                if (padding == 2)
                {
                    const string validForDoublePad = "AQgw";
                    if (!validForDoublePad.Contains(lastData))
                        return false;
                }
                else if (padding == 1)
                {
                    const string validForSinglePad = "AEIMQUYcgkosw048";
                    if (!validForSinglePad.Contains(lastData))
                        return false;
                }
            }
        }
        return true;
    }

    // Annotation assertions must not be in a reserved namespace (XQST0045,
    // annotation-assertion-11..18). Unprefixed names (e.g. %public/%private) are in no
    // namespace and are always allowed.
    private static readonly string[] ReservedAnnotationNamespaces =
    {
        "http://www.w3.org/XML/1998/namespace",
        "http://www.w3.org/2001/XMLSchema",
        "http://www.w3.org/2001/XMLSchema-instance",
        "http://www.w3.org/2005/xpath-functions",
        "http://www.w3.org/2005/xpath-functions/math",
        "http://www.w3.org/2012/xquery",
    };

    // Strips leading annotation assertions ('%name', '%name(literal, ...)') from a
    // function-test type text, validating the annotation namespaces (XQST0045) and
    // resolving prefixes against the evaluation context (XPST0081 when unbound).
    private static string StripAnnotationAssertions(string typeName, EvaluationContext? context)
    {
        var s = typeName.TrimStart();
        while (s.Length > 0 && s[0] == '%')
        {
            int i = 1;
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            string? annotationNs = null;
            if (i + 1 < s.Length && s[i] == 'Q' && s[i + 1] == '{')
            {
                int closeBrace = s.IndexOf('}', i + 2);
                if (closeBrace < 0)
                    throw new InvalidOperationException($"XPST0003: Unclosed EQName in annotation assertion '{s}'.");
                annotationNs = s[(i + 2)..closeBrace];
                i = closeBrace + 1;
                while (i < s.Length && IsAnnotationNameChar(s[i])) i++;
            }
            else
            {
                int nameStart = i;
                while (i < s.Length && (IsAnnotationNameChar(s[i]) || s[i] == ':')) i++;
                var rawName = s[nameStart..i];
                int colon = rawName.IndexOf(':');
                if (colon > 0)
                {
                    var prefix = rawName[..colon];
                    if (context is null || !context.TryResolveNamespace(prefix, out var resolvedNs))
                        throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
                    annotationNs = resolvedNs;
                }
            }
            if (annotationNs is not null && ReservedAnnotationNamespaces.Contains(annotationNs))
                throw new InvalidOperationException($"XQST0045: The annotation namespace '{annotationNs}' is reserved.");
            // Optional literal argument list: skip balanced parens with string awareness.
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            if (i < s.Length && s[i] == '(')
            {
                int depth = 0;
                do
                {
                    char c = s[i];
                    if (c is '"' or '\'')
                    {
                        char quote = c;
                        i++;
                        while (i < s.Length && s[i] != quote) i++;
                        if (i < s.Length) i++;
                        continue;
                    }
                    if (c == '(') depth++;
                    else if (c == ')') depth--;
                    i++;
                } while (depth > 0 && i < s.Length);
            }
            s = s[i..].TrimStart();
        }
        return s;
    }

    private static bool IsAnnotationNameChar(char c)
        => char.IsLetterOrDigit(c) || c is '.' or '-' or '_';

    // Unwraps a singleton sequence whose item matches the declared parameter kind
    // (map, array, or function item); anything else passes through unchanged so the
    // implementation's own type error still applies.
    private static XdmValue UnwrapSingletonItem(XdmValue value, XdmValueKind kind)
    {
        if (!value.IsSequence || value.SequenceValue is null)
            return value;
        XdmValue single = default;
        int count = 0;
        foreach (var item in XdmSequence.FromSource(value.SequenceValue))
        {
            count++;
            if (count == 1) single = item;
            if (count > 1) return value;
        }
        return count == 1 && single.Kind == kind ? single : value;
    }

    private static bool InstanceOf(XdmValue value, string typeName, OccurrenceIndicator occurrence, string? defaultElementNamespace, EvaluationContext? context = null)
    {
        // Annotation assertions preceding a function test are validated (XQST0045) and
        // ignored: they can only restrict matches, which implementations may decline to do.
        typeName = StripAnnotationAssertions(typeName, context);
        // Unwrap redundant outer parentheses (hof-013).
        while (typeName.Length > 1 && typeName[0] == '(' && FindMatchingParen(typeName, 0) == typeName.Length - 1)
            typeName = typeName[1..^1].Trim();
        string normalized = NormalizeTypeName(typeName);

        if (normalized is "empty-sequence" or "empty-sequence()")
            return value.IsUndefined || (value.IsSequence && TryGetSequenceLength(value.SequenceValue, out var len) && len == 0);

        // Check cardinality
        int count;
        if (value.IsUndefined)
            count = 0;
        else if (!value.IsSequence)
            count = 1;
        else if (!TryGetSequenceLength(value.SequenceValue, out count))
        {
            // Materialize to count
            count = 0;
            foreach (var _ in XdmSequence.FromSource(value.SequenceValue!))
                count++;
        }

        bool cardinalityOk = occurrence switch
        {
            OccurrenceIndicator.One => count == 1,
            OccurrenceIndicator.ZeroOrOne => count <= 1,
            OccurrenceIndicator.ZeroOrMore => true,
            OccurrenceIndicator.OneOrMore => count >= 1,
            _ => count == 1
        };

        if (!cardinalityOk)
            return false;

        if (count == 0)
            return true;

        string effective;
        if (typeName.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
        {
            effective = normalized.StartsWith("xs:") ? normalized[3..] : normalized;
        }
        else if (typeName.StartsWith("xsd:", StringComparison.OrdinalIgnoreCase))
        {
            effective = normalized.StartsWith("xsd:") ? normalized[4..] : normalized;
        }
        else if (typeName.Contains(':') && !typeName.Contains('(') && !typeName.Contains('{'))
        {
            // A prefixed type name resolves via the in-scope namespaces: the XML Schema
            // namespace maps to the bare type; a non-XS prefix is valid only when it names
            // a user-defined schema simple type.
            int colon = typeName.IndexOf(':');
            var typePrefix = typeName[..colon];
            if (context is not null && context.TryResolveNamespace(typePrefix, out var resolvedTypeNs))
            {
                if (resolvedTypeNs == "http://www.w3.org/2001/XMLSchema")
                {
                    effective = normalized[(normalized.IndexOf(':') + 1)..];
                }
                else if (IsUserDefinedSchemaType(typeName, context, out _))
                {
                    effective = typeName;
                }
                else
                {
                    throw new InvalidOperationException("XPST0051");
                }
            }
            else
            {
                throw new InvalidOperationException($"XPST0081: Prefix '{typePrefix}' is not declared.");
            }
        }
        else
        {
            if (defaultElementNamespace == "http://www.w3.org/2001/XMLSchema")
            {
                effective = normalized;
            }
            else
            {
                // No prefix and the default namespace is not XML Schema: only node kind
                // and function/map/array/item tests are valid; bare atomic type names are not.
                if (IsKnownSequenceTypeName(normalized))
                    effective = normalized;
                else
                    throw new InvalidOperationException("XPST0051");
            }
        }

        // Function, map, array and item tests are valid regardless of the default namespace.
        if (effective is "function" or "function(*)" or "function()")
        {
            if (HasNonGenericParameters(typeName))
            {
                // Typed function test function(A) as R: delegate per item (maps and
                // arrays are matched against their implied function signature).
                if (!value.IsSequence) return ValueMatchesType(value, typeName, context);
                foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                    if (!ValueMatchesType(item, typeName, context)) return false;
                return true;
            }
            // function(*) matches every function item, including maps and arrays.
            if (!value.IsSequence) return value.IsFunction || value.IsMap || value.IsArray;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                if (!item.IsFunction && !item.IsMap && !item.IsArray) return false;
            return true;
        }

        if (effective is "map" or "map(*)" or "map()")
        {
            if (HasNonGenericParameters(typeName))
            {
                ValidateParameterizedMapArrayTypeNames(typeName, defaultElementNamespace);
                if (!value.IsSequence) return ValueMatchesType(value, typeName, context);
                foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                    if (!ValueMatchesType(item, typeName, context)) return false;
                return true;
            }
            if (!value.IsSequence) return value.IsMap;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                if (!item.IsMap) return false;
            return true;
        }

        if (effective is "array" or "array(*)" or "array()")
        {
            if (HasNonGenericParameters(typeName))
            {
                ValidateParameterizedMapArrayTypeNames(typeName, defaultElementNamespace);
                if (!value.IsSequence) return ValueMatchesType(value, typeName, context);
                foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                    if (!ValueMatchesType(item, typeName, context)) return false;
                return true;
            }
            if (!value.IsSequence) return value.IsArray;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                if (!item.IsArray) return false;
            return true;
        }

        if (effective is "item" or "item()")
        {
            if (!value.IsSequence) return !value.IsUndefined;
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                if (item.IsUndefined) return false;
            return true;
        }

        // Node kind tests (use the original typeName so that parameterised forms
        // such as element(*, xs:anyType) are evaluated by ValueMatchesType).
        if (IsKnownSequenceTypeName(effective))
        {
            if (!value.IsSequence) return ValueMatchesType(value, typeName, context);
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
            {
                if (!ValueMatchesType(item, typeName, context))
                    return false;
            }
            return true;
        }

        // Atomic type names: must be in the XML Schema namespace (either via xs: prefix
        // or via the default element/type namespace) or a user-defined schema simple type.
        if (!IsKnownAtomicTypeName(effective) && !IsUserDefinedSchemaType(effective, context, out _))
            throw new InvalidOperationException("XPST0051");

        // xs:QName is case-sensitive: only the exact local name "QName" is valid.
        // xs:qname, xs:QNAME, etc. are not known types and must raise XPST0051.
        if (effective == "qname" && GetTypeLocalName(typeName) != "QName")
            throw new InvalidOperationException("XPST0051");

        if (!value.IsSequence)
            return ValueMatchesType(value, effective, context);

        foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
        {
            if (!ValueMatchesType(item, effective, context))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns true when the type test carries parenthesised parameters other than
    /// the generic <c>()</c> and <c>(*)</c> forms (e.g. <c>map(xs:string, xs:integer)</c>,
    /// <c>array(xs:string)</c>, <c>function(xs:integer) as xs:string</c>).
    /// </summary>
    private static bool HasNonGenericParameters(string typeName)
    {
        int paren = typeName.IndexOf('(');
        if (paren < 0) return false;
        int close = FindMatchingParen(typeName, paren);
        if (close < 0) return false;
        var inner = typeName.Substring(paren + 1, close - paren - 1).Trim();
        return inner.Length > 0 && inner != "*";
    }

    /// <summary>
    /// Validates that type names nested inside a parameterised map/array/function test
    /// are namespace-qualified (or kind tests). Bare atomic names are only valid when
    /// the default element/type namespace is the XML Schema namespace (MapTest-008).
    /// </summary>
    private static void ValidateParameterizedMapArrayTypeNames(string typeName, string? defaultElementNamespace)
    {
        if (defaultElementNamespace == "http://www.w3.org/2001/XMLSchema")
            return;
        int paren = typeName.IndexOf('(');
        if (paren < 0) return;
        int close = FindMatchingParen(typeName, paren);
        if (close < 0) return;
        var inner = typeName.Substring(paren + 1, close - paren - 1);
        foreach (var part in SplitTopLevel(inner, ','))
        {
            var t = part.Trim();
            if (t.Length > 0 && t[^1] is '?' or '*' or '+')
                t = t[..^1].TrimEnd();
            if (t.Length == 0 || t == "*")
                continue;
            var lower = t.ToLowerInvariant();
            if (lower.StartsWith("map(") || lower.StartsWith("array(") || lower.StartsWith("function("))
            {
                ValidateParameterizedMapArrayTypeNames(t, defaultElementNamespace);
                continue;
            }
            if (t.Contains('(') || t.Contains(':'))
                continue; // kind test (element(), item(), ...) or prefixed QName
            throw new InvalidOperationException(
                $"XPST0051: Type name '{t}' in a map/array type test is not in the XML Schema namespace");
        }
    }

    private static string NormalizeTypeName(string typeName)
    {
        var s = typeName.Trim().ToLowerInvariant();
        if (s.Length > 0 && (s[^1] is '?' or '*' or '+'))
            s = s[..^1].TrimEnd();
        // Strip any parenthesised parameters so that forms such as
        // element(*, xs:anyType), attribute(*, T), and item() all reduce
        // to their base kind name for the initial classification.
        int paren = s.IndexOf('(');
        if (paren >= 0)
            s = s[..paren].TrimEnd();
        return s;
    }

    /// <summary>
    /// Maps an EQName atomic type name in the XML Schema namespace
    /// (<c>Q{http://www.w3.org/2001/XMLSchema}integer</c>) to the equivalent
    /// <c>xs:</c>-prefixed form (eqname-004: EQName parameter types in user function
    /// declarations). Any other EQName is returned unchanged.
    /// </summary>
    private static string NormalizeEQNameTypeName(string typeName)
    {
        if (typeName.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = typeName.IndexOf('}');
            if (close > 2 && string.Equals(typeName[2..close].Trim(), "http://www.w3.org/2001/XMLSchema", StringComparison.Ordinal))
                return string.Concat("xs:", typeName.AsSpan(close + 1));
        }
        return typeName;
    }

    /// <summary>
    /// Extracts the local name from a sequence type string, stripping an optional
    /// xs:/xsd: prefix or Q{uri} EQName wrapper. Used for case-sensitive checks
    /// where the lower-cased normalized form is no longer sufficient.
    /// </summary>
    private static string GetTypeLocalName(string typeName)
    {
        var s = typeName.Trim();
        int brace = s.IndexOf('}');
        if (brace >= 0 && brace < s.Length - 1)
            s = s[(brace + 1)..];
        int colon = s.IndexOf(':');
        if (colon >= 0 && colon < s.Length - 1)
            s = s[(colon + 1)..];
        // Strip occurrence indicator if present.
        if (s.Length > 0 && (s[^1] is '?' or '*' or '+'))
            s = s[..^1].TrimEnd();
        return s;
    }

    private static string ResolveTypeName(string original, string normalized, string? defaultElementNamespace)
    {
        if (original.Contains(':'))
        {
            if (original.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
                return normalized.StartsWith("xs:") ? normalized[3..] : normalized;
            if (original.StartsWith("xsd:", StringComparison.OrdinalIgnoreCase))
                return normalized.StartsWith("xsd:") ? normalized[4..] : normalized;
            throw new InvalidOperationException("XPST0051");
        }

        if (defaultElementNamespace == "http://www.w3.org/2001/XMLSchema")
            return normalized;

        return normalized;
    }

    private static bool IsKnownSequenceTypeName(string name)
        => name is "node" or "node()" or "element" or "element()" or "attribute" or "attribute()"
            or "document-node" or "document-node()" or "text" or "text()" or "comment" or "comment()"
            or "processing-instruction" or "processing-instruction()" or "namespace-node" or "namespace-node()"
            or "item" or "item()"
            or "function" or "function(*)" or "function()"
            or "map" or "map(*)" or "map()"
            or "array" or "array(*)" or "array()";

    private static bool IsKnownAtomicTypeName(string name)
        => name is "string" or "normalizedstring" or "token" or "language" or "nmtoken" or "name"
            or "ncname" or "id" or "idref" or "entity" or "boolean" or "integer" or "int" or "long"
            or "short" or "byte" or "unsignedshort" or "unsignedint" or "unsignedlong" or "unsignedbyte"
            or "positiveinteger" or "negativeinteger" or "nonpositiveinteger" or "nonnegativeinteger"
            or "decimal" or "double" or "float" or "numeric" or "datetime" or "datetimestamp" or "date" or "time"
            or "duration" or "daytimeduration" or "yearmonthduration" or "qname" or "anyuri" or "notation"
            or "gyear" or "gyearmonth" or "gmonthday" or "gday" or "gmonth"
            or "hexbinary" or "base64binary" or "untypedatomic" or "anyatomictype";

    private static bool ItemInstanceOf(XdmValue value, string normalized)
    {
        return normalized switch
        {
            "string" or "normalizedstring" or "token" or "language" or "nmtoken" or "name"
                or "ncname" or "id" or "idref" or "entity"
                => value.Kind == XdmValueKind.String && IsAtomicTypeSubtype(value.SchemaTypeName ?? "string", normalized),
            "integer" or "int" or "long" or "short" or "byte"
                or "unsignedshort" or "unsignedint" or "unsignedlong" or "unsignedbyte"
                or "positiveinteger" or "negativeinteger" or "nonpositiveinteger" or "nonnegativeinteger"
                => (value.Kind == XdmValueKind.Integer || (value.Kind == XdmValueKind.Decimal && IsIntegerSchemaType(value.SchemaTypeName)))
                   && IsAtomicTypeSubtype(value.SchemaTypeName ?? "integer", normalized),
            "decimal" => value.Kind is XdmValueKind.Decimal or XdmValueKind.Integer,
            "double" => value.Kind == XdmValueKind.Double,
            "float" => value.Kind == XdmValueKind.Float,
            "numeric" => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float,
            "anyatomictype" => value.Kind is >= XdmValueKind.String and <= XdmValueKind.Binary,
            "boolean" => value.Kind == XdmValueKind.Boolean,
            "datetime" => value.Kind == XdmValueKind.DateTime,
            "datetimestamp" => value.Kind == XdmValueKind.DateTime && value.HasTimezone,
            "date" => value.Kind == XdmValueKind.Date,
            "time" => value.Kind == XdmValueKind.Time,
            "duration" => value.Kind == XdmValueKind.Duration,
            "daytimeduration" => value.Kind == XdmValueKind.Duration &&
                (value.SchemaTypeName is null || value.SchemaTypeName.Equals("dayTimeDuration", StringComparison.OrdinalIgnoreCase)),
            "yearmonthduration" => value.Kind == XdmValueKind.Duration &&
                (value.SchemaTypeName is null || value.SchemaTypeName.Equals("yearMonthDuration", StringComparison.OrdinalIgnoreCase)),
            "qname" => value.Kind == XdmValueKind.QName,
            "gyear" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gYear", StringComparison.OrdinalIgnoreCase) == true,
            "gyearmonth" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gYearMonth", StringComparison.OrdinalIgnoreCase) == true,
            "gmonthday" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gMonthDay", StringComparison.OrdinalIgnoreCase) == true,
            "gday" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gDay", StringComparison.OrdinalIgnoreCase) == true,
            "gmonth" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("gMonth", StringComparison.OrdinalIgnoreCase) == true,
            "hexbinary" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("hexBinary", StringComparison.OrdinalIgnoreCase) == true,
            "base64binary" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("base64Binary", StringComparison.OrdinalIgnoreCase) == true,
            "anyuri" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("anyURI", StringComparison.OrdinalIgnoreCase) == true,
            "untypedatomic" => value.Kind == XdmValueKind.String && value.SchemaTypeName?.Equals("untypedAtomic", StringComparison.OrdinalIgnoreCase) == true,
            "notation" => false, // xs:NOTATION is abstract and cannot be instantiated in XDM
            "node" => value.IsNode,
            "element" or "element()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Element,
            "attribute" or "attribute()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Attribute,
            "document-node" or "document-node()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Document,
            "text" or "text()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Text,
            "comment" or "comment()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Comment,
            "processing-instruction" or "processing-instruction()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.ProcessingInstruction,
            "namespace-node" or "namespace-node()" => value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Namespace,
            "item" => !value.IsUndefined,
            _ => false
        };
    }

    /// <summary>
    /// Returns true when the atomic type <paramref name="actual"/> equals or derives from
    /// <paramref name="target"/> per the XSD type hierarchy (instance-of semantics).
    /// Names are case-insensitive and compared without the xs: prefix.
    /// </summary>
    private static bool IsAtomicTypeSubtype(string actual, string target)
    {
        actual = actual.ToLowerInvariant().Replace("xs:", "");
        target = target.ToLowerInvariant().Replace("xs:", "");
        if (actual == target) return true;
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(actual);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current)) continue;
            foreach (var super in GetDirectSupertypes(current))
            {
                if (super == target) return true;
                queue.Enqueue(super);
            }
        }
        return false;
    }

    private static bool IsElementTypeCompatible(string typeName, EvaluationContext? context, IXdmNode node)
    {
        var (targetNs, targetLocal) = ResolveKindTestTypeName(typeName, context);
        if (node.SchemaTypeAnnotation is not { } actual)
        {
            // Untyped element: only xs:anyType and xs:untyped match.
            return targetNs == XmlSchema.Namespace && targetLocal.ToLowerInvariant() is "anytype" or "untyped";
        }
        return IsSchemaTypeSubtype(context, actual.NamespaceUri, actual.LocalName, targetNs, targetLocal);
    }

    private static bool IsAttributeTypeCompatible(string typeName, EvaluationContext? context, IXdmNode node)
    {
        var (targetNs, targetLocal) = ResolveKindTestTypeName(typeName, context);
        if (node.SchemaTypeAnnotation is not { } actual)
        {
            // Untyped attribute: xs:untypedAtomic and its supertypes match.
            return targetNs == XmlSchema.Namespace && targetLocal.ToLowerInvariant() is "untypedatomic" or "anyatomictype" or "anysimpletype" or "anytype";
        }
        return IsSchemaTypeSubtype(context, actual.NamespaceUri, actual.LocalName, targetNs, targetLocal);
    }

    /// <summary>
    /// Resolves a type name used in <c>element(*, T)</c> / <c>attribute(*, T)</c> to an
    /// expanded QName. The xs: prefix maps to the XML Schema namespace; prefixed names are
    /// resolved against the context namespace bindings; unprefixed names use the default
    /// element namespace when one is declared.
    /// </summary>
    private static (string NamespaceUri, string LocalName) ResolveKindTestTypeName(string typeName, EvaluationContext? context)
    {
        var s = typeName.Trim().Replace("*", "").Trim();
        if (s.EndsWith('?'))
            s = s[..^1].Trim();
        if (s.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
            return (XmlSchema.Namespace, s[3..]);
        if (s.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = s.IndexOf('}');
            string ns = close > 2 ? s[2..close] : string.Empty;
            string local = close >= 0 ? s[(close + 1)..] : s;
            return (ns, local);
        }
        int colon = s.IndexOf(':');
        if (colon > 0)
        {
            string prefix = s[..colon];
            if (context is null || !context.TryResolveNamespace(prefix, out var ns))
                throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
            return (ns, s[(colon + 1)..]);
        }
        return (context?.DefaultElementNamespace ?? string.Empty, s);
    }

    /// <summary>
    /// Resolves a lexical type name (xs:local, Q{uri}local, prefix:local, or unprefixed local)
    /// to an expanded QName using the in-scope namespace bindings and default element namespace.
    /// </summary>
    private static (string NamespaceUri, string LocalName) ResolveTypeQName(string typeName, EvaluationContext? context)
    {
        var s = typeName.Trim();
        if (s.EndsWith('?') || s.EndsWith('*') || s.EndsWith('+'))
            s = s[..^1].TrimEnd();
        if (s.StartsWith("xs:", StringComparison.OrdinalIgnoreCase))
            return (XmlSchema.Namespace, s[3..]);
        if (s.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = s.IndexOf('}');
            string ns = close > 2 ? s[2..close] : string.Empty;
            string local = close >= 0 ? s[(close + 1)..] : s;
            return (ns, local);
        }
        int colon = s.IndexOf(':');
        if (colon > 0)
        {
            string prefix = s[..colon];
            if (context is null || !context.TryResolveNamespace(prefix, out var ns))
                throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
            return (ns, s[(colon + 1)..]);
        }
        return (context?.DefaultElementNamespace ?? string.Empty, s);
    }

    /// <summary>
    /// Looks up a type by expanded QName in the evaluation context's schema set.
    /// Returns true when the type is present and is a simple type with a datatype.
    /// </summary>
    private static bool TryGetSchemaSimpleType(string namespaceUri, string localName, EvaluationContext? context, [NotNullWhen(true)] out XmlSchemaSimpleType? simpleType)
    {
        simpleType = null;
        if (context?.SchemaSet is null)
            return false;
        if (context.SchemaSet.GlobalTypes[new XmlQualifiedName(localName, namespaceUri)] is not XmlSchemaSimpleType simple)
            return false;
        simpleType = simple;
        return simpleType.Datatype is not null;
    }

    /// <summary>
    /// Returns true when the sequence-type name denotes a user-defined schema simple type
    /// (i.e. not a built-in xs:* type) that is present in the evaluation context's schema set.
    /// </summary>
    private static bool IsUserDefinedSchemaType(string typeName, EvaluationContext? context, [NotNullWhen(true)] out XmlSchemaSimpleType? simpleType)
    {
        simpleType = null;
        if (context?.SchemaSet is null)
            return false;
        var (ns, local) = ResolveTypeQName(typeName, context);
        if (ns == XmlSchema.Namespace)
            return false;
        return TryGetSchemaSimpleType(ns, local, context, out simpleType);
    }

    /// <summary>
    /// Walks a derived schema type up to its built-in XML Schema base and returns the
    /// local name, or null when no built-in base can be reached.
    /// </summary>
    private static string? GetBuiltInBaseTypeName(XmlSchemaType type)
    {
        var visited = new HashSet<XmlSchemaType>();
        var current = type;
        while (current is not null && visited.Add(current))
        {
            if (current.QualifiedName.Namespace == XmlSchema.Namespace)
                return current.QualifiedName.Name;
            current = current.BaseXmlSchemaType;
        }
        return null;
    }

    /// <summary>
    /// Converts a .NET value returned by <see cref="XmlSchemaDatatype.ParseValue"/> into an
    /// <see cref="XdmValue"/> with the appropriate schema-type annotation.
    /// </summary>
    private static XdmValue ConvertSchemaValue(object value, XmlSchemaDatatype datatype, XmlSchemaType schemaType, bool hasTimezone = true)
    {
        string typeName = schemaType.QualifiedName.Name;
        string typeNs = schemaType.QualifiedName.Namespace;

        if (typeNs != XmlSchema.Namespace)
        {
            // User-defined type: derive the annotation from the ultimate built-in base type.
            typeName = GetBuiltInBaseTypeName(schemaType) ?? "untypedAtomic";
        }

        switch (value)
        {
            case bool b:
                return XdmValue.FromBoolean(b);
            case decimal d:
                return XdmValue.FromDecimal(d, typeName);
            case float f:
                return XdmValue.FromFloat(f);
            case double d:
                return XdmValue.FromDouble(d);
            case byte u8: return XdmValue.FromInteger(u8, typeName);
            case sbyte i8: return XdmValue.FromInteger(i8, typeName);
            case short i16: return XdmValue.FromInteger(i16, typeName);
            case ushort u16: return XdmValue.FromInteger(u16, typeName);
            case int i32: return XdmValue.FromInteger(i32, typeName);
            case uint u32:
                return XdmValue.FromInteger((long)u32, typeName);
            case long i64:
                return XdmValue.FromInteger(i64, typeName);
            case ulong u64:
                if (u64 <= (ulong)long.MaxValue)
                    return XdmValue.FromInteger((long)u64, typeName);
                return XdmValue.FromDecimal(u64, typeName);
            case DateTime dt:
                return ConvertSchemaDateTime(dt, typeName, hasTimezone);
            case DateTimeOffset dto:
                return ConvertSchemaDateTime(dto.DateTime, typeName, hasTimezone);
            case XmlQualifiedName qn:
                return XdmValue.FromQName(new XsQName(qn.Namespace, qn.Name));
            case string s:
                return XdmValue.FromString(s, typeName);
            case byte[] bytes:
                // XdmValue stores binary types as annotated strings.
                string text = typeName.Equals("hexBinary", StringComparison.OrdinalIgnoreCase)
                    ? Convert.ToHexString(bytes)
                    : Convert.ToBase64String(bytes);
                return XdmValue.FromString(text, typeName);
            case TimeSpan ts:
                // xs:dayTimeDuration / xs:yearMonthDuration are represented as strings in Bosak.
                return XdmValue.FromDuration(ts.ToString(), typeName);
            default:
                return XdmValue.FromString(value.ToString() ?? string.Empty, typeName);
        }
    }

    private static XdmValue ConvertSchemaDateTime(DateTime dt, string typeName, bool hasTimezone)
    {
        var offset = dt.Kind == DateTimeKind.Unspecified ? TimeSpan.Zero : TimeZoneInfo.Local.GetUtcOffset(dt);
        var dto = new DateTimeOffset(dt, offset);
        return typeName.ToLowerInvariant() switch
        {
            "date" => XdmValue.FromDate(dto, hasTimezone),
            "time" => XdmValue.FromTime(dto, hasTimezone),
            "gyear" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gYear"),
            "gyearmonth" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gYearMonth"),
            "gmonth" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gMonth"),
            "gmonthday" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gMonthDay"),
            "gday" => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: "gDay"),
            _ => XdmValue.FromDateTime(dto, hasTimezone, schemaTypeName: typeName)
        };
    }

    /// <summary>
    /// Returns true when a schema-validated lexical date/time value carries an explicit
    /// timezone (trailing <c>Z</c> or <c>+/-hh:mm</c>).
    /// </summary>
    private static bool LexicalHasTimezone(string? lexical)
    {
        if (string.IsNullOrEmpty(lexical))
            return false;
        var s = lexical.AsSpan().Trim();
        if (s.EndsWith("Z".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return true;
        // Match a trailing +hh:mm or -hh:mm timezone offset.
        if (s.Length >= 6)
        {
            char c = s[^6];
            if ((c == '+' || c == '-') && s[^3] == ':')
            {
                for (int i = s.Length - 5; i < s.Length - 3; i++)
                {
                    if (!char.IsDigit(s[i]))
                        return false;
                }
                for (int i = s.Length - 2; i < s.Length; i++)
                {
                    if (!char.IsDigit(s[i]))
                        return false;
                }
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true when the schema type <paramref name="actualNs"/>:<paramref name="actualLocal"/>
    /// equals or derives from <paramref name="targetNs"/>:<paramref name="targetLocal"/>.
    /// </summary>
    private static bool IsSchemaTypeSubtype(EvaluationContext? context, string actualNs, string actualLocal, string targetNs, string targetLocal)
    {
        if (string.Equals(actualNs, targetNs, StringComparison.Ordinal)
            && string.Equals(actualLocal, targetLocal, StringComparison.Ordinal))
            return true;

        // Built-in atomic type hierarchy.
        if (targetNs == XmlSchema.Namespace && actualNs == XmlSchema.Namespace)
            return IsAtomicTypeSubtype(actualLocal, targetLocal);

        // User-defined or mixed: walk the schema-set type hierarchy.
        if (context?.SchemaSet is null)
            return false;

        try
        {
            var actualQName = new XmlQualifiedName(actualLocal, actualNs);
            var actualType = context.SchemaSet.GlobalTypes[actualQName] as XmlSchemaType;
            if (actualType is null)
                return false;
            var targetQName = new XmlQualifiedName(targetLocal, targetNs);
            var visited = new HashSet<XmlSchemaType>();
            var current = actualType;
            while (current is not null && visited.Add(current))
            {
                if (current.QualifiedName == targetQName)
                    return true;
                current = current.BaseXmlSchemaType;
            }
        }
        catch
        {
            // Ignore schema-lookup errors and fall through to false.
        }
        return false;
    }

    // Validates the schema type name of an element(name, T) / attribute(name, T) test.
    // A prefixed or URI-qualified T must resolve to a built-in schema type: XPST0081 for
    // an undeclared prefix, XPST0008 for an unknown type (static-context-1). Unprefixed
    // names keep the lenient local interpretation used for element-type compatibility.
    private static void ValidateKindTestSchemaType(string typeName, EvaluationContext? context)
    {
        if (context is null)
            return;
        var s = GetCasePreservedTypeName(typeName);
        int open = s.IndexOf('(');
        int close = s.LastIndexOf(')');
        if (open < 0 || close <= open)
            return;
        var inner = s.Substring(open + 1, close - open - 1);
        int comma = inner.IndexOf(',');
        if (comma < 0)
            return;
        var typePart = inner[(comma + 1)..].Trim();
        if (typePart.EndsWith('?'))
            typePart = typePart[..^1].TrimEnd();
        if (typePart.Length == 0 || typePart == "*")
            return;
        if (!typePart.Contains(':') && !typePart.StartsWith("Q{", StringComparison.Ordinal))
            return;
        ValidateKindTestTypeName(typePart, context);
    }

    // Validates a schema type name used in a kind test (element(foo, T) / attribute(foo, T)):
    // the name must resolve (XPST0081 for an undeclared prefix) and must designate a known
    // built-in schema type or a type declared in an imported schema (XPST0008 otherwise).
    private static void ValidateKindTestTypeName(string typeName, EvaluationContext context)
    {
        string local;
        string ns;
        if (typeName.StartsWith("Q{", StringComparison.Ordinal))
        {
            int close = typeName.IndexOf('}');
            ns = close > 2 ? typeName[2..close] : string.Empty;
            local = close >= 0 ? typeName[(close + 1)..] : typeName;
        }
        else
        {
            int colon = typeName.IndexOf(':');
            if (colon > 0)
            {
                var prefix = typeName[..colon];
                if (!context.TryResolveNamespace(prefix, out var resolved))
                    throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
                ns = resolved;
                local = typeName[(colon + 1)..];
            }
            else
            {
                ns = context.DefaultElementNamespace ?? string.Empty;
                local = typeName;
            }
        }
        if (ns == "http://www.w3.org/2001/XMLSchema" && BuiltInSchemaTypes.Contains(local))
            return;
        if (context.SchemaSet is not null && context.SchemaSet.GlobalTypes[new XmlQualifiedName(local, ns)] is XmlSchemaType)
            return;
        throw new InvalidOperationException($"XPST0008: The type '{typeName}' is not declared.");
    }

    /// <summary>The built-in XML Schema type names (XSD 1.0 + 1.1) plus the XPath types.</summary>
    private static readonly HashSet<string> BuiltInSchemaTypes = new(StringComparer.Ordinal)
    {
        "anyType", "anySimpleType", "anyAtomicType", "untyped", "untypedAtomic",
        "string", "normalizedString", "token", "language", "NMTOKEN", "NMTOKENS",
        "Name", "NCName", "ID", "IDREF", "IDREFS", "ENTITY", "ENTITIES",
        "boolean", "decimal", "float", "double", "duration", "dateTime", "time", "date",
        "gYearMonth", "gYear", "gMonthDay", "gDay", "gMonth",
        "hexBinary", "base64Binary", "anyURI", "QName", "NOTATION",
        "integer", "nonPositiveInteger", "negativeInteger", "long", "int", "short", "byte",
        "nonNegativeInteger", "unsignedLong", "unsignedInt", "unsignedShort", "unsignedByte",
        "positiveInteger", "dateTimeStamp", "dayTimeDuration", "yearMonthDuration",
    };

    private static bool IsCastAllowed(string? sourceSchemaType, string targetType)
    {
        if (string.IsNullOrEmpty(sourceSchemaType))
            return true;

        sourceSchemaType = sourceSchemaType.ToLowerInvariant().Replace("xs:", "");
        targetType = targetType.ToLowerInvariant().Replace("xs:", "");

        // gYear, gYearMonth, gMonthDay, gDay, gMonth can only cast to themselves, string, untypedAtomic
        if (sourceSchemaType is "gyear" or "gyearmonth" or "gmonthday" or "gday" or "gmonth")
        {
            return sourceSchemaType == targetType || targetType is "string" or "untypedatomic";
        }

        // hexBinary and base64Binary can cast to themselves, each other, string, untypedAtomic
        if (sourceSchemaType is "hexbinary" or "base64binary")
        {
            return targetType is "hexbinary" or "base64binary" or "string" or "untypedatomic";
        }

        // anyURI can cast to itself, string, untypedAtomic
        if (sourceSchemaType == "anyuri")
        {
            return targetType is "anyuri" or "string" or "untypedatomic";
        }

        // Other schema types (normalizedString, token, etc.) allow any cast
        return true;
    }

    private static bool IsIntegerInRange(long value, string typeName)
    {
        return typeName switch
        {
            "byte" => value >= sbyte.MinValue && value <= sbyte.MaxValue,
            "short" => value >= short.MinValue && value <= short.MaxValue,
            "int" => value >= int.MinValue && value <= int.MaxValue,
            "long" or "integer" => true,
            "unsignedbyte" => value >= byte.MinValue && value <= byte.MaxValue,
            "unsignedshort" => value >= ushort.MinValue && value <= ushort.MaxValue,
            "unsignedint" => value >= uint.MinValue && value <= uint.MaxValue,
            "unsignedlong" => value >= 0,
            "positiveinteger" => value > 0,
            "negativeinteger" => value < 0,
            "nonpositiveinteger" => value <= 0,
            "nonnegativeinteger" => value >= 0,
            _ => true
        };
    }

    private static bool IsIntegerSchemaType(string? schemaTypeName)
    {
        if (string.IsNullOrEmpty(schemaTypeName)) return false;
        return schemaTypeName.ToLowerInvariant() is
            "integer" or "int" or "long" or "short" or "byte"
            or "unsignedshort" or "unsignedint" or "unsignedlong" or "unsignedbyte"
            or "positiveinteger" or "negativeinteger" or "nonpositiveinteger" or "nonnegativeinteger";
    }

    /// <summary>
    /// Checks whether an XDM value matches a declared type name (e.g. "xs:string", "element(foo)").
    /// </summary>
    public static bool ValueMatchesType(XdmValue value, string typeName)
        => ValueMatchesType(value, typeName, null);

    /// <summary>
    /// Checks whether a value matches a sequence type, with an optional evaluation
    /// context that enables signature-aware matching of named function items.
    /// </summary>
    public static bool ValueMatchesType(XdmValue value, string typeName, EvaluationContext? context)
    {
        if (string.IsNullOrEmpty(typeName)) return true;

        // Unwrap redundant outer parentheses: (function(xs:integer) as xs:integer) is the
        // same type as function(xs:integer) as xs:integer (hof-013).
        var unwrapped = typeName.Trim();
        while (unwrapped.Length > 1 && unwrapped[0] == '(' && FindMatchingParen(unwrapped, 0) == unwrapped.Length - 1)
            unwrapped = unwrapped[1..^1].Trim();
        typeName = NormalizeEQNameTypeName(unwrapped);

        // empty-sequence() only matches the empty sequence.
        if (typeName.Trim().Equals("empty-sequence()", StringComparison.OrdinalIgnoreCase))
        {
            if (value.IsUndefined) return true;
            return value.IsSequence && TryGetSequenceLength(value.SequenceValue, out var esl) && esl == 0;
        }

        // Sequence values (including the empty sequence) must be checked against the
        // occurrence indicator of the sequence type. Each item is matched against the
        // base type recursively so that node tests, function types, and atomic types
        // are handled uniformly.
        if (value.IsUndefined || (value.IsSequence && value.SequenceValue != null))
        {
            var trimmed = typeName.Trim();
            char occ = '\0';
            if (trimmed.Length > 0 && "?+*".Contains(trimmed[^1]))
            {
                occ = trimmed[^1];
                trimmed = trimmed[..^1].TrimEnd();
            }

            var items = new List<XdmValue>();
            if (!value.IsUndefined && value.SequenceValue != null)
            {
                foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                    items.Add(item);
            }

            switch (occ)
            {
                case '?':
                    if (items.Count > 1) return false;
                    break;
                case '*':
                    break;
                case '+':
                    if (items.Count == 0) return false;
                    break;
                default:
                    if (items.Count != 1) return false;
                    break;
            }

            foreach (var item in items)
            {
                if (!ValueMatchesType(item, trimmed, context))
                    return false;
            }
            return true;
        }

        string normalized = typeName.Trim().ToLowerInvariant();

        // Unwrap one layer of redundant outer parentheses: (function(...) as T) is
        // equivalent to function(...) as T.
        if (normalized.Length > 1 && normalized[0] == '(' && FindMatchingParen(normalized, 0) == normalized.Length - 1)
            return ValueMatchesType(value, typeName.Trim()[1..^1], context);

        // Strip occurrence indicator for non-sequence values. For function-family
        // types a trailing indicator only counts as an outer occurrence when it
        // directly follows the closing parenthesis of the parameter list; after an
        // 'as' clause it belongs to the return type (ArrayTest-063).
        if (normalized.Length > 0 && normalized[^1] is '?' or '*' or '+')
        {
            if (normalized.StartsWith("function(") || normalized.StartsWith("map(") || normalized.StartsWith("array("))
                (normalized, _) = StripOuterOccurrence(normalized);
            else
                normalized = normalized[..^1].TrimEnd();
        }

        // Strip xs:/xsd: prefix
        if (normalized.StartsWith("xs:"))
            normalized = normalized[3..];
        else if (normalized.StartsWith("xsd:"))
            normalized = normalized[4..];

        if (normalized == "item()")
            return !value.IsUndefined;

        if (normalized == "node()")
            return value.IsNode;

        if (normalized.StartsWith("element(") && normalized.EndsWith(')'))
        {
            ValidateKindTestSchemaType(typeName, context);
            if (!value.IsNode || value.NodeValue.NodeKind != XdmNodeKind.Element)
                return false;
            var inner = normalized.Substring(8, normalized.Length - 9).Trim();
            // element() or element(*) → any element
            if (string.IsNullOrEmpty(inner) || inner == "*")
                return true;
            // element(*, T) → check type compatibility
            if (inner.StartsWith("*, "))
            {
                var typePart = inner.Substring(3).Trim();
                return IsElementTypeCompatible(typePart, context, value.NodeValue);
            }
            // element(name) or element(name, T) → check name match.
            // Use the case-preserved type string so local names such as 'A' are not lowercased.
            var casePreserved = GetCasePreservedTypeName(typeName);
            var cpInner = casePreserved.Substring(8, casePreserved.Length - 9).Trim();
            var namePart = cpInner.Split(',')[0].Trim();
            if (namePart != "*")
            {
                if (namePart.StartsWith("Q{", StringComparison.Ordinal))
                {
                    // Q{uri}local — match namespace URI and local name exactly.
                    var closeBrace = namePart.IndexOf('}');
                    if (closeBrace > 2)
                    {
                        if (value.NodeValue.LocalName != namePart[(closeBrace + 1)..] ||
                            value.NodeValue.NamespaceUri != namePart[2..closeBrace])
                            return false;
                    }
                }
                else
                {
                    var testLocalName = namePart.Contains(':') ? namePart[(namePart.IndexOf(':') + 1)..] : namePart;
                    if (value.NodeValue.LocalName != testLocalName)
                        return false;
                    if (namePart.Contains(':'))
                    {
                        // A prefixed element name also matches on the resolved namespace URI when a
                        // resolution context is available (K2-DirectConElemNamespace-79: element(P:L)
                        // distinguishes URL1 from URL2); context-less checks stay local-name-only.
                        if (context is not null)
                        {
                            var namePrefix = namePart[..namePart.IndexOf(':')];
                            if (!context.TryResolveNamespace(namePrefix, out var nameNs))
                                throw new InvalidOperationException($"XPST0081: Prefix '{namePrefix}' is not declared.");
                            if (value.NodeValue.NamespaceUri != nameNs)
                                return false;
                        }
                    }
                    else if (value.NodeValue.NamespaceUri != (context?.DefaultElementNamespace ?? ""))
                    {
                        // An unprefixed element name uses the default element namespace
                        // (or no namespace when none is declared).
                        return false;
                    }
                }
            }
            if (inner.Contains(','))
            {
                var typePart = inner.Substring(inner.IndexOf(',') + 1).Trim();
                return IsElementTypeCompatible(typePart, context, value.NodeValue);
            }
            return true;
        }

        if (normalized.StartsWith("attribute(") && normalized.EndsWith(')'))
        {
            ValidateKindTestSchemaType(typeName, context);
            if (!value.IsNode || value.NodeValue.NodeKind != XdmNodeKind.Attribute)
                return false;
            var inner = normalized.Substring(10, normalized.Length - 11).Trim();
            // attribute() or attribute(*) → any attribute
            if (string.IsNullOrEmpty(inner) || inner == "*")
                return true;
            // attribute(*, T) → check type compatibility
            if (inner.StartsWith("*, "))
            {
                var typePart = inner.Substring(3).Trim();
                return IsAttributeTypeCompatible(typePart, context, value.NodeValue);
            }
            // attribute(name) or attribute(name, T) → check name match.
            // Use the case-preserved type string so local names keep their original case.
            var casePreserved = GetCasePreservedTypeName(typeName);
            var cpInner = casePreserved.Substring(10, casePreserved.Length - 11).Trim();
            var namePart = cpInner.Split(',')[0].Trim();
            if (namePart != "*")
            {
                if (namePart.StartsWith("Q{", StringComparison.Ordinal))
                {
                    // Q{uri}local — match namespace URI and local name exactly.
                    var closeBrace = namePart.IndexOf('}');
                    if (closeBrace > 2)
                    {
                        if (value.NodeValue.LocalName != namePart[(closeBrace + 1)..] ||
                            value.NodeValue.NamespaceUri != namePart[2..closeBrace])
                            return false;
                    }
                }
                else
                {
                    var testLocalName = namePart.Contains(':') ? namePart[(namePart.IndexOf(':') + 1)..] : namePart;
                    if (value.NodeValue.LocalName != testLocalName)
                        return false;
                    if (namePart.Contains(':'))
                    {
                        // A prefixed attribute name also matches on the resolved namespace URI when a
                        // resolution context is available; context-less checks stay local-name-only.
                        if (context is not null)
                        {
                            var namePrefix = namePart[..namePart.IndexOf(':')];
                            if (!context.TryResolveNamespace(namePrefix, out var nameNs))
                                throw new InvalidOperationException($"XPST0081: Prefix '{namePrefix}' is not declared.");
                            if (value.NodeValue.NamespaceUri != nameNs)
                                return false;
                        }
                    }
                    else if (value.NodeValue.NamespaceUri != "")
                    {
                        // An unprefixed attribute name always matches no namespace.
                        return false;
                    }
                }
            }
            if (inner.Contains(','))
            {
                var typePart = inner.Substring(inner.IndexOf(',') + 1).Trim();
                return IsAttributeTypeCompatible(typePart, context, value.NodeValue);
            }
            return true;
        }

        // document-node(element(...)) — check document node and its single child element
        if (normalized.StartsWith("document-node(element(") && normalized.EndsWith(')'))
        {
            if (!value.IsNode || value.NodeValue.NodeKind != XdmNodeKind.Document)
                return false;
            var childElems = new List<XdmValue>();
            foreach (var c in value.NodeValue.Axis(XdmAxis.Child))
            {
                if (c.NodeValue?.NodeKind == XdmNodeKind.Element)
                    childElems.Add(c);
            }
            if (childElems.Count != 1)
                return false;
            // Preserve the original case of the nested kind test (e.g. element(Root)).
            var casePreserved = GetCasePreservedTypeName(typeName);
            var inner = casePreserved.Substring("document-node(".Length, casePreserved.Length - "document-node(".Length - 1);
            return ValueMatchesType(XdmValue.FromNode(childElems[0].NodeValue!), inner, context);
        }

        if (normalized is "document-node()" or "document-node")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Document;

        if (normalized is "text()" or "text")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Text;

        if (normalized is "comment()" or "comment")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Comment;

        if (normalized is "processing-instruction()" or "processing-instruction")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.ProcessingInstruction;

        if (normalized is "namespace-node()" or "namespace-node")
            return value.IsNode && value.NodeValue.NodeKind == XdmNodeKind.Namespace;

        // Handle typed function signatures before general normalization, because function
        // type strings contain nested type names whose occurrence indicators and xs: prefixes
        // must not be stripped by the general normalization logic.
        string trimmedLower = typeName.Trim().ToLowerInvariant();
        if (trimmedLower.StartsWith("function(") && !trimmedLower.StartsWith("function(*)"))
        {
            if (TryParseFunctionType(typeName.Trim(), out var testParamTypes, out var testReturnType))
            {
                // function(*) wildcard falls through to the check below
                bool isFunctionStar = testParamTypes.Length == 1 && testParamTypes[0] == "*";
                if (!isFunctionStar)
                {
                    // Maps and arrays are function items: match them against their
                    // implied signature (MapTest-059..066, ArrayTest-043).
                    if (value.IsMap || value.IsArray)
                        return MapOrArrayMatchesFunctionType(value, testParamTypes, testReturnType);
                    if (!value.IsFunction) return false;
                    if (TryGetInlineFunctionSignature(value, out var actualParamTypes, out var actualReturnType))
                    {
                        if (actualParamTypes.Length != testParamTypes.Length) return false;
                        // Parameter types are contravariant: test param must be subtype of actual param
                        for (int i = 0; i < testParamTypes.Length; i++)
                        {
                            if (!IsSequenceTypeSubtype(testParamTypes[i], actualParamTypes[i]))
                                return false;
                        }
                        // Return type is covariant: actual return must be subtype of test return
                        if (!IsSequenceTypeSubtype(actualReturnType, testReturnType))
                            return false;
                        return true;
                    }
                    // Named function items: when an evaluation context is available,
                    // match against the registered declared signature (ArrayTest-064/084).
                    if (context != null)
                        return FunctionItemInstanceOf(value, typeName, context);
                    // Named, curried, or delegate function items: the declared parameter and
                    // return types are not available without an evaluation context, so match
                    // on arity only. Argument types are re-checked against the target
                    // function's own signature when the item is invoked.
                    return TryGetFunctionArity(value, out var arity) && arity == testParamTypes.Length;
                }
            }
        }

        if (normalized is "function(*)" or "function")
            return value.IsFunction || value.IsMap || value.IsArray;

        // Parameterized map types: map(K, V). Empty maps match any key/value types;
        // otherwise every entry must match the declared key and value types.
        if (normalized.StartsWith("map(") && normalized.EndsWith(')'))
        {
            if (!value.IsMap) return false;
            var inner = normalized.Substring(4, normalized.Length - 5).Trim();
            if (string.IsNullOrEmpty(inner) || inner == "*")
                return true;
            var parts = SplitTopLevel(inner, ',');
            if (parts.Length != 2)
                throw new InvalidOperationException(
                    "XPST0003: A map type test takes either zero or two arguments, e.g. map(xs:string, xs:integer)");
            string keyType = parts[0].Trim();
            string valueType = parts[1].Trim();
            if (keyType.Length > 0 && keyType[^1] is '?' or '*' or '+')
                throw new InvalidOperationException(
                    "XPST0003: The key type of a map type test must be an item type without an occurrence indicator");
            foreach (var entry in value.MapValue.Entries)
            {
                if (!ValueMatchesType(entry.Key, keyType, context)) return false;
                if (!ValueMatchesType(entry.Value, valueType, context)) return false;
            }
            return true;
        }

        // Parameterized array types: array(T). Empty arrays match any member type;
        // otherwise every member must match the declared type.
        if (normalized.StartsWith("array(") && normalized.EndsWith(')'))
        {
            if (!value.IsArray) return false;
            var inner = normalized.Substring(6, normalized.Length - 7).Trim();
            if (string.IsNullOrEmpty(inner) || inner == "*")
                return true;
            foreach (var member in value.ArrayValue.Values)
            {
                if (!ValueMatchesType(member, inner, context)) return false;
            }
            return true;
        }

        if (normalized is "map(*)" or "map")
            return value.IsMap;

        if (normalized is "array(*)" or "array")
            return value.IsArray;

        // User-defined schema simple types: a value matches when it is atomic and can be
        // cast to the type under XSD facet rules (qischema040 parameter/return validation).
        if (IsUserDefinedSchemaType(typeName, context, out _))
            return value.IsAtomic && TryCast(value, typeName, context, out _);

        return ItemInstanceOf(value, normalized);
    }

    /// <summary>
    /// Strips occurrence indicators and the <c>xs:/xsd:</c> prefix from a type name
    /// while preserving the original case of the remaining text (needed for element
    /// and attribute local-name matching).
    /// </summary>
    private static string GetCasePreservedTypeName(string typeName)
    {
        var s = typeName.Trim();
        if (s.EndsWith('?') || s.EndsWith('*') || s.EndsWith('+'))
            s = s[..^1].TrimEnd();
        if (s.StartsWith("xs:"))
            s = s[3..];
        else if (s.StartsWith("xsd:"))
            s = s[4..];
        return s;
    }

    /// <summary>
    /// Parses a function type string such as <c>function(item()*, xs:double) as xs:double</c>.
    /// </summary>
    /// <summary>
    /// Parses a function type string such as <c>function(item()*, xs:double) as xs:double</c>.
    /// Returns false for malformed types; a typed function test must include the
    /// <c>as</c> return clause (only <c>function(*)</c> may omit it).
    /// </summary>
    /// <param name="typeName">The function type string.</param>
    /// <param name="paramTypes">The declared parameter sequence types.</param>
    /// <param name="returnType">The declared return sequence type.</param>
    /// <returns>True if the type string is well-formed.</returns>
    public static bool TryParseFunctionType(string typeName, out string[] paramTypes, out string returnType)
    {
        paramTypes = [];
        returnType = "item()*";

        string s = typeName.Trim();
        if (!s.StartsWith("function(", StringComparison.OrdinalIgnoreCase))
            return false;

        int openIdx = s.IndexOf('(');
        int closeIdx = FindMatchingParen(s, openIdx);
        if (closeIdx < 0) return false;

        string paramList = s.Substring(openIdx + 1, closeIdx - openIdx - 1).Trim();
        if (string.IsNullOrEmpty(paramList))
        {
            paramTypes = [];
        }
        else if (paramList == "*")
        {
            paramTypes = ["*"];
        }
        else
        {
            paramTypes = SplitTopLevel(paramList, ',');
        }

        string after = s.Substring(closeIdx + 1).Trim();
        if (after.StartsWith("as ", StringComparison.OrdinalIgnoreCase))
        {
            returnType = after.Substring(3).Trim();
            return true;
        }

        // function(*) is the only form that may omit the 'as' return clause; a typed
        // function test without 'as' is malformed (XPST0003).
        if (paramList == "*" && string.IsNullOrEmpty(after))
            return true;
        return false;
    }

    /// <summary>
    /// Finds the index of the closing parenthesis that matches the opening parenthesis at <paramref name="openIdx"/>.
    /// </summary>
    private static int FindMatchingParen(string s, int openIdx)
    {
        int depth = 0;
        for (int i = openIdx; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')') depth--;
            if (depth == 0) return i;
        }
        return -1;
    }

    /// <summary>
    /// Splits a string by a delimiter, respecting nested parentheses.
    /// </summary>
    private static string[] SplitTopLevel(string s, char delimiter)
    {
        var parts = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')') depth--;
            else if (s[i] == delimiter && depth == 0)
            {
                parts.Add(s.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        parts.Add(s.Substring(start).Trim());
        return parts.ToArray();
    }

    /// <summary>
    /// Extracts the declared parameter and return types from an inline function item.
    /// </summary>
    private static bool TryGetInlineFunctionSignature(XdmValue value, out string[] paramTypes, out string returnType)
    {
        paramTypes = [];
        returnType = "item()*";

        if (!value.IsFunction) return false;

        var func = value.FunctionValue as FunctionItem;
        if (func is InlineFunctionItem inline)
        {
            paramTypes = inline.ParameterTypes.Select(pt => pt ?? "item()*").ToArray();
            returnType = inline.ReturnType ?? "item()*";
            return true;
        }

        return false;
    }

    /// <summary>
    /// XPath 3.1 function conversion rules: a function item is coercible to a function
    /// type with the same arity. Parameter and return type mismatches surface as dynamic
    /// errors when the coerced function is invoked, not at coercion time.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <param name="functionType">The target function type, e.g. <c>function(xs:string) as xs:string</c>.</param>
    /// <returns>True if <paramref name="value"/> is a function item coercible to <paramref name="functionType"/>.</returns>
    public static bool FunctionItemCoercibleTo(XdmValue value, string functionType)
    {
        var t = functionType.Trim();
        while (t.Length > 1 && t[0] == '(' && FindMatchingParen(t, 0) == t.Length - 1)
            t = t[1..^1].Trim();
        if (!t.StartsWith("function(", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!value.IsFunction)
            return false;
        if (!TryParseFunctionType(t, out var paramTypes, out _))
            throw new InvalidOperationException($"XPST0003: Malformed function type '{functionType}'");
        if (paramTypes.Length == 1 && paramTypes[0] == "*")
            return true; // function(*)
        return TryGetFunctionArity(value, out var arity) && arity == paramTypes.Length;
    }

    /// <summary>
    /// Returns the effective arity of a function item (the number of arguments a dynamic
    /// call must supply). For curried items this is the number of unbound placeholders.
    /// </summary>
    private static bool TryGetFunctionArity(XdmValue value, out int arity)
    {
        arity = 0;
        if (!value.IsFunction) return false;
        switch (value.FunctionValue as FunctionItem)
        {
            case InlineFunctionItem inline:
                arity = inline.Parameters.Count;
                return true;
            case NamedFunctionItem named:
                arity = named.ArityValue;
                return true;
            case DelegateFunctionItem del:
                arity = del.ArityValue;
                return true;
            case CurriedFunctionItem curried:
                arity = curried.FixedArgs.Count(a => a is null);
                return true;
            case CoercedFunctionItem coerced:
                arity = coerced.ParamTypes.Count;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Context-aware function type test (<c>instance of function(...) as ...</c>):
    /// named function items are matched against their registered declared signature
    /// using XPath 3.1 function subtyping (contravariant parameters, covariant result);
    /// all other function items use the static matching rules.
    /// </summary>
    private static bool FunctionItemInstanceOf(XdmValue value, string typeName, EvaluationContext context)
    {
        if (!TryParseFunctionType(typeName.Trim(), out var testParamTypes, out var testReturnType))
            return false;
        if (testParamTypes.Length == 1 && testParamTypes[0] == "*")
            return value.IsFunction || value.IsMap || value.IsArray;
        // Maps and arrays are function items (MapTest-059..066, ArrayTest-042/043).
        if (value.IsMap || value.IsArray)
            return MapOrArrayMatchesFunctionType(value, testParamTypes, testReturnType);
        if (value.FunctionValue is not FunctionItem func)
            return false;

        if (func is NamedFunctionItem named
            && context.TryResolveFunction(named.NamespaceUri, named.LocalName, named.ArityValue, out var sig))
        {
            // Precise declared sequence types when the signature provides them; otherwise
            // derive a coarse but sound approximation from the kind-level metadata
            // (Sequence → item()*, Function → function(*), ...) so built-ins such as
            // filter#2 subtype-check structurally instead of by arity alone
            // (instanceof132/133/134).
            var actualParams = sig.ParameterTypeNames ?? CoarseParameterTypeNames(sig, named.ArityValue);
            if (actualParams.Count != testParamTypes.Length)
                return false;
            // Parameter types are contravariant: test param must be a subtype of the actual param.
            for (int i = 0; i < testParamTypes.Length; i++)
            {
                var actualParam = actualParams[i] ?? "item()*";
                if (!IsSequenceTypeSubtype(testParamTypes[i], actualParam))
                    return false;
            }
            // Return type is covariant: actual return must be a subtype of the test return.
            // A signature whose return kind is Undefined never returns a value (fn:error, xs:error);
            // that is equivalent to empty-sequence(), which is a subtype of every sequence type
            // (xs-error-006/007).
            var actualReturn = sig.ReturnTypeName
                ?? (sig.ReturnType == XdmValueKind.Undefined ? "empty-sequence()" : CoarseKindTypeName(sig.ReturnType));
            return IsSequenceTypeSubtype(actualReturn, testReturnType);
        }

        if (func is InlineFunctionItem inline)
        {
            // Inline functions: undeclared parameter/return types are item()*; subsumption
            // uses contravariant parameters and a covariant result.
            if (inline.Parameters.Count != testParamTypes.Length)
                return false;
            for (int i = 0; i < testParamTypes.Length; i++)
            {
                var actualParam = i < inline.ParameterTypes.Count ? inline.ParameterTypes[i] ?? "item()*" : "item()*";
                if (!IsSequenceTypeSubtype(testParamTypes[i], actualParam))
                    return false;
            }
            return IsSequenceTypeSubtype(inline.ReturnType ?? "item()*", testReturnType);
        }

        return ValueMatchesType(value, typeName);
    }

    /// <summary>
    /// Builds coarse parameter sequence-type names from a signature's kind-level metadata,
    /// sized to the referenced arity (a variadic registration such as fn:concat may carry
    /// fewer kinds than the referenced arity — the extras default to <c>item()*</c>).
    /// </summary>
    private static IReadOnlyList<string?> CoarseParameterTypeNames(FunctionSignature sig, int arity)
    {
        var names = new string?[arity];
        for (int i = 0; i < arity; i++)
            names[i] = CoarseKindTypeName(i < sig.ParameterTypes.Count ? sig.ParameterTypes[i] : XdmValueKind.Sequence);
        return names;
    }

    /// <summary>
    /// Maps a kind-level parameter/return type to a coarse sequence-type name for
    /// function-item subtyping when no precise declared type name is registered.
    /// </summary>
    private static string CoarseKindTypeName(XdmValueKind kind) => kind switch
    {
        XdmValueKind.String => "xs:string",
        XdmValueKind.Integer => "xs:integer",
        XdmValueKind.Decimal => "xs:decimal",
        XdmValueKind.Double => "xs:double",
        XdmValueKind.Float => "xs:float",
        XdmValueKind.Boolean => "xs:boolean",
        XdmValueKind.Uri => "xs:anyURI",
        XdmValueKind.QName => "xs:QName",
        XdmValueKind.Date => "xs:date",
        XdmValueKind.Time => "xs:time",
        XdmValueKind.DateTime => "xs:dateTime",
        XdmValueKind.Duration => "xs:duration",
        XdmValueKind.Binary => "xs:base64Binary",
        XdmValueKind.Node => "node()",
        XdmValueKind.Function => "function(*)",
        XdmValueKind.Map => "map(*)",
        XdmValueKind.Array => "array(*)",
        _ => "item()*",
    };

    /// <summary>
    /// Matches a map or array value against a typed function test <c>function(A) as R</c>.
    /// A map behaves as <c>function(xs:anyAtomicType) as V?</c> (an absent key returns the
    /// empty sequence, so () must also match R); an array behaves as
    /// <c>function(xs:integer) as T</c>. The parameter type is contravariant.
    /// </summary>
    private static bool MapOrArrayMatchesFunctionType(XdmValue value, string[] testParamTypes, string testReturnType)
    {
        if (testParamTypes.Length != 1)
            return false;
        // Contravariant domain: the test's parameter type must accept every key/index
        // the map or array itself accepts.
        string domain = value.IsMap ? "xs:anyAtomicType" : "xs:integer";
        if (!IsSequenceTypeSubtype(testParamTypes[0], domain))
            return false;
        if (value.IsMap)
        {
            if (!ValueMatchesType(XdmValue.Undefined, testReturnType))
                return false;
            foreach (var v in value.MapValue.Values)
                if (!ValueMatchesType(v, testReturnType))
                    return false;
            return true;
        }
        foreach (var member in value.ArrayValue.Values)
            if (!ValueMatchesType(member, testReturnType))
                return false;
        return true;
    }

    /// <summary>
    /// Like <see cref="MapOrArrayMatchesFunctionType"/> but used for function coercion:
    /// a map or array can be coerced to a one-argument function type even when the
    /// target return type does not allow an empty sequence, because a missing key
    /// will raise XPTY0004 at call time rather than at the coercion point.
    /// </summary>
    private static bool MapOrArrayCoercibleToFunctionType(XdmValue value, string[] testParamTypes, string testReturnType)
    {
        if (testParamTypes.Length != 1)
            return false;
        string domain = value.IsMap ? "xs:anyAtomicType" : "xs:integer";
        if (!IsSequenceTypeSubtype(testParamTypes[0], domain))
            return false;
        if (value.IsMap)
        {
            foreach (var v in value.MapValue.Values)
                if (!ValueMatchesType(v, testReturnType))
                    return false;
            return true;
        }
        foreach (var member in value.ArrayValue.Values)
            if (!ValueMatchesType(member, testReturnType))
                return false;
        return true;
    }

    /// <summary>
    /// Applies kind-level function conversion to the arguments of a dynamically invoked
    /// named function: node atomization, untypedAtomic casting, numeric promotion, and
    /// URI promotion; anything else raises XPTY0004 (higher-order-functions-064).
    /// </summary>
    private static XdmValue[] ConvertDynamicCallArgs(FunctionSignature sig, ReadOnlySpan<XdmValue> args, EvaluationContext? context = null)
    {
        var converted = new XdmValue[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            // User-declared functions register precise parameter type names: apply the full
            // function conversion rules (atomization, untypedAtomic casting, numeric/URI
            // promotion) — hof-043: xs:untypedAtomic against xs:double.
            if (sig.ParameterTypeNames is not null && i < sig.ParameterTypeNames.Count
                && !string.IsNullOrEmpty(sig.ParameterTypeNames[i]))
            {
                converted[i] = ApplyFunctionConversion(args[i], sig.ParameterTypeNames[i]!, context);
                continue;
            }
            var expected = i < sig.ParameterTypes.Count ? sig.ParameterTypes[i] : XdmValueKind.Sequence;
            // External is an opaque/unconstrained kind (user-declared XQuery functions
            // register with External fillers and convert via type names instead).
            converted[i] = expected is XdmValueKind.Undefined or XdmValueKind.Sequence or XdmValueKind.Node or XdmValueKind.Function
                or XdmValueKind.Map or XdmValueKind.Array or XdmValueKind.External
                ? args[i]
                : ConvertArgToKind(args[i], expected);
        }
        return converted;
    }

    private static XdmValue ConvertArgToKind(XdmValue arg, XdmValueKind expected)
    {
        if (arg.Kind == expected || arg.IsUndefined)
            return arg;

        // An empty sequence satisfies any optional parameter (xs:T? / xs:T*) — the
        // kind-level metadata cannot express optionality, so pass it through.
        if (arg.Kind == XdmValueKind.Sequence)
        {
            bool isEmpty = true;
            foreach (var _ in XdmSequence.FromSource(arg.SequenceValue!))
            {
                isEmpty = false;
                break;
            }
            if (isEmpty)
                return arg;
        }

        // Function conversion atomizes nodes to xs:untypedAtomic before casting.
        // A singleton sequence unwraps first (hof-042: an attribute node argument).
        if (arg.IsSequence && arg.SequenceValue is not null)
        {
            var argItems = MaterializeSequence(arg);
            if (argItems.Length == 1)
                arg = argItems[0];
        }
        var atomic = arg.IsNode ? XdmValue.FromString(arg.NodeValue.StringValue, "untypedAtomic") : arg;
        if (atomic.Kind == expected)
            return atomic;

        bool untyped = IsUntypedAtomicValue(atomic);
        switch (expected)
        {
            case XdmValueKind.Double:
                if ((atomic.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float || untyped)
                    && TryCast(atomic, "xs:double", out var d))
                    return d;
                break;
            case XdmValueKind.Float:
                if ((atomic.Kind is XdmValueKind.Integer or XdmValueKind.Decimal || untyped)
                    && TryCast(atomic, "xs:float", out var f))
                    return f;
                break;
            case XdmValueKind.Decimal:
                if (atomic.Kind == XdmValueKind.Integer)
                    return atomic; // xs:integer is a subtype of xs:decimal
                if (untyped && TryCast(atomic, "xs:decimal", out var dec))
                    return dec;
                break;
            case XdmValueKind.Integer:
                if (untyped && TryCast(atomic, "xs:integer", out var n))
                    return n;
                break;
            case XdmValueKind.String:
                if (untyped)
                    return atomic;
                // URI promotion: xs:anyURI may be promoted to xs:string.
                if (atomic.Kind == XdmValueKind.Uri && TryCast(atomic, "xs:string", out var s))
                    return s;
                break;
            case XdmValueKind.Boolean:
                if (untyped && TryCast(atomic, "xs:boolean", out var b))
                    return b;
                break;
            case XdmValueKind.Uri:
                if (untyped && TryCast(atomic, "xs:anyURI", out var u))
                    return u;
                break;
        }

        throw new InvalidOperationException($"XPTY0004: Cannot convert argument of kind {arg.Kind} to {expected}");
    }

    private static bool IsUntypedAtomicValue(XdmValue value)
        => value.Kind == XdmValueKind.String
           && string.Equals(value.SchemaTypeName, "untypedAtomic", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Applies the XPath 3.1 function conversion rules to convert a value to a target
    /// sequence type: subtype substitution, node atomization, untypedAtomic casting,
    /// numeric promotion, and URI promotion. Raises XPTY0004 when no rule applies.
    /// </summary>
    public static XdmValue ApplyFunctionConversion(XdmValue value, string targetType, EvaluationContext? context = null)
    {
        if (ValueMatchesType(value, targetType, context))
            return value;

        var type = NormalizeEQNameTypeName(targetType.Trim());
        while (type.Length > 1 && type[0] == '(' && FindMatchingParen(type, 0) == type.Length - 1)
            type = type[1..^1].Trim();

        // Normalize 'function (' / 'map (' / 'array (' to the compact form so function
        // tests parse regardless of source whitespace (hof-049).
        foreach (var family in new[] { "function", "map", "array" })
        {
            if (type.StartsWith(family, StringComparison.OrdinalIgnoreCase))
            {
                int afterKeyword = family.Length;
                while (afterKeyword < type.Length && char.IsWhiteSpace(type[afterKeyword]))
                    afterKeyword++;
                if (afterKeyword < type.Length && type[afterKeyword] == '(' && afterKeyword != family.Length)
                    type = string.Concat(type.AsSpan(0, family.Length), type.AsSpan(afterKeyword));
                break;
            }
        }

        // Function tests: a trailing occurrence indicator belongs to the function item
        // only when the test has no 'as' return clause; otherwise it is part of the
        // return type (function(xs:string) as xs:string*). Handle occurrence before the
        // generic stripping so the return type is not mangled (hof-028).
        bool isFunctionTest = type.StartsWith("function(", StringComparison.OrdinalIgnoreCase);
        bool allowsMultiple;
        bool allowsEmpty;
        if (isFunctionTest)
        {
            allowsMultiple = false;
            allowsEmpty = false;
            if (!TryParseFunctionType(type, out _, out _) && type.Length > 1 && type[^1] is '?' or '*' or '+')
            {
                allowsMultiple = type[^1] is '*' or '+';
                allowsEmpty = type[^1] is '?' or '*';
                type = type[..^1].TrimEnd();
            }
        }
        else
        {
            allowsMultiple = type.EndsWith('*') || type.EndsWith('+');
            allowsEmpty = type.EndsWith('?') || type.EndsWith('*');
            if (type.EndsWith('?') || type.EndsWith('*') || type.EndsWith('+'))
                type = type[..^1].Trim();
            while (type.Length > 1 && type[0] == '(' && FindMatchingParen(type, 0) == type.Length - 1)
                type = type[1..^1].Trim();
            // An occurrence-wrapped parenthesized function test: (function(...) as ...)?
            isFunctionTest = type.StartsWith("function(", StringComparison.OrdinalIgnoreCase);
        }

        var items = new List<XdmValue>();
        if (!value.IsUndefined)
        {
            if (value.IsSequence && value.SequenceValue != null)
            {
                foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                    AddConversionItems(item, items);
            }
            else
            {
                AddConversionItems(value, items);
            }
        }

        // XPath 1.0 backwards compatibility (XSLT 3.0 §3.9.1): an argument to a singleton
        // parameter is truncated to its first item (xpath-compat-0301, string-003/031);
        // a numeric parameter additionally converts as by fn:number.
        bool bcTruncate = context?.BackwardsCompatible == true && !allowsMultiple;
        bool bcNumeric = bcTruncate && IsNumericTypeName(type);

        if (items.Count == 0)
        {
            if (allowsEmpty)
                return XdmValue.Undefined;
            throw new InvalidOperationException($"XPTY0004: Empty sequence not allowed for type {targetType}");
        }
        if (items.Count > 1 && !allowsMultiple)
        {
            if (bcTruncate)
            {
                items.RemoveRange(1, items.Count - 1);
            }
            else
            {
                throw new InvalidOperationException($"XPTY0004: Sequence of more than one item not allowed for type {targetType}");
            }
        }

        var converted = new List<XdmValue>(items.Count);
        foreach (var item in items)
        {
            // An item that already matches the target type passes through unchanged —
            // checked before atomization so node items survive node-typed parameters
            // (e.g. a node truncated to a node()? parameter in backwards-compatible mode).
            if (ValueMatchesType(item, type, context))
            {
                converted.Add(item);
                continue;
            }
            // Element/text/document/attribute nodes atomize to xs:untypedAtomic; comment,
            // processing-instruction, and namespace nodes atomize to xs:string (which is
            // not cast to numeric types by function conversion — K2-FunctionProlog-18/20).
            var atomic = item.IsNode && item.NodeValue.NodeKind is XdmNodeKind.Element or XdmNodeKind.Text or XdmNodeKind.Document or XdmNodeKind.Attribute
                ? XdmValue.FromString(item.NodeValue.StringValue, "untypedAtomic")
                : item;
            if (ValueMatchesType(atomic, type, context))
            {
                converted.Add(atomic);
            }
            else if (isFunctionTest && atomic.IsFunction && FunctionItemCoercibleTo(atomic, type))
            {
                // XPath 3.1 function conversion: wrap the item in a CoercedFunctionItem so
                // invocation converts the arguments and the result to the declared types
                // (the XSLT engine applies the same pattern — hof-028).
                if (!type.StartsWith("function(*)", StringComparison.OrdinalIgnoreCase)
                    && TryParseFunctionType(type, out var coercionParamTypes, out var coercionReturnType)
                    && !(coercionParamTypes.Length == 1 && coercionParamTypes[0] == "*")
                    && atomic.FunctionValue is FunctionItem functionItem)
                {
                    converted.Add(XdmValue.FromFunction(new CoercedFunctionItem(functionItem, coercionParamTypes, coercionReturnType)));
                }
                else
                {
                    converted.Add(atomic);
                }
            }
            else if (isFunctionTest && (atomic.IsMap || atomic.IsArray)
                && !type.StartsWith("function(*)", StringComparison.OrdinalIgnoreCase)
                && TryParseFunctionType(type, out var mapCoercionParamTypes, out var mapCoercionReturnType)
                && !(mapCoercionParamTypes.Length == 1 && mapCoercionParamTypes[0] == "*")
                && MapOrArrayCoercibleToFunctionType(atomic, mapCoercionParamTypes, mapCoercionReturnType))
            {
                var capturedValue = atomic;
                var inner = new DelegateFunctionItem(1, (ctx, args) => InvokeFunctionItem(capturedValue, ctx, args));
                converted.Add(XdmValue.FromFunction(new CoercedFunctionItem(inner, mapCoercionParamTypes, mapCoercionReturnType)));
            }
            else if (IsUntypedAtomicValue(atomic) && TryCast(atomic, type, out var casted))
            {
                converted.Add(casted);
            }
            else if (TryPromoteNumericOrUri(atomic, type, out var promoted))
            {
                converted.Add(promoted);
            }
            else if (IsUserDefinedSchemaType(type, context, out _) && TryCast(atomic, type, context, out var schemaCasted))
            {
                // Derived schema simple types (e.g. hat:hatsize) accept values that can be
                // cast to them under XSD facet rules (qischema040).
                converted.Add(schemaCasted);
            }
            else if (bcNumeric)
            {
                // backwards-022: round(concat(...)) under version="1.0" — xs:string and
                // xs:boolean arguments convert to xs:double via fn:number semantics.
                converted.Add(XdmValue.FromDouble(ToDoubleOrNaN(atomic)));
            }
            else
            {
                throw new InvalidOperationException($"XPTY0004: Cannot convert value to type {targetType}");
            }
        }

        if (converted.Count == 0)
            return XdmValue.Undefined;
        if (converted.Count == 1)
            return converted[0];
        return XdmValue.FromSequence(MaterializedSequence.FromList(converted));
    }

    /// <summary>
    /// Collects the items of a function-conversion input, expanding arrays into their
    /// (recursively atomized) members: the function conversion rules atomize the
    /// supplied value, and atomizing an array yields the atomization of its members
    /// (FunctionCall-022: an array argument to an xs:integer* parameter contributes
    /// its members).
    /// </summary>
    private static void AddConversionItems(XdmValue item, List<XdmValue> items)
    {
        if (item.IsArray && item.ArrayValue is not null)
        {
            foreach (var member in item.ArrayValue.Values)
            {
                if (member.IsUndefined)
                    continue;
                if (member.IsSequence && member.SequenceValue is not null)
                {
                    foreach (var inner in XdmSequence.FromSource(member.SequenceValue))
                        AddConversionItems(inner, items);
                }
                else
                {
                    AddConversionItems(member, items);
                }
            }
            return;
        }
        items.Add(item);
    }

    private static bool TryPromoteNumericOrUri(XdmValue value, string type, out XdmValue result)
    {
        result = XdmValue.Undefined;
        var t = type.Trim().ToLowerInvariant();
        if (t.StartsWith("xs:"))
            t = t[3..];

        // xs:anyURI values are sometimes stored as String kind with the schema type name
        // annotation rather than as Uri kind, so consult both sources.
        bool isUri = value.Kind == XdmValueKind.Uri
            || (value.Kind == XdmValueKind.String
                && value.SchemaTypeName?.Equals("anyURI", StringComparison.OrdinalIgnoreCase) == true);

        bool promotable = t switch
        {
            "double" => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Float,
            "float" => value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal,
            "string" => isUri,
            _ => false
        };
        return promotable && TryCast(value, type, out result);
    }

    /// <summary>
    /// True when the (occurrence-stripped) sequence-type name is a numeric type:
    /// xs:numeric or one of the numeric primitives. Used for XPath 1.0 backwards-
    /// compatible argument conversion (XSLT 3.0 §3.9.1).
    /// </summary>
    private static bool IsNumericTypeName(string type)
    {
        var t = type.StartsWith("xs:", StringComparison.OrdinalIgnoreCase) ? type[3..] : type;
        return t.Equals("numeric", StringComparison.OrdinalIgnoreCase)
            || t.Equals("double", StringComparison.OrdinalIgnoreCase)
            || t.Equals("float", StringComparison.OrdinalIgnoreCase)
            || t.Equals("decimal", StringComparison.OrdinalIgnoreCase)
            || t.Equals("integer", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether <paramref name="actualType"/> is a subtype of <paramref name="testType"/>
    /// according to XPath 3.1 sequence type subtyping rules.
    /// </summary>
    private static bool IsSequenceTypeSubtype(string actualType, string testType)
    {
        string actual = actualType.Trim().ToLowerInvariant();
        string test = testType.Trim().ToLowerInvariant();

        if (actual.StartsWith("xs:")) actual = actual[3..];
        if (test.StartsWith("xs:")) test = test[3..];

        // Function-family types (function/map/array) need structured comparison:
        // an occurrence indicator at the end of such a type only counts as an outer
        // occurrence when it directly follows the closing parenthesis (otherwise it
        // belongs to the return type or the map/array member type).
        bool actualFamily = IsFunctionFamilyType(actual);
        bool testFamily = IsFunctionFamilyType(test);

        char actualOcc = '\0';
        char testOcc = '\0';
        if (actualFamily)
            (actual, actualOcc) = StripOuterOccurrence(actual);
        else if (actual.Length > 0 && "?+*".Contains(actual[^1]))
        {
            actualOcc = actual[^1];
            actual = actual[..^1].TrimEnd();
        }
        if (testFamily)
            (test, testOcc) = StripOuterOccurrence(test);
        else if (test.Length > 0 && "?+*".Contains(test[^1]))
        {
            testOcc = test[^1];
            test = test[..^1].TrimEnd();
        }

        // Every sequence matching actual must also match test
        bool occOk = (actualOcc, testOcc) switch
        {
            ('\0', '\0') => true,
            ('\0', '?') => true,
            ('\0', '+') => true,
            ('\0', '*') => true,
            ('?', '?') => true,
            ('?', '*') => true,
            ('+', '+') => true,
            ('+', '*') => true,
            ('*', '*') => true,
            _ => false,
        };

        if (!occOk) return false;
        if (actual == test) return true;
        // empty-sequence() is a subtype of every sequence type (xs-015/016).
        if (actual is "empty-sequence" or "empty-sequence()") return true;

        if (actualFamily || testFamily)
            return IsFunctionFamilySubtype(actual, test);

        return IsBaseTypeSubtype(actual, test);
    }

    /// <summary>
    /// Whether the (lower-cased, occurrence-free) base type belongs to the function
    /// family: function types, map types, and array types.
    /// </summary>
    private static bool IsFunctionFamilyType(string baseType)
        => baseType.StartsWith("function(") || baseType.StartsWith("map(") || baseType.StartsWith("array(")
            || baseType is "function(*)" or "map(*)" or "array(*)" or "function" or "map" or "array";

    /// <summary>
    /// Strips an outer occurrence indicator from a function-family type, but only when
    /// it directly follows the closing parenthesis (e.g. <c>map(*)*</c>); a trailing
    /// indicator after a return type (e.g. <c>function(xs:int) as xs:string*</c>) is
    /// part of the type itself.
    /// </summary>
    private static (string Type, char Occurrence) StripOuterOccurrence(string type)
    {
        if (type.Length >= 2 && type[^1] is '?' or '*' or '+' && type[^2] == ')')
        {
            // The trailing indicator is an outer occurrence only when it directly
            // follows the closing parenthesis of the parameter/member list; after an
            // 'as' return clause it belongs to the return type instead.
            int open = type.IndexOf('(');
            if (open >= 0 && FindMatchingParen(type, open) == type.Length - 2)
                return (type[..^1].TrimEnd(), type[^1]);
        }
        return (type, '\0');
    }

    /// <summary>
    /// Adds an empty-sequence alternative to a sequence type (used for the result type
    /// of a map viewed as a function: looking up an absent key returns ()).
    /// </summary>
    private static string Optionalize(string sequenceType)
    {
        var t = sequenceType.Trim();
        if (t.Length == 0) return "item()*";
        return t[^1] switch
        {
            '*' or '?' => t,
            '+' => t[..^1] + "*",
            _ => t + "?"
        };
    }

    /// <summary>
    /// Structural subtyping within the function family (XPath 3.1 §2.5.6): function
    /// subsumption with contravariant parameters and a covariant result; map/array
    /// type covariance; and the implied function signatures of maps and arrays
    /// (MapTest-050..054).
    /// </summary>
    private static bool IsFunctionFamilySubtype(string actual, string test)
    {
        // Normalize the bare/generic names.
        if (actual is "map" or "map()") actual = "map(*)";
        if (actual is "array" or "array()") actual = "array(*)";
        if (actual is "function" or "function()") actual = "function(*)";
        if (test is "map" or "map()") test = "map(*)";
        if (test is "array" or "array()") test = "array(*)";
        if (test is "function" or "function()") test = "function(*)";

        if (test == "item()") return true;
        if (test == "function(*)") return true; // every function-family type is a function(*)
        if (actual == "function(*)") return false;

        if (actual == "map(*)" || actual.StartsWith("map("))
        {
            if (test == "map(*)") return true;
            if (test.StartsWith("map("))
            {
                if (!TryGetMapTypeParts(actual, out var ak, out var av) || !TryGetMapTypeParts(test, out var tk, out var tv))
                    return false;
                return IsSequenceTypeSubtype(ak, tk) && IsSequenceTypeSubtype(av, tv);
            }
            if (test.StartsWith("function("))
            {
                if (!TryParseFunctionType(test, out var tp, out var tr)) return false;
                if (tp.Length != 1) return false;
                // A map accepts any single atomic key; contravariance: test param ≤ xs:anyAtomicType.
                if (!IsSequenceTypeSubtype(tp[0], "xs:anyAtomicType")) return false;
                string v = actual == "map(*)" ? "item()*" : MapTypeParts(actual).Value;
                return IsSequenceTypeSubtype(Optionalize(v), tr);
            }
            return false;
        }

        if (actual == "array(*)" || actual.StartsWith("array("))
        {
            if (test == "array(*)") return true;
            if (test.StartsWith("array("))
            {
                return IsSequenceTypeSubtype(ArrayTypeInner(actual), ArrayTypeInner(test));
            }
            if (test.StartsWith("function("))
            {
                if (!TryParseFunctionType(test, out var tp, out var tr)) return false;
                if (tp.Length != 1) return false;
                // An array accepts any integer index; contravariance: test param ≤ xs:integer.
                if (!IsSequenceTypeSubtype(tp[0], "xs:integer")) return false;
                string t = actual == "array(*)" ? "item()*" : ArrayTypeInner(actual);
                return IsSequenceTypeSubtype(t, tr);
            }
            return false;
        }

        if (actual.StartsWith("function(") && test.StartsWith("function("))
        {
            if (!TryParseFunctionType(actual, out var ap, out var ar)) return false;
            if (!TryParseFunctionType(test, out var tp, out var tr)) return false;
            if (ap.Length != tp.Length) return false;
            // Parameters are contravariant; the result is covariant.
            for (int i = 0; i < ap.Length; i++)
                if (!IsSequenceTypeSubtype(tp[i], ap[i])) return false;
            return IsSequenceTypeSubtype(ar, tr);
        }

        return false;
    }

    /// <summary>Extracts the key and value type parts of a <c>map(K, V)</c> type.</summary>
    private static bool TryGetMapTypeParts(string mapType, out string keyType, out string valueType)
    {
        keyType = "xs:anyAtomicType";
        valueType = "item()*";
        if (!mapType.StartsWith("map(") || !mapType.EndsWith(')'))
            return false;
        var inner = mapType.Substring(4, mapType.Length - 5).Trim();
        if (inner.Length == 0 || inner == "*")
            return false;
        var parts = SplitTopLevel(inner, ',');
        if (parts.Length != 2)
            return false;
        keyType = parts[0];
        valueType = parts[1];
        return true;
    }

    /// <summary>Returns the value type part of a <c>map(K, V)</c> type (assumed well-formed).</summary>
    private static (string Key, string Value) MapTypeParts(string mapType)
    {
        TryGetMapTypeParts(mapType, out var k, out var v);
        return (k, v);
    }

    /// <summary>Extracts the member type of an <c>array(T)</c> type.</summary>
    private static string ArrayTypeInner(string arrayType)
    {
        if (!arrayType.StartsWith("array(") || !arrayType.EndsWith(')'))
            return "item()*";
        var inner = arrayType.Substring(6, arrayType.Length - 7).Trim();
        return inner.Length == 0 || inner == "*" ? "item()*" : inner;
    }

    /// <summary>
    /// Checks whether <paramref name="actual"/> is a subtype of <paramref name="test"/>
    /// by walking the type hierarchy.
    /// </summary>
    private static bool IsBaseTypeSubtype(string actual, string test)
    {
        var queue = new Queue<string>();
        queue.Enqueue(actual);
        var visited = new HashSet<string>();

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            if (current == test) return true;
            if (!visited.Add(current)) continue;

            foreach (var super in GetDirectSupertypes(current))
                queue.Enqueue(super);
        }

        return false;
    }

    /// <summary>
    /// Returns the immediate supertypes of a given base type name.
    /// </summary>
    private static IEnumerable<string> GetDirectSupertypes(string type)
    {
        // Named element/attribute tests are subtypes of the generic node-kind tests.
        if (type.StartsWith("element(") && type != "element()")
            return ["element()"];
        if (type.StartsWith("attribute(") && type != "attribute()")
            return ["attribute()"];

        return type switch
        {
            "byte" => ["short"],
            "short" => ["int"],
            "int" => ["long"],
            "long" => ["integer"],
            "unsignedbyte" => ["unsignedshort"],
            "unsignedshort" => ["unsignedint"],
            "unsignedint" => ["unsignedlong"],
            "unsignedlong" => ["nonnegativeinteger"],
            "positiveinteger" => ["nonnegativeinteger"],
            "nonnegativeinteger" => ["integer"],
            "negativeinteger" => ["nonpositiveinteger"],
            "nonpositiveinteger" => ["integer"],
            "integer" => ["decimal"],
            "decimal" => ["numeric"],
            "double" => ["numeric"],
            "float" => ["numeric"],
            "numeric" => ["anyatomictype"],
            "ncname" => ["name"],
            "name" => ["token"],
            "nmtoken" => ["token"],
            "language" => ["token"],
            "id" => ["ncname"],
            "idref" => ["ncname"],
            "entity" => ["ncname"],
            "token" => ["normalizedstring"],
            "normalizedstring" => ["string"],
            "string" => ["anyatomictype"],
            "untypedatomic" => ["anyatomictype"],
            "boolean" => ["anyatomictype"],
            "date" => ["anyatomictype"],
            "time" => ["anyatomictype"],
            "datetime" => ["anyatomictype"],
            "datetimestamp" => ["datetime"],
            "duration" => ["anyatomictype"],
            "daytimeduration" => ["duration"],
            "yearmonthduration" => ["duration"],
            "anyuri" => ["anyatomictype"],
            "qname" => ["anyatomictype"],
            "notation" => ["anyatomictype"],
            "hexbinary" => ["anyatomictype"],
            "base64binary" => ["anyatomictype"],
            "anyatomictype" => ["item()"],
            "element()" => ["node()"],
            "attribute()" => ["node()"],
            "text()" => ["node()"],
            "comment()" => ["node()"],
            "processing-instruction()" => ["node()"],
            "document-node()" => ["node()"],
            "namespace-node()" => ["node()"],
            "node()" => ["item()"],
            "function(*)" => ["item()"],
            "map(*)" => ["item()"],
            "array(*)" => ["item()"],
            _ => [],
        };
    }

    /// <summary>
    /// Checks whether an XDM value matches a simple <see cref="XdmValueKind"/>.
    /// </summary>
    private static bool ValueMatchesXdmKind(XdmValue value, XdmValueKind kind)
    {
        return kind switch
        {
            XdmValueKind.String => value.Kind == XdmValueKind.String,
            XdmValueKind.Integer => value.Kind == XdmValueKind.Integer,
            XdmValueKind.Decimal => value.Kind == XdmValueKind.Decimal,
            XdmValueKind.Double => value.Kind == XdmValueKind.Double,
            XdmValueKind.Float => value.Kind == XdmValueKind.Float,
            XdmValueKind.Boolean => value.Kind == XdmValueKind.Boolean,
            XdmValueKind.DateTime => value.Kind == XdmValueKind.DateTime,
            XdmValueKind.Date => value.Kind == XdmValueKind.Date,
            XdmValueKind.Time => value.Kind == XdmValueKind.Time,
            XdmValueKind.Duration => value.Kind == XdmValueKind.Duration,
            XdmValueKind.Node => value.IsNode,
            _ => true
        };
    }

    // ------------------------------------------------------------------
    // Type promotion helpers
    // ------------------------------------------------------------------

    private static bool HasTimezoneSuffix(string s)
    {
        string t = s.Trim();
        return t.EndsWith('Z') || t.EndsWith('z') || System.Text.RegularExpressions.Regex.IsMatch(t, @"[Tt]\d{2}:\d{2}:\d{2}[Zz]|[Tt]\d{2}:\d{2}:\d{2}[+\-]\d{2}:\d{2}$|[+\-]\d{2}:\d{2}$");
    }

    private static string FormatXPathTimezone(DateTimeOffset dto, bool hasTz)
    {
        if (!hasTz) return "";
        string tz = dto.ToString("zzz", System.Globalization.CultureInfo.InvariantCulture);
        return tz == "+00:00" ? "Z" : tz;
    }

    private static readonly Regex XPathDateTimeRegex = new(
        @"^(?<year>[+-]?\d{4,})-(?<month>\d{2})-(?<day>\d{2})T(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2})(?:\.(?<frac>\d+))?(?<tz>Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.Compiled);

    private static readonly Regex XPathDateRegex = new(
        @"^(?<year>[+-]?\d{4,})-(?<month>\d{2})-(?<day>\d{2})(?<tz>Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.Compiled);

    private static readonly Regex XPathTimeRegex = new(
        @"^(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2})(?:\.(?<frac>\d+))?(?<tz>Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.Compiled);

    private static bool TryParseXPathDateTime(string s, out XPathDateTime xdt, out bool hasTz)
    {
        xdt = default;
        hasTz = false;
        var m = XPathDateTimeRegex.Match(s);
        if (!m.Success) return false;

        string yearStr = m.Groups["year"].Value;
        long year = long.Parse(yearStr, CultureInfo.InvariantCulture);
        if (year > int.MaxValue || year < int.MinValue) return false;
        // Reject + sign and leading zeros for years longer than 4 digits
        if (yearStr.StartsWith('+')) return false;
        if (yearStr.Length > 4 && yearStr[0] == '0') return false;

        int month = int.Parse(m.Groups["month"].Value, CultureInfo.InvariantCulture);
        int day = int.Parse(m.Groups["day"].Value, CultureInfo.InvariantCulture);
        if (month < 1 || month > 12) return false;
        if (day < 1 || day > 31) return false;
        if (day > DaysInMonth(year, month)) return false;

        int hour = int.Parse(m.Groups["hour"].Value, CultureInfo.InvariantCulture);
        int minute = int.Parse(m.Groups["minute"].Value, CultureInfo.InvariantCulture);
        int second = int.Parse(m.Groups["second"].Value, CultureInfo.InvariantCulture);
        int millisecond = 0;
        bool hasFrac = m.Groups["frac"].Success;
        if (hasFrac)
        {
            string frac = m.Groups["frac"].Value;
            // Take up to 3 digits for milliseconds
            if (frac.Length > 3) frac = frac[..3];
            millisecond = int.Parse(frac.PadRight(3, '0'), CultureInfo.InvariantCulture);
        }

        // Validate time components
        if (hour > 24 || minute > 59 || second > 59) return false;
        if (hour == 24 && (minute != 0 || second != 0 || millisecond != 0)) return false;

        int tzMinutes = 0;
        hasTz = m.Groups["tz"].Success;
        if (hasTz)
        {
            string tz = m.Groups["tz"].Value;
            if (tz == "Z" || tz == "z")
            {
                tzMinutes = 0;
            }
            else
            {
                if (!IsValidTimezone(tz)) return false;
                tzMinutes = ParseTimezoneOffset(tz);
            }
        }

        xdt = NormalizeHour24(new XPathDateTime(year, month, day, hour, minute, second, millisecond, tzMinutes, hasTz));
        return true;
    }

    private static bool TryParseXPathDate(string s, out XPathDateTime xdt, out bool hasTz)
    {
        xdt = default;
        hasTz = false;
        var m = XPathDateRegex.Match(s);
        if (!m.Success) return false;

        string yearStr = m.Groups["year"].Value;
        long year = long.Parse(yearStr, CultureInfo.InvariantCulture);
        if (year > int.MaxValue || year < int.MinValue) return false;
        // Reject + sign and leading zeros for years longer than 4 digits
        if (yearStr.StartsWith('+')) return false;
        if (yearStr.Length > 4 && yearStr[0] == '0') return false;

        int month = int.Parse(m.Groups["month"].Value, CultureInfo.InvariantCulture);
        int day = int.Parse(m.Groups["day"].Value, CultureInfo.InvariantCulture);
        if (month < 1 || month > 12) return false;
        if (day < 1 || day > 31) return false;
        if (day > DaysInMonth(year, month)) return false;

        int tzMinutes = 0;
        hasTz = m.Groups["tz"].Success;
        if (hasTz)
        {
            string tz = m.Groups["tz"].Value;
            if (tz == "Z" || tz == "z")
                tzMinutes = 0;
            else
            {
                if (!IsValidTimezone(tz)) return false;
                tzMinutes = ParseTimezoneOffset(tz);
            }
        }

        xdt = new XPathDateTime(year, month, day, 0, 0, 0, 0, tzMinutes, hasTz);
        return true;
    }

    private static bool TryParseXPathTime(string s, out XPathDateTime xdt, out bool hasTz)
    {
        xdt = default;
        hasTz = false;
        var m = XPathTimeRegex.Match(s);
        if (!m.Success) return false;

        int hour = int.Parse(m.Groups["hour"].Value, CultureInfo.InvariantCulture);
        int minute = int.Parse(m.Groups["minute"].Value, CultureInfo.InvariantCulture);
        int second = int.Parse(m.Groups["second"].Value, CultureInfo.InvariantCulture);
        int millisecond = 0;
        bool hasFrac = m.Groups["frac"].Success;
        if (hasFrac)
        {
            string frac = m.Groups["frac"].Value;
            if (frac.Length > 3) frac = frac[..3];
            millisecond = int.Parse(frac.PadRight(3, '0'), CultureInfo.InvariantCulture);
        }

        // Validate time components
        if (hour > 24 || minute > 59 || second > 59) return false;
        if (hour == 24 && (minute != 0 || second != 0 || millisecond != 0)) return false;

        int tzMinutes = 0;
        hasTz = m.Groups["tz"].Success;
        if (hasTz)
        {
            string tz = m.Groups["tz"].Value;
            if (tz == "Z" || tz == "z")
                tzMinutes = 0;
            else
            {
                if (!IsValidTimezone(tz)) return false;
                tzMinutes = ParseTimezoneOffset(tz);
            }
        }

        // xs:time normalizes 24:00:00 to 00:00:00 on the same (reference) day.
        if (hour == 24)
            hour = 0;
        xdt = new XPathDateTime(1, 1, 1, hour, minute, second, millisecond, tzMinutes, hasTz);
        return true;
    }

    private static int DaysInMonth(long year, int month)
    {
        return month switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
            4 or 6 or 9 or 11 => 30,
            2 => IsLeapYear(year) ? 29 : 28,
            _ => 0
        };
    }

    private static bool IsLeapYear(long year)
    {
        if (year == 0) return true; // Year 0 is a leap year in XML Schema (proleptic Gregorian)
        if (year < 0) year = -year; // BCE leap years align with the negated year number
        return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    }

    private static bool IsValidTimezone(string tz)
    {
        // tz is like "+14:00" or "-05:00"
        int hours = int.Parse(tz[1..3], CultureInfo.InvariantCulture);
        int minutes = int.Parse(tz[4..6], CultureInfo.InvariantCulture);
        return hours <= 14 && !(hours == 14 && minutes > 0) && minutes <= 59;
    }

    private static int ParseTimezoneOffset(string tz)
    {
        // tz is like "+14:00" or "-05:00"
        bool negative = tz[0] == '-';
        var parts = tz[1..].Split(':');
        int hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
        int minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
        int total = hours * 60 + minutes;
        return negative ? -total : total;
    }

    private static string NormalizeDateTimeString(string s)
    {
        // XML Schema allows T24:00:00 to represent midnight of the next day.
        // .NET's DateTimeOffset.TryParse does not handle this, so normalize it.
        int idx = s.IndexOf("T24:00:00");
        if (idx >= 0)
        {
            int after = idx + "T24:00:00".Length;
            // Allow T24:00:00 followed by all-zero fractional seconds.
            if (after >= s.Length || s[after] != '.')
            {
                // no fractional seconds - normalize directly
            }
            else
            {
                int i = after + 1;
                while (i < s.Length && char.IsDigit(s[i])) i++;
                if (!s[(after + 1)..i].All(c => c == '0'))
                    return s; // leave non-zero fractional T24 for the parser to reject
                after = i;
            }
            string datePart = s[..idx];
            string rest = s[after..];
            if (DateTimeOffset.TryParse(datePart, out var dto))
            {
                dto = dto.AddDays(1);
                string newDate = dto.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                return newDate + "T00:00:00" + rest;
            }
        }
        return s;
    }

    private static XPathDateTime NormalizeHour24(XPathDateTime xdt)
    {
        if (xdt.Hour != 24)
            return xdt;
        var (year, month, day) = XPathDateTimeHelper.AddDays(xdt.Year, xdt.Month, xdt.Day, 1);
        return new XPathDateTime(year, month, day, 0, 0, 0, 0, xdt.TimezoneOffsetMinutes, xdt.HasTimezone);
    }

    private static bool TryParseDouble(string s, out double result)
    {
        s = s.Trim();
        if (s == "INF" || s == "+INF")
        {
            result = double.PositiveInfinity;
            return true;
        }
        if (s == "-INF")
        {
            result = double.NegativeInfinity;
            return true;
        }
        if (s == "NaN")
        {
            result = double.NaN;
            return true;
        }
        // Explicitly reject case variants that .NET's double.TryParse would accept
        string upper = s.ToUpperInvariant();
        if (upper is "NAN" or "INF" or "+INF" or "-INF" or "INFINITY" or "+INFINITY" or "-INFINITY")
        {
            result = 0;
            return false;
        }
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseFloat(string s, out float result)
    {
        s = s.Trim();
        if (s == "INF" || s == "+INF")
        {
            result = float.PositiveInfinity;
            return true;
        }
        if (s == "-INF")
        {
            result = float.NegativeInfinity;
            return true;
        }
        if (s == "NaN")
        {
            result = float.NaN;
            return true;
        }
        // Explicitly reject case variants that .NET's float.TryParse would accept
        string upper = s.ToUpperInvariant();
        if (upper is "NAN" or "INF" or "+INF" or "-INF" or "INFINITY" or "+INFINITY" or "-INFINITY")
        {
            result = 0;
            return false;
        }
        return float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static bool IsDouble(XdmValue value) =>
        value.Kind == XdmValueKind.Double;

    private static bool IsFloat(XdmValue value) =>
        value.Kind == XdmValueKind.Float;

    private static bool IsDuration(XdmValue value) =>
        value.Kind == XdmValueKind.Duration;

    private static bool IsEmptySeq(XdmValue value)
    {
        if (!value.IsSequence || value.SequenceValue is null)
            return false;
        foreach (var _ in XdmSequence.FromSource(value.SequenceValue))
            return false;
        return true;
    }

    /// <summary>
    /// Returns the first item of a sequence, or an undefined value for an empty sequence
    /// or a non-sequence input. Used for XPath 1.0 backwards-compatible first-item rules.
    /// </summary>
    private static XdmValue FirstItemOrUndefined(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;
        if (!value.IsSequence || value.SequenceValue is null)
            return value;
        foreach (var item in XdmSequence.FromSource(value.SequenceValue))
        {
            if (!item.IsUndefined)
                return item;
        }
        return XdmValue.Undefined;
    }

    private static bool IsDecimal(XdmValue value) =>
        value.Kind == XdmValueKind.Decimal;

    private static double ToDouble(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0 : 0.0,
            _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : throw new InvalidOperationException($"Cannot convert {value.Kind} to double")
        };
    }

    private static decimal ToDecimal(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (decimal)value.DoubleValue,
            _ => decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : throw new InvalidOperationException($"Cannot convert {value.Kind} to decimal")
        };
    }

    private static long ToInteger(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (long)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (long)value.DoubleValue,
            _ => long.TryParse(value.ToString(), out var l) ? l : throw new InvalidOperationException($"Cannot convert {value.Kind} to integer")
        };
    }

    private static float ToFloat(XdmValue value)
    {
        value = Atomize(value);
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (float)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => (float)value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0f : 0.0f,
            _ => float.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var f) ? f : throw new InvalidOperationException($"Cannot convert {value.Kind} to float")
        };
    }

    // ------------------------------------------------------------------
    // Opcode helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns whether the value contains at least one item (fn:exists/fn:empty semantics).
    /// </summary>
    private static bool SequenceHasAnyItem(XdmValue value)
    {
        if (value.IsUndefined)
            return false;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            if (value.SequenceValue.TryGetLength(out var length))
                return length > 0;
            foreach (var _ in XdmSequence.FromSource(value.SequenceValue))
                return true;
            return false;
        }
        return true;
    }
    private static XdmValue AtomizeMapKey(XdmValue value)
    {
        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");
        if (value.IsUndefined)
            throw new InvalidOperationException("XPTY0004: A map key must be a single atomic value, not the empty sequence");
        if (value.IsSequence)
        {
            var items = MaterializeSequence(value);
            if (items.Length != 1)
                throw new InvalidOperationException("XPTY0004: A map key must be a single atomic value, not a sequence");
            value = items[0];
        }
        return Atomize(value);
    }

    private static XdmValue LookupValue(XdmValue container, XdmValue key)
    {
        var results = new List<XdmValue>();
        LookupInto(container, key, results);
        return results.Count switch
        {
            0 => XdmValue.Undefined,
            1 => results[0],
            _ => XdmValue.FromSequence(MaterializedSequence.FromList(results))
        };
    }

    /// <summary>
    /// Expands the lookup over container items (outer) and key items (inner), matching
    /// XPath 3.1 §3.11.3 result ordering (Lookup-107: for each container, for each key).
    /// </summary>
    private static void LookupInto(XdmValue container, XdmValue key, List<XdmValue> results)
    {
        if (container.IsUndefined)
            return;
        if (container.IsSequence && container.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(container.SequenceValue))
                LookupInto(item, key, results);
            return;
        }
        if (key.IsUndefined)
            return;
        if (key.IsSequence && key.SequenceValue is not null)
        {
            foreach (var singleKey in XdmSequence.FromSource(key.SequenceValue))
                LookupSingle(container, singleKey, results);
            return;
        }
        LookupSingle(container, key, results);
    }

    private static void LookupSingle(XdmValue container, XdmValue key, List<XdmValue> results)
    {
        if (container.Kind == XdmValueKind.Map)
        {
            var vkey = AtomizeMapKey(key);
            if (container.MapValue.TryGetValue(vkey, out var value))
                AppendLookupResult(results, value);
            return;
        }
        if (container.Kind == XdmValueKind.Array)
        {
            AppendLookupResult(results, ArrayLookup(container.ArrayValue, key));
            return;
        }
        // Lookup on anything other than a map or an array is a type error (Lookup-012).
        throw new InvalidOperationException($"XPTY0004: Lookup operator requires a map or an array, got {container.Kind}.");
    }

    /// <summary>
    /// Array member access shared by the lookup operator and array-as-function calls.
    /// The key must be xs:integer (Lookup-119: a double is XPTY0004, not truncated);
    /// out-of-range indexes raise FOAY0001. Bounds are checked against Count (not Get)
    /// so that a stored empty-sequence member is not mistaken for an out-of-range index.
    /// </summary>
    private static XdmValue ArrayLookup(XdmArray array, XdmValue key)
    {
        var akey = Atomize(key);
        long idx;
        if (akey.Kind == XdmValueKind.Integer)
        {
            idx = akey.IntegerValue;
        }
        else if (IsUntypedAtomicValue(akey) && long.TryParse(akey.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            idx = parsed;
        }
        else
        {
            throw new InvalidOperationException($"XPTY0004: Array lookup key must be xs:integer, got {akey.Kind}.");
        }
        if (idx < 1 || idx > array.Count)
            throw new InvalidOperationException($"FOAY0001: Array index {idx} is out of bounds (array size {array.Count}).");
        return array.Get((int)idx);
    }

    private static void AppendLookupResult(List<XdmValue> results, XdmValue value)
    {
        if (value.IsUndefined)
            return;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                results.Add(item);
        }
        else
        {
            results.Add(value);
        }
    }

    private static string AtomizedString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;

        if (value.IsNode)
            return value.NodeValue.StringValue;

        if (value.IsFunction || value.IsMap || value.IsArray)
            throw new InvalidOperationException("FOTY0013");

        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizedString(item);
            return string.Empty;
        }

        return value.ToString();
    }

    private static string NormalizeSpaceString(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var parts = s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private static RegexOptions ParseRegexFlags(string flags)
    {
        var options = RegexOptions.None;
        foreach (char c in flags)
        {
            switch (c)
            {
                case 'i': options |= RegexOptions.IgnoreCase; break;
                case 'm': options |= RegexOptions.Multiline; break;
                case 's': options |= RegexOptions.Singleline; break;
                case 'x': options |= RegexOptions.IgnorePatternWhitespace; break;
            }
        }
        return options;
    }

    private static XdmValue Sum(System.Collections.Generic.List<XdmValue> items)
    {
        bool allIntegerOrDecimal = true;
        foreach (var item in items)
        {
            var a = Atomize(item);
            if (a.Kind != XdmValueKind.Integer && a.Kind != XdmValueKind.Decimal)
            {
                allIntegerOrDecimal = false;
                break;
            }
        }
        if (allIntegerOrDecimal)
        {
            decimal sum = 0m;
            foreach (var item in items)
                sum += ToDecimal(Atomize(item));
            return XdmValue.FromDecimal(sum);
        }
        double sumD = 0.0;
        foreach (var item in items)
            sumD += ToDouble(Atomize(item));
        return XdmValue.FromDouble(sumD);
    }

    private static XdmValue MinMax(System.Collections.Generic.List<XdmValue> items, bool min)
    {
        bool allIntegerOrDecimal = true;
        foreach (var item in items)
        {
            var a = Atomize(item);
            if (a.Kind != XdmValueKind.Integer && a.Kind != XdmValueKind.Decimal)
            {
                allIntegerOrDecimal = false;
                break;
            }
        }
        if (allIntegerOrDecimal)
        {
            decimal result = ToDecimal(Atomize(items[0]));
            for (int i = 1; i < items.Count; i++)
            {
                decimal v = ToDecimal(Atomize(items[i]));
                if (min ? v < result : v > result)
                    result = v;
            }
            return XdmValue.FromDecimal(result);
        }
        double resultD = ToDouble(Atomize(items[0]));
        for (int i = 1; i < items.Count; i++)
        {
            double v = ToDouble(Atomize(items[i]));
            if (min ? v < resultD : v > resultD)
                resultD = v;
        }
        return XdmValue.FromDouble(resultD);
    }

    // ------------------------------------------------------------------
    // OrderBy helpers
    // ------------------------------------------------------------------

    private static List<(string Name, XdmValue Value)> CaptureWindowBindings(
        XdmValue[] items,
        int index,
        int position,
        string? currentVar,
        string? posVar,
        string? prevVar,
        string? nextVar)
    {
        var bindings = new List<(string, XdmValue)>(4);
        if (currentVar is not null)
            bindings.Add((currentVar, items[index]));
        if (posVar is not null)
            bindings.Add((posVar, XdmValue.FromInteger(position)));
        if (prevVar is not null)
            bindings.Add((prevVar, index > 0 ? items[index - 1] : XdmValue.FromSequence(XdmSequence.Empty)));
        if (nextVar is not null)
            bindings.Add((nextVar, index < items.Length - 1 ? items[index + 1] : XdmValue.FromSequence(XdmSequence.Empty)));
        return bindings;
    }

    private static bool EvaluateWindowCondition(
        IrModule module,
        EvaluationContext context,
        XdmValue[] registers,
        int entryPoint,
        XdmValue[] items,
        int index,
        int position,
        string? currentVar,
        string? posVar,
        string? prevVar,
        string? nextVar)
    {
        foreach (var (name, value) in CaptureWindowBindings(items, index, position, currentVar, posVar, prevVar, nextVar))
        {
            var (local, ns) = ResolveWindowVariableName(name, context);
            context.WithVariable(local, value, ns);
        }
        var (condResult, _) = ExecuteBlock(module, context, registers, entryPoint);
        return condResult.EffectiveBooleanValue();
    }

    private static void EmitFlworWindow(
        IrModule module,
        EvaluationContext context,
        XdmValue[] registers,
        WindowInfo info,
        List<XdmValue> results,
        List<XdmValue> windowItems,
        List<(string Name, XdmValue Value)> startBindings,
        XdmValue[] items,
        int endIndex,
        int endPosition)
    {
        // Bind the window variable to the window's item sequence.
        var windowValue = windowItems.Count == 1
            ? windowItems[0]
            : XdmValue.FromSequence(MaterializedSequence.FromList(new List<XdmValue>(windowItems)));

        // XPTY0004: enforce an optional 'as SequenceType' declaration on the window variable.
        if (info.DeclaredTypeName is not null &&
            !InstanceOf(windowValue, info.DeclaredTypeName, info.DeclaredTypeOccurrence, null, context))
        {
            throw new InvalidOperationException(
                $"XPTY0004: Window variable '${info.VariableName}' does not match the declared type '{info.DeclaredTypeName}'.");
        }

        var (windowLocal, windowNs) = ResolveWindowVariableName(info.VariableName, context);
        context.WithVariable(windowLocal, windowValue, windowNs);

        // Bind the start condition variables to the values captured when the window opened.
        foreach (var (name, value) in startBindings)
        {
            var (local, ns) = ResolveWindowVariableName(name, context);
            context.WithVariable(local, value, ns);
        }

        // Bind the end condition variables to the values at the closing item.
        foreach (var (name, value) in CaptureWindowBindings(items, endIndex, endPosition,
                     info.EndCurrent, info.EndPos, info.EndPrev, info.EndNext))
        {
            var (local, ns) = ResolveWindowVariableName(name, context);
            context.WithVariable(local, value, ns);
        }

        var (rhsResult, _) = ExecuteBlock(module, context, registers, info.RhsEntryPoint);
        if (rhsResult.IsSequence && rhsResult.SequenceValue is not null)
        {
            foreach (var r in XdmSequence.FromSource(rhsResult.SequenceValue))
                results.Add(r);
        }
        else if (!rhsResult.IsUndefined)
        {
            results.Add(rhsResult);
        }
    }

    // Resolves a window-clause variable name in lexical form (local, prefix:local, or
    // Q{uri}local) to its (local name, namespace URI) pair the way variable references
    // resolve; an undeclared prefix raises XPST0081.
    private static (string Local, string Ns) ResolveWindowVariableName(string rawName, EvaluationContext context)
    {
        if (rawName.StartsWith("Q{", StringComparison.Ordinal))
        {
            int closeBrace = rawName.IndexOf('}');
            if (closeBrace > 1)
                return (rawName[(closeBrace + 1)..], rawName[2..closeBrace]);
        }
        int colon = rawName.IndexOf(':');
        if (colon > 0)
        {
            if (!context.TryResolveNamespace(rawName[..colon], out var prefixNs))
                throw new InvalidOperationException($"XPST0081: Prefix '{rawName[..colon]}' is not declared.");
            return (rawName[(colon + 1)..], prefixNs);
        }
        return (rawName, "");
    }

    private static string ResolveCollationUri(string collation, string? baseUri)
    {
        if (string.IsNullOrEmpty(collation))
            return string.Empty;
        if (Uri.IsWellFormedUriString(collation, UriKind.Absolute))
            return collation;
        if (!string.IsNullOrEmpty(baseUri) &&
            Uri.TryCreate(new Uri(baseUri), collation, out var resolved))
        {
            return resolved.AbsoluteUri;
        }
        return collation;
    }

    /// <summary>
    /// Atomizes a value and joins the string forms of its items with a separator
    /// (used for computed attribute values in element constructors).
    /// </summary>
    private static string JoinAtomizedItems(XdmValue value, string separator)
    {
        if (value.IsUndefined)
            return string.Empty;
        if (!value.IsSequence || value.SequenceValue is null)
        {
            // Function items cannot be atomized (function-item-6: attribute a { avg#1 }).
            if (value.IsFunction)
                throw new InvalidOperationException("FOTY0013: Atomization of a function item is not allowed.");
            // An array in attribute content: its members are joined with single spaces
            // (ArrayTest-050).
            if (value.IsArray && value.ArrayValue is not null)
            {
                var memberParts = new List<string>();
                foreach (var member in value.ArrayValue.Values)
                    memberParts.Add(JoinAtomizedItems(member, separator));
                return string.Join(separator, memberParts);
            }
            return Atomize(value).ToString();
        }
        var items = MaterializeSequence(value);
        if (items.Length == 0)
            return string.Empty;
        var parts = new string[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].IsFunction)
                throw new InvalidOperationException("FOTY0013: Atomization of a function item is not allowed.");
            parts[i] = Atomize(items[i]).ToString();
        }
        return string.Join(separator, parts);
    }

    /// <summary>
    /// Accumulates computed-constructor content items, applying the XQuery content rules:
    /// attributes only before any other content (XQTY0024), duplicate attribute detection
    /// (XQDY0025), array flattening, and single-space joining of adjacent atomic values.
    /// </summary>
    // Constructor-local namespace bindings (xmlns declarations of enclosing constructors)
    // propagate to a built element; prefixes the element itself declares (xmlns
    // attributes, computed namespace constructors in content, or the tag prefix) are
    // skipped so propagation neither duplicates them nor triggers the name-conflict
    // resolution reserved for real content declarations (K2-DirectConElemNamespace-77).
    private static void AppendConstructorLocalNamespaces(
        List<XdmContentItem> content, IReadOnlyList<XdmAttributeValue> attributes, EvaluationContext context,
        string? tagPrefix)
    {
        var declared = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attr in attributes)
        {
            if (attr.Prefix == "xmlns") declared.Add(attr.LocalName);
            else if (attr.LocalName == "xmlns" && attr.Prefix is null) declared.Add("");
        }
        foreach (var item in content)
            if (item.Kind == XdmContentKind.Namespace)
                declared.Add(item.Target ?? "");
        if (tagPrefix is not null)
            declared.Add(tagPrefix);
        foreach (var (prefix, uri) in context.ConstructorLocalNamespaces)
        {
            if (!declared.Contains(prefix))
                content.Add(new XdmContentItem(XdmContentKind.Namespace, uri, null, prefix));
        }
    }

    private sealed class ComputedContentAccumulator
    {
        private readonly List<XdmAttributeValue> _attributes = new();
        private readonly HashSet<(string, string)> _seenAttrs = new();
        private readonly List<XdmContentItem> _content = new();
        private string? _pendingAtomic;
        private bool _lastWasAtomic;
        private bool _seenNonAttributeContent;
        private readonly bool _allowAttributes;
        private readonly string? _elementNamespaceUri;

        public ComputedContentAccumulator(bool allowAttributes = true, string? elementNamespaceUri = null)
        {
            _allowAttributes = allowAttributes;
            // Null: not constructing an element (no default-namespace conflict check);
            // empty: the constructed element is in no namespace.
            _elementNamespaceUri = elementNamespaceUri;
        }

        public List<XdmAttributeValue> Attributes => _attributes;
        public List<XdmContentItem> Content => _content;

        public void Add(XdmValue item, EvaluationContext context)
        {
            // A sequence member is flattened item by item (array members are sequences).
            if (item.IsSequence && item.SequenceValue is not null)
            {
                foreach (var inner in XdmSequence.FromSource(item.SequenceValue))
                    Add(inner, context);
                return;
            }
            if (item.IsNode && item.NodeValue.NodeKind == XdmNodeKind.Attribute)
            {
                if (!_allowAttributes || _seenNonAttributeContent)
                    throw new InvalidOperationException("XQTY0024: An attribute node in content must not follow other content.");
                var attrNode = item.NodeValue;
                string? itemNs = string.IsNullOrEmpty(attrNode.NamespaceUri) ? null : attrNode.NamespaceUri;
                if (attrNode.LocalName != "xmlns" && attrNode.Prefix != "xmlns" &&
                    !_seenAttrs.Add((attrNode.LocalName, itemNs ?? "")))
                {
                    throw new InvalidOperationException($"XQDY0025: Duplicate attribute '{attrNode.LocalName}'.");
                }
                _attributes.Add(new XdmAttributeValue(attrNode.LocalName, attrNode.Prefix, itemNs, attrNode.StringValue));
                return;
            }
            if (item.IsNode && item.NodeValue.NodeKind == XdmNodeKind.Namespace)
            {
                // A namespace node in content becomes a namespace declaration (XQDY0102 on
                // conflicting redeclaration of the same prefix). The node's name is the
                // bound prefix (empty for a default declaration); its string value is the URI.
                var nsNode = item.NodeValue;
                string declPrefix = nsNode.LocalName;
                string declUri = nsNode.StringValue;
                if (declPrefix.Length == 0 && _elementNamespaceUri is not null)
                {
                    // Spec bug 22032: a default namespace declaration conflicts with an
                    // element name in no namespace, and an empty-URI undeclaration conflicts
                    // with an element name in a namespace (XQDY0102).
                    if (_elementNamespaceUri.Length == 0 && declUri.Length > 0)
                        throw new InvalidOperationException("XQDY0102: A default namespace declaration must not be added to an element in no namespace.");
                    if (_elementNamespaceUri.Length > 0 && declUri.Length == 0)
                        throw new InvalidOperationException("XQDY0102: The default namespace must not be undeclared on an element in a namespace.");
                }
                if (context.TryResolveNamespace(declPrefix, out var existing) && existing != declUri)
                    throw new InvalidOperationException($"XQDY0102: The namespace prefix '{declPrefix}' is redeclared with a different URI.");
                if (declPrefix.Length == 0 && !string.IsNullOrEmpty(context.DefaultElementNamespace) && context.DefaultElementNamespace != declUri)
                    throw new InvalidOperationException("XQDY0102: The default namespace is redeclared with a different URI.");
                if (declPrefix.Length == 0)
                    context.DefaultElementNamespace = declUri.Length == 0 ? null : declUri;
                else
                    context.WithNamespace(declPrefix, declUri);
                _content.Add(new XdmContentItem(XdmContentKind.Namespace, declUri, null, declPrefix));
                // Namespace declarations are not "other content": they may interleave
                // freely with attributes at the start of the content (no XQTY0024).
                return;
            }
            if (item.IsNode && item.NodeValue.NodeKind is XdmNodeKind.Text)
            {
                // Text nodes merge with adjacent atomic text rather than being copied;
                // no separator is inserted at a text-node boundary.
                var textNodeValue = item.NodeValue.StringValue;
                _pendingAtomic = _pendingAtomic is null ? textNodeValue : _pendingAtomic + textNodeValue;
                _lastWasAtomic = false;
                _seenNonAttributeContent = true;
                return;
            }
            if (item.IsNode)
            {
                Flush();
                _content.Add(new XdmContentItem(XdmContentKind.Node, null, item));
                _seenNonAttributeContent = true;
                return;
            }
            if (item.IsFunction)
            {
                // XQTY0105: element content must not contain function items.
                throw new InvalidOperationException("XQTY0105: Element content must not contain a function item.");
            }
            if (item.IsArray && item.ArrayValue is not null)
            {
                foreach (var member in item.ArrayValue.Values)
                    Add(member, context);
                return;
            }

            // A single space separates two ADJACENT ATOMIC values only.
            var text = item.ToString();
            _pendingAtomic = _pendingAtomic is null ? text : _pendingAtomic + (_lastWasAtomic ? " " : "") + text;
            _lastWasAtomic = true;
            _seenNonAttributeContent = true;
        }

        public void Flush()
        {
            if (_pendingAtomic is not null)
            {
                _content.Add(new XdmContentItem(XdmContentKind.Text, _pendingAtomic));
                _pendingAtomic = null;
                _lastWasAtomic = false;
                _seenNonAttributeContent = true;
            }
        }
    }

    private static (string Local, string? Prefix, string? NamespaceUri) ResolveComputedName(
        ComputedConstructorInfo info, XdmValue nameValue, EvaluationContext context, string construct)
    {
        if (!info.HasNameExpression)
        {
            string? ns = info.NamespaceUri;
            if (ns is null && info.Prefix is not null)
            {
                if (!context.TryResolveNamespace(info.Prefix, out var resolved))
                    throw new InvalidOperationException($"XPST0081: Prefix '{info.Prefix}' is not declared.");
                ns = resolved;
            }
            // An unprefixed computed element name uses the default element namespace
            // (K2-InScopePrefixesFunc-12/13; attribute names never do).
            if (ns is null && construct == "element" && !string.IsNullOrEmpty(context.DefaultElementNamespace))
                ns = context.DefaultElementNamespace;
            return (info.LocalName!, info.Prefix, ns);
        }

        var atomized = Atomize(nameValue);
        if (atomized.IsUndefined)
            throw new InvalidOperationException($"XPTY0004: The computed {construct} name is the empty sequence.");
        if (atomized.Kind == XdmValueKind.QName)
        {
            var qn = atomized.QNameValue;
            var qPrefix = qn.Prefix.Length > 0 ? qn.Prefix : null;
            var qNs = qn.NamespaceUri.Length > 0 ? qn.NamespaceUri : null;
            ValidateComputedNamePrefix(qPrefix, qNs, construct);
            return (qn.LocalName, qPrefix, qNs);
        }

        var text = atomized.ToString().Trim();
        // EQName braced form: Q{uri}local (empty URI means no namespace).
        if (text.StartsWith("Q{", StringComparison.Ordinal))
        {
            int closeBrace = text.IndexOf('}');
            if (closeBrace < 0 || closeBrace == text.Length - 1)
                throw new InvalidOperationException($"XQDY0074: Invalid {construct} name '{text}'.");
            var rawUri = text[2..closeBrace];
            // References are expanded by the string literal, not here; a literal
            // '{' can therefore appear in the value and is not permitted in the
            // URI part of the lexical EQName form.
            if (rawUri.Contains('{'))
                throw new InvalidOperationException($"XQDY0074: Invalid {construct} name '{text}'.");
            var uri = NormalizeEQNameUriText(rawUri);
            var local = text[(closeBrace + 1)..];
            if (!IsValidNcName(local))
                throw new InvalidOperationException($"XQDY0074: Invalid {construct} name '{text}'.");
            return (local, null, uri.Length == 0 ? null : uri);
        }

        int colon = text.IndexOf(':');
        if (colon >= 0)
        {
            var prefixPart = text[..colon];
            var localPart = text[(colon + 1)..];
            // XQDY0096: the 'xmlns' prefix must not be used, whether or not it is declared.
            if (prefixPart == "xmlns")
                throw new InvalidOperationException($"XQDY0096: A computed {construct} name must not use the 'xmlns' prefix.");
            if (!context.TryResolveNamespace(prefixPart, out var pns))
                throw new InvalidOperationException($"XQDY0074: The prefix of the computed {construct} name '{text}' cannot be resolved.");
            if (!IsValidNcName(localPart))
                throw new InvalidOperationException($"XQDY0074: Invalid {construct} name '{text}'.");
            ValidateComputedNamePrefix(prefixPart, pns, construct);
            return (localPart, prefixPart, pns);
        }
        if (!IsValidNcName(text))
            throw new InvalidOperationException($"XQDY0074: Invalid {construct} name '{text}'.");
        // Unprefixed computed element names use the default element namespace; unprefixed
        // computed attribute names do not (currencysvg).
        if (construct == "attribute")
            return (text, null, null);
        return (text, null, string.IsNullOrEmpty(context.DefaultElementNamespace) ? null : context.DefaultElementNamespace);
    }

    // EQName URI normalization: trim and collapse internal whitespace runs to single spaces.
    private static string NormalizeEQNameUriText(string uri)
    {
        var parts = uri.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    // XQDY0096: computed element/attribute names must not misuse the xml/xmlns prefixes.
    private static void ValidateComputedNamePrefix(string? prefix, string? namespaceUri, string construct)
    {
        if (prefix == "xmlns")
            throw new InvalidOperationException($"XQDY0096: A computed {construct} name must not use the 'xmlns' prefix.");
        if (prefix == "xml" && namespaceUri != "http://www.w3.org/XML/1998/namespace")
            throw new InvalidOperationException($"XQDY0096: The 'xml' prefix in a computed {construct} name must be bound to the XML namespace URI.");
        if (prefix is not (null or "xml") && namespaceUri == "http://www.w3.org/XML/1998/namespace")
            throw new InvalidOperationException($"XQDY0096: A computed {construct} name must not bind a non-'xml' prefix to the XML namespace URI.");
    }

    // Computed PI targets accept only string-like atomic values (xs:string,
    // xs:untypedAtomic, xs:NCName); other types such as xs:anyURI raise XPTY0004.
    private static bool IsValidPiTargetType(XdmValue value)
        => value.Kind == XdmValueKind.String
            && value.SchemaTypeName is null or "string" or "untypedAtomic" or "NCName";

    private static bool IsSupportedOrderByCollation(string collation)
    {
        if (collation == "http://www.w3.org/2005/xpath-functions/collation/codepoint")
            return true;
        if (collation == "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive")
            return true;
        if (collation == "http://www.w3.org/2010/09/qt-fots-catalog/collation/caseblind")
            return true;
        if (collation.StartsWith("http://www.w3.org/2013/collation/UCA", StringComparison.Ordinal))
            return true;
        return false;
    }

    private static bool IsGroupKeySlot(int slot, GroupByInfo info)
    {
        for (int i = 0; i < info.KeyIndices.Count; i++)
        {
            if (info.KeyIndices[i] == slot)
                return true;
        }
        return false;
    }

    private static bool GroupKeysEqual(XdmValue[] x, XdmValue[] y, GroupByInfo info, EvaluationContext context)
    {
        for (int i = 0; i < info.KeyIndices.Count; i++)
        {
            int keyIndex = info.KeyIndices[i];
            var keyX = keyIndex < x.Length ? x[keyIndex] : XdmValue.Undefined;
            var keyY = keyIndex < y.Length ? y[keyIndex] : XdmValue.Undefined;
            var collation = info.CollationUri[i] ?? context.DefaultCollation;
            if (!GroupKeyValuesEqual(keyX, keyY, collation, context))
                return false;
        }
        return true;
    }

    private static bool GroupKeyValuesEqual(XdmValue left, XdmValue right, string? collation, EvaluationContext context)
    {
        left = SingleGroupKeyItem(Atomize(left));
        right = SingleGroupKeyItem(Atomize(right));

        // Two empty grouping keys group together.
        if (left.IsUndefined || right.IsUndefined)
            return left.IsUndefined && right.IsUndefined;

        if (IsNumeric(left) && IsNumeric(right))
        {
            // Double.Equals treats NaN as equal to NaN, as XQuery grouping requires.
            return ToDouble(left).Equals(ToDouble(right));
        }

        // Date/time values group by their instant on the timeline (with implicit
        // timezone), not by lexical form. g* date values are stored as annotated
        // strings, so this check must precede the plain string comparison.
        var leftDateSub = GetDateTimeSubtype(left);
        var rightDateSub = GetDateTimeSubtype(right);
        if (leftDateSub is not null && rightDateSub is not null && leftDateSub == rightDateSub)
        {
            var cmp = CompareDateTimeValues(left, right, leftDateSub, GetImplicitTimezoneOffsetMinutes(context));
            if (cmp.HasValue)
                return cmp.Value == 0;
            return string.Equals(left.ToString(), right.ToString(), StringComparison.Ordinal);
        }

        if (left.Kind == XdmValueKind.String && right.Kind == XdmValueKind.String)
        {
            // Grouping compares strings with the spec collation (or the default collation).
            var effectiveCollation = collation is null ? context.DefaultCollation : ResolveCollationUri(collation, context.BaseUri);
            return CompareStrings(left.StringValue, right.StringValue, effectiveCollation ?? "", context) == 0;
        }

        if (left.Kind == XdmValueKind.Boolean && right.Kind == XdmValueKind.Boolean)
        {
            return left.BooleanValue == right.BooleanValue;
        }

        if (left.Kind == right.Kind)
        {
            return string.Equals(left.ToString(), right.ToString(), StringComparison.Ordinal);
        }

        return false;
    }

    private static XdmValue SingleGroupKeyItem(XdmValue value)
    {
        if (value.IsUndefined)
            return XdmValue.Undefined;
        if (!value.IsSequence || value.SequenceValue is null)
            return value;
        var items = MaterializeSequence(value);
        if (items.Length > 1)
            throw new InvalidOperationException("XPTY0004: A grouping key must evaluate to a single atomic value or the empty sequence.");
        return items.Length == 0 ? XdmValue.Undefined : items[0];
    }

    private static int CompareTuples(XdmValue[] x, XdmValue[] y, OrderByInfo info, EvaluationContext context)
    {
        for (int i = 0; i < info.KeyCount; i++)
        {
            int keyIndex = info.ValueCount + i;
            var keyX = keyIndex < x.Length ? x[keyIndex] : XdmValue.Undefined;
            var keyY = keyIndex < y.Length ? y[keyIndex] : XdmValue.Undefined;

            int cmp = CompareOrderByValues(keyX, keyY, info.EmptyOrder[i], info.CollationUri[i], context);
            if (cmp != 0)
                return info.Descending[i] ? -cmp : cmp;
        }
        return 0;
    }

    private static int CompareOrderByValues(XdmValue left, XdmValue right, EmptyOrder emptyOrder, string? collationUri, EvaluationContext context)
    {
        left = Atomize(left);
        right = Atomize(right);

        bool leftEmpty = left.IsUndefined || IsEmptySeq(left);
        bool rightEmpty = right.IsUndefined || IsEmptySeq(right);
        if (leftEmpty || rightEmpty)
        {
            if (leftEmpty && rightEmpty) return 0;
            int emptyRank = emptyOrder == EmptyOrder.Greatest ? 1 : -1;
            return leftEmpty ? emptyRank : -emptyRank;
        }

        if (left.IsSequence) left = FirstItemOrUndefined(left);
        if (right.IsSequence) right = FirstItemOrUndefined(right);

        if (left.IsUndefined || right.IsUndefined)
        {
            if (left.IsUndefined && right.IsUndefined) return 0;
            int emptyRank = emptyOrder == EmptyOrder.Greatest ? 1 : -1;
            return left.IsUndefined ? emptyRank : -emptyRank;
        }

        if (IsNumeric(left) && IsNumeric(right))
        {
            double l = ToDouble(left);
            double r = ToDouble(right);
            // NaN follows the empty-sequence ordering (least/greatest) rather than
            // comparing less than every other value.
            bool leftNaN = double.IsNaN(l);
            bool rightNaN = double.IsNaN(r);
            if (leftNaN || rightNaN)
            {
                if (leftNaN && rightNaN) return 0;
                int nanRank = emptyOrder == EmptyOrder.Greatest ? 1 : -1;
                return leftNaN ? nanRank : -nanRank;
            }
            return l.CompareTo(r);
        }

        // xs:hexBinary / xs:base64Binary (annotated strings) order by their decoded
        // octets; mixing binary types or binary with non-binary is a type error
        // (base64Binary-lt-15/gt-15).
        if (IsBinaryTypedString(left) || IsBinaryTypedString(right))
        {
            if (!IsBinaryTypedString(left) || !IsBinaryTypedString(right)
                || !string.Equals(left.SchemaTypeName, right.SchemaTypeName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"XPTY0004: Cannot compare {left.Kind} with {right.Kind} in an order by clause.");
            }
            return CompareBinaryValues(left, right);
        }

        if (left.Kind == XdmValueKind.String && right.Kind == XdmValueKind.String)
        {
            string effectiveCollation = ResolveCollationUri(collationUri ?? context.DefaultCollation ?? "", context.BaseUri);
            return CompareStrings(left.StringValue, right.StringValue, effectiveCollation, context);
        }

        if (left.Kind == XdmValueKind.Boolean && right.Kind == XdmValueKind.Boolean)
        {
            return left.BooleanValue.CompareTo(right.BooleanValue);
        }

        if (left.Kind == XdmValueKind.Integer && right.Kind == XdmValueKind.Integer)
        {
            return left.IntegerValue.CompareTo(right.IntegerValue);
        }

        // XQuery §3.8.3: values of type xs:untypedAtomic are cast to xs:string for the
        // purposes of order-by comparison (orderBy68).
        if (IsUntypedAtomicValue(left))
            left = XdmValue.FromString(left.StringValue);
        if (IsUntypedAtomicValue(right))
            right = XdmValue.FromString(right.StringValue);

        // Remaining atomic kinds are comparable only within the same type family
        // (xs:string with xs:anyURI; otherwise identical kinds). Cross-family pairs such
        // as xs:string vs xs:date raise XPTY0004 rather than comparing by string form.
        bool sameFamily = left.Kind == right.Kind
            || (left.Kind is XdmValueKind.String or XdmValueKind.Uri
                && right.Kind is XdmValueKind.String or XdmValueKind.Uri);
        if (!sameFamily)
        {
            throw new InvalidOperationException(
                $"XPTY0004: Cannot compare {left.Kind} with {right.Kind} in an order by clause.");
        }

        // Fallback: convert to string and compare.
        return string.CompareOrdinal(left.ToString(), right.ToString());
    }
}
