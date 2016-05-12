using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspNetMvcUrlHelper
{
    public class ExpressionCalculate
    {
        MethodInfo[] _methods; 

        public object Calculate(Expression expression)
        {
            if (_methods == null)
            {
                _methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                    .Where(w => w.Name == nameof(Calculate))
                                    .ToArray();
            }

            var _calculate = _methods.Where(w =>
                expression.GetType() == w.GetParameters().FirstOrDefault().ParameterType ||
                expression.GetType().IsSubclassOf(w.GetParameters().FirstOrDefault().ParameterType)
            ).FirstOrDefault();
                                

            if(_calculate != null)
            {
                return _calculate.Invoke(this, new object[] { expression });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("using LambdaExpression.Compile",nameof(ExpressionCalculate));

                return Expression.Lambda(expression)
                    .Compile()
                    .DynamicInvoke();
            }
        }

        protected object Calculate(MethodCallExpression expression)
        {
            return expression.Method.Invoke(
                Calculate(expression.Object),
                expression.Arguments
                    .Select(s => Calculate(s))
                    .ToArray()
            );
        }

        protected object Calculate(ConstantExpression expression)
        {
            return expression.Value;
        }

        protected object Calculate(MemberExpression expression)
        {
            if(expression.Member.MemberType == MemberTypes.Property)
            {
                return ((PropertyInfo)expression.Member).GetValue(Calculate(expression.Expression));
            }else if(expression.Member.MemberType == MemberTypes.Field)
            {
                return ((FieldInfo)expression.Member).GetValue(Calculate(expression.Expression));
            }

            throw new NotSupportedException($"不支持 {expression.NodeType} 类型的表达式。");

        }

        protected object Calculate(NewExpression expression)
        {
            return expression.Constructor.Invoke(expression.Arguments.Select(s => Calculate(s)).ToArray());
        }

        protected object Calculate(IndexExpression expression)
        {
            return expression.Indexer.GetValue(
                Calculate(expression.Object), 
                expression.Arguments
                    .Select(s => Calculate(s))
                    .ToArray()
            );
        }

        protected object Calculate(UnaryExpression expression)
        {
            if(expression.NodeType == ExpressionType.Convert ||
               expression.NodeType == ExpressionType.ConvertChecked)
            {
                return Calculate(expression.Operand);
            }

            throw new NotSupportedException($"不支持 {expression.NodeType} 类型的表达式。");
        }

        protected object Calculate(LambdaExpression expression)
        {
            return Calculate(expression.Body);
        }

    }
}
