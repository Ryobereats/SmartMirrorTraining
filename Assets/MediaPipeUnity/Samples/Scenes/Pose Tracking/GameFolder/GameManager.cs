using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.IO;

using System.Linq;
using System.Text;

using Mediapipe.Unity;

//2023/9/6水追加
using MathNet.Numerics;
//using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using static Mediapipe.ImageFrame;

public class GameManager : MonoBehaviour
{
  private STATE _state;
  private int _count = 0;
  public Vector3 headHighPos { get; private set; }
  public Vector3 headLowPos { get; private set; }
  //20237/6(木)追加
  public static GameManager Instance;

  //2023/7/5(水)追加
  private float time = 0;
  private bool isObjectFound = false;
  private string _folderPath;
  private Vector3 _landMarkPos;//ランドマークの(x,y,z)
  private float x;
  private float y;
  private float z;

  //2023/7/18(火)追加
  private List<Vector3> _mirrorCalibPointList;//Webcamとミラーのキャリブレーションのための球体のVector3が格納された2次元配列
  private List<Vector3> _mpLandmarkList;//Mediapipeで取得したランドマークのVector3が格納された2次元配列

  //2023/7/19(水)追加
  private Vector3 _mpLeftWristPos;
  private Vector3 _mpRightWristPos;
  private Vector3 _mpLeftKneePos;
  private Vector3 _mpRightKneePos;

  //2023/7/21(金)追加
  private float[,] _mirrorCalibArray;

  //2023/7/24(月)追加
  private bool isGetArray = false;

  //Gameに関連した変数
  private bool isGameOn = false;
  private bool isResultGo = false;
  private GameObject curve;
  private int trainingTimes = 5;
  public int trainingTypes = -1;
  private int trainingStrength = 0;
  private int gameCountDownTime = 0;
  private bool isFinishJudge = false;
  public int goldCoinNum { get; private set; } = 0;
  public int redCoinNum { get; private set; } = 0;
  public List<GameObject> Coins = new List<GameObject>();

  //2023/10/9(月)追加
  private Vector3 _Head2UpButtonVec;
  private Vector3 _Head2DownButtonVec;

  //2023/10/11(水)追加
  //スクワットキャリブレーション時におけるMediaPipe座標
  private Vector3 _mpHeadHighPos;
  private Vector3 _mpHeadLowPos;

  //2023/10/19(木)追加
  [SerializeField] private float _squat_speed = 40;
  public float squat_speed { get => _squat_speed; private set => _squat_speed = value; }
  private int _mirrorCalibCount = 0;//ミラーキャリブレーションを初期姿勢と低姿勢時の2回行う

  //コインの座標を格納した2次元配列
  //CurveArray1
  float[,] squatCurveArray1 = new float[,] { { 0, 1, 0 }, { 0, 0.75f, 0 }, { 0, 0.5f, 0 }, { 0, 0.25f, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0.25f, 0 }, { 0, 0.5f, 0 }, { 0, 0.75f, 0 }, { 0, 1, 0 } };

  [SerializeField] private TextMeshProUGUI _countDownText;
  [SerializeField] private TextMeshProUGUI _explanationText;
  [SerializeField] private GameObject _calibrationButton;
  [SerializeField] private GameObject coin;
  [SerializeField] private GameObject Parent;
  [SerializeField] private Material keepCoinColor;

  //2023/7/18(火)追加
  [SerializeField] private GameObject _HeadCalibPoint;
  [SerializeField] private GameObject _LeftWristCalibPoint;
  [SerializeField] private GameObject _RightWristCalibPoint;
  [SerializeField] private GameObject _LeftKneeCalibPoint;
  [SerializeField] private GameObject _RightKneeCalibPoint;

  //2023/7/19(水)追加
  [SerializeField] private TextMeshProUGUI _mirrorCalibCountDownText;
  [SerializeField] private TextMeshProUGUI _mirrorCalibExplanationText;
  [SerializeField] private GameObject _mirrorCalibButton;

  //2023/9/11(月)追加
  [SerializeField] private GameObject _Header;
  [SerializeField] private GameObject _Footer;
  //[SerializeField] private GameObject CalibrationButton;
  //[SerializeField] private GameObject MirrorCalibButton;
  [SerializeField] private GameObject _GameStartButton;
  //[SerializeField] private GameObject explanationText;
  //[SerializeField] private GameObject countdownText;
  //[SerializeField] private GameObject MiirorCalibExplanationText;
  //[SerializeField] private GameObject MirrorCalibCountdownText;
  [SerializeField] private TextMeshProUGUI _MirrorCalibText;
  [SerializeField] private TextMeshProUGUI _LeftWristText;
  [SerializeField] private TextMeshProUGUI _RightWristText;
  [SerializeField] private TextMeshProUGUI _LeftKneeText;
  [SerializeField] private TextMeshProUGUI _RightKneeText;
  [SerializeField] private TextMeshProUGUI _TrainingTypeText;
  [SerializeField] private TMP_Dropdown _TrainingTypeDropdown;
  [SerializeField] private GameObject _Horizon;//ok
  [SerializeField] private GameObject _Vertical;//ok

  //2023/10/9(月)追加
  [SerializeField] private GameObject _EyeGazeCalibUpButton;
  [SerializeField] private GameObject _EyeGazeCalibDownButton;
  [SerializeField] private float _HeadCalibMoveDis = 3;
  [SerializeField] private GameObject _DecideButton1;
  [SerializeField] private GameObject _DecideButton2;

  //2023/10/10(火)追加
  [SerializeField] private GameObject _UserUpPos;
  [SerializeField] private GameObject _UserDownPos;

  //2023/10/11(水)追加
  [SerializeField] private int _mirrorCalibTime = 0;
  [SerializeField] private int _squatCalibTime = 0;
  [SerializeField] private GameObject _MpSquatCalibUpObject;
  [SerializeField] private GameObject _MpSquatCalibDownObject;

