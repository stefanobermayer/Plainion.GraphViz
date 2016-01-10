﻿
namespace Plainion.GraphViz.Modules.Documents.DotLang
{
    public class Token
    {
        public TokenType Type { get; private set; }

        public string Value { get; private set; }

        public Token( TokenType tokenType, string token )
        {
            Type = tokenType;
            Value = token;
        }

        public Token( TokenType tokenType )
        {
            Value = null;
            Type = tokenType;
        }

        public override string ToString()
        {
            return Type + ": " + Value;
        }
    }

    public enum TokenType
    {
        Edge,
        Graph,
        WhiteSpace,
        GraphBegin,
        GraphEnd,
        QuotedString,
        Word,
        Comma,
        Assignment,
        CommentBegin,
        CommentEnd,
        EdgeDef,
        Strict,
        SemiColon,
        Node,
        Subgraph,
        EndOfStream,
        DirectedGraph,
        AttributeBegin,
        AttributeEnd,
        Number,
        SingleLineComment,
        NewLine
    }
}
