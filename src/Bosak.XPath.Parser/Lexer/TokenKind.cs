// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : The kinds of tokens produced by the XPath 3.1 lexer
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 29-07-2026     | Percent token kind for XQuery annotations |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Parser.Lexer;

/// <summary>
/// The kinds of tokens produced by the XPath 3.1 lexer.
/// </summary>
public enum TokenKind : short
{
    Invalid = 0,
    Eof,

    // ---- Literals ----------------------------------------------------
    StringLiteral,
    IntegerLiteral,
    DecimalLiteral,
    DoubleLiteral,

    // ---- Identifiers -------------------------------------------------
    /// <summary>Any NCName or QName (prefix:local).</summary>
    Name,
    /// <summary>The wildcard '*'.</summary>
    Star,
    /// <summary>A whole XQuery direct element constructor (&lt;name ...&gt;...&lt;/name&gt;), emitted as one token when the lexer is in constructor mode.</summary>
    Constructor,

    // ---- Grouping / punctuation --------------------------------------
    LParen,          // (
    RParen,          // )
    LBracket,        // [
    RBracket,        // ]
    LBrace,          // {
    RBrace,          // }
    Comma,           // ,
    Colon,           // :
    DoubleColon,     // ::
    Semicolon,       // ;
    Dot,             // .
    DotDot,          // ..

    // ---- Primary expression prefixes ---------------------------------
    Dollar,          // $
    At,              // @

    // ---- Operators ---------------------------------------------------
    Plus,            // +
    Minus,           // -
    Slash,           // /
    SlashSlash,      // //
    VBar,            // |
    Bang,            // !  (simple map operator, XPath 3.0+)
    Question,        // ?  (lookup operator, XPath 3.1)
    Hash,            // #  (function item arity)
    Percent,         // %  (annotation marker, XPath/XQuery 3.0+)
    Arrow,           // => (arrow operator, XPath 3.1)
    StringConcat,    // || (string concatenation, XPath 3.0+)

    // ---- General comparisons -----------------------------------------
    Equal,           // =
    NotEqual,        // !=
    LessThan,        // <
    LessThanOrEqual, // <=
    GreaterThan,     // >
    GreaterThanOrEqual, // >=

    // ---- Node comparisons / Assignment -----------------------------
    NodeBefore,      // <<
    NodeAfter,       // >>
    Assign,          // :=

    // ---- Value comparisons -------------------------------------------
    ValueEq,         // eq
    ValueNe,         // ne
    ValueLt,         // lt
    ValueLe,         // le
    ValueGt,         // gt
    ValueGe,         // ge
    ValueIs,         // is

    // ---- Boolean operators -------------------------------------------
    KeywordAnd,      // and
    KeywordOr,       // or

    // ---- Arithmetic operators ----------------------------------------
    KeywordDiv,      // div
    KeywordIdiv,     // idiv
    KeywordMod,      // mod

    // ---- Sequence operators ------------------------------------------
    KeywordUnion,    // union
    KeywordIntersect,// intersect
    KeywordExcept,   // except
    KeywordTo,       // to

    // ---- Type operators ----------------------------------------------
    KeywordInstance, // instance
    KeywordOf,       // of
    KeywordTreat,    // treat
    KeywordAs,       // as
    KeywordCastable, // castable
    KeywordCast,     // cast

    // ---- Conditional -------------------------------------------------
    KeywordIf,       // if
    KeywordThen,     // then
    KeywordElse,     // else

    // ---- FLWOR / Quantified ------------------------------------------
    KeywordFor,      // for
    KeywordLet,      // let
    KeywordIn,       // in
    KeywordReturn,   // return
    KeywordSome,     // some
    KeywordEvery,    // every
    KeywordSatisfies,// satisfies

    // ---- Constructors / Higher-order ---------------------------------
    KeywordFunction, // function
    KeywordMap,      // map
    KeywordArray,    // array

    // ---- Try/Catch ---------------------------------------------------
    KeywordTry,      // try
    KeywordCatch,    // catch
}