  //2023/10/19(木)追加
  [SerializeField] private GameObject _MainCanvas;
  [SerializeField] private GameObject _reflectionUpPos;
  [SerializeField] private GameObject _reflectionDownPos;
  [SerializeField] private float _calibPointMoveDis = 0;
  private void Awake()
  {
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 60;
    Instance = this;
  }

  // Start is called before the first frame update
  private void Start()
  {
    _state = STATE.Calibration;
    trainingTypes = 1;
    if (trainingTypes == 1) gameCountDownTime = 5;
    _mirrorCalibPointList = new List<Vector3>();
    _mpLandmarkList = new List<Vector3>();
    AddElement(_mirrorCalibPointList, _LeftWristCalibPoint, _RightWristCalibPoint, _LeftKneeCalibPoint, _RightKneeCalibPoint);//MpLandmarkListへの要素追加はCSV経由で代入する
    _mirrorCalibArray = new float[8,1];

    //2023/10/9(月)追加 Up/DownButtonの位置を青玉の位置に応じて変化させるためのベクトルを算出
    Vector3 HeadCalibPos = _HeadCalibPoint.transform.position;
    Vector3 UpButtonPos = _EyeGazeCalibUpButton.transform.position;
    Vector3 DownButtonPos = _EyeGazeCalibDownButton.transform.position;
    //_Head2UpButtonVec = new Vector3(UpButtonPos.x - HeadCalibPos.x, UpButtonPos.y - HeadCalibPos.y, 0);
    //_Head2DownButtonVec = new Vector3(DownButtonPos.x - HeadCalibPos.x, DownButtonPos.y - HeadCalibPos.y, 0);
    _Head2UpButtonVec = new Vector3(15.5f, 2.8f, 0);
    _Head2DownButtonVec = new Vector3(15.5f, -7.2f, 0);
  }

  // Update is called once per frame

  private void LateUpdate()
  {
    if(GameObject.Find("Capsule") != null && !isObjectFound)
    {
      GameObject capsule = GameObject.Find("Capsule");
      //CSVのパスを読み込む(=ReadCSV)
      Debug.Log("カプセルが見つかりました！！！");
      GameObject cylinder = capsule.transform.GetChild(0).gameObject;
      Debug.Log(cylinder.name + "参上！！！！");
      isObjectFound = true;
      _folderPath = cylinder.name;
    }

    ReadCsv(_folderPath,STATE.SquatCalib);
    //2023/7/21(金)追加
    ReadCsv(_folderPath, STATE.MirrorCalib);

    //Debug.Log(_landMarkPos);

    //2023/7/24(月)追加
    if (isGetArray)//ミラーキャリブレーション行列が算出できたらCSVファイルに縦書きで行列の要素を出力する
    {
      if (_mirrorCalibCount == 1)
      {
        Debug.Log("isGetArrayがtrueになりました");
        //string folderPath = "C:/Users/ig/AppData/LocalLow/DefaultCompany/MediaPipeUnityPlugin/SmartMirror/MirrorCalibration";
        string folderPath = "C:/Users/inoue/ig/SmartMirror/MirrorCalibration";
        string fileName = "CalibrationArray";
        SaveTaskData(folderPath, fileName, _mirrorCalibArray);

        //2023/7/25(水)追加　キャリブレーション行列をCSVに書き込んだことをPoseLamdmarkListAnnotation.csに伝えるためにゲームオブジェクトを生成
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.position = new Vector3(1000, 1000, 1000);
        capsule.name = "MirrorCalibrationCompleted";
      } 
      
      if (_mirrorCalibCount == 2)
      {
        //string folderPath = "C:/Users/ig/AppData/LocalLow/DefaultCompany/MediaPipeUnityPlugin/SmartMirror/MirrorCalibration";
        string folderPath = "C:/Users/inoue/ig/SmartMirror/MirrorCalibration";
        string fileName = "SecondCalibrationArray";
        SaveTaskData(folderPath, fileName, _mirrorCalibArray);

        //2023/7/25(水)追加　キャリブレーション行列をCSVに書き込んだことをPoseLamdmarkListAnnotation.csに伝えるためにゲームオブジェクトを生成
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.position = new Vector3(1000, 1000, 1000);
        capsule.name = "SecondMirrorCalibrationCompleted";
      }
      isGetArray = false;
    }

    //2023/10/19(木)追加
    if (GameObject.Find("UpPos"))
    {
      _reflectionUpPos.transform.position = GameObject.Find("UpPos").transform.position;
      headHighPos = _reflectionUpPos.transform.position;
    }

    if (GameObject.Find("DownPos"))
    {
      _reflectionDownPos.transform.position = GameObject.Find("DownPos").transform.position;
      headLowPos = _reflectionDownPos.transform.position;
    }
    if (isGameOn)
    {
      MoveWave(curve, trainingTypes);
      Debug.Log("MoveWaveしました！！！");
      if (CheckGameEnd()) GameEnd();
    }

  }

  private void AddElement(List<Vector3>posList,GameObject point1,GameObject point2,GameObject point3,GameObject point4)
  {
    posList.Add(point1.transform.position);
    posList.Add(point2.transform.position);
    posList.Add(point3.transform.position);
    posList.Add(point4.transform.position);

    ////2023/9/11月追加
    //posList.Clear();
    //posList.Add(new Vector3(50,50,90));
    //posList.Add(new Vector3(50,400,90));
    //posList.Add(new Vector3(500,400,90));
    //posList.Add(new Vector3(500,50,90));
  }

