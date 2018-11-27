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
            if (memberList.Count == 0)
            {
                return "this";
            }

            return "this." + string.Join(".", memberList);
        }
    }
}