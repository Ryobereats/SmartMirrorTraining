using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;

using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
//using MathNet.Numerics.LinearAlgebra.Complex;

public class TestHomography : MonoBehaviour
{
  private List<UnityEngine.Vector3> RPList;
  private List<UnityEngine.Vector3> MPList;
  private bool flag = false;

  //private double[,] array;
  
  // Start is called before the first frame update
  void Start()
  {
    Debug.Log("x'の値は" + (-0.78985 * -72.37984 + 0.006399 * -85.43807 + 1.772926) / (0.000912 * -72.37984 + 0.002469 * -85.43807 +1));

    //var A = Matrix.Build.DenseOfArray(new Complex[,]
    //            {
    //                {  2.0, 1.0,  1.0},
    //                { -1.0, 1.0, -1.0},
    //                {  1.0, 2.0,  3.0}
    //            }
    //        );
    //var b = MathNet.Numerics.LinearAlgebra.Vector<Complex>.Build.DenseOfArray(new Complex[]
    //    {2.0, 3.0, -10.0});


    //var x = A.Solve(b);

    //Debug.Log("こんにちは" + x);

    //Debug.Log("こんにちは2" + A.Inverse());
    //Debug.Log("こんにちは3" + A.Determinant());

    RPList = new List<UnityEngine.Vector3>() { new UnityEngine.Vector3(50, 50, 0), new UnityEngine.Vector3(50, 400, 0), new UnityEngine.Vector3(500, 400, 0), new UnityEngine.Vector3(500, 50, 0) };
    MPList = new List<UnityEngine.Vector3>() { new UnityEngine.Vector3(100, 50, 0), new UnityEngine.Vector3(120, 350, 0), new UnityEngine.Vector3(500, 500, 0), new UnityEngine.Vector3(600, 200, 0) };


    //if (!flag)
    //{
    //  for (int i = 0; i < 8; i++)
    //  {
    //    Debug.Log("答えは" + GetAns(RPList, MPList)[i, 0] + "です");
    //  }
    //  flag = true;
    //}

    //array = new double[4, 4] { { 3, 1, 2, 2 }, { 5, 1, 3, 4 }, { 2, 0, 1, 0 }, { 1, 3, 2, 1 } };

    var A = Matrix<float>.Build.DenseOfArray(new float[,]
                {
                    { 3,1,1,2},
                    { 5,1,3,4},
                    { 2,0,1,0}, 
                    { 1,3,2,1},
                }
            );
    
    var B = Matrix<float>.Build.DenseOfArray(new float[,]
                {
                    {1,2,3,4},
                    {1,2,3,4},
                    {1,2,3,4},
                    {1,2,3,4},
                }
            );
    
    if (!flag)
    {
      for (int i = 0; i < A.ColumnCount; i++)
      {
        for (int j = 0; j < A.RowCount; j++)
        {
          Debug.Log("逆行列は" + A.Inverse()[i, j] + "です");
          Debug.Log("行列の積は" + A.Multiply(B)[i,j] + "です");
        }
      }
      flag = true;
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  //private float[,] GetAns(List<Vector3> RealPosList, List<Vector3> MpPosList)//ホモグラフィ変換行列を返してくれる関数
  //{
  //  for (int i = 0; i < RealPosList.Count; i++)
  //  {
  //    Debug.Log(i + "番目のRealPosListは" + RealPosList[i] + "です！！");
  //    Debug.Log(i + "番目のMpPosListは" + MpPosList[i] + "です！！");
  //  }

  //  List<float> x = new List<float>() { RealPosList[0][0], RealPosList[1][0], RealPosList[2][0], RealPosList[3][0] };
  //  List<float> y = new List<float>() { RealPosList[0][1], RealPosList[1][1], RealPosList[2][1], RealPosList[3][1] };
  //  List<float> X = new List<float>() { MpPosList[0][0], MpPosList[1][0], MpPosList[2][0], MpPosList[3][0] };
  //  List<float> Y = new List<float>() { MpPosList[0][1], MpPosList[1][1], MpPosList[2][1], MpPosList[3][1] };

  //  for (int i = 0; i < RealPosList.Count; i++)
  //  {
  //    //Debug.Log(i + "番目の要素は" + RealPosList[0][i]);
  //    //Debug.Log(i + "番目の要素は" + RealPosList[i][0]);
  //  }
  //  float[,] Ans;

  //  float[,] L = new float[,]{{x[0], y[0], 1, 0, 0, 0, -X[0] * x[0], -X[0] * y[0] },
  //    {    0,    0,   0, x[0], y[0],   1, -Y[0] * x[0], -Y[0] * y[0] },
  //    { x[1], y[1],   1,    0,    0,   0, -X[1] * x[1], -X[1] * y[1] },
  //    {    0,    0,   0, x[1], y[1],   1, -Y[1] * x[1], -Y[1] * y[1] },
  //    {x[2], y[2],   1,    0,    0,   0, -X[2] * x[2], -X[2] * y[2] },
  //    {    0,    0,   0, x[2], y[2],   1, -Y[2] * x[2], -Y[2] * y[2] },
  //    {x[3], y[3],   1,    0,    0,   0, -X[3] * x[3], -X[3] * y[3] },
  //    {    0,    0,   0, x[3], y[3],   1, -Y[3] * x[3], -Y[3] * y[3]} };
  //  Debug.Log("Lの値は" + L + "です！！");


  //  float[,] R = new float[,] { { X[0] },
  //    { Y[0] },
  //    { X[1] },
  //    { Y[1] },
  //    { X[2] },
  //    { Y[2] },
  //    { X[3] },
  //    { Y[3] } };

  //  Debug.Log("Rの値は" + R + "です！！");

  //  Ans = Solve(L, R);
  //  for (int i = 0; i < Ans.GetLength(0); i++)
  //  {
  //    Debug.Log("Ans行列の" + i + "番目の要素は" + Ans[i, 0]);
  //  }
  //  Debug.Log("RealPosListは" + RealPosList + "です！！" + "MpPosListは" + MpPosList + "です！！" + "Ans行列は" + Ans + "です！！");

  //  return Ans;
  //}

  //2023/7/18(火)追加
  //private float[,] Solve(float[,] l, float[,] r)//numpyのSolve関数を実装
  //{
  //  float[,] ans;

  //  //lの逆行列を算出する＋行列の積を求める処理をかく
  //  l = CalcInverseMatrix(l);
  //  ans = MultiplyMatrix(l, r);

  //  return ans;
  //}

  private double[,] CalcInverseMatrix(double[,] A)//逆行列を求める関数
  {

    int n = A.GetLength(0);
    int m = A.GetLength(1);

    double[,] invA = new double[n, m];

    if (n == m)
    {

      int max;
      double tmp;

      for (int j = 0; j < n; j++)
      {
        for (int i = 0; i < n; i++)
        {
          invA[j, i] = (i == j) ? 1 : 0;
        }
      }

      for (int k = 0; k < n; k++)
      {
        max = k;
        for (int j = k + 1; j < n; j++)
        {
          if (Math.Abs(A[j, k]) > Math.Abs(A[max, k]))
          {
            max = j;
          }
        }

        if (max != k)
        {
          for (int i = 0; i < n; i++)
          {
            // 入力行列側
            tmp = A[max, i];
            A[max, i] = A[k, i];
            A[k, i] = tmp;
            // 単位行列側
            tmp = invA[max, i];
            invA[max, i] = invA[k, i];
            invA[k, i] = tmp;
          }
        }

        tmp = A[k, k];

        for (int i = 0; i < n; i++)
        {
          A[k, i] /= tmp;
          invA[k, i] /= tmp;
        }

        for (int j = 0; j < n; j++)
        {
          if (j != k)
          {
            tmp = A[j, k] / A[k, k];
            for (int i = 0; i < n; i++)
            {
              A[j, i] = A[j, i] - A[k, i] * tmp;
              invA[j, i] = invA[j, i] - invA[k, i] * tmp;
            }
          }
        }

      }
      //逆行列が計算できなかった時の措置
      for (int j = 0; j < n; j++)
      {
        for (int i = 0; i < n; i++)
        {
          if (double.IsNaN(invA[j, i]))
          {
            Console.WriteLine("Error : Unable to compute inverse matrix");
            invA[j, i] = 0;//ここでは，とりあえずゼロに置き換えることにする
          }
        }
      }
      return invA;
    }
    else
    {
      Console.WriteLine("Error : It is not a square matrix");
      return invA;
    }
  }


  float[,] MultiplyMatrix(float[,] A, float[,] B)//行列の掛け算
  {

    float[,] product = new float[A.GetLength(0), B.GetLength(1)];

    for (int i = 0; i < A.GetLength(0); i++)
    {
      for (int j = 0; j < B.GetLength(1); j++)
      {
        for (int k = 0; k < A.GetLength(1); k++)
        {
          product[i, j] += A[i, k] * B[k, j];
        }
      }
    }
    return product;
  }
}