  //2023/7/18(火)追加
  private float[,] GetAns(List<Vector3> RealPosList, List<Vector3> MpPosList)//ホモグラフィ変換行列を返してくれる関数
  {
    for(int i= 0; i < RealPosList.Count; i++)
    {
      Debug.Log(i + "番目のRealPosListは" + RealPosList[i] + "です！！");
      Debug.Log(i + "番目のMpPosListは" + MpPosList[i] + "です！！");
    }

    List<float> X = new List<float>(){ RealPosList[0][0],RealPosList[1][0],RealPosList[2][0],RealPosList[3][0]};
    List<float> Y = new List<float>(){ RealPosList[0][1],RealPosList[1][1],RealPosList[2][1],RealPosList[3][1]};
    List<float> x = new List<float>(){ MpPosList[0][0], MpPosList[1][0], MpPosList[2][0], MpPosList[3][0]};
    List<float> y = new List<float>(){ MpPosList[0][1], MpPosList[1][1], MpPosList[2][1], MpPosList[3][1]};
    
    for(int i = 0; i < RealPosList.Count; i++)
    {
      //Debug.Log(i + "番目の要素は" + RealPosList[0][i]);
      //Debug.Log(i + "番目の要素は" + RealPosList[i][0]);
    }
    float[,] Ans ;

    float[,] L = new float[,]{{x[0], y[0], 1, 0, 0, 0, -X[0] * x[0], -X[0] * y[0] },
      {    0,    0,   0, x[0], y[0],   1, -Y[0] * x[0], -Y[0] * y[0] },
      { x[1], y[1],   1,    0,    0,   0, -X[1] * x[1], -X[1] * y[1] },
      {    0,    0,   0, x[1], y[1],   1, -Y[1] * x[1], -Y[1] * y[1] },
      {x[2], y[2],   1,    0,    0,   0, -X[2] * x[2], -X[2] * y[2] },
      {    0,    0,   0, x[2], y[2],   1, -Y[2] * x[2], -Y[2] * y[2] },
      {x[3], y[3],   1,    0,    0,   0, -X[3] * x[3], -X[3] * y[3] },
      {    0,    0,   0, x[3], y[3],   1, -Y[3] * x[3], -Y[3] * y[3]} };
    
    for(int i=0;i < L.GetLength(0); i++)
    {
      for(int j = 0; j < L.GetLength(1); j++)
      {
        Debug.Log("Lの" + i + "," + j + "番目の要素は" + L[i, j] + "です！！");
      }
    }


    float[,] R = new float[,]
    { { X[0] },
      { Y[0] },
      { X[1] },
      { Y[1] },
      { X[2] },
      { Y[2] },
      { X[3] },
      { Y[3] } };

    ////2023/9/10変更（Rの中身をRealPosListに直す）
    //float[,] R = new float[,] 
    //{ { x[0] },
    //  { y[0] },
    //  { x[1] },
    //  { y[1] },
    //  { x[2] },
    //  { y[2] },
    //  { x[3] },
    //  { y[3] } };

    for (int i = 0; i < R.GetLength(0); i++)
    {
      for (int j = 0; j < R.GetLength(1); j++)
      {
        Debug.Log("Rの" + i + "," + j + "番目の要素は" + R[i, j] + "です！！");
      }
    }

    Ans = Solve(L, R);
    for(int i = 0; i < Ans.GetLength(0); i++)
    {
      Debug.Log("Ans行列の" + i + "番目の要素は" + Ans[i,0]);
    }
    Debug.Log("RealPosListは" + RealPosList + "です！！"　+ "MpPosListは"　+ MpPosList + "です！！" + "Ans行列は" + Ans +"です！！");

    return Ans;
  }

  //2023/7/18(火)追加
  private float[,]Solve(float[,]l,float[,]r)//numpyのSolve関数を実装
  {
    float[,] ans ;

    //lの逆行列を算出する＋行列の積を求める処理をかく
    l = CalcInverseMatrix(l);
    ans = MultiplyMatrix(l, r);

    return ans;
  }

  //2023/9/6水変更
  private float[,] CalcInverseMatrix(float[,] A)//逆行列を求める関数
  {
    float[,] InverseMatrix = new float[A.GetLength(0),A.GetLength(1)];
    var Matrix = Matrix<float>.Build.DenseOfArray(new float[,]
                {
                    {A[0,0],A[0,1],A[0,2],A[0,3],A[0,4],A[0,5],A[0,6],A[0,7]},
                    {A[1,0],A[1,1],A[1,2],A[1,3],A[1,4],A[1,5],A[1,6],A[1,7]},
                    {A[2,0],A[2,1],A[2,2],A[2,3],A[2,4],A[2,5],A[2,6],A[2,7]},
                    {A[3,0],A[3,1],A[3,2],A[3,3],A[3,4],A[3,5],A[3,6],A[3,7]},
                    {A[4,0],A[4,1],A[4,2],A[4,3],A[4,4],A[4,5],A[4,6],A[4,7]},
                    {A[5,0],A[5,1],A[5,2],A[5,3],A[5,4],A[5,5],A[5,6],A[5,7]},
                    {A[6,0],A[6,1],A[6,2],A[6,3],A[6,4],A[6,5],A[6,6],A[6,7]},
                    {A[7,0],A[7,1],A[7,2],A[7,3],A[7,4],A[7,5],A[7,6],A[7,7]},
                }
            );
    for(int i = 0; i < A.GetLength(0); i++)
    {
      for(int j = 0; j < A.GetLength(1); j++)
      {
        InverseMatrix[i,j] = Matrix.Inverse()[i,j];
      }
    }
    return InverseMatrix;
  }

