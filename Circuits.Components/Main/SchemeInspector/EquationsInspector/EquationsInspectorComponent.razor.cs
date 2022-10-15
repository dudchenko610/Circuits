using System.Globalization;
using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;
using Microsoft.AspNetCore.Components;

namespace Circuits.Components.Main.SchemeInspector.EquationsInspector;

public partial class EquationsInspectorComponent
{
    [Inject] private IEquationSystemService _equationSystemService { get; set; } = null!;

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

    private EquationSystem _equationSystem;
    
    private readonly NumberFormatInfo _nF = new NumberFormatInfo { NumberDecimalSeparator = "." };

    protected override void OnInitialized()
    {
        _equationSystem = new EquationSystem(_i2, _i3, _i1Derivative, _ucDerivative)
        {
            Matrix = new []
            {
                new Expression[] { new ExpressionValue(-1), new ExpressionValue(-1), new ExpressionValue(0), new ExpressionValue(0), -_i1 },
                new Expression[] { new ExpressionValue(1), new ExpressionValue(0), new ExpressionValue(_l), new ExpressionValue(0), _e - (_i1 * _r) },
                new Expression[] { new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(_c), -_uc },
                new Expression[] { new ExpressionValue(0), new ExpressionValue(0), new ExpressionValue(_l), new ExpressionValue(0), _i1 },
            }
        };
    }

    private void OnPerformKirchhoffElimination()
    {
        /* reduction into r.e.f. */
        int singular_flag = forwardElim();
        
        /* if matrix is singular */
        if (singular_flag != -1)
        {
            Console.WriteLine("Singular Matrix.");
         
            /* if the RHS of equation corresponding to
               zero row  is 0, * system has infinitely
               many solutions, else inconsistent*/
            if (_equationSystem.Matrix[singular_flag][_equationSystem.Matrix.Length] is ExpressionValue val)
            {
                Console.WriteLine(val.Value != 0 ? "Inconsistent System." : "May have infinitely many solutions.");
            }
        }
        
        // var mat = _equationSystem.Matrix;
        //
        // for (var i = 0; i < mat.Length; i ++) {
        //
        //     if (mat[i][i] is ExpressionValue { Value: 0.0 }) {
        //         // I need to swap rows
        //
        //         for (var j = i + 1; j < mat.Length; j ++) {
        //             if (mat[j][i] is ExpressionValue { Value: 0.0 }) {
        //                 // swap
        //                 (mat[i], mat[j]) = (mat[j], mat[i]);
        //                 break;
        //             }
        //         }
        //
        //     }
        //
        //     for (var j = i + 1; j < mat.Length; j ++) {
        //         var frac = mat[j][i] / mat[i][i];
        //
        //         for (var k = i; k < mat[i].Length; k ++) {
        //             mat[j][k] -= frac * mat[i][k];
        //         }
        //     }
        // }
        
        StateHasChanged();
    }
    
    int forwardElim()
    {
        var N = _equationSystem.Matrix.Length;
        var mat = _equationSystem.Matrix;
        
        for(var k = 0; k < N; k++)
        {
            // Initialize maximum value and index for pivot
            var iMax = k;
            var vMax = mat[iMax][k] is ExpressionValue expVal ? Math.Abs(expVal.Value) : 0;
         
            /* find greater amplitude for pivot if any */
            for(var i = k + 1; i < N; i++)
            {
                if (mat[i][k] is ExpressionValue exVal && Math.Abs(exVal.Value) > vMax)
                {
                    vMax = Math.Abs(exVal.Value);
                    iMax = i;
                }
         
                /* If a principal diagonal element  is zero,
                *  it denotes that matrix is singular, and
                *  will lead to a division-by-zero later. */
                if (mat[k][iMax] is ExpressionValue { Value: 0 }) //mat[k][i_max] is ExpressionValue exValMax && exValMax.Value == 0
                    return k; // Matrix is singular
             
                /* Swap the greatest value row with
                   current row
                */
                if (iMax != k)
                {
                    (mat[k], mat[iMax]) = (mat[iMax], mat[k]);
                }
             
                for(var n = k + 1; n < N; n++)
                {
                    /* factor f to set current row kth element
                    *  to 0, and subsequently remaining kth
                    *  column to 0 */
                    var f = mat[n][k] / mat[k][k];
                 
                    /* subtract fth multiple of corresponding
                       kth row element*/
                    for (var j = k + 1; j <= N; j++)
                    {
                        mat[n][j] -= mat[k][j] * f;
                    }

                    /* filling lower triangular matrix with
                    *  zeros*/
                    mat[n][k] = new ExpressionValue(0);
                }
            }
        }
     
        return -1;
    }
}