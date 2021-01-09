﻿using System;
using System.Collections.Generic;
using NoStringEvaluating.Extensions;
using NoStringEvaluating.Nodes;
using NoStringEvaluating.Nodes.Base;

namespace NoStringEvaluating.Services.Parsing.NodeReaders
{
    /// <summary>
    /// Variable reader
    /// </summary>
    public static class VariableReader
    {
        /// <summary>
        /// Read variable
        /// </summary>
        public static bool TryProceedBorderedVariable(List<IFormulaNode> nodes, ReadOnlySpan<char> formula, ref int index)
        {
            // Read unary minus
            var localIndex = UnaryMinusReader.ReadUnaryMinus(nodes, formula, index, out var isNegativeLocal);

            // Check out of range
            if (localIndex >= formula.Length)
                return false;

            // Read variable
            if (formula[localIndex] != START_CHAR)
            {
                return false;
            }

            // Skip start char
            localIndex++;

            var variableBuilder = new IndexWatcher();
            for (int i = localIndex; i < formula.Length; i++)
            {
                var ch = formula[i];
   
                if (ch == END_CHAR)
                {
                    var variableSpan = formula.Slice(variableBuilder.StartIndex.GetValueOrDefault(), variableBuilder.Length);
                    var variableName = variableSpan.ToString();
                    var valNode = new VariableNode(variableName, isNegativeLocal);
                    nodes.Add(valNode);

                    index = i;
                    return true;
                }

                variableBuilder.Remember(i);
            }

            return false;
        }

        /// <summary>
        /// Read variable
        /// </summary>
        public static bool TryProceedSimpleVariable(List<IFormulaNode> nodes, ReadOnlySpan<char> formula, ref int index)
        {
            // Read unary minus
            var localIndex = UnaryMinusReader.ReadUnaryMinus(nodes, formula, index, out var isNegativeLocal);

            var numberBuilder = new IndexWatcher();
            for (int i = localIndex; i < formula.Length; i++)
            {
                var ch = formula[i];
                var isLastChar = i + 1 == formula.Length;

                if (ch.IsSimpleVariable())
                {
                    numberBuilder.Remember(i);

                    if (isLastChar && TryAddSimpleVariable(nodes, formula, numberBuilder, isNegativeLocal))
                    {
                        index = i;
                        return true;
                    }
                }
                else if (TryAddSimpleVariable(nodes, formula, numberBuilder, isNegativeLocal))
                {
                    index = i - 1;
                    return true;
                }
                else
                {
                    break;
                }
            }

            return false;
        }

        private static bool TryAddSimpleVariable(List<IFormulaNode> nodes, ReadOnlySpan<char> formula, IndexWatcher nodeBuilder, bool isNegative)
        {
            if (nodeBuilder.InProcess)
            {
                var variableSpan = formula.Slice(nodeBuilder.StartIndex.GetValueOrDefault(), nodeBuilder.Length);
                var variableName = variableSpan.ToString();
                var valNode = new VariableNode(variableName, isNegative);
                nodes.Add(valNode);

                return true;
            }

            return false;
        }

        private const char START_CHAR = '[';
        private const char END_CHAR = ']';
    }
}
