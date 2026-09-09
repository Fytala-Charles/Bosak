// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : The kinds of tokens produced by the XPath 3.1 lexer
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
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 29-07-2026     | Percent token kind for XQuery annotations |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 22-08-2026     | Added KeywordValidate for XQuery validate expressions |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Parser.Lexer;

/// <summary>
/// The kinds of tokens produced by the XPath 3.1 lexer.
/// </summary>
public enum TokenKind : short
{
    /// <summary>An invalid token; no valid token could be scanned at the position.</summary>
    Invalid = 0,
    /// <summary>The end-of-input token.</summary>
    Eof,

    // ---- Literals ----------------------------------------------------
    /// <summary>A string literal: <c>'...'</c> or <c>"..."</c>.</summary>
    StringLiteral,
    /// <summary>An integer literal: <c>42</c>.</summary>
    IntegerLiteral,
    /// <summary>A decimal literal: <c>3.14</c>.</summary>
    DecimalLiteral,
    /// <summary>A double literal: <c>1e3</c>.</summary>
    DoubleLiteral,

    // ---- Identifiers -------------------------------------------------
    /// <summary>Any NCName or QName (prefix:local).</summary>
    Name,
    /// <summary>The wildcard '*'.</summary>
    Star,
    /// <summary>A whole XQuery direct element constructor (&lt;name ...&gt;...&lt;/name&gt;), emitted as one token when the lexer is in constructor mode.</summary>
    Constructor,

    // ---- Grouping / punctuation --------------------------------------
    /// <summary>The <c>(</c> token.</summary>
    LParen,
    /// <summary>The <c>)</c> token.</summary>
    RParen,
    /// <summary>The <c>[</c> token.</summary>
    LBracket,
    /// <summary>The <c>]</c> token.</summary>
    RBracket,
    /// <summary>The <c>{</c> token.</summary>
    LBrace,
    /// <summary>The <c>}</c> token.</summary>
    RBrace,
    /// <summary>The <c>,</c> token.</summary>
    Comma,
    /// <summary>The <c>:</c> token.</summary>
    Colon,
    /// <summary>The <c>::</c> axis separator.</summary>
    DoubleColon,
    /// <summary>The <c>;</c> token.</summary>
    Semicolon,
    /// <summary>The <c>.</c> context item token.</summary>
    Dot,
    /// <summary>The <c>..</c> parent step token.</summary>
    DotDot,

    // ---- Primary expression prefixes ---------------------------------
    /// <summary>The <c>$</c> variable reference prefix.</summary>
    Dollar,
    /// <summary>The <c>@</c> attribute axis abbreviation.</summary>
    At,

    // ---- Operators ---------------------------------------------------
    /// <summary>The <c>+</c> operator.</summary>
    Plus,
    /// <summary>The <c>-</c> operator.</summary>
    Minus,
    /// <summary>The <c>/</c> path operator.</summary>
    Slash,
    /// <summary>The <c>//</c> descendant-or-self path abbreviation.</summary>
    SlashSlash,
    /// <summary>The <c>|</c> union operator.</summary>
    VBar,
    /// <summary>The <c>!</c> simple map operator (XPath 3.0+).</summary>
    Bang,
    /// <summary>The <c>?</c> lookup operator (XPath 3.1).</summary>
    Question,
    /// <summary>The <c>#</c> function item arity marker.</summary>
    Hash,
    /// <summary>The <c>%</c> annotation marker (XPath/XQuery 3.0+).</summary>
    Percent,
    /// <summary>The <c>=&gt;</c> arrow operator (XPath 3.1).</summary>
    Arrow,
    /// <summary>The <c>||</c> string concatenation operator (XPath 3.0+).</summary>
    StringConcat,

    // ---- General comparisons -----------------------------------------
    /// <summary>The <c>=</c> general comparison.</summary>
    Equal,
    /// <summary>The <c>!=</c> general comparison.</summary>
    NotEqual,
    /// <summary>The <c>&lt;</c> general comparison.</summary>
    LessThan,
    /// <summary>The <c>&lt;=</c> general comparison.</summary>
    LessThanOrEqual,
    /// <summary>The <c>&gt;</c> general comparison.</summary>
    GreaterThan,
    /// <summary>The <c>&gt;=</c> general comparison.</summary>
    GreaterThanOrEqual,

