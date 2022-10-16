using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Circuits.ViewModels.Entities.Structures;

namespace Circuits.Services.Services;

public class EquationSystemService : IEquationSystemService
{
    private static ExpressionVariable _e = new() { Label = "ε" };
    private static ExpressionVariable _i1 = new() { Label = "i₁" };
    private static ExpressionVariable _i2 = new() { Label = "i₂" };
    private static ExpressionVariable _i3 = new() { Label = "i₃" };
    private static ExpressionVariable _uc = new() { Label = "Uc₁" };
    
    private ExpressionDerivative _i1Derivative = new() { Variable = _i1, Label = "d(i₁)/dt" };
    private ExpressionDerivative _ucDerivative = new() { Variable = _uc, Label = "d(Uc₁)/dt" };

    private double _l = 1.0;
    private double _c = 1.0;
    private double _r = 1.0;
    
    public List<EquationSystem> BuildEquationSystemsFromGraphs(IEnumerable<Graph> graphs)
    {
        var equationSystems = new List<EquationSystem>();

        foreach (var graph in graphs)
        {
            foreach (var circuit in graph.Circuits)
            {
                
            }
        }
        
        var equationSystem = new EquationSystem(_i2, _i3, _i1Derivative, _ucDerivative)
        {
            Matrix = new []
            {
                new Expression[] { new ExpressionValue(-1), new ExpressionValue(-1), new ExpressionValue(0), new ExpressionValue(0), -_i1 },
                new Expression[] { new ExpressionValue(1), new ExpressionValue(0), new ExpressionValue(_l), new ExpressionValue(0), _e - (_i1 * _r) },
                new Expression[] { new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(_c), -_uc },
                new Expression[] { new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(_l), new ExpressionValue(0), _i1 },
            }
        };
        
        equationSystems.Add(equationSystem);
        
        return equationSystems;
    }

    public string PerformKirchhoffElimination(EquationSystem equationSystem)
    {
        var matrixLength = equationSystem.Matrix.Length;
        var mat = equationSystem.Matrix;

        for (var k = 0; k < matrixLength; k++)
        {
            // Initialize maximum value and index for pivot
            var iMax = k;
            var vMax = mat[iMax][k] is ExpressionValue expVal ? Math.Abs(expVal.Value) : 0;

            /* find greater amplitude for pivot if any */
            for (var i = k + 1; i < matrixLength; i++)
            {
                if (mat[i][k] is ExpressionValue exVal && Math.Abs(exVal.Value) > vMax)
                {
                    vMax = Math.Abs(exVal.Value);
                    iMax = i;
                }

                /* If a principal diagonal element  is zero,
                *  it denotes that matrix is singular, and
                *  will lead to a division-by-zero later. */
                if (mat[iMax][k] is ExpressionValue { Value: 0 })
                {
                    return mat[k][matrixLength] is ExpressionValue { Value: 0 }
                        ? "May have infinitely many solutions"
                        : "Inconsistent System";
                }
            }

            /* Swap the greatest value row with
               current row
            */
            if (iMax != k)
            {
                (mat[k], mat[iMax]) = (mat[iMax], mat[k]);
            }

            for (var i = k + 1; i < matrixLength; i++)
            {
                /* factor f to set current row kth element
                *  to 0, and subsequently remaining kth
                *  column to 0 */
                var f = mat[i][k] / mat[k][k];

                /* subtract fth multiple of corresponding
                   kth row element*/
                for (var j = k + 1; j <= matrixLength; j++)
                {
                    mat[i][j] -= mat[k][j] * f;
                }

                /* filling lower triangular matrix with
                *  zeros*/
                mat[i][k] = new ExpressionValue(0);
            }
        }

        return "Success";
    }
}