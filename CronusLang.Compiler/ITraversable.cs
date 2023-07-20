using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronusLang.Compiler
{
    public interface ITraversable<T> where T: ITraversable<T>
    {
        /// <summary>
        /// Count the immediate children of the current instance.
        /// </summary>
        ///
        /// <example>
        /// Given a representation of the expression <c>(1+2)+3</c>.
        /// <code>
        /// Expr expr = new Add(
        ///     new Add(
        ///         new Lit(1),
        ///         new Lit(2)
        ///     ),
        ///     new Lit(3)
        /// );
        /// </code>
        /// <see cref="CountChildren" /> counts the immediate children of the topmost (Add) node.
        /// <code>
        /// Assert.Equal(2, expr.CountChildren());
        /// </code>
        /// </example>
        ///
        /// <returns>The current instance's number of immediate children.</returns>
        int CountChildren();

        /// <summary>
        /// Copy the immediate children of the current instance into <paramref name="childrenReceiver" />.
        /// </summary>
        /// <example>
        /// Given a representation of the expression <c>(1+2)+3</c>.
        /// <code>
        /// Expr expr = new Add(
        ///     new Add(
        ///         new Lit(1),
        ///         new Lit(2)
        ///     ),
        ///     new Lit(3)
        /// );
        /// </code>
        /// <see cref="GetChildren" /> copies the immediate children of the topmost node into the span.
        /// <code>
        /// Expr[] expected = new[]
        ///     {
        ///         new Add(
        ///             new Lit(1),
        ///             new Lit(2)
        ///         ),
        ///         new Lit(3)
        ///     };
        /// var array = new Expr[expr.CountChildren()];
        /// expr.GetChildren(array);
        /// Assert.Equal(expected, array);
        /// </code>
        /// </example>
        ///
        /// <param name="childrenReceiver">
        /// A <see cref="Span{T}" /> to copy the current instance's immediate children into.
        /// The <see cref="Span{T}" />'s <see cref="Span{T}.Length" /> should be equal to the number returned by <see cref="CountChildren" />.
        /// </param>
        void GetChildren(Span<T> childrenReceiver);
    }
}
