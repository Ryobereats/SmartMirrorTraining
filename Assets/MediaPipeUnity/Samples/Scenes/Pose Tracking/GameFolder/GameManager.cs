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

    ReadCsv(_folderPath);
    //Debug.Log(_landMarkPos);

    if (isGameOn)
    {
      MoveWave(curve, trainingTypes);
      Debug.Log("MoveWaveしました！！！");
      if (CheckGameEnd()) GameEnd();
    }

  }

  public void ClickCalibrationButton()
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

  private void ReadCsv(string filePath)
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
      int xClm = 1;
      int yClm = 2;
      int zClm = 3;

      string[]  _latestDatas = _endLine.Split(',');
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

    }
    catch (System.Exception e)
    {
      Debug.Log("CSV読み込み失敗: Path:" + filePath);
      System.Console.WriteLine(e.Message);
    }
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
  }
}
