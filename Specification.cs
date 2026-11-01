using System;
using System.Linq.Expressions;

namespace TejaratBank.Statement.Identity.Core.Data
{
    public abstract class Specification<T>
    {
        public abstract Expression<Func<T, bool>> ToExpression();
        public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);
    }
    
    public sealed class AndSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public AndSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var p = Expression.Parameter(typeof(T), "x");

            var body = Expression.AndAlso(
                Expression.Invoke(_left.ToExpression(), p),
                Expression.Invoke(_right.ToExpression(), p));

            return Expression.Lambda<Func<T, bool>>(body, p);
        }
    }

    public sealed class OrSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;
        public OrSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }
        public override Expression<Func<T, bool>> ToExpression()
        {
            var p = Expression.Parameter(typeof(T), "x");

            var body = Expression.OrElse(
                Expression.Invoke(_left.ToExpression(), p),
                Expression.Invoke(_right.ToExpression(), p));

            return Expression.Lambda<Func<T, bool>>(body, p);
        }
    }

    public sealed class NotSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _inner;
        public NotSpecification(Specification<T> inner) => _inner = inner;
        public override Expression<Func<T, bool>> ToExpression()
        {
            var e = _inner.ToExpression();
            return Expression.Lambda<Func<T, bool>>(Expression.Not(e.Body), e.Parameters[0]);
        }
    }
    
    public static class SpecificationExtensions
    {
        public static Specification<T> And<T>(this Specification<T> left, Specification<T> right)
            => new AndSpecification<T>(left, right);

        public static Specification<T> Or<T>(this Specification<T> left, Specification<T> right)
            => new OrSpecification<T>(left, right);

        public static Specification<T> Not<T>(this Specification<T> inner)
            => new NotSpecification<T>(inner);
    }
}
