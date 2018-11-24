using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TicketManager.Common.Linq.Expressions;

namespace TicketManager.DataAccess.Documents.DataStructures
{
    public class PropertyUpdateDescriptor<TObject, TProperty> : PropertyUpdateDescriptor
    {
        private readonly List<string> memberList;

        public PropertyUpdateDescriptor(Expression<Func<TObject, TProperty>> memberSelector, TProperty newValue)
            : base(newValue)
        {
            memberList = ExpressionHelper.GetMemberList(memberSelector);
        }

        public override string ToJavaScriptPropertyExpression()
        {
            // TODO: Check that the memberlist has at least 1 element.
            return "this." + string.Join(".", memberList);
        }
    }
}