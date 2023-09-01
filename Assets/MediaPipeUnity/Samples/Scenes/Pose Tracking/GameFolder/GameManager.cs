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

  //コインの座標を格納した2次元配列
  //CurveArray1
  float[,] squatCurveArray1 = new float[,] { { 0, 1, 0 }, { 0, 0.75f, 0 }, { 0, 0.5f, 0 }, { 0, 0.25f, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0.25f, 0 }, { 0, 0.5f, 0 }, { 0, 0.75f, 0 }, { 0, 1, 0 } };

  [SerializeField] private TextMeshProUGUI _countDownText;
  [SerializeField] private TextMeshProUGUI _explanationText;
  [SerializeField] private Button _calibrationButton;
  [SerializeField] private GameObject coin;
  [SerializeField] private GameObject Parent;
  [SerializeField] private Material keepCoinColor;
  [SerializeField] private float squat_speed;

  //2023/7/18(火)追加
  [SerializeField] private GameObject HeadCalibPoint;
  [SerializeField] private GameObject LeftWristCalibPoint;
  [SerializeField] private GameObject RightWristCalibPoint;
  [SerializeField] private GameObject LeftKneeCalibPoint;
  [SerializeField] private GameObject RightKneeCalibPoint;

  //2023/7/19(水)追加
  [SerializeField] private TextMeshProUGUI _mirrorCalibCountDownText;
  [SerializeField] private TextMeshProUGUI _mirrorCalibExplanationText;
  [SerializeField] private Button _mirrorCalibButton;

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
    AddElement(_mirrorCalibPointList, LeftWristCalibPoint, RightWristCalibPoint, LeftKneeCalibPoint, RightKneeCalibPoint);//MpLandmarkListへの要素追加はCSV経由で代入する
    _mirrorCalibArray = new float[8,1];
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
      string folderPath = "C:/Users/ig/AppData/LocalLow/DefaultCompany/MediaPipeUnityPlugin/SmartMirror/MirrorCalibration";
      string fileName = "CalibrationArray";
      SaveTaskData(folderPath, fileName, _mirrorCalibArray);

      //2023/7/25(水)追加　キャリブレーション行列をCSVに書き込んだことをPoseLamdmarkListAnnotation.csに伝えるためにゲームオブジェクトを生成
      GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      capsule.transform.position = new Vector3(1000, 1000, 1000);
      capsule.name = "MirrorCalibrationCompleted";

      isGetArray = false;
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
  }

  //2023/7/18(火)追加
  private float[,] GetAns(List<Vector3> RealPosList, List<Vector3> MpPosList)//ホモグラフィ変換行列を返してくれる関数
  {
    for(int i= 0; i < RealPosList.Count; i++)
    {
      Debug.Log(i + "番目のRealPosListは" + RealPosList[i] + "です！！");
      Debug.Log(i + "番目のMpPosListは" + MpPosList[i] + "です！！");
    }

    List<float> x = new List<float>(){ RealPosList[0][0],RealPosList[1][0],RealPosList[2][0],RealPosList[3][0]};
    List<float> y = new List<float>(){ RealPosList[0][1],RealPosList[1][1],RealPosList[2][1],RealPosList[3][1]};
    List<float> X = new List<float>(){ MpPosList[0][0], MpPosList[1][0], MpPosList[2][0], MpPosList[3][0]};
    List<float> Y = new List<float>(){ MpPosList[0][1], MpPosList[1][1], MpPosList[2][1], MpPosList[3][1]};
    
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
    Debug.Log("Lの値は" + L + "です！！");
    

    float[,] R = new float[,] { { X[0] },
      { Y[0] },
      { X[1] },
      { Y[1] },
      { X[2] },
      { Y[2] },
      { X[3] },
      { Y[3] } };

    Debug.Log("Rの値は" + R + "です！！");

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

  private float[,] CalcInverseMatrix(float[,] A)//逆行列を求める関数
  {

    int n = A.GetLength(0);
    int m = A.GetLength(1);

    float[,] invA = new float[n, m];

    if (n == m)
    {

      int max;
      float tmp;

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
          if (float.IsNaN(invA[j, i]))
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
    _mirrorCalibButton.enabled = false;
    StartCoroutine(MirrorCalibCountDown(20));
  }

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

    _mirrorCalibCountDownText.text = " 計測完了！！";
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
    _calibrationButton.enabled = false;
    StartCoroutine(CountDown());
  }

  IEnumerator CountDown()
  {
    for(int i = 5; i > -1; i--)
    {
      if(i!=5) yield return new WaitForSeconds(1);
      _countDownText.text = i.ToString();
    }

    _countDownText.text = " ";

    if(_count == 1)
    {
      //頭のy_highを計測する処理
      headHighPos = _landMarkPos;
      Debug.Log("スクワット時の頭の最高点は" + headHighPos + ("です！！"));
      _explanationText.text = "低姿勢の計測";
    }

    if (_count == 2)
    {
      //頭のy_lowを計測する処理
      headLowPos = _landMarkPos;
      Debug.Log("スクワット時の頭の最低点は" + headLowPos + ("です！！"));
    }

    _calibrationButton.enabled = true;
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
      curve = CreateCurves();
      Debug.Log("CreateCurvesしました！！！");
    }
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

    else if (trainingTypes ==1 && Parent.transform.GetChild(childCount - 1).transform.position.x > 0)
    {
      isFinishJudge = true;
      Debug.Log("isFinishJudgeがtrueになりました！！！");
    }
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

        v = new Vector3(points[i, 0] * 2 * radius - squat_speed * index, points[i, 1] * 2 * radius, points[i, 2] * 2 * radius );//2023/7/6(木)編集
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
      Parent.transform.position = new Vector3(-squat_speed * gameCountDownTime, y, 90);//2023/7/6(木)編集
    }
   
    Debug.Log("Parentの座標は、" + Parent.transform.position + ("です！！！！"));

    isGameOn = true;
    return Parent;
  }
  private void MoveWave(GameObject curve, int type)
  {
    if (type == 1)curve.transform.position += new Vector3(Time.deltaTime * squat_speed, 0, 0);
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
