using System.Globalization;
using Circuits.Shared.Extensions;
using Circuits.ViewModels.Attributes;
using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Extensions;

public static class ExpressionExtensions
{
    private static readonly NumberFormatInfo Nf = new()
    {
        NumberDecimalSeparator = ".",
        NumberDecimalDigits = 4
    };
    
    public static string GetLabel(this Expression expression, Func<ExpressionVariable, string> namePredicate = null!)
    {
        if (expression is ExpressionValue)
        {
            var res = string.Format(Nf, "{0:N}", expression.Value).TrimEnd('0');
            if (res.EndsWith(".")) res += "0";
            
            return expression.Value < 0 ? $"({res})" : res;
        }
        
        if (expression is ExpressionVariable expVar)
        {
            return namePredicate != null! ? namePredicate.Invoke(expVar) : expVar.Label;
        }
        
        if (expression is ExpressionAdditions expAdd)
        {
            var res = "";

            for (var i = 0; i < expAdd.Nodes.Count; i ++)
            {
                if (expAdd.Nodes[i] is ExpressionAdditions or ExpressionMultipliers)
                {
                    if (i == 0)
                    {
                        res += $"({expAdd.Nodes[i].GetLabel(namePredicate)})";
                        continue;
                    }
                
                    res += $"{expAdd.Signs[i - 1].GetValue<string, DisplayNameAttribute>(x => x.Name)}({expAdd.Nodes[i].GetLabel(namePredicate)})";
                }
                else
                {
                    if (i == 0)
                    {
                        res += $"{expAdd.Nodes[i].GetLabel(namePredicate)}";
                        continue;
                    }
                
                    res += $"{expAdd.Signs[i - 1].GetValue<string, DisplayNameAttribute>(x => x.Name)}{expAdd.Nodes[i].GetLabel(namePredicate)}";
                }
            }

            return res;
        }
        
        if (expression is ExpressionMultipliers expMul)
        {
            var res = "";

            for (var i = 0; i < expMul.Nodes.Count; i ++)
            {
                if (expMul.Nodes[i] is ExpressionAdditions or ExpressionMultipliers)
                {
                    if (i == 0)
                    {
                        res += $"({expMul.Nodes[i].GetLabel(namePredicate)})";
                        continue;
                    }
                
                    res += $"{expMul.Multipliers[i - 1].GetValue<string, DisplayNameAttribute>(x => x.Name)}({expMul.Nodes[i].GetLabel(namePredicate)})";
                }
                else
                {
                    if (i == 0)
                    {
                        res += $"{expMul.Nodes[i].GetLabel(namePredicate)}";
                        continue;
                    }
                
                    res += $"{expMul.Multipliers[i - 1].GetValue<string, DisplayNameAttribute>(x => x.Name)}{expMul.Nodes[i].GetLabel(namePredicate)}";
                }
            }
            
            return res;
        }

        return string.Empty;
    }
}