    // ---- Node comparisons / Assignment -----------------------------
    /// <summary>The <c>&lt;&lt;</c> node-before comparison.</summary>
    NodeBefore,
    /// <summary>The <c>&gt;&gt;</c> node-after comparison.</summary>
    NodeAfter,
    /// <summary>The <c>:=</c> assignment operator (XQuery let bindings).</summary>
    Assign,

    // ---- Value comparisons -------------------------------------------
    /// <summary>The <c>eq</c> value comparison.</summary>
    ValueEq,
    /// <summary>The <c>ne</c> value comparison.</summary>
    ValueNe,
    /// <summary>The <c>lt</c> value comparison.</summary>
    ValueLt,
    /// <summary>The <c>le</c> value comparison.</summary>
    ValueLe,
    /// <summary>The <c>gt</c> value comparison.</summary>
    ValueGt,
    /// <summary>The <c>ge</c> value comparison.</summary>
    ValueGe,
    /// <summary>The <c>is</c> node identity comparison.</summary>
    ValueIs,

    // ---- Boolean operators -------------------------------------------
    /// <summary>The <c>and</c> boolean operator.</summary>
    KeywordAnd,
    /// <summary>The <c>or</c> boolean operator.</summary>
    KeywordOr,

    // ---- Arithmetic operators ----------------------------------------
    /// <summary>The <c>div</c> arithmetic operator.</summary>
    KeywordDiv,
    /// <summary>The <c>idiv</c> integer division operator.</summary>
    KeywordIdiv,
    /// <summary>The <c>mod</c> arithmetic operator.</summary>
    KeywordMod,

    // ---- Sequence operators ------------------------------------------
    /// <summary>The <c>union</c> node set operator.</summary>
    KeywordUnion,
    /// <summary>The <c>intersect</c> node set operator.</summary>
    KeywordIntersect,
    /// <summary>The <c>except</c> node set operator.</summary>
    KeywordExcept,
    /// <summary>The <c>to</c> range operator.</summary>
    KeywordTo,

    // ---- Type operators ----------------------------------------------
    /// <summary>The <c>instance</c> keyword of <c>instance of</c>.</summary>
    KeywordInstance,
    /// <summary>The <c>of</c> keyword of <c>instance of</c>.</summary>
    KeywordOf,
    /// <summary>The <c>treat</c> keyword of <c>treat as</c>.</summary>
    KeywordTreat,
    /// <summary>The <c>as</c> keyword of type expressions.</summary>
    KeywordAs,
    /// <summary>The <c>castable</c> keyword of <c>castable as</c>.</summary>
    KeywordCastable,
    /// <summary>The <c>cast</c> keyword of <c>cast as</c>.</summary>
    KeywordCast,

    // ---- Conditional -------------------------------------------------
    /// <summary>The <c>if</c> keyword.</summary>
    KeywordIf,
    /// <summary>The <c>then</c> keyword.</summary>
    KeywordThen,
    /// <summary>The <c>else</c> keyword.</summary>
    KeywordElse,

    // ---- FLWOR / Quantified ------------------------------------------
    /// <summary>The <c>for</c> keyword.</summary>
    KeywordFor,
    /// <summary>The <c>let</c> keyword.</summary>
    KeywordLet,
    /// <summary>The <c>in</c> keyword.</summary>
    KeywordIn,
    /// <summary>The <c>return</c> keyword.</summary>
    KeywordReturn,
    /// <summary>The <c>some</c> keyword.</summary>
    KeywordSome,
    /// <summary>The <c>every</c> keyword.</summary>
    KeywordEvery,
    /// <summary>The <c>satisfies</c> keyword.</summary>
    KeywordSatisfies,

    // ---- Constructors / Higher-order ---------------------------------
    /// <summary>The <c>function</c> keyword of inline functions.</summary>
    KeywordFunction,
    /// <summary>The <c>map</c> keyword of map constructors.</summary>
    KeywordMap,
    /// <summary>The <c>array</c> keyword of array constructors.</summary>
    KeywordArray,

    // ---- Try/Catch ---------------------------------------------------
    /// <summary>The <c>try</c> keyword.</summary>
    KeywordTry,
    /// <summary>The <c>catch</c> keyword.</summary>
    KeywordCatch,

    // ---- XQuery validate expression ----------------------------------
    /// <summary>The <c>validate</c> keyword of XQuery validate expressions.</summary>
    KeywordValidate,
}
