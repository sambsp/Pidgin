using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Xunit;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Pidgin.Tests
{
    public class StringParserTests : ParserTestBase
    {
        [Fact]
        public void TestReturn()
        {
            {
                var parser = Return('a');
                AssertSuccess(parser.Parse(""), 'a', false);
                AssertSuccess(parser.Parse("foobar"), 'a', false);
            }
            {
                var parser = FromResult('a');
                AssertSuccess(parser.Parse(""), 'a', false);
                AssertSuccess(parser.Parse("foobar"), 'a', false);
            }
        }

        [Fact]
        public void TestFail()
        {
            {
                var parser = Fail<Unit>("message");
                var expectedError = new ParseError<char>(
                    Maybe.Nothing<char>(),
                    false,
                    ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create<char>())),
                    new SourcePos(1,1),
                    "message"
                );
                AssertFailure(parser.Parse(""), expectedError, false);
                AssertFailure(parser.Parse("foobar"), expectedError, false);
            }
        }

        [Fact]
        public void TestToken()
        {
            {
                var parser = Char('a');
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("ab"), 'a', true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('a'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("b"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('a'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = AnyCharExcept('a', 'b', 'c');
                AssertSuccess(parser.Parse("e"), 'e', true);
                AssertFailure(
                    parser.Parse("b"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = Token('a'.Equals);
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("ab"), 'a', true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("b"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = Any;
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("b"), 'b', true);
                AssertSuccess(parser.Parse("ab"), 'a', true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>("any character")),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = Whitespace;
                AssertSuccess(parser.Parse("\r"), '\r', true);
                AssertSuccess(parser.Parse("\n"), '\n', true);
                AssertSuccess(parser.Parse("\t"), '\t', true);
                AssertSuccess(parser.Parse(" "), ' ', true);
                AssertSuccess(parser.Parse(" abc"), ' ', true);
                AssertFailure(
                    parser.Parse("abc"),
                    new ParseError<char>(
                        Maybe.Just('a'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("whitespace")),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>("whitespace")),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestCIChar()
        {
            {
                var parser = CIChar('a');
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("ab"), 'a', true);
                AssertSuccess(parser.Parse("A"), 'A', true);
                AssertSuccess(parser.Parse("AB"), 'A', true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('A')), new Expected<char>(ImmutableArray.Create('a'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("b"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('A')), new Expected<char>(ImmutableArray.Create('a'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestEnd()
        {
            {
                var parser = End;
                AssertSuccess(parser.Parse(""), Unit.Value, false);
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Just('a'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>()),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestNumber()
        {
            {
                var parser = Num;
                AssertSuccess(parser.Parse("0"), 0, true);
                AssertSuccess(parser.Parse("+0"), +0, true);
                AssertSuccess(parser.Parse("-0"), -0, true);
                AssertSuccess(parser.Parse("1"), 1, true);
                AssertSuccess(parser.Parse("+1"), +1, true);
                AssertSuccess(parser.Parse("-1"), -1, true);
                AssertSuccess(parser.Parse("12345"), 12345, true);
                AssertSuccess(parser.Parse("1a"), 1, true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>("number")),
                        new SourcePos(1, 1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Just('a'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("number")),
                        new SourcePos(1, 1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("+"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>("number")),
                        new SourcePos(1, 2),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("-"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>("number")),
                        new SourcePos(1, 2),
                        null
                    ),
                    true
                );
            }
            {
                var parser = HexNum;
                AssertSuccess(parser.Parse("ab"), 0xab, true);
                AssertSuccess(parser.Parse("cd"), 0xcd, true);
                AssertSuccess(parser.Parse("ef"), 0xef, true);
                AssertSuccess(parser.Parse("AB"), 0xAB, true);
                AssertSuccess(parser.Parse("CD"), 0xCD, true);
                AssertSuccess(parser.Parse("EF"), 0xEF, true);
                AssertFailure(
                    parser.Parse("g"),
                    new ParseError<char>(
                        Maybe.Just('g'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("hexadecimal number")),
                        new SourcePos(1, 1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = OctalNum;
                AssertSuccess(parser.Parse("7"), 7, true);
                AssertFailure(
                    parser.Parse("8"),
                    new ParseError<char>(
                        Maybe.Just('8'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("octal number")),
                        new SourcePos(1, 1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = LongNum;
                AssertSuccess(parser.Parse("0"), 0L, true);
                AssertSuccess(parser.Parse("+0"), +0L, true);
                AssertSuccess(parser.Parse("-0"), -0L, true);
                AssertSuccess(parser.Parse("1"), 1L, true);
                AssertSuccess(parser.Parse("+1"), +1L, true);
                AssertSuccess(parser.Parse("-1"), -1L, true);
                AssertSuccess(parser.Parse("12345"), 12345L, true);
                var tooBigForInt = ((long)int.MaxValue) + 1;
                AssertSuccess(parser.Parse(tooBigForInt.ToString()), tooBigForInt, true);
                AssertSuccess(parser.Parse("1a"), 1, true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>("number")),
                        new SourcePos(1, 1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Just('a'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("number")),
                        new SourcePos(1, 1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("+"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>("number")),
                        new SourcePos(1, 2),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("-"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>("number")),
                        new SourcePos(1, 2),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestSequence()
        {
            {
                var parser = String("foo");
                AssertSuccess(parser.Parse("foo"), "foo", true);
                AssertSuccess(parser.Parse("food"), "foo", true);
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = Sequence(Char('f'), Char('o'), Char('o'));
                AssertSuccess(parser.Parse("foo"), "foo", true);
                AssertSuccess(parser.Parse("food"), "foo", true);
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("f"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("o"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("f"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestCIString()
        {
            {
                var parser = CIString("foo");
                AssertSuccess(parser.Parse("foo"), "foo", true);
                AssertSuccess(parser.Parse("food"), "foo", true);
                AssertSuccess(parser.Parse("FOO"), "FOO", true);
                AssertSuccess(parser.Parse("FOOD"), "FOO", true);
                AssertSuccess(parser.Parse("fOo"), "fOo", true);
                AssertSuccess(parser.Parse("Food"), "Foo", true);
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("FOul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestBind()
        {
            {
                // any two equal characters
                var parser = Any.Then(c => Token(c.Equals));
                AssertSuccess(parser.Parse("aa"), 'a', true);
                AssertFailure(
                    parser.Parse("ab"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1,2),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Any.Bind(c => Token(c.Equals), (x, y) => new { x, y });
                AssertSuccess(parser.Parse("aa"), new { x = 'a', y = 'a' }, true);
                AssertFailure(
                    parser.Parse("ab"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1,2),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Any.Then(c => Token(c.Equals), (x, y) => new { x, y });
                AssertSuccess(parser.Parse("aa"), new { x = 'a', y = 'a' }, true);
                AssertFailure(
                    parser.Parse("ab"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1,2),
                        null
                    ),
                    true
                );
            }
            {
                var parser =
                    from x in Any
                    from y in Token(x.Equals)
                    select new { x, y };
                AssertSuccess(parser.Parse("aa"), new { x = 'a', y = 'a' }, true);
                AssertFailure(
                    parser.Parse("ab"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1,2),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Char('x').Then(c => Char('y'));
                AssertSuccess(parser.Parse("xy"), 'y', true);
                AssertFailure(
                    parser.Parse("yy"),
                    new ParseError<char>(
                        Maybe.Just('y'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('x'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("xx"),
                    new ParseError<char>(
                        Maybe.Just('x'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('y'))),
                        new SourcePos(1,2),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestThen()
        {
            {
                var parser = Char('a').Then(Char('b'));
                AssertSuccess(parser.Parse("ab"), 'b', true);
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("b"))),
                        new SourcePos(1,2),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("a"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = Char('a').Then(Char('b'), (a, b) => new { a, b });
                AssertSuccess(parser.Parse("ab"), new { a = 'a', b = 'b' }, true);
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("b"))),
                        new SourcePos(1,2),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("a"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = Char('a').Before(Char('b'));
                AssertSuccess(parser.Parse("ab"), 'a', true);
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("b"))),
                        new SourcePos(1,2),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("a"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestMap()
        {
            {
                var parser = Map((x, y, z) => new { x, y, z }, Char('a'), Char('b'), Char('c'));
                AssertSuccess(parser.Parse("abc"), new { x = 'a', y = 'b', z = 'c' }, true);
                AssertFailure(
                    parser.Parse("abd"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("c"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Char('a').Select(a => new { a });
                AssertSuccess(parser.Parse("a"), new { a = 'a' }, true);
            }
            {
                var parser =
                    from a in Char('a')
                    select new { a };
                AssertSuccess(parser.Parse("a"), new { a = 'a' }, true);
            }
        }

        [Fact]
        public void TestOr()
        {
            {
                var parser = Fail<char>("test").Or(Char('a'));
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertFailure(
                    parser.Parse("b"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create<char>()), new Expected<char>(ImmutableArray.Create('a'))),
                        new SourcePos(1,1),
                        "test"
                    ),
                    false
                );
            }
            {
                var parser = Char('a').Or(Char('b'));
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("b"), 'b', true);
                AssertFailure(
                    parser.Parse("c"),
                    new ParseError<char>(
                        Maybe.Just('c'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('a')), new Expected<char>(ImmutableArray.Create('b'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = String("foo").Or(String("bar"));
                AssertSuccess(parser.Parse("foo"), "foo", true);
                AssertSuccess(parser.Parse("bar"), "bar", true);
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
            }
            {
                var parser = String("foo").Or(String("foul"));
                // because the first parser consumed input
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Try(String("foo")).Or(String("foul"));
                AssertSuccess(parser.Parse("foul"), "foul", true);
            }
        }

        [Fact]
        public void TestOneOf()
        {
            {
                var parser = OneOf(Char('a'), Char('b'), Char('c'));
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("b"), 'b', true);
                AssertSuccess(parser.Parse("c"), 'c', true);
                AssertFailure(
                    parser.Parse("d"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('a')), new Expected<char>(ImmutableArray.Create('b')), new Expected<char>(ImmutableArray.Create('c'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = OneOf("abc");
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("b"), 'b', true);
                AssertSuccess(parser.Parse("c"), 'c', true);
                AssertFailure(
                    parser.Parse("d"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('a')), new Expected<char>(ImmutableArray.Create('b')), new Expected<char>(ImmutableArray.Create('c'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = OneOf(String("foo"), String("bar"));
                AssertSuccess(parser.Parse("foo"), "foo", true);
                AssertSuccess(parser.Parse("bar"), "bar", true);
                AssertFailure(
                    parser.Parse("quux"),
                    new ParseError<char>(
                        Maybe.Just('q'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("bar")), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestCIOneOf()
        {
            {
                var parser = CIOneOf('a', 'b', 'c');
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("b"), 'b', true);
                AssertSuccess(parser.Parse("c"), 'c', true);
                AssertSuccess(parser.Parse("A"), 'A', true);
                AssertSuccess(parser.Parse("B"), 'B', true);
                AssertSuccess(parser.Parse("C"), 'C', true);
                AssertFailure(
                    parser.Parse("d"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(
                            new Expected<char>(ImmutableArray.Create('A')),
                            new Expected<char>(ImmutableArray.Create('B')),
                            new Expected<char>(ImmutableArray.Create('C')),
                            new Expected<char>(ImmutableArray.Create('a')),
                            new Expected<char>(ImmutableArray.Create('b')),
                            new Expected<char>(ImmutableArray.Create('c'))
                        ),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
            {
                var parser = CIOneOf("abc");
                AssertSuccess(parser.Parse("a"), 'a', true);
                AssertSuccess(parser.Parse("b"), 'b', true);
                AssertSuccess(parser.Parse("c"), 'c', true);
                AssertSuccess(parser.Parse("A"), 'A', true);
                AssertSuccess(parser.Parse("B"), 'B', true);
                AssertSuccess(parser.Parse("C"), 'C', true);
                AssertFailure(
                    parser.Parse("d"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(
                            new Expected<char>(ImmutableArray.Create('A')),
                            new Expected<char>(ImmutableArray.Create('B')),
                            new Expected<char>(ImmutableArray.Create('C')),
                            new Expected<char>(ImmutableArray.Create('a')),
                            new Expected<char>(ImmutableArray.Create('b')),
                            new Expected<char>(ImmutableArray.Create('c'))
                        ),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestNot()
        {
            {
                var parser = Not(String("food")).Then(String("bar"));
                AssertSuccess(parser.Parse("foobar"), "bar", true);
            }
            {
                var parser = Not(OneOf(Char('a'), Char('b'), Char('c')));
                AssertSuccess(parser.Parse("e"), Unit.Value, false);
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Just('a'),
                        false,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1, 1),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Not(Return('f'));
                AssertFailure(
                    parser.Parse("foobar"),
                    new ParseError<char>(
                        Maybe.Just('f'),
                        false,
                        ImmutableSortedSet<Expected<char>>.Empty,
                        new SourcePos(1, 1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestLookahead()
        {
            {
                var parser = Lookahead(String("foo"));
                AssertSuccess(parser.Parse("foo"), "foo", false);
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foe"),
                    new ParseError<char>(
                        Maybe.Just('e'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
            }
            {
                // should backtrack on success
                var parser = Lookahead(String("foo")).Then(String("food"));
                AssertSuccess(parser.Parse("food"), "food", true);
            }
        }

        [Fact]
        public void TestRecoverWith()
        {
            {
                var parser = String("foo").ThenReturn((ParseError<char>)null)
                    .RecoverWith(err => String("bar").ThenReturn(err));

                AssertSuccess(
                    parser.Parse("fobar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1, 3),
                        null
                    ),
                    true
                );
            }
            {
                var parser = String("nabble").ThenReturn((ParseError<char>)null)
                    .Or(
                        String("foo").ThenReturn((ParseError<char>)null)
                            .RecoverWith(err => String("bar").ThenReturn(err))
                    );
                
                // shouldn't get the expected from nabble
                AssertSuccess(
                    parser.Parse("fobar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1, 3),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestTryUsingStaticExample()
        {
            {
                string MkString(char first, IEnumerable<char> rest)
                {
                    var sb = new StringBuilder();
                    sb.Append(first);
                    sb.Append(string.Concat(rest));
                    return sb.ToString();
                }

                var pUsing = String("using");
                var pStatic = String("static");
                var identifier = Token(char.IsLetter)
                    .Then(Token(char.IsLetterOrDigit).Many(), MkString)
                    .Labelled("identifier");
                var usingStatic =
                    from kws in Try(
                        from u in pUsing.Before(Whitespace.AtLeastOnce())
                        from s in pStatic.Before(Whitespace.AtLeastOnce())
                        select new {}
                    )
                    from id in identifier
                    select new { isStatic = true, id };
                var notStatic =
                    from u in pUsing
                    from ws in Whitespace.AtLeastOnce()
                    from id in identifier
                    select new { isStatic = false, id };
                var parser = usingStatic.Or(notStatic);

                AssertSuccess(parser.Parse("using static Console"), new { isStatic = true, id = "Console" }, true);
                AssertSuccess(parser.Parse("using System"), new { isStatic = false, id = "System" }, true);
                AssertFailure(
                    parser.Parse("usine"),
                    new ParseError<char>(
                        Maybe.Just('e'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("using"))),
                        new SourcePos(1,5),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("using 123"),
                    new ParseError<char>(
                        Maybe.Just('1'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("identifier")),
                        new SourcePos(1,7),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestAssert()
        {
            {
                var parser = Char('a').Assert('a'.Equals);
                AssertSuccess(parser.Parse("a"), 'a', true);
            }
            {
                var parser = Char('a').Assert('b'.Equals);
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("result satisfying assertion")),
                        new SourcePos(1,2),
                        "Assertion failed"
                    ),
                    true
                );
            }
            {
                var parser = Char('a').Where('a'.Equals);
                AssertSuccess(parser.Parse("a"), 'a', true);
            }
            {
                var parser = Char('a').Where('b'.Equals);
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("result satisfying assertion")),
                        new SourcePos(1,2),
                        "Assertion failed"
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestMany()
        {
            {
                var parser = String("foo").Many();
                AssertSuccess(parser.Parse(""), Enumerable.Empty<string>(), false);
                AssertSuccess(parser.Parse("bar"), Enumerable.Empty<string>(), false);
                AssertSuccess(parser.Parse("foo"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foofoo"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("food"), new[] { "foo" }, true);
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foofoul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,6),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Whitespaces;
                AssertSuccess(parser.Parse("    "), new[] { ' ', ' ', ' ', ' ' }, true);
                AssertSuccess(parser.Parse("\r\n"), new[] { '\r', '\n' }, true);
                AssertSuccess(parser.Parse(" abc"), new[] { ' ' }, true);
                AssertSuccess(parser.Parse("abc"), Enumerable.Empty<char>(), false);
                AssertSuccess(parser.Parse(""), Enumerable.Empty<char>(), false);
            }
            {
                var parser = Return(1).Many();
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestManyString()
        {
            {
                var parser = Char('f').ManyString();
                AssertSuccess(parser.Parse(""), "", false);
                AssertSuccess(parser.Parse("bar"), "", false);
                AssertSuccess(parser.Parse("f"), "f", true);
                AssertSuccess(parser.Parse("ff"), "ff", true);
                AssertSuccess(parser.Parse("fo"), "f", true);
            }
            {
                var parser = String("f").ManyString();
                AssertSuccess(parser.Parse(""), "", false);
                AssertSuccess(parser.Parse("bar"), "", false);
                AssertSuccess(parser.Parse("f"), "f", true);
                AssertSuccess(parser.Parse("ff"), "ff", true);
                AssertSuccess(parser.Parse("fo"), "f", true);
            }
            {
                var parser = Return('f').ManyString();
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestSkipMany()
        {
            {
                var parser = String("foo").SkipMany();
                AssertSuccess(parser.Parse(""), Unit.Value, false);
                AssertSuccess(parser.Parse("bar"), Unit.Value, false);
                AssertSuccess(parser.Parse("foo"), Unit.Value, true);
                AssertSuccess(parser.Parse("foofoo"), Unit.Value, true);
                AssertSuccess(parser.Parse("food"), Unit.Value, true);
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foofoul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,6),
                        null
                    ),
                    true
                );
            }
            {
                var parser = SkipWhitespaces;
                AssertSuccess(parser.Parse("    "), Unit.Value, true);
                AssertSuccess(parser.Parse("\r\n"), Unit.Value, true);
                AssertSuccess(parser.Parse(" abc"), Unit.Value, true);
                AssertSuccess(parser.Parse("abc"), Unit.Value, false);
                AssertSuccess(parser.Parse(""), Unit.Value, false);
            }
            {
                var parser = Return(1).SkipMany();
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestAtLeastOnce()
        {
            {
                var parser = String("foo").AtLeastOnce();
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertSuccess(parser.Parse("foo"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foofoo"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("food"), new[] { "foo" }, true);
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foofoul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,6),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Return(1).AtLeastOnce();
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestAtLeastOnceString()
        {
            {
                var parser = Char('f').AtLeastOnceString();
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('f'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("b"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('f'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertSuccess(parser.Parse("f"), "f", true);
                AssertSuccess(parser.Parse("ff"), "ff", true);
                AssertSuccess(parser.Parse("fg"), "f", true);
            }
            {
                var parser = String("f").AtLeastOnceString();
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('f'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("b"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create('f'))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertSuccess(parser.Parse("f"), "f", true);
                AssertSuccess(parser.Parse("ff"), "ff", true);
                AssertSuccess(parser.Parse("fg"), "f", true);
            }
            {
                var parser = Return('f').AtLeastOnceString();
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestSkipAtLeastOnce()
        {
            {
                var parser = String("foo").SkipAtLeastOnce();
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertSuccess(parser.Parse("foo"), Unit.Value, true);
                AssertSuccess(parser.Parse("foofoo"), Unit.Value, true);
                AssertSuccess(parser.Parse("food"), Unit.Value, true);
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foofoul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,6),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Return(1).SkipAtLeastOnce();
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestUntil()
        {
            {
                var parser = String("foo").Until(Char(' '));
                AssertSuccess(parser.Parse(" "), Enumerable.Empty<string>(), true);
                AssertSuccess(parser.Parse(" bar"), Enumerable.Empty<string>(), true);
                AssertSuccess(parser.Parse("foo "), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foofoo "), new[] { "foo", "foo" }, true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foo"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("food"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foofoul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,6),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Return(1).Until(Char(' '));
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestSkipUntil()
        {
            {
                var parser = String("foo").SkipUntil(Char(' '));
                AssertSuccess(parser.Parse(" "), Unit.Value, true);
                AssertSuccess(parser.Parse(" bar"), Unit.Value, true);
                AssertSuccess(parser.Parse("foo "), Unit.Value, true);
                AssertSuccess(parser.Parse("foofoo "), Unit.Value, true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foo"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("food"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foofoul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,6),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Return(1).SkipUntil(Char(' '));
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestAtLeastOnceUntil()
        {
            {
                var parser = String("foo").AtLeastOnceUntil(Char(' '));
                AssertSuccess(parser.Parse("foo "), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foofoo "), new[] { "foo", "foo" }, true);
                AssertFailure(
                    parser.Parse(" "),
                    new ParseError<char>(
                        Maybe.Just(' '),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse(" bar"),
                    new ParseError<char>(
                        Maybe.Just(' '),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foo"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("food"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foofoul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,6),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Return(1).AtLeastOnceUntil(Char(' '));
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestSkipAtLeastOnceUntil()
        {
            {
                var parser = String("foo").SkipAtLeastOnceUntil(Char(' '));
                AssertSuccess(parser.Parse("foo "), Unit.Value, true);
                AssertSuccess(parser.Parse("foofoo "), Unit.Value, true);
                AssertFailure(
                    parser.Parse(" "),
                    new ParseError<char>(
                        Maybe.Just(' '),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse(" bar"),
                    new ParseError<char>(
                        Maybe.Just(' '),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foo"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("food"),
                    new ParseError<char>(
                        Maybe.Just('d'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' ')), new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foofoul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,6),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Return(1).SkipAtLeastOnceUntil(Char(' '));
                Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
            }
        }

        [Fact]
        public void TestRepeat()
        {
            {
                var parser = String("foo").Repeat(3);
                AssertSuccess(parser.Parse("foofoofoo"), new[] { "foo", "foo", "foo" }, true);
                AssertFailure(
                    parser.Parse("foofoo"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,7),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestSeparated()
        {
            {
                var parser = String("foo").Separated(Char(' '));
                AssertSuccess(parser.Parse(""), Enumerable.Empty<string>(), false);
                AssertSuccess(parser.Parse("foo"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foo foo"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foobar"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("bar"), Enumerable.Empty<string>(), false);
                AssertFailure(
                    parser.Parse("four"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foo bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,5),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestSeparatedAtLeastOnce()
        {
            {
                var parser = String("foo").SeparatedAtLeastOnce(Char(' '));
                AssertSuccess(parser.Parse("foo"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foo foo"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foobar"), new[] { "foo" }, true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("four"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foo bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,5),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestSeparatedAndTerminated()
        {
            {
                var parser = String("foo").SeparatedAndTerminated(Char(' '));
                AssertSuccess(parser.Parse("foo "), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foo foo "), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo bar"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse(""), new string[]{}, false);
                AssertSuccess(parser.Parse("bar"), new string[]{}, false);
                AssertFailure(
                    parser.Parse("foo"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange(" "))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("four"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foobar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange(" "))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foo foobar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange(" "))),
                        new SourcePos(1,8),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestSeparatedAndTerminatedAtLeastOnce()
        {
            {
                var parser = String("foo").SeparatedAndTerminatedAtLeastOnce(Char(' '));
                AssertSuccess(parser.Parse("foo "), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foo foo "), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo bar"), new[] { "foo" }, true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("foo"),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.Create(' '))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("four"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foobar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange(" "))),
                        new SourcePos(1,4),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foo foobar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange(" "))),
                        new SourcePos(1,8),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestSeparatedAndOptionallyTerminated()
        {
            {
                var parser = String("foo").SeparatedAndOptionallyTerminated(Char(' '));
                AssertSuccess(parser.Parse("foo "), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foo"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foo foo "), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo foo"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo foobar"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo foo bar"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo bar"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foobar"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse(""), new string[]{}, false);
                AssertSuccess(parser.Parse("bar"), new string[]{}, false);
                AssertFailure(
                    parser.Parse("four"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
                AssertFailure(
                    parser.Parse("foo four"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,7),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestSeparatedAndOptionallyTerminatedAtLeastOnce()
        {
            {
                var parser = String("foo").SeparatedAndOptionallyTerminatedAtLeastOnce(Char(' '));
                AssertSuccess(parser.Parse("foo "), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foo"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foo foo "), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo foo"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo foobar"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo foo bar"), new[] { "foo", "foo" }, true);
                AssertSuccess(parser.Parse("foo bar"), new[] { "foo" }, true);
                AssertSuccess(parser.Parse("foobar"), new[] { "foo" }, true);
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        true,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("bar"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    parser.Parse("four"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
            }
        }

        [Fact]
        public void TestBetween()
        {
            {
                var parser = String("foo").Between(Char('{'), Char('}'));
                AssertSuccess(parser.Parse("{foo}"), "foo", true);
            }
        }

        [Fact]
        public void TestOptional()
        {
            {
                var parser = String("foo").Optional();
                AssertSuccess(parser.Parse("foo"), Maybe.Just("foo"), true);
                AssertSuccess(parser.Parse("food"), Maybe.Just("foo"), true);
                AssertSuccess(parser.Parse("bar"), Maybe.Nothing<string>(), false);
                AssertSuccess(parser.Parse(""), Maybe.Nothing<string>(), false);
                AssertFailure(
                    parser.Parse("four"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>(ImmutableArray.CreateRange("foo"))),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
            }
            {
                var parser = Try(String("foo")).Optional();
                AssertSuccess(parser.Parse("foo"), Maybe.Just("foo"), true);
                AssertSuccess(parser.Parse("food"), Maybe.Just("foo"), true);
                AssertSuccess(parser.Parse("bar"), Maybe.Nothing<string>(), false);
                AssertSuccess(parser.Parse(""), Maybe.Nothing<string>(), false);
                AssertSuccess(parser.Parse("four"), Maybe.Nothing<string>(), false);
            }
            {
                var parser = Char('+').Optional().Then(Digit).Select(char.GetNumericValue);
                AssertSuccess(parser.Parse("1"), 1, true);
                AssertSuccess(parser.Parse("+1"), 1, true);
                AssertFailure(
                    parser.Parse("a"),
                    new ParseError<char>(
                        Maybe.Just('a'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("digit")),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestRec()
        {
            // roughly equivalent to String("foo").Separated(Char(' '))
            Parser<char, string> p2 = null;
            var p1 = String("foo").Then(
                Rec(() => p2).Optional(),
                (x, y) => y.HasValue ? x + y.Value : x
            );
            p2 = Char(' ').Then(Rec(() => p1));

            AssertSuccess(p1.Parse("foo foo"), "foofoo", true);
        }

        [Fact]
        public void TestLabelled()
        {
            {
                var p = String("foo").Labelled("bar");
                AssertFailure(
                    p.Parse("baz"),
                    new ParseError<char>(
                        Maybe.Just('b'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("bar")),
                        new SourcePos(1,1),
                        null
                    ),
                    false
                );
                AssertFailure(
                    p.Parse("foul"),
                    new ParseError<char>(
                        Maybe.Just('u'),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("bar")),
                        new SourcePos(1,3),
                        null
                    ),
                    true
                );
            }
        }

        private class TestCast1{}
        private class TestCast2 : TestCast1
        {
            public override bool Equals(object other) => other is TestCast2;
            public override int GetHashCode() => 1;
        }
        [Fact]
        public void TestCast()
        {
            {
                var parser = Return(new TestCast2()).Cast<TestCast1>();
                AssertSuccess(parser.Parse(""), new TestCast2(), false);
            }
            {
                var parser = Return(new TestCast1()).OfType<TestCast2>();
                AssertFailure(
                    parser.Parse(""),
                    new ParseError<char>(
                        Maybe.Nothing<char>(),
                        false,
                        ImmutableSortedSet.Create(new Expected<char>("result of type TestCast2")),
                        new SourcePos(1,1),
                        "Expected a TestCast2 but got a TestCast1"
                    ),
                    false
                );
            }
        }

        [Fact]
        public void TestCurrentPos()
        {
            {
                var parser = CurrentPos;
                AssertSuccess(parser.Parse(""), new SourcePos(1,1), false);
            }
            {
                var parser = String("foo").Then(CurrentPos);
                AssertSuccess(parser.Parse("foo"), new SourcePos(1,4), true);
            }
            {
                var parser = Try(String("foo")).Or(Return("")).Then(CurrentPos);
                AssertSuccess(parser.Parse("f"), new SourcePos(1,1), false);  // it should backtrack
            }
        }
    }
}
