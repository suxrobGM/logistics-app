using System.Linq.Expressions;

namespace Logistics.Domain.Specifications;

public static class ExpressionExtensions
{
    /// <summary>
    /// Combines two predicate expressions: left AND right.
    /// If either expression is null, it returns the other one.
    /// For example, if left is "x => x.Age > 18" and right is "x => x.IsActive",
    /// the result will be "x => x.Age > 18 && x.IsActive".
    /// If both expressions are null, it returns null.
    /// </summary>
    /// <param name="left">The first expression.</param>
    /// <param name="right">The second expression.</param>
    /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
    /// <returns>A new expression that represents the logical AND of the two expressions.</returns>
    public static Expression<Func<T, bool>>? AndAlso<T>(
        this Expression<Func<T, bool>>? left,
        Expression<Func<T, bool>>? right)
    {
        // handle nulls gracefully
        if (left is null)
            return right;
        if (right is null)
            return left;

        // x =>
        var param = Expression.Parameter(typeof(T), "x");

        // invoke both sides with the same parameter
        var leftBody = ReplaceParameter(left, param);
        var rightBody = ReplaceParameter(right, param);

        var body = Expression.AndAlso(leftBody, rightBody);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    /// <summary>
    /// Replaces the parameter in the expression with a new one.
    /// </summary>
    /// <param name="expr">The expression to modify.</param>
    /// <param name="newParameter">The new parameter to replace the existing one.</param>
    /// <returns>A new expression with the parameter replaced.</returns>
    private static Expression ReplaceParameter(
        LambdaExpression expr,
        ParameterExpression newParameter)
    {
        return new ParameterReplacer(expr.Parameters[0], newParameter)
            .Visit(expr.Body);
    }

    /// <summary>
    /// A visitor that replaces a specific parameter in an expression with a new one.
    /// </summary>
    private sealed class ParameterReplacer(
        ParameterExpression oldParam,
        ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}
