using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                return CompileAndInvoke(expression);
            }
        }

        protected object CompileAndInvoke(Expression expression)
        {
            try
            {

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var result = Expression.Lambda(expression)
                    .Compile()
                    .DynamicInvoke();

                stopWatch.Stop();

                Debug.WriteLine($"using LambdaExpression.Compile [{expression.NodeType}] delay:{stopWatch.ElapsedMilliseconds} ms", nameof(AspNetMvcUrlHelper));

                return result;
            }
            catch(Exception ex)
            {
                throw new NotSupportedException($"无法计算 {expression} 的值。", ex);
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

            return CompileAndInvoke(expression);

        }

        protected object Calculate(NewExpression expression)
        {
            return expression.Constructor.Invoke(expression.Arguments.Select(s => Calculate(s)).ToArray());
        }

        protected object Calculate(BinaryExpression expression)
        {
            if(expression.NodeType== ExpressionType.ArrayIndex)
            {
                var array = Calculate(expression.Left);
                var indexer = Calculate(expression.Right);
                return array.GetType()
                    .GetMethod("GetValue", new Type[] { indexer.GetType() })
                    .Invoke(array, new object[] { indexer });
            }else
            {
                return CompileAndInvoke(expression);
            }
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

            return CompileAndInvoke(expression);
        }

        protected object Calculate(LambdaExpression expression)
        {
            return Calculate(expression.Body);
        }

    }
}
