using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TicketManager.Common.Linq.Expressions
{
    public static class ExpressionHelper
    {
        public static List<string> GetMemberList(MemberExpression expression)
        {
            var result = new List<string>();

            if (expression.Expression is MemberExpression memberExpression)
            {
                result.AddRange(GetMemberList(memberExpression));
            }

            result.Add(expression.Member.Name);

            return result;
        }

        public static List<string> GetMemberList(LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is MemberExpression memberExpression)
            {
                return GetMemberList((MemberExpression)memberExpression);
            }
            else
            {
                throw new ArgumentException($"The body of the {nameof(lambdaExpression)} parameter must be a {nameof(MemberExpression)}.", nameof(lambdaExpression));
            }
        }
    }
}