  //2023/9/6水変更
  float[,] MultiplyMatrix(float[,] A, float[,] B)//行列の掛け算
  {
    float[,] result2 = new float[B.GetLength(0), B.GetLength(1)];
    var MatrixA = Matrix<float>.Build.DenseOfArray(new float[,]
                {
                    {A[0,0],A[0,1],A[0,2],A[0,3],A[0,4],A[0,5],A[0,6],A[0,7]},
                    {A[1,0],A[1,1],A[1,2],A[1,3],A[1,4],A[1,5],A[1,6],A[1,7]},
                    {A[2,0],A[2,1],A[2,2],A[2,3],A[2,4],A[2,5],A[2,6],A[2,7]},
                    {A[3,0],A[3,1],A[3,2],A[3,3],A[3,4],A[3,5],A[3,6],A[3,7]},
                    {A[4,0],A[4,1],A[4,2],A[4,3],A[4,4],A[4,5],A[4,6],A[4,7]},
                    {A[5,0],A[5,1],A[5,2],A[5,3],A[5,4],A[5,5],A[5,6],A[5,7]},
                    {A[6,0],A[6,1],A[6,2],A[6,3],A[6,4],A[6,5],A[6,6],A[6,7]},
                    {A[7,0],A[7,1],A[7,2],A[7,3],A[7,4],A[7,5],A[7,6],A[7,7]},
                }
            );
    //var MatrixB = Matrix<float>.Build.DenseOfArray(new float[,]
    //            {
    //                {B[0,0],
    //                 B[1,0],
    //                 B[2,0],
    //                 B[3,0],
    //                 B[4,0],
    //                 B[5,0],
    //                 B[6,0],
    //                 B[7,0]},
    //            }
    //        );
    
    var MatrixB = Matrix<float>.Build.DenseOfArray(new float[,]
                {
                     {B[0,0] },
                     { B[1,0] },
                     { B[2,0] },
                     { B[3,0] },
                     { B[4,0] },
                     { B[5,0] },
                     { B[6,0] },
                     { B[7,0]} 
                }
            );
    var result1 = MatrixA.Multiply(MatrixB);

    for( int i = 0; i < result2.GetLength(0); i++)
    {
      result2[i, 0] = result1[i, 0];
    }
    return result2;
  }
  //float[,] MultiplyMatrix(float[,] A, float[,] B)//行列の掛け算
  //{

  //  float[,] product = new float[A.GetLength(0), B.GetLength(1)];

  //  for (int i = 0; i < A.GetLength(0); i++)
  //  {
  //    for (int j = 0; j < B.GetLength(1); j++)
  //    {
  //      for (int k = 0; k < A.GetLength(1); k++)
  //      {
  //        product[i, j] += A[i, k] * B[k, j];
  //      }
  //    }
  //  }
  //  return product;
  //}

  public void ClickMirrorCalibButton()
  {
    //2023/10/19(木)追加
    _mirrorCalibCountDownText.enabled = true;
    _mirrorCalibCount++;

    _mirrorCalibButton.SetActive(false);
    StartCoroutine(MirrorCalibCountDown(_mirrorCalibTime));

    

    //GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    //capsule.name = "FinishMirrorCalib";
  }

  //[SerializeField] private GameObject _LeftWristCalibPoint;
  //[SerializeField] private GameObject _RightWristCalibPoint;
  //[SerializeField] private GameObject _LeftKneeCalibPoint;
  //[SerializeField] private GameObject _RightKneeCalibPoint;

  //2023/7/21(金)追加
  //public void ClickCalibButton()
  //{
  //  if (_count == 0)
  //  {
  //    _count = 0;
  //    _explanationText.text = "ミラーキャリブレーション";
  //    MirrorCalibCountDown(15);
  //  }
  //  _count++;
  //  _calibrationButton.enabled = false;
  //  StartCoroutine(CountDown());
  //}

