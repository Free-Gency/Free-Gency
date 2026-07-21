using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FreeGency.Domain.Interfaces
{
    public interface ISpecifiaction<T>
    {
        Expression<Func<T, bool>>? Criteria { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        bool IsDistinct { get; }
        int Take { get; }
        int Skip { get; }
        bool IspagingEnabled { get; }
        IQueryable<T> ApplyCriteria(IQueryable<T> query);
    }
    public interface ISpecifiaction<T, TResult> : ISpecifiaction<T>
    {
        Expression<Func<T, TResult>>? Select { get; }
    }
}
