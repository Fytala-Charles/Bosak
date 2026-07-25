// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Opcodes for the register-based XPath intermediate representation. These are lowered from the AST ...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added Intersect, Except, and SimpleMap opcodes                                         |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added MapAdd and ArrayAdd opcodes                                                      |
//                      | Charles Korthout | 0.4   | 27-05-2026     | Added DocumentRoot opcode for absolute XPath paths                                     |
//                      | Charles Korthout | 0.5   | 30-05-2026     | Added PathStepMap opcode for per-context-item predicate evaluation                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 19-07-2026     | Added LoadNode opcode for unit-test node literals                                       |
//                      | Charles Korthout | 0.7   | 22-07-2026     | Added OrderBy and TupleBind opcodes for XQuery FLWOR order by                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 25-07-2026     | Added GroupBy opcode for XQuery FLWOR group by                                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 25-07-2026     | Added Window opcode for XQuery FLWOR window clause                                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.0   | 25-07-2026     | Added EnforceType opcode for XQuery 'as SequenceType' FLWOR bindings                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Compiler.Ir;

/// <summary>
/// Opcodes for the register-based XPath intermediate representation.
/// These are lowered from the AST and consumed by the bytecode emitter or IL JIT.
/// </summary>
public enum IrOpCode : byte
{
    // ---- Control flow ------------------------------------------------
    Nop = 0,
    Return,
    Jump,
    JumpIfTrue,
    JumpIfFalse,
    JumpIfEmpty,
    Call,
    TailCall,
    For,
    Some,
    Every,
    TryCatch,
    OrderBy,
    TupleBind,
    GroupBy,
    Window,
    EnforceType,
    
    // ---- Context -----------------------------------------------------
    LoadContext,
    LoadContextItem,
    LoadContextPosition,
    LoadContextSize,
    SetContext,
    
    // ---- Variables ---------------------------------------------------
    LoadVariable,
    StoreVariable,
    
    // ---- Literals ----------------------------------------------------
    LoadString,
    LoadInteger,
    LoadDecimal,
    LoadDouble,
    LoadBoolean,
    LoadNode,
    LoadEmptySequence,
    Move,
    
    // ---- Sequences ---------------------------------------------------
    SequenceStart,
    SequenceAdd,
    SequenceEnd,
    Singleton,
    Range,
    Concatenate,
    Intersect,
    Except,
    SimpleMap,
    PathStepMap,
    Normalize,          // Sort nodes into document order, deduplicate
    
    // ---- Nodes / Axes ------------------------------------------------
    Axis,
    Attribute,
    Child,
    Descendant,
    DescendantOrSelf,
    Ancestor,
    AncestorOrSelf,
    Parent,
    Self,
    Following,
    FollowingSibling,
    Preceding,
    PrecedingSibling,
    Namespace,
    DocumentRoot,
    
    // ---- Node tests --------------------------------------------------
    NameTest,
    KindTest,
    NamespaceTest,
    
    // ---- Predicates / Filtering ---------------------------------------
    Filter,
    Subscript,          // [n]
    First,
    Last,
    Position,
    
    // ---- Comparisons -------------------------------------------------
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    ValueEqual,
    ValueNotEqual,
    ValueLessThan,
    ValueLessThanOrEqual,
    ValueGreaterThan,
    ValueGreaterThanOrEqual,
    GeneralEqual,
    GeneralNotEqual,
    GeneralLessThan,
    GeneralLessThanOrEqual,
    GeneralGreaterThan,
    GeneralGreaterThanOrEqual,
    IsSameNode,
    PrecedesNode,
    FollowsNode,
    
    // ---- Arithmetic --------------------------------------------------
    Add,
    Subtract,
    Multiply,
    Divide,
    IntegerDivide,
    Modulo,
    UnaryPlus,
    UnaryMinus,
    
    // ---- Boolean logic -----------------------------------------------
    And,
    Or,
    Not,
    
    // ---- String ------------------------------------------------------
    StringConcat,
    StringLength,
    Substring,
    Contains,
    StartsWith,
    EndsWith,
    NormalizeSpace,
    Translate,
    UpperCase,
    LowerCase,
    MatchesRegex,
    ReplaceRegex,
    TokenizeRegex,
    
    // ---- Type operations ---------------------------------------------
    Cast,
    Castable,
    InstanceOf,
    TreatAs,
    
    // ---- Sequence functions ------------------------------------------
    Count,
    Exists,
    Empty,
    Head,
    Tail,
    InsertBefore,
    Remove,
    Reverse,
    Subsequence,
    DistinctValues,
    IndexOf,
    
    // ---- Aggregation -------------------------------------------------
    Sum,
    Avg,
    Min,
    Max,
    StringJoin,
    
    // ---- Higher-order (XPath 3.1) ------------------------------------
    Map,
    MapAdd,
    Array,
    ArrayAdd,
    ArrayAddAll,        // Add all items from a sequence to an array (curly constructor)
    Lookup,
    LookupWildcard,
    LoadFunction,       // Load a function item from the literal pool
    Curry,              // Partial function application
    Apply,
    
    // ---- Constructors ------------------------------------------------
    ElementConstructor,
    AttributeConstructor,
    TextConstructor,
    DocumentConstructor,
    
    // ---- Error -------------------------------------------------------
    Error
}