  IEnumerator MirrorCalibCountDown(int time)
  {
    Debug.Log("ミラーキャリブレーション開始");
    for (int i = time; i > -1; i--)
    {
      if (i != time) yield return new WaitForSeconds(1);
      _mirrorCalibCountDownText.text = i.ToString();

      

    }

    //2023/10/19(木)追加
    _mirrorCalibCountDownText.text = " ";

    //2023/10/19(木)追加

    if (_mirrorCalibCount == 1)
    {
      //行列を取得するための処理
      _mpLandmarkList.Add(_mpLeftWristPos);
      _mpLandmarkList.Add(_mpRightWristPos);
      _mpLandmarkList.Add(_mpLeftKneePos);
      _mpLandmarkList.Add(_mpRightKneePos);
     
      _mirrorCalibArray = GetAns(_mirrorCalibPointList, _mpLandmarkList);
      isGetArray = true;

      _LeftWristCalibPoint.transform.position = new Vector3(_LeftWristCalibPoint.transform.position.x -5, _LeftWristCalibPoint.transform.position.y - _calibPointMoveDis, _LeftWristCalibPoint.transform.position.z);
      _RightWristCalibPoint.transform.position = new Vector3(_RightWristCalibPoint.transform.position.x +5, _RightWristCalibPoint.transform.position.y - _calibPointMoveDis, _RightWristCalibPoint.transform.position.z); 
      _LeftKneeCalibPoint.transform.position = new Vector3(_LeftKneeCalibPoint.transform.position.x -5, _LeftKneeCalibPoint.transform.position.y - _calibPointMoveDis, _LeftKneeCalibPoint.transform.position.z);
      _RightKneeCalibPoint.transform.position = new Vector3(_RightKneeCalibPoint.transform.position.x +5, _RightKneeCalibPoint.transform.position.y - _calibPointMoveDis, _RightKneeCalibPoint.transform.position.z); ;

      _mirrorCalibButton.SetActive(true);
      _mirrorCalibCountDownText.text = _mirrorCalibTime.ToString();

      yield break;
    }
    //2023/10/19(木)追加

    if(_mirrorCalibCount == 2)
    {
      //行列を取得するための処理
      _mpLandmarkList.Clear();
      _mpLandmarkList.Add(_mpLeftWristPos);
      _mpLandmarkList.Add(_mpRightWristPos);
      _mpLandmarkList.Add(_mpLeftKneePos);
      _mpLandmarkList.Add(_mpRightKneePos);
      _mirrorCalibPointList.Clear();
      _mirrorCalibPointList.Add(_LeftWristCalibPoint.transform.position);
      _mirrorCalibPointList.Add(_RightWristCalibPoint.transform.position);
      _mirrorCalibPointList.Add(_LeftKneeCalibPoint.transform.position);
      _mirrorCalibPointList.Add(_RightKneeCalibPoint.transform.position);

      _mirrorCalibArray = GetAns(_mirrorCalibPointList, _mpLandmarkList);
      isGetArray = true;

      _LeftWristCalibPoint.SetActive(false);
      _RightWristCalibPoint.SetActive(false);
      _LeftKneeCalibPoint.SetActive(false);
      _RightKneeCalibPoint.SetActive(false);
      _mirrorCalibCountDownText.enabled = false;

      //ゲーム開始ボタンを出現させるための処理
      GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      capsule.name = "FinishTrainingLowCalib";
      capsule.transform.position = new Vector3(1000, 1000, 1000);

      _explanationText.rectTransform.sizeDelta = new Vector2(400, 50);
      _explanationText.text = "ゲームを始めましょう";
      _GameStartButton.SetActive(true);
      _GameStartButton.transform.position = _calibrationButton.transform.position;

      yield break;

    }


    //2023/10/9(月)追加
    //2023/10/9(月)追加 赤玉削除＋視点キャリブレーション用の青玉生成
    _LeftWristCalibPoint.SetActive(false);
    _RightWristCalibPoint.SetActive(false);
    _LeftKneeCalibPoint.SetActive(false);
    _RightKneeCalibPoint.SetActive(false);
    //2023/10/9(月)追加
    Vector3 mirrorCalibTextPos = _mirrorCalibExplanationText.transform.localPosition;
    _mirrorCalibExplanationText.transform.localPosition = new Vector3(mirrorCalibTextPos.x, mirrorCalibTextPos.y + 10, mirrorCalibTextPos.z);
    _mirrorCalibExplanationText.GetComponent<TextMeshProUGUI>().fontSize = 28;
    _mirrorCalibExplanationText.text = "青玉を頭の位置へ";
    _HeadCalibPoint.SetActive(true);
    _HeadCalibPoint.GetComponent<Renderer>().material.color = new Color32(0, 0, 255, 120);
    _HeadCalibPoint.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);
    _HeadCalibPoint.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.5f);
    Vector3 generatePos = _mirrorCalibExplanationText.transform.localPosition;
    _HeadCalibPoint.transform.localPosition = new Vector3(generatePos.x, generatePos.y - 10, _HeadCalibPoint.transform.localPosition.z);

    _EyeGazeCalibUpButton.SetActive(true);
    _EyeGazeCalibDownButton.SetActive(true);
    float EyeGazeX = 20.8f;
    _EyeGazeCalibUpButton.transform.localPosition = new Vector3(EyeGazeX, 41.2f, 0);
    _EyeGazeCalibDownButton.transform.localPosition = new Vector3(EyeGazeX, 31.2f, 0);
    Vector3 DecideButtonPos = _DecideButton1.transform.localPosition;
    _DecideButton1.transform.localPosition = new Vector3(EyeGazeX, DecideButtonPos.y, DecideButtonPos.z);
    _DecideButton1.SetActive(true);

    _mirrorCalibCountDownText.color = Color.red;
    _mirrorCalibCountDownText.text = " ";

    //2023/10/10(火)追加
    _LeftWristText.enabled = false;
    _RightWristText.enabled = false;
    _LeftKneeText.enabled = false;
    _RightKneeText.enabled = false;
    _Header.GetComponent<Image>().color = new Color(255, 255, 255, 0);
    _MirrorCalibText.enabled = false;
    

    //2023/9/10修正・追加(以下４行) 2023/10/7(土)以下4行は不必要なのでコメントアウトしました
    //_mpLeftWristPos.x *= -1;
    //_mpRightWristPos.x *= -1;
    //_mpLeftKneePos.x *= -1;
    //_mpRightKneePos.x *= -1;

    ////2023/9/11修正・追加(以下8行)
    //_mpLeftWristPos.x = 100;
    //_mpLeftWristPos.y = 50;
    //_mpRightWristPos.x = 120;
    //_mpRightWristPos.y = 350;
    //_mpLeftKneePos.x = 500;
    //_mpLeftKneePos.y = 500;
    //_mpRightKneePos.x = 600;
    //_mpRightKneePos.y = 200;

    _mpLandmarkList.Add(_mpLeftWristPos);
    _mpLandmarkList.Add(_mpRightWristPos);
    _mpLandmarkList.Add(_mpLeftKneePos);
    _mpLandmarkList.Add(_mpRightKneePos);
    Debug.Log("わはは"　+ _mpLeftWristPos);
    Debug.Log("わはは"　+ _mpRightWristPos);
    Debug.Log("わはは"　+ _mpLeftKneePos);
    Debug.Log("わはは"　+ _mpRightKneePos);

    _mirrorCalibArray = GetAns(_mirrorCalibPointList, _mpLandmarkList);
    isGetArray = true;
    Debug.Log("_mpLeftWristPosの値は" + _mpLeftWristPos + "です！！！");
    Debug.Log("_mpRightWristPosの値は" + _mpRightWristPos + "です！！！");
    Debug.Log("_mpLeftKneePosの値は" + _mpLeftKneePos + "です！！！");
    Debug.Log("_mpRightKneePosの値は" + _mpRightKneePos + "です！！！");
    Debug.Log("_mirrorCalibArrayの値は" + _mirrorCalibArray + "です！！！");
    yield break;
  }
  public void ClickSquatCalibButton()
  {
    _count++;
    //_calibrationButton.enabled = false;
    _calibrationButton.SetActive(false);
    StartCoroutine(CountDown());
  }

  IEnumerator CountDown()
  {
    for(int i = _squatCalibTime; i > -1; i--)
    {
      if(i!= _squatCalibTime) yield return new WaitForSeconds(1);
      //_countDownText.text = i.ToString();
      _countDownText.text = i.ToString();
    }

    //_countDownText.text = " ";
    
    _countDownText.text = " ";

    if(_count == 1)
    {
      //頭のy_highを計測する処理
      //2023/10/9(月)追加　ミラー反射時のスクワットの初期姿勢におけるユーザの頭の位置
      _UserUpPos.transform.position = _HeadCalibPoint.transform.position;

      //2023/10/13(金)追加　（（やめた以下4行をDecideButton1くっりく関数の位置から移動しました））
      //GameObject capsule0 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      //capsule0.name = "FinishEyeGazeUpCalib";
      //capsule0.transform.position = new Vector3(1001, 1000, 1000);
      //Debug.Log("FinishEyeGazeUpCalibを生成したよ");

      //2023/10/11(水)編集　以下の1行をコメントアウトしてheadHighPosにUserUpPosの値を代入します：ミラー反射の頭の位置にコインを出すために必要な変更
      //headHighPos = _landMarkPos;

      headHighPos = _UserUpPos.transform.position;

      //2023/10/11(水)追加
      //スクワット初期姿勢時のMediaPipe座標を格納
      _mpHeadHighPos = _landMarkPos;

      //PoseLandmarkListAnnotation.csでConvertするために必要な座標をもつGameObject
      _MpSquatCalibUpObject.transform.position = _mpHeadHighPos;

      //_explanationText.text = "低姿勢の計測";
      //_explanationText.text = "低姿勢の計測";2023/10/10(火)編集

      //_calibrationButton.SetActive(true); 2023/10/10(火)編集

      //2023/10/9(月)追加 青玉の移動（スクワット低姿勢）
      //_HeadCalibPoint.transform.position = new Vector3(0, 17, -13.4f);
      _HeadCalibPoint.transform.position = new Vector3(0, 18, 76f);
      Vector3 HeadCalibPos = _HeadCalibPoint.transform.position;
      _EyeGazeCalibUpButton.SetActive(true);
      _EyeGazeCalibDownButton.SetActive(true);
      _EyeGazeCalibUpButton.transform.position = new Vector3(HeadCalibPos.x + _Head2UpButtonVec.x, HeadCalibPos.y + _Head2UpButtonVec.y, 90);
      //_EyeGazeCalibUpButton.transform.position = new Vector3(HeadCalibPos.x + _Head2UpButtonVec.x, HeadCalibPos.y + _Head2UpButtonVec.y, HeadCalibPos.z + _Head2UpButtonVec.z);
      _EyeGazeCalibDownButton.transform.position = new Vector3(HeadCalibPos.x + _Head2DownButtonVec.x, HeadCalibPos.y + _Head2DownButtonVec.y, 90);
      _DecideButton2.SetActive(true);
      _DecideButton2.transform.position = new Vector3(_EyeGazeCalibDownButton.transform.position.x, _DecideButton2.transform.position.y, _DecideButton2.transform.position.z);
      _explanationText.rectTransform.sizeDelta = new Vector2(250, 50);
      _explanationText.text = "青玉を頭の位置へ";

      //2023/9/22(金)追加
      GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      capsule.name = "FinishTrainingHighCalib";
      capsule.transform.position = new Vector3(1000, 1000, 1000);
    }

    if (_count == 2)
    {
      //頭のy_lowを計測する処理

      //2023/10/9(月)追加　ミラー反射時のスクワットの低姿勢におけるユーザの頭の位置
      _UserDownPos.transform.position = _HeadCalibPoint.transform.position;

      //2023/10/13(金)追加　（（やめた以下4行をDecideButton2くっりく関数の位置から移動しました））
      //GameObject capsule0 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      //capsule0.name = "FinishEyeGazeDownCalib";
      //capsule0.transform.position = new Vector3(1001, 1000, 1000);
      //Debug.Log("FinishEyeGazeDownCalibを生成したよ");

      //2023/10/11(水)編集　以下の1行をコメントアウトしてheadHighPosにUserLowPosの値を代入します：ミラー反射の頭の位置にコインを出すために必要な変更
      //headLowPos = _landMarkPos;
      headLowPos = _UserDownPos.transform.position;

      //2023/10/11(水)追加
      //スクワット低姿勢時のMediaPipe座標を格納
      _mpHeadLowPos = _landMarkPos;

      //PoseLandmarkListAnnotation.csでConvertするために必要な座標をもつGameObject
      _MpSquatCalibDownObject.transform.position = _mpHeadLowPos;

      //2023/9/22(金)追加
     
      _countDownText.color = Color.red;
      //_countDownText.text = "完了";
      _countDownText.enabled = false;

      //2023/10/10(火)追加
      _HeadCalibPoint.SetActive(false);
      _explanationText.rectTransform.sizeDelta = new Vector2(400, 50);
      _explanationText.text = "ゲームを始めましょう";
      _GameStartButton.SetActive(true);
      _GameStartButton.transform.position = _calibrationButton.transform.position;

      GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      capsule.name = "FinishTrainingLowCalib";
      capsule.transform.position = new Vector3(1000, 1000, 1000);
    }

    //_calibrationButton.enabled = true;
    yield break;
  }

  private void ReadCsv(string filePath,STATE state)
  {
    //Debug.Log("CSV読み込み");
    try
    {
      // 全行読込
      string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding("Shift_JIS"));
      
      //最後の行を代入する
      string _endLine = File.ReadAllLines(filePath, Encoding.GetEncoding("Shift_JIS")).LastOrDefault();

      // 列番号を指定する
      int timeClm = 0;
      int xClm;
      int yClm;
      int zClm;

      string[]  _latestDatas = _endLine.Split(',');

      if(state == STATE.SquatCalib)
      {
        xClm = 1;
        yClm = 2;
        zClm = 3;

        float x = 0;
        float y = 0;
        float z = 0;

        if (float.TryParse(_latestDatas[xClm], out float xValue))
        {
          x = xValue;
        }

        if (float.TryParse(_latestDatas[yClm], out float yValue))
        {
          y = yValue;
        }

        if (float.TryParse(_latestDatas[zClm], out float zValue))
        {
          z = zValue;
        }

        _landMarkPos = new Vector3(x, y, z);
        Debug.Log("_landMarkPosは" + _landMarkPos +"です！！！");
      }
      
      if(state == STATE.MirrorCalib)
      {
        xClm = 4;
        float x = 0;
        float y = 0;
        float z = 0;

        for (int i = 0; i < 4; i++)
        {
          yClm = xClm + 1;
          zClm = xClm + 2;
          
          if (float.TryParse(_latestDatas[xClm], out float xValue))
          {
            x = xValue;
          }

          if (float.TryParse(_latestDatas[yClm], out float yValue))
          {
            y = yValue;
          }

          if (float.TryParse(_latestDatas[zClm], out float zValue))
          {
            z = zValue;
          }
          if (i == 0) _mpLeftWristPos = new Vector3(x, y, z);
          else if (i == 1) _mpRightWristPos = new Vector3(x, y, z);
          else if (i == 2) _mpLeftKneePos = new Vector3(x, y, z);
          else if (i == 3) _mpRightKneePos = new Vector3(x, y, z);
          Debug.Log("_mpLeftWristPosは" + _mpLeftWristPos + "です！！！");
          Debug.Log("_mpRightWristPosは" + _mpRightWristPos + "です！！！");
          Debug.Log("_mpLeftKneePosは" + _mpLeftKneePos + "です！！！");
          Debug.Log("_mpRightKneePosは" + _mpRightKneePos + "です！！！");
          xClm += 3;
        }
      }
    }
    catch (System.Exception e)
    {
      Debug.Log("CSV読み込み失敗: Path:" + filePath);
      System.Console.WriteLine(e.Message);
    }
  }

  private void SaveData(string path, string data, string fileName)
  {

    //using(StreamWriter sw = new StreamWriter(path + "/" + fileName + ".csv"))
    //{
    //  sw.Write(data);
    //}

    StreamWriter sw; ;
    FileInfo fi;

    fi = new FileInfo(path + "/" + fileName + ".csv");

    sw = fi.AppendText();
    sw.WriteLine(data);
    sw.Flush();
    sw.Close();
    //Debug.Log(path + "/" + fileName + ".csv" + data + "を保存");
  }

  private void SaveTaskData(string path,string fileName,float[,] array)
  {
    //_folderPathは最初にセット済み
    string data = "";
    for(int i = 0; i < 8; i++)
    {
      if (i < 7) data += (array[i, 0].ToString() + ",");
      else if (i == 7) data += array[i, 0].ToString();
      Debug.Log("dataの値は" + data + "です");
    }
    SaveData(path, data, fileName);
  }

  public void StartGame()
  {
    if (!isResultGo)
    {
      //2023/9/11(月)追加
      DeleteUI();
      curve = CreateCurves();
      //2023/9/22(金)追加
      GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      capsule.name = "StartGame";
      capsule.transform.position = new Vector3(1000, 1000, 1000);
      Debug.Log("CreateCurvesしました！！！");

      _GameStartButton.SetActive(false);
    }
  }

  //2023/9/11(月)追加
  private void DeleteUI()
  {
    _mirrorCalibCountDownText.enabled = false;
    _mirrorCalibExplanationText.enabled = false;
    _mirrorCalibButton.SetActive(false);
    _Header.SetActive(false);
    _Footer.SetActive(false);
    _calibrationButton.SetActive(false);
    _mirrorCalibButton.SetActive(false);
    _GameStartButton.SetActive(false);
    _explanationText.enabled = false;
    _countDownText.enabled = false;
    _mirrorCalibExplanationText.enabled = false;
    _mirrorCalibCountDownText.enabled = false;
    _MirrorCalibText.enabled = false;
    _LeftWristText.enabled = false;
    _RightWristText.enabled = false;
    _LeftKneeText.enabled = false;
    _RightKneeText.enabled = false;
    _TrainingTypeText.enabled = false;
    //_TrainingTypeDropdown.
    _Horizon.SetActive(false);
    _Vertical.SetActive(false);
    _HeadCalibPoint.SetActive(false);
    _LeftWristCalibPoint.SetActive(false);
    _RightWristCalibPoint.SetActive(false);
    _LeftKneeCalibPoint.SetActive(false);
    _RightKneeCalibPoint.SetActive(false) ;
  }

  //2023/10/9(月)追加
  public void ClickEyeGazeCalibUpButton()
  {
    Vector3 headCalibPos = _HeadCalibPoint.transform.position;
    _HeadCalibPoint.transform.position = new Vector3(headCalibPos.x, headCalibPos.y + _HeadCalibMoveDis, headCalibPos.z);
    
  } 
  
  public void ClickEyeGazeCalibDownButton()
  {
    Vector3 headCalibPos = _HeadCalibPoint.transform.position;
    _HeadCalibPoint.transform.position = new Vector3(headCalibPos.x, headCalibPos.y - _HeadCalibMoveDis, headCalibPos.z);
    
  }

  public void ClickDecideButton1()
  {
    //2023/10/9(月)追加 
    //2023/10/13(金)修正　（（やめた　以下4行を初期姿勢計測処理部分に移動します））
    GameObject capsule0 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    capsule0.name = "FinishEyeGazeUpCalib";
    capsule0.transform.position = new Vector3(1001, 1000, 1000);
    Debug.Log("FinishEyeGazeUpCalibを生成したよ");
    _DecideButton1.SetActive(false);

    

    //_explanationText.text = "低姿勢の計測";

    //2023/10/9(月)追加
    //_HeadCalibPoint.SetActive(false);
    _EyeGazeCalibUpButton.SetActive(false);
    _EyeGazeCalibDownButton.SetActive(false);

    //2023/10/10(火)追加
    _mirrorCalibExplanationText.enabled = false;

    _explanationText.enabled= true;
    Debug.Log("もも_explanationText.enabled = true;");
    _countDownText.enabled = true;
    Debug.Log("もも    _calibrationButton.SetActive(true);");
    _calibrationButton.SetActive(true);
    Debug.Log("もも    _calibrationButton.SetActive(true);");
    _explanationText.transform.localPosition = _mirrorCalibExplanationText.transform.localPosition;
    _calibrationButton.transform.localPosition = _mirrorCalibButton.transform.localPosition;
    _countDownText.transform.localPosition = _mirrorCalibCountDownText.transform.localPosition;
  }

  public void ClickDecideButton2()
  {
    //2023/10/9(月)追加
    //2023/10/13(金)修正　（（やめた　以下3行を初期姿勢計測処理部分に移動します））
    GameObject capsule0 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    capsule0.name = "FinishEyeGazeDownCalib";
    capsule0.transform.position = new Vector3(1000, 1000, 1000);
    _DecideButton2.SetActive(false);

    

    //2023/10/10(火)追加
    _EyeGazeCalibUpButton.SetActive(false);
    _EyeGazeCalibDownButton.SetActive(false);
    _calibrationButton.SetActive(true);
    _explanationText.text = "低姿勢の計測";
  }

  private void GameEnd()
  {
    isGameOn = false;
    isResultGo = true;
  }

  public bool CheckGameEnd()
  {
    int childCount = Parent.transform.childCount;

    if (childCount == 0) isFinishJudge = true;

    //2023/10/16(月)修正　以下をコメントアウトします
    //else if (trainingTypes ==1 && Parent.transform.GetChild(childCount - 1).transform.position.x > 0)
    //{
    //  isFinishJudge = true;
    //  Debug.Log("isFinishJudgeがtrueになりました！！！");
    //}
    return isFinishJudge;
  }

  public GameObject CreateCurves()
  {
    float[,] points = GetCurveList();
    Parent.transform.eulerAngles = new Vector3(0, 0, 0);
    Parent.transform.position = new Vector3(0, 0, 0);

    float radius = 1.0f;

    float maxY = GetMax(points.GetLength(0), 1, points);
    float maxZ = GetMax(points.GetLength(0), 2, points);

    if (trainingTypes == 1)
    {
      radius = (headHighPos.y - headLowPos.y) / 2;
    }

    int index = 0;
    for (int j = 0; j < trainingTimes; ++j)
    {
      for (int i = 0; i < points.GetLength(0); ++i)
      {
        Vector3 v;

        v = new Vector3(points[i, 0] * 2 * radius - _squat_speed * index, points[i, 1] * 2 * radius, points[i, 2] * 2 * radius );//2023/7/6(木)編集
        GameObject coinClone = Instantiate(coin, v, Quaternion.Euler(90, 0, 0), Parent.transform);
        coinClone.tag = "GoldCoin";
        goldCoinNum++;

        if(trainingTypes ==1)
        {
          if (v[1] == 0)
          {
            coinClone.GetComponentInChildren<Renderer>().material = keepCoinColor;
            coinClone.tag = "RedCoin";
            redCoinNum++;
            goldCoinNum--;
          }
        }

        Debug.Log(index + ("番目の処理が終了しました！！！"));
        Coins.Add(coinClone);
        ++index;
      }
    }

    if (trainingTypes == 1)
    {
      float y = headLowPos.y;

      //2023/10/16(月)追加
      //Parent.transform.position = new Vector3(-squat_speed * gameCountDownTime, y, 90);//2023/7/6(木)編集
      Parent.transform.eulerAngles = new Vector3(0, 90, 0);
      Parent.transform.position = new Vector3(_LeftWristCalibPoint.transform.position.x -7, y, _MainCanvas.transform.position.z + _squat_speed * gameCountDownTime);

    }

    Debug.Log("Parentの座標は、" + Parent.transform.position + ("です！！！！"));

    isGameOn = true;
    return Parent;
  }
  private void MoveWave(GameObject curve, int type)
  {
    //2023/10/16(月)修正
    //if (type == 1)curve.transform.position += new Vector3(Time.deltaTime * squat_speed, 0, 0);
    if (type == 1)curve.transform.position += new Vector3(0, 0, -Time.deltaTime * _squat_speed);
  }

  private float[,] GetCurveList()
  {
    float[,] emptyCurveArray = new float[,] { };

    if (trainingTypes == 1 && trainingStrength == 0) return squatCurveArray1;
    
    else return emptyCurveArray;
  }

  private float GetMax(int n, int a, float[,] points)
  {
    float maxValue = -100;

    for (int i = 0; i < n; ++i)
    {
      if (points[i, a] > maxValue)
      {
        maxValue = points[i, a];
      }
    }
    return maxValue;
  }

  public void SaveTrainingTypes(TMP_Dropdown dropdown)
  {
    if (dropdown.value == 0)
    {
      trainingTypes = 1;
      Debug.Log("スクワットが選択されました");
    }
  }

  private enum STATE
  {
    Calibration,
    MirrorCalib,
    SquatCalib,
  }
}
