// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Opcodes for the register-based XPath intermediate representation. These are lowered from the AST ...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
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
//                      | Charles Korthout | 1.1   | 25-07-2026     | Added ConstructElement and ConstructContentNode opcodes for XQuery constructors         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.2   | 25-07-2026     | Added SaveNamespaces/DeclareNamespace/RestoreNamespaces for constructor-local scopes    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.3   | 25-07-2026     | Added ConstructComputed opcode and ComputedConstructorKind for XQuery computed constructors |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.2   | 28-07-2026     | KindTestType opcode for schema-typed kind tests |
//                      | Charles Korthout | 1.3   | 21-08-2026     | SchemaElementTest and SchemaAttributeTest opcodes                                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.4   | 22-08-2026     | Added Validate opcode for XQuery validate expressions |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.5   | 07-09-2026     | Added CheckFunction opcode for pre-argument callee resolution (XPST0017)                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.6   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
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
    /// <summary>No operation.</summary>
    Nop = 0,
    /// <summary>Returns the value in a register as the expression result.</summary>
    Return,
    /// <summary>Unconditional jump to an instruction offset.</summary>
    Jump,
    /// <summary>Jumps when the operand register's effective boolean value is true.</summary>
    JumpIfTrue,
    /// <summary>Jumps when the operand register's effective boolean value is false.</summary>
    JumpIfFalse,
    /// <summary>Jumps when the operand register holds the empty sequence.</summary>
    JumpIfEmpty,
    /// <summary>Calls a function with arguments from consecutive registers.</summary>
    Call,
    /// <summary>Calls a function, reusing the current frame (tail call).</summary>
    TailCall,
    /// <summary>Resolves a static call target before arguments are evaluated (XPST0017 precedence).</summary>
    CheckFunction,
    /// <summary>Iterates the bindings of a for loop (loop info in the literal pool).</summary>
    For,
    /// <summary>Evaluates an existential quantification (<c>some ... satisfies</c>).</summary>
    Some,
    /// <summary>Evaluates a universal quantification (<c>every ... satisfies</c>).</summary>
    Every,
    /// <summary>Executes a try block with catch clauses (info in the literal pool).</summary>
    TryCatch,
    /// <summary>Sorts FLWOR tuples by their sort keys.</summary>
    OrderBy,
    /// <summary>Binds tuple items to FLWOR variables.</summary>
    TupleBind,
    /// <summary>Groups FLWOR tuples by their grouping keys.</summary>
    GroupBy,
    /// <summary>Evaluates a FLWOR window clause.</summary>
    Window,
    /// <summary>Raises an error when a bound value is not an instance of the declared type.</summary>
    EnforceType,
    /// <summary>Constructs a direct element node with attributes and content.</summary>
    ConstructElement,
    /// <summary>Constructs a content node (text, comment, or processing instruction) in element content.</summary>
    ConstructContentNode,
    /// <summary>Constructs a node via an XQuery computed constructor.</summary>
    ConstructComputed,
    /// <summary>Saves the in-scope namespace bindings before a constructor-local declaration.</summary>
    SaveNamespaces,
    /// <summary>Declares a constructor-local namespace binding.</summary>
    DeclareNamespace,
    /// <summary>Restores the saved in-scope namespace bindings.</summary>
    RestoreNamespaces,

    // ---- Context -----------------------------------------------------
    /// <summary>Loads the current context (item, position, size) for a nested evaluation.</summary>
    LoadContext,
    /// <summary>Loads the context item.</summary>
    LoadContextItem,
    /// <summary>Loads the context position.</summary>
    LoadContextPosition,
    /// <summary>Loads the context size.</summary>
    LoadContextSize,
    /// <summary>Reserved: sets the context item for a nested evaluation (not yet emitted).</summary>
    SetContext,

    // ---- Variables ---------------------------------------------------
    /// <summary>Loads the value of a variable.</summary>
    LoadVariable,
    /// <summary>Stores a value into a variable.</summary>
    StoreVariable,

    // ---- Literals ----------------------------------------------------
    /// <summary>Loads a string literal from the literal pool.</summary>
    LoadString,
    /// <summary>Loads an integer literal from the literal pool.</summary>
    LoadInteger,
    /// <summary>Loads a decimal literal from the literal pool.</summary>
    LoadDecimal,
    /// <summary>Loads a double literal from the literal pool.</summary>
    LoadDouble,
    /// <summary>Loads a boolean literal from the literal pool.</summary>
    LoadBoolean,
    /// <summary>Loads a node literal from the literal pool (unit-test support).</summary>
    LoadNode,
    /// <summary>Loads the empty sequence.</summary>
    LoadEmptySequence,
    /// <summary>Copies a value between registers.</summary>
    Move,

    // ---- Sequences ---------------------------------------------------
    /// <summary>Begins building a sequence in a register.</summary>
    SequenceStart,
    /// <summary>Appends an item to the sequence under construction.</summary>
    SequenceAdd,
    /// <summary>Finishes the sequence under construction.</summary>
    SequenceEnd,
    /// <summary>Wraps a value in a singleton sequence.</summary>
    Singleton,
    /// <summary>Builds an integer range sequence (the <c>to</c> operator).</summary>
    Range,
    /// <summary>Concatenates two sequences.</summary>
    Concatenate,
    /// <summary>Computes the intersection of two node sequences.</summary>
    Intersect,
    /// <summary>Computes the difference of two node sequences.</summary>
    Except,
    /// <summary>Maps the left sequence through the right expression (the <c>!</c> operator).</summary>
    SimpleMap,
    /// <summary>Evaluates a predicated path step per context item.</summary>
    PathStepMap,
    /// <summary>Sorts nodes into document order and removes duplicates.</summary>
    Normalize,

    // ---- Nodes / Axes ------------------------------------------------
    /// <summary>Reserved generic axis opcode; the lowerer emits the specific axis opcodes instead.</summary>
    Axis,
    /// <summary>Navigates the attribute axis.</summary>
    Attribute,
    /// <summary>Navigates the child axis.</summary>
    Child,
    /// <summary>Navigates the descendant axis.</summary>
    Descendant,
    /// <summary>Navigates the descendant-or-self axis.</summary>
    DescendantOrSelf,
    /// <summary>Navigates the ancestor axis.</summary>
    Ancestor,
    /// <summary>Navigates the ancestor-or-self axis.</summary>
    AncestorOrSelf,
    /// <summary>Navigates the parent axis.</summary>
    Parent,
    /// <summary>Navigates the self axis.</summary>
    Self,
    /// <summary>Navigates the following axis.</summary>
    Following,
    /// <summary>Navigates the following-sibling axis.</summary>
    FollowingSibling,
    /// <summary>Navigates the preceding axis.</summary>
    Preceding,
    /// <summary>Navigates the preceding-sibling axis.</summary>
    PrecedingSibling,
    /// <summary>Navigates the namespace axis.</summary>
    Namespace,
    /// <summary>Loads the document root of the context node (absolute path).</summary>
    DocumentRoot,

    // ---- Node tests --------------------------------------------------
    /// <summary>Filters nodes by expanded name.</summary>
    NameTest,
    /// <summary>Filters nodes by node kind.</summary>
    KindTest,
    /// <summary>Filters nodes by namespace (prefix, URI, or wildcard).</summary>
    NamespaceTest,
    /// <summary>Kind test with a schema type name argument.</summary>
    KindTestType,
    /// <summary>The <c>schema-element()</c> kind test (rejected without schema awareness).</summary>
    SchemaElementTest,
    /// <summary>The <c>schema-attribute()</c> kind test (rejected without schema awareness).</summary>
    SchemaAttributeTest,

    // ---- Predicates / Filtering ---------------------------------------
    /// <summary>Applies a predicate block to a sequence.</summary>
    Filter,
    /// <summary>Selects a single item by position: <c>[n]</c>.</summary>
    Subscript,
    /// <summary>Keeps the first item of a sequence.</summary>
    First,
    /// <summary>Keeps the last item of a sequence.</summary>
    Last,
    /// <summary>Loads the context position inside a predicate.</summary>
    Position,

    // ---- Comparisons -------------------------------------------------
    /// <summary>The <c>eq</c> value comparison.</summary>
    Equal,
    /// <summary>The <c>ne</c> value comparison.</summary>
    NotEqual,
    /// <summary>The <c>lt</c> value comparison.</summary>
    LessThan,
    /// <summary>The <c>le</c> value comparison.</summary>
    LessThanOrEqual,
    /// <summary>The <c>gt</c> value comparison.</summary>
    GreaterThan,
    /// <summary>The <c>ge</c> value comparison.</summary>
    GreaterThanOrEqual,
    /// <summary>Value-equal comparison (<c>eq</c> semantics).</summary>
    ValueEqual,
    /// <summary>Value-not-equal comparison (<c>ne</c> semantics).</summary>
    ValueNotEqual,
    /// <summary>Value less-than comparison (<c>lt</c> semantics).</summary>
    ValueLessThan,
    /// <summary>Value less-than-or-equal comparison (<c>le</c> semantics).</summary>
    ValueLessThanOrEqual,
    /// <summary>Value greater-than comparison (<c>gt</c> semantics).</summary>
    ValueGreaterThan,
    /// <summary>Value greater-than-or-equal comparison (<c>ge</c> semantics).</summary>
    ValueGreaterThanOrEqual,
    /// <summary>The <c>=</c> general comparison.</summary>
    GeneralEqual,
    /// <summary>The <c>!=</c> general comparison.</summary>
    GeneralNotEqual,
    /// <summary>The <c>&lt;</c> general comparison.</summary>
    GeneralLessThan,
    /// <summary>The <c>&lt;=</c> general comparison.</summary>
    GeneralLessThanOrEqual,
    /// <summary>The <c>&gt;</c> general comparison.</summary>
    GeneralGreaterThan,
    /// <summary>The <c>&gt;=</c> general comparison.</summary>
    GeneralGreaterThanOrEqual,
    /// <summary>The <c>is</c> node identity comparison.</summary>
    IsSameNode,
    /// <summary>The <c>&lt;&lt;</c> node-before comparison.</summary>
    PrecedesNode,
    /// <summary>The <c>&gt;&gt;</c> node-after comparison.</summary>
    FollowsNode,

    // ---- Arithmetic --------------------------------------------------
    /// <summary>The <c>+</c> arithmetic operator.</summary>
    Add,
    /// <summary>The <c>-</c> arithmetic operator.</summary>
    Subtract,
    /// <summary>The <c>*</c> arithmetic operator.</summary>
    Multiply,
    /// <summary>The <c>div</c> arithmetic operator.</summary>
    Divide,
    /// <summary>The <c>idiv</c> integer division operator.</summary>
    IntegerDivide,
    /// <summary>The <c>mod</c> arithmetic operator.</summary>
    Modulo,
    /// <summary>Unary plus, with a runtime type check.</summary>
    UnaryPlus,
    /// <summary>Unary minus, with a runtime type check.</summary>
    UnaryMinus,

    // ---- Boolean logic -----------------------------------------------
    /// <summary>Boolean conjunction (<c>and</c>).</summary>
    And,
    /// <summary>Boolean disjunction (<c>or</c>).</summary>
    Or,
    /// <summary>Boolean negation (<c>fn:not</c>).</summary>
    Not,

    // ---- String ------------------------------------------------------
    /// <summary>The <c>||</c> string concatenation operator.</summary>
    StringConcat,
    /// <summary>The <c>fn:string-length</c> operation.</summary>
    StringLength,
    /// <summary>The <c>fn:substring</c> operation.</summary>
    Substring,
    /// <summary>The <c>fn:contains</c> operation.</summary>
    Contains,
    /// <summary>The <c>fn:starts-with</c> operation.</summary>
    StartsWith,
    /// <summary>The <c>fn:ends-with</c> operation.</summary>
    EndsWith,
    /// <summary>The <c>fn:normalize-space</c> operation.</summary>
    NormalizeSpace,
    /// <summary>The <c>fn:translate</c> operation.</summary>
    Translate,
    /// <summary>The <c>fn:upper-case</c> operation.</summary>
    UpperCase,
    /// <summary>The <c>fn:lower-case</c> operation.</summary>
    LowerCase,
    /// <summary>The <c>fn:matches</c> regular-expression match.</summary>
    MatchesRegex,
    /// <summary>The <c>fn:replace</c> regular-expression replacement.</summary>
    ReplaceRegex,
    /// <summary>The <c>fn:tokenize</c> regular-expression split.</summary>
    TokenizeRegex,

    // ---- Type operations ---------------------------------------------
    /// <summary>The <c>cast as</c> conversion.</summary>
    Cast,
    /// <summary>The <c>castable as</c> type test.</summary>
    Castable,
    /// <summary>The <c>instance of</c> type test.</summary>
    InstanceOf,
    /// <summary>The <c>treat as</c> type assertion.</summary>
    TreatAs,
    /// <summary>The XQuery <c>validate</c> expression.</summary>
    Validate,

    // ---- Sequence functions ------------------------------------------
    /// <summary>The <c>fn:count</c> operation.</summary>
    Count,
    /// <summary>The <c>fn:exists</c> operation.</summary>
    Exists,
    /// <summary>The <c>fn:empty</c> operation.</summary>
    Empty,
    /// <summary>The <c>fn:head</c> operation.</summary>
    Head,
    /// <summary>The <c>fn:tail</c> operation.</summary>
    Tail,
    /// <summary>The <c>fn:insert-before</c> operation.</summary>
    InsertBefore,
    /// <summary>The <c>fn:remove</c> operation.</summary>
    Remove,
    /// <summary>The <c>fn:reverse</c> operation.</summary>
    Reverse,
    /// <summary>The <c>fn:subsequence</c> operation.</summary>
    Subsequence,
    /// <summary>The <c>fn:distinct-values</c> operation.</summary>
    DistinctValues,
    /// <summary>The <c>fn:index-of</c> operation.</summary>
    IndexOf,

    // ---- Aggregation -------------------------------------------------
    /// <summary>The <c>fn:sum</c> operation.</summary>
    Sum,
    /// <summary>The <c>fn:avg</c> operation.</summary>
    Avg,
    /// <summary>The <c>fn:min</c> operation.</summary>
    Min,
    /// <summary>The <c>fn:max</c> operation.</summary>
    Max,
    /// <summary>The <c>fn:string-join</c> operation.</summary>
    StringJoin,

    // ---- Higher-order (XPath 3.1) ------------------------------------
    /// <summary>Begins a map constructor.</summary>
    Map,
    /// <summary>Adds an entry to a map under construction.</summary>
    MapAdd,
    /// <summary>Begins an array constructor.</summary>
    Array,
    /// <summary>Adds a member to an array under construction (square constructor).</summary>
    ArrayAdd,
    /// <summary>Adds all items from a sequence to an array (curly constructor).</summary>
    ArrayAddAll,
    /// <summary>Looks up a key or index in a map or array (<c>?</c>).</summary>
    Lookup,
    /// <summary>Looks up all values of a map or array (<c>?*</c>).</summary>
    LookupWildcard,
    /// <summary>Loads a function item from the literal pool.</summary>
    LoadFunction,
    /// <summary>Applies partial application (currying) to a function item.</summary>
    Curry,
    /// <summary>Applies a function item to its arguments.</summary>
    Apply,

    // ---- Constructors ------------------------------------------------
    /// <summary>Constructs an element node.</summary>
    ElementConstructor,
    /// <summary>Constructs an attribute node.</summary>
    AttributeConstructor,
    /// <summary>Constructs a text node.</summary>
    TextConstructor,
    /// <summary>Constructs a document node.</summary>
    DocumentConstructor,

    // ---- Error -------------------------------------------------------
    /// <summary>Raises a dynamic error (code in the literal pool).</summary>
    Error
}
