using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Database.Interfaces;

namespace Server.Repositories
{

// This is a duplicate checker. It is made to be reused. That is why it appears confusing
// It is not type specific, it will simply find the duplicate you provide it with. 
    public class DuplicateCheckResult
    {
        public bool IsDuplicate { get; set; }
        public string? DuplicateField { get; set; }
    }
    public class DuplicateChecker
    {
        private readonly IDatabaseContext _context;

        public DuplicateChecker(IDatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<DuplicateCheckResult>> CheckForDuplicate<T>(T entity, params Expression<Func<T, object>>[] properties) where T : class
        {
            var results = new List<DuplicateCheckResult>();

            foreach (var property in properties)
            {
                var propertyName = GetPropertyName(property);
                var propertyValue = GetEntityPropertyValue(entity, property);

                var parameter = Expression.Parameter(typeof(T), "u");
                var member = Expression.Property(parameter, propertyName);
                var body = Expression.Equal(member, Expression.Constant(propertyValue));
                var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);

                bool isDuplicate = await _context.Set<T>().AnyAsync(predicate);
                results.Add(new DuplicateCheckResult
                {
                    IsDuplicate = isDuplicate,
                    DuplicateField = propertyName
                });
            }

            return results;
        }

        private string GetPropertyName<T>(Expression<Func<T, object>> property)
        {
            if (property.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (property.Body is MemberExpression member)
            {
                return member.Member.Name;
            }

            throw new ArgumentException("Invalid property expression");
        }

        private object GetEntityPropertyValue<T>(T entity, Expression<Func<T, object>> property)
        {
            var compiledExpression = property.Compile();
            return compiledExpression.Invoke(entity);
        }
    }
}

