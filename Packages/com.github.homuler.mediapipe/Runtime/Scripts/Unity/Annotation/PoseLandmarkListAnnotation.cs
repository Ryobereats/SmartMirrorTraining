// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine.UI;


namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
  using Color = UnityEngine.Color;
#pragma warning restore IDE0065

  public sealed class PoseLandmarkListAnnotation : HierarchicalAnnotation
  {
    [SerializeField] private PointListAnnotation _landmarkListAnnotation;
    [SerializeField] private ConnectionListAnnotation _connectionListAnnotation;
    [SerializeField] private Color _leftLandmarkColor = Color.green;
    [SerializeField] private Color _rightLandmarkColor = Color.green;

    //2023/7/3(月)追加
    public Vector3 _headPos { get; private set; }
    public static PoseLandmarkListAnnotation Instance;
    private float _time = 0;
    float _timecount = 0;
    private string _folderPath;

    //2023/7/5(水)追加
    private bool isCreateCapsule = false;
    private GameObject sphere = null;
    private void Awake()
    {
      Instance = this;
    }

    //2023/7/17(月)追加
    private Vector3 _leftEar;
    private Vector3 _rightEar;

    //2023/7/21(金)追加
    private Vector3 _leftWrist;
    private Vector3 _rightWrist;
    private Vector3 _leftKnee;
    private Vector3 _rightKnee;

    //2023/9/22(金)追加
    private Order _order;
    private bool _isFIndButtons = false;
    private GameObject _mirrorCalibButton;
    private GameObject _calibButton;
    private GameObject _gameStartButton;
    private bool isInitOrder = false;
    GameObject _button = null;

    //2023/9/23(土)追加
    private float _startTime = -1;//ボタンクリック判定のための開始時間
    private float _endTime = -1;
    private int _lineIndex = -1;//CalibratedLandmarkPos.csvの中で注目したい行のインデックスを格納する変数
    private List<float> _timeList;
    private List<string> _orderList;
    private List<string> _boolList;
    int _linesIndex = -1;

    //2023/9/24(日)追加
    private int _trueCount = -1;

    //2023/9/25(月)追加
    private int _csvLatestIndex = -1;

    //2023/9/28(木)追加
    private float _clickJudgeTime = 0;//タッチレスクリック機能におけるクリック判定のための時間条件
    private GameObject _pointingCursor = null;//タッチレスクリック機能における右手首の位置にカーソルを表示
    private bool _isCameraOn = false;
    private Image _circleImage;
    private float _waitTime = 1f;//3倍かかる
    private bool _isButtonClicked = false;

    //2023/10/5(木)追加
    private List<GameObject> _switchingButtonList = new List<GameObject>();
    private int _switchingButtonIndex = -1;
    private float _switchingClickJudgeTime = -1;//switchingボタンのクリック判定を行うための時間カウント
    private Image _circleImageInstanse;
    private bool _isSwitchingButtonClicked = false;
    private bool _isSwitchingButtonOn = false;
    private int _clickedSwitchingButtonIndex = -1;

    //2023/10/7(土)デバッグ用変数
    private bool _is1 = false;
    GameObject _pointingCursorLeftWrist;
    GameObject _pointingCursorRightWrist;
    GameObject _pointingCursorLeftKnee;
    GameObject _pointingCursorRightKnee;
    Vector3 _calibedLeftWrist;

    //2023/10/9(月)追加
    private List<GameObject> _directionButtonList = new List<GameObject>();
    private List<GameObject> _buttonList = new List<GameObject>();
    private GameObject _dummyButton1;
    private GameObject _dummyButton2;

    //2023/10/10(火)追加
    private Vector3 _MirrorUpPos;
    private Vector3 _MirrorDownPos;
    private float _eyeGazeClickJudgeTime = -1;

    [Flags]
    public enum BodyParts : short
    {
      None = 0,
      Face = 1,
      // Torso = 2,
      LeftArm = 4,
      LeftHand = 8,
      RightArm = 16,
      RightHand = 32,
      LowerBody = 64,
      All = 127,
    }

    private const int _LandmarkCount = 33;
    private static readonly int[] _LeftLandmarks = new int[] {
      1, 2, 3, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31
    };
    private static readonly int[] _RightLandmarks = new int[] {
      4, 5, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32
    };
    private static readonly List<(int, int)> _Connections = new List<(int, int)> {
      // Left Eye
      (0, 1),
      (1, 2),
      (2, 3),
      (3, 7),
      // Right Eye
      (0, 4),
      (4, 5),
      (5, 6),
      (6, 8),
      // Lips
      (9, 10),
      // Left Arm
      (11, 13),
      (13, 15),
      // Left Hand
      (15, 17),
      (15, 19),
      (15, 21),
      (17, 19),
      // Right Arm
      (12, 14),
      (14, 16),
      // Right Hand
      (16, 18),
      (16, 20),
      (16, 22),
      (18, 20),
      // Torso
      (11, 12),
      (12, 24),
      (24, 23),
      (23, 11),
      // Left Leg
      (23, 25),
      (25, 27),
      (27, 29),
      (27, 31),
      (29, 31),
      // Right Leg
      (24, 26),
      (26, 28),
      (28, 30),
      (28, 32),
      (30, 32),
    };

    public override bool isMirrored
    {
      set
      {
        _landmarkListAnnotation.isMirrored = value;
        _connectionListAnnotation.isMirrored = value;
        base.isMirrored = value;
      }
    }

    public override RotationAngle rotationAngle
    {
      set
      {
        _landmarkListAnnotation.rotationAngle = value;
        _connectionListAnnotation.rotationAngle = value;
        base.rotationAngle = value;
      }
    }

    public PointAnnotation this[int index] => _landmarkListAnnotation[index];

    private void Start()
    {
      _landmarkListAnnotation.Fill(_LandmarkCount);
      ApplyLeftLandmarkColor(_leftLandmarkColor);
      ApplyRightLandmarkColor(_rightLandmarkColor);

      _connectionListAnnotation.Fill(_Connections, _landmarkListAnnotation);

      //2023/7/4追加
      _folderPath = GetFolderPath();
      CreateDirectory(_folderPath);

      //2023/9/28(木)追加
      _circleImage = GameObject.Find("CircleImage").GetComponent<Image>();

      //2023/10/5(木)追加
      _circleImageInstanse = Instantiate(_circleImage,new Vector3(200,200,90),Quaternion.identity);//生成位置を変えた
      _circleImageInstanse.name = "circleImageInstance";
      //2023/10/10(火)追加　ALLUI配下に移動することで表示されるようにする
      _circleImageInstanse.transform.parent = GameObject.Find("ALLUI").transform;

      GameObject switchingButtonParent = GameObject.Find("SwitchingButtonParent");
      for (int i = 0; i < switchingButtonParent.transform.childCount; i++)
      {
        _switchingButtonList.Add(switchingButtonParent.transform.GetChild(i).gameObject);
      }

      //2023/10/9(月)追加
      _dummyButton1 = GameObject.Find("DummyButton1");
      _dummyButton2 = GameObject.Find("DummyButton2");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
      if (!UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this))
      {
        ApplyLeftLandmarkColor(_leftLandmarkColor);
        ApplyRightLandmarkColor(_rightLandmarkColor);
      }
    }
#endif

    public void SetLeftLandmarkColor(Color leftLandmarkColor)
    {
      _leftLandmarkColor = leftLandmarkColor;
      ApplyLeftLandmarkColor(_leftLandmarkColor);
    }

    public void SetRightLandmarkColor(Color rightLandmarkColor)
    {
      _rightLandmarkColor = rightLandmarkColor;
      ApplyRightLandmarkColor(_rightLandmarkColor);
    }

    public void SetLandmarkRadius(float landmarkRadius)
    {
      _landmarkListAnnotation.SetRadius(landmarkRadius);
    }

    public void SetConnectionColor(Color connectionColor)
    {
      _connectionListAnnotation.SetColor(connectionColor);
    }

    public void SetConnectionWidth(float connectionWidth)
    {
      _connectionListAnnotation.SetLineWidth(connectionWidth);
    }

    public void Draw(IList<Landmark> target, Vector3 scale, bool visualizeZ = false)//多分ランドマーク（点）の描画処理
    {
      ////2023/7/25(火)&7/26(水)追加
      //if (GameObject.Find("MirrorCalibrationCompleted"))
      //{
      //  Debug.Log("今からCSVを読み取ります1");
      //  string filePath = "C:/Users/ig/AppData/LocalLow/DefaultCompany/MediaPipeUnityPlugin/SmartMirror/MirrorCalibration/CalibrationArray.csv";
      //  float[] lines = ReadCsv(filePath);
      //  for (int i = 0; i < 33; i++)
      //  {
      //    float x = _landmarkListAnnotation[i].transform.position.x;
      //    float y = _landmarkListAnnotation[i].transform.position.y;
      //    float z = _landmarkListAnnotation[i].transform.position.z;

      //    if(i==0) Debug.Log(i + "番目のランドマークの元々の座標は" + "(" + x + "," + y + "," + z + "," + ")" + "です");
      //    //Debug.Log(i + "番目のランドマークの元々の座標は" + "(" + x + "," + y + "," + z + "," + ")" + "です");
      //    _landmarkListAnnotation[i].transform.position = new Vector3(ConvertCoordinate(lines, x, y, CoordinateType.x), ConvertCoordinate(lines, x, y, CoordinateType.y), z);
      //    //Debug.Log(i + "番目のランドマークのキャリブレーション後の座標は" + _landmarkListAnnotation[i].transform.position + "です");

      //    //List<Vector3> convertedLandmarkList.Add(_landmarkListAnnotation[i].transform.position);
      //    Debug.Log("キャリブレーション行列の書かれたCSVの読み取り完了");
      //  }
      //}
      //else
      //{
      //  Debug.Log("ミラーキャリブレーション行列が存在しないためCSVを開けません");
      //}
      if (ActivateFor(target))
      {
        _landmarkListAnnotation.Draw(target, scale, visualizeZ);
        // Draw explicitly because connection annotation's targets remain the same.
        _connectionListAnnotation.Redraw();
      }
    }


    //2023/7/25(火)追加
    private float[] ReadCsv(string filePath)
    {
      //Debug.Log("CSV読み込み");

      float[] floatLines = new float[8];
      try
      {
        // 全行読込
        string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding("Shift_JIS"));

        //最後の行を代入する
        string _endLine = File.ReadAllLines(filePath, Encoding.GetEncoding("Shift_JIS")).LastOrDefault();

        string[] _latestDatas = _endLine.Split(',');

        for (int i = 0; i < 8; i++)
        {
          floatLines[i] = float.Parse(_latestDatas[i]);
        }
        return floatLines;
      }
      catch (System.Exception e)
      {
        Debug.Log("CSV読み込み失敗: Path:" + filePath);
        System.Console.WriteLine(e.Message);
        for (int i = 0; i < 8; i++)
        {
          floatLines[i] = 0;
          Debug.Log("floatLinesの中身は" + floatLines[i]);
        }
        return floatLines;
      }
    }

    //2023/9/23(土)追加
    //private void ReadCsv2(string filePath)
    //{
    //  Debug.Log("CSV読み込み");
    //  List<float> floatLines = new List<float>();
    //  List<string> orderLines = new List<string>();
    //  List<string> boolLines = new List<string>();

    //  List<List<string>> linesList = new List<List<string>>();

    //  //List<List<float>> floatLinesList = new List<List<float>>();
    //  //List<List<string>> orderLinesList = new List<List<string>>();
    //  //List<List<string>> boolLinesList = new List<List<string>>();

    //  try
    //  {
    //    // 全行読込
    //    string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding("Shift_JIS"));
    //    Debug.Log("読み込んだぜ");

    //    foreach (string line in lines)
    //    {
    //      Debug.Log("フォーイーチに入ったよ");
    //      //コンマ区切りで要素を配列に追加
    //      string[] dataArray = line.Split(',');
    //      Debug.Log("スプリットしたよ");

    //      List<string> dataList = dataArray.ToList();
    //      Debug.Log("配列からリストに変換したよ");

    //      linesList.Add(dataList);
    //      Debug.Log("２次元リストに要素を追加したよ");

    //      floatLines.Add(float.Parse(dataList[0]));
    //      Debug.Log("フロートリストに代入されたよ");

    //      orderLines.Add(dataList[4]);
    //      Debug.Log("オーダーリストに代入されたよ");

    //      boolLines.Add(dataList[5]);
    //      Debug.Log("ブールリストに代入されたよ");

    //    }
    //    _timeList = floatLines;
    //    _orderList = orderLines;
    //    _boolList = boolLines;
    //  }
    //  catch (System.Exception e)
    //  {
    //    Debug.Log("CSV読み込み失敗: Path:" + filePath);
    //    System.Console.WriteLine(e.Message);
    //  }
    //}

    //2023/9/25(月)修正
    private void ReadCsv2(string filePath)
    {
      Debug.Log("CSV読み込み");

      //2023/9/25(月)追加
      _csvLatestIndex++;

      List<float> floatLines = new List<float>();
      List<string> orderLines = new List<string>();
      List<string> boolLines = new List<string>();

      List<List<string>> linesList = new List<List<string>>();

      //List<List<float>> floatLinesList = new List<List<float>>();
      //List<List<string>> orderLinesList = new List<List<string>>();
      //List<List<string>> boolLinesList = new List<List<string>>();

      try
      {
        // 全行読込
        string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding("Shift_JIS"));
        Debug.Log("読み込んだぜ");

        foreach (string line in lines)
        {
          Debug.Log("フォーイーチに入ったよ");
          //コンマ区切りで要素を配列に追加
          string[] dataArray = line.Split(',');
          Debug.Log("スプリットしたよ");

          List<string> dataList = dataArray.ToList();
          Debug.Log("配列からリストに変換したよ");

          linesList.Add(dataList);
          Debug.Log("２次元リストに要素を追加したよ");

          floatLines.Add(float.Parse(dataList[0]));
          Debug.Log("フロートリストに代入されたよ");


          orderLines.Add(dataList[4]);
          Debug.Log("オーダーリストに代入されたよ");
          Debug.Log("オーダーリストの要素数の確認");
          Debug.Log("オーダーラインズの要素数の確認" + orderLines.Count);

          boolLines.Add(dataList[5]);
          Debug.Log("ブールリストに代入されたよ");
        }
        //2023/9/25(月)追加
        _timeList.Add(floatLines[_csvLatestIndex]);
        _orderList.Add(orderLines[_csvLatestIndex]);
        _boolList.Add(boolLines[_csvLatestIndex]);
        Debug.Log("オーダーリストの要素数" + _orderList.Count);

      }
      catch (System.Exception e)
      {
        Debug.Log("CSV読み込み失敗: Path:" + filePath);
        System.Console.WriteLine(e.Message);
      }
    }

    private List<List<string>> ReadStringCsv(string filePath)
    {
      //Debug.Log("CSV読み込み");

      //List<List<float>> linesList = new List<List<float>>();
      List<List<string>> linesList = new List<List<string>>();

      try
      {
        // 全行読込
        string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding("Shift_JIS"));


        foreach (string line in lines)
        {
          //コンマ区切りで要素を配列に追加
          string[] dataArray = line.Split(',');
          List<string> dataList = dataArray.ToList();
          linesList.Add(dataList);
        }
      }
      catch (System.Exception e)
      {
        Debug.Log("CSV読み込み失敗: Path:" + filePath);
        System.Console.WriteLine(e.Message);
      }
      return linesList;
    }

    //2023/7/25(火)追加
    private float ConvertCoordinate(float[] lines, float x, float y, CoordinateType type)
    {
      float X = 0;
      float Y = 0;
      if (type == CoordinateType.x)
      {
        X = (lines[0] * x + lines[1] * y + lines[2]) / (lines[6] * x + lines[7] * y + 1);
        return X;
      }
      else if (type == CoordinateType.y)
      {
        Y = (lines[3] * x + lines[4] * y + lines[5]) / (lines[6] * x + lines[7] * y + 1);
        return Y;
      }
      else return 0;
    }
    public void Draw(LandmarkList target, Vector3 scale, bool visualizeZ = false)
    {
      Draw(target?.Landmark, scale, visualizeZ);
    }

    private enum CoordinateType
    {
      x,
      y,
    }
    public void Draw(IList<NormalizedLandmark> target, BodyParts mask, bool visualizeZ = false)//多分線の描画処理
    {
      //2023/7/25(火)&7/26(水)追加
      //if (GameObject.Find("MirrorCalibrationCompleted"))
      //{
      //  Debug.Log("今からCSVを読み取ります1");
      //  string filePath = "C:/Users/ig/AppData/LocalLow/DefaultCompany/MediaPipeUnityPlugin/SmartMirror/MirrorCalibration/CalibrationArray.csv";
      //  float[] lines = ReadCsv(filePath);
      //  for (int i = 0; i < 33; i++)
      //  {
      //    float x = _landmarkListAnnotation[i].transform.position.x;
      //    float y = _landmarkListAnnotation[i].transform.position.y;
      //    float z = _landmarkListAnnotation[i].transform.position.z;

      //    if (i == 0) Debug.Log(i + "番目のランドマークの元々の座標は" + "(" + x + "," + y + "," + z + "," + ")" + "です");
      //    //Debug.Log(i + "番目のランドマークの元々の座標は" + "(" + x + "," + y + "," + z + "," + ")" + "です");
      //    _landmarkListAnnotation[i].transform.position = new Vector3(ConvertCoordinate(lines, x, y, CoordinateType.x), ConvertCoordinate(lines, x, y, CoordinateType.y), z);
      //    //Debug.Log(i + "番目のランドマークのキャリブレーション後の座標は" + _landmarkListAnnotation[i].transform.position + "です");
      //    Debug.Log("キャリブレーション行列の書かれたCSVの読み取り完了");
      //  }
      //}
      //else
      //{
      //  Debug.Log("ミラーキャリブレーション行列が存在しないためCSVを開けません");
      //}


      if (ActivateFor(target))
      {
        _landmarkListAnnotation.Draw(target, visualizeZ);
        ApplyMask(mask);

        // Draw explicitly because connection annotation's targets remain the same.
        _connectionListAnnotation.Redraw();
      }
    }

    public void Draw(NormalizedLandmarkList target, BodyParts mask, bool visualizeZ = false)
    {
      Draw(target?.Landmark, mask, visualizeZ);
    }

    public void Draw(IList<NormalizedLandmark> target, bool visualizeZ = false)
    {
      Draw(target, BodyParts.All, visualizeZ);
    }

    public void Draw(NormalizedLandmarkList target, bool visualizeZ = false)
    {
      Draw(target?.Landmark, BodyParts.All, visualizeZ);
    }

    private void ApplyLeftLandmarkColor(Color color)
    {
      var annotationCount = _landmarkListAnnotation == null ? 0 : _landmarkListAnnotation.count;
      if (annotationCount >= _LandmarkCount)
      {
        foreach (var index in _LeftLandmarks)
        {
          _landmarkListAnnotation[index].SetColor(color);
        }
      }
    }

    private void ApplyRightLandmarkColor(Color color)
    {
      var annotationCount = _landmarkListAnnotation == null ? 0 : _landmarkListAnnotation.count;
      if (annotationCount >= _LandmarkCount)
      {
        foreach (var index in _RightLandmarks)
        {
          _landmarkListAnnotation[index].SetColor(color);
        }
      }
    }

    //2023/7/26(水)追加
    private void DrawBone()//0～32番目のランドマークにキャリブレーション行列を掛け合わせて新しいボーンを描くための関数
    {

    }


    private void ApplyMask(BodyParts mask)
    {
      //2023/9/22(金)追加

      if (!_isFIndButtons)
      {
        _mirrorCalibButton = GameObject.Find(("MirrorCalibButton"));
        //_calibButton = GameObject.Find(("CalibrationButton")); シーン再生時はFalseなのでここには書かない
        //_gameStartButton = GameObject.Find(("GameStartButton"));シーン再生時はFalseなのでここには書かない
        _isFIndButtons = true;
      }

      //2023/7/3(月)追加　多分_landmarkListAnnotation[i]がランドマークなはず！
      Debug.Log("鼻の位置は" + _landmarkListAnnotation[0].transform.position + "です！！！！");
      _headPos = _landmarkListAnnotation[0].transform.position;
      //Debug.Log(headPos + "です！！！！");

      //2023/7/17(月)追加
      _leftEar = _landmarkListAnnotation[7].transform.position;
      _rightEar = _landmarkListAnnotation[8].transform.position;
      float faceRadi = Vector3.Distance(_leftEar, _rightEar) / 2;

      //2023/7/21(金)追加 2023/10/7(土)変更　左手首：15→16　右手首：16→15　左ひざ：25→26　右ひざ：26→25
      _leftWrist = _landmarkListAnnotation[16].transform.position;
      _rightWrist = _landmarkListAnnotation[15].transform.position;
      _leftKnee = _landmarkListAnnotation[26].transform.position;
      _rightKnee = _landmarkListAnnotation[25].transform.position;

      Debug.Log("右手首" + _rightWrist);

      //2023/7/27(木)追加
      if (!GameObject.Find("MirrorCalibrationCompleted"))
      {
        SaveTaskData("SmartMirrorGame", _headPos, _leftWrist, _rightWrist, _leftKnee, _rightKnee);

        //2023/10/7(土)追加　デバッグ用 キャリブレーション前

        //if (!_is1)
        //{
        //  //左手首
        //  _pointingCursorLeftWrist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //  _pointingCursorLeftWrist.GetComponent<Renderer>().material.color = Color.white;
        //  _pointingCursorLeftWrist.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);
        //  _pointingCursorLeftWrist.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.5f);
        //  _pointingCursorLeftWrist.transform.localScale = new Vector3(5, 5, 5);
        //  _pointingCursorLeftWrist.name = "_pointingCursorLeftWrist";

        //  //右手首
        //  _pointingCursorRightWrist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //  _pointingCursorRightWrist.GetComponent<Renderer>().material.color = Color.black;
        //  _pointingCursorRightWrist.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);
        //  _pointingCursorRightWrist.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.5f);
        //  _pointingCursorRightWrist.transform.localScale = new Vector3(5, 5, 5);
        //  _pointingCursorRightWrist.name = "_pointingCursorRightWrist";


        //  //左ひざ
        //  _pointingCursorLeftKnee = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //  _pointingCursorLeftKnee.GetComponent<Renderer>().material.color = Color.blue;
        //  _pointingCursorLeftKnee.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);
        //  _pointingCursorLeftKnee.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.5f);
        //  _pointingCursorLeftKnee.transform.localScale = new Vector3(5, 5, 5);
        //  _pointingCursorLeftKnee.name = "_pointingCursorLeftKnee";


        //  //右ひざ
        //  _pointingCursorRightKnee = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //  _pointingCursorRightKnee.GetComponent<Renderer>().material.color = Color.green;
        //  _pointingCursorRightKnee.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);
        //  _pointingCursorRightKnee.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.5f);
        //  _pointingCursorRightKnee.transform.localScale = new Vector3(5, 5, 5);
        //  _pointingCursorRightKnee.name = "_pointingCursorRightKnee";


        //  _is1 = true;
        //}
        //_pointingCursorLeftWrist.transform.position = _leftWrist;
        //_pointingCursorRightWrist.transform.position = _rightWrist;
        //_pointingCursorLeftKnee.transform.position = _leftKnee;
        //_pointingCursorRightKnee.transform.position = _rightKnee;

        //Debug.Log("あはも左手首の座標" + _rightWrist);
        //Debug.Log("あはも右手首の座標" + _leftWrist);
        //Debug.Log("あはも左ひざの座標" + _rightKnee);
        //Debug.Log("あはも右ひざの座標" + _leftKnee);



      }

      //2023/7/25(火)&7/26(水)追加
      if (GameObject.Find("MirrorCalibrationCompleted"))
      {
        //2023/9/22(金)追加　タッチレスポインティング機能

        //if (!isInitOrder)//_orderの初期化
        //{
        //  _order = Order.TrainingHighCalib;
        //  _button = _calibButton;
        //  isInitOrder = true;
        //}

        if (!isInitOrder)//_orderの初期化
        {
          _order = Order.EyeGazeUpCalib;
          //_button = _calibButton;
          //buttonListの処理
          //if (GameObject.Find("UseHighPos"))
          //{
          //  _directionButtonList.Add(GameObject.Find("UseHighPos"));
          //}

          //if (GameObject.Find("UseLowPos"))
          //{
          //  _directionButtonList.Add(GameObject.Find("UseLowPos"));
          //}
          //_buttonList = new List<GameObject>(_directionButtonList);

          //2023/10/10(火)追加 EyeGazeCalibUp(Down)Buttonを1度だけFindしたらそのGameObjectをリストに格納しておく
          _directionButtonList.Add(GameObject.Find("EyeGazeCalibUpButton"));
          _directionButtonList.Add(GameObject.Find("EyeGazeCalibDownButton"));
          _directionButtonList.Add(GameObject.Find("DecideButton1"));

          //2023/10/10(火)追加 ミラーキャリブレーションが終わったと同時に_buttonListの初期化を行う
          _buttonList.Add(_directionButtonList[0]);
          _buttonList.Add(_directionButtonList[1]);
          _buttonList.Add(_directionButtonList[2]);

          for (int i = 0; i < _buttonList.Count; i++)
          {
            Debug.Log("おっす" + _buttonList[i].name);
          }

          _button = _dummyButton1;
          isInitOrder = true;
        }

        //2023/10/9(月)追加
        if (GameObject.Find("FinishEyeGazeUpCalib"))
        {
          Debug.Log("FinishEyeGazeUpCalib" + _button.name);
          //2023/10/10(火)追加　ミラー反射の初期姿勢頭の位置を代入
          _MirrorUpPos = GameObject.Find("UserUpPos").transform.position;

          //2023/10/10(火)追加　矢印ボタンが表示されないときには_buttonListにダミーボタン要素を追加する
          _buttonList.Clear();
          _buttonList.Add(_dummyButton1);
          _buttonList.Add(_dummyButton2);

          _calibButton = GameObject.Find(("CalibrationButton"));

          _order = Order.TrainingHighCalib;
          Destroy(GameObject.Find("FinishEyeGazeUpCalib"));
          _button = _calibButton;
          Debug.Log("ボタン代入１" + _button.name);

          _isButtonClicked = false;
        }

        //if (GameObject.Find("FinishTrainingHighCalib"))
        //{
        //  _order = Order.TrainingLowCalib;
        //  Destroy(GameObject.Find("FinishTrainingHighCalib"));

        //  //2023/9/28(木)追加
        //  _isButtonClicked = false;
        //}

        if (GameObject.Find("FinishTrainingHighCalib"))
        {


          _order = Order.EyeGazeDownCalib;
          Destroy(GameObject.Find("FinishTrainingHighCalib"));
          //buttonListの処理
          _directionButtonList.Add(GameObject.Find("DecideButton2"));

          _buttonList.Clear();
          _buttonList.Add(_directionButtonList[0]);
          _buttonList.Add(_directionButtonList[1]);
          _buttonList.Add(_directionButtonList[2]);
          _buttonList.Add(_directionButtonList[3]);
          _button = _dummyButton1;
          _isButtonClicked = false;
        }

        //2023/10/9(月)追加
        if (GameObject.Find("FinishEyeGazeDownCalib"))
        {
          //2023/10/10(火)追加　ミラー反射の初期姿勢頭の位置を代入
          _MirrorDownPos = GameObject.Find("UserDownPos").transform.position;

          //2023/10/10(火)追加　矢印ボタンが表示されないときには_buttonListにダミーボタン要素を追加する
          _buttonList.Clear();
          _buttonList.Add(_dummyButton1);
          _buttonList.Add(_dummyButton2);
          _button = _calibButton;

          _order = Order.TrainingLowCalib;
          Destroy(GameObject.Find("FinishEyeGazeDownCalib"));
          _isButtonClicked = false;
        }

        if (GameObject.Find("FinishTrainingLowCalib"))
        {
          _order = Order.GameOn;
          _gameStartButton = GameObject.Find(("GameStartButton"));
          _button = _gameStartButton;
          Destroy(GameObject.Find("FinishTrainingLowCalib"));
          //2023/9/28(木)追加
          _isButtonClicked = false;
        }

        Debug.Log("今からCSVを読み取ります1");
        //string filePath = "C:/Users/ig/AppData/LocalLow/DefaultCompany/MediaPipeUnityPlugin/SmartMirror/MirrorCalibration/CalibrationArray.csv";
        string filePath = "C:/Users/inoue/ig/SmartMirror/MirrorCalibration/CalibrationArray.csv";
        float[] lines = ReadCsv(filePath);
        for (int i = 0; i < 33; i++)
        {
          float x = _landmarkListAnnotation[i].transform.position.x;
          float y = _landmarkListAnnotation[i].transform.position.y;
          float z = _landmarkListAnnotation[i].transform.position.z;

          //if (i == 0) Debug.Log(i + "番目のランドマークの元々の座標は" + "(" + x + "," + y + "," + z + "," + ")" + "です");
          Debug.Log(i + "番目のランドマークの元々の座標は" + "(" + x + "," + y + "," + z + "," + ")" + "です");
          _landmarkListAnnotation[i].transform.position = new Vector3(ConvertCoordinate(lines, x, y, CoordinateType.x), ConvertCoordinate(lines, x, y, CoordinateType.y), z);

          //2023/9/22(金)追加　タッチレスポインティング機能

          if(i == 16)
          {
            _calibedLeftWrist = _landmarkListAnnotation[16].transform.position;
          }

          //Vector3 calibedRightWrist = _landmarkListAnnotation[16].transform.position;//2023/9/24(日)消してみた
          if (i == 15)
          {
            Vector3 calibedRightWrist = _landmarkListAnnotation[15].transform.position;//2023/9/24(日)追加

            Debug.Log("左手の位置は" + calibedRightWrist);

            //2023/10/5(金)追加 画面表示ボタン＆カメラ切り替えボタンのためのタッチレスクリック機能
            for (int k = 0; k < _switchingButtonList.Count; k++)//どのボタンの上にカーソルが乗っているかどうかを判定・インデックスを取得
            {
              Debug.Log("スイッチングボタンリストのカウント" + _switchingButtonList.Count);
              Debug.Log(k + "番目のスイッチング");

              if (_isSwitchingButtonClicked == false && _isSwitchingButtonOn == true)
              {
                k = _switchingButtonIndex;
              }

              if (_switchingClickJudgeTime > _waitTime)
              {
                _isSwitchingButtonClicked = true;
                _clickedSwitchingButtonIndex = k;
                _switchingButtonList[_switchingButtonIndex].GetComponent<Button>().onClick.Invoke();
                Debug.Log("ボタンがスクリプトから押されました");
                _switchingClickJudgeTime = 0;
                _circleImage.fillAmount = 1;//悪さしていない
                Debug.Log("アマウント3");

                _circleImage.enabled = false;
              }


              if (DetectLandmarkOnButton(_switchingButtonList[k], calibedRightWrist))
              {
                Debug.Log("スイッチングボタンの上にカーソルが乗りました" + k + "どらえまん");
                _isSwitchingButtonOn = true;
                _switchingButtonIndex = k;
                if (_clickedSwitchingButtonIndex == k) break;//一度クリックされたボタンを2回連続でクリックすることはできない

                if (_isSwitchingButtonClicked == true && k == _switchingButtonIndex)//直前にクリックしたボタンを連続でクリックしたときには何もしない(1回クリックしたボタンの上にカーソルを乗せ続けてクリックすることはできない)
                {
                  Debug.Log("ばいばい" + "k=" + k + "_switchingButtonIndex=" + _switchingButtonIndex);
                  break;
                }
                else _isSwitchingButtonClicked = false;

                if (k != _switchingButtonIndex)//初めてボタンの上にカーソルが乗った時または前のフレーム時とは異なるボタンにカーソルが乗った時
                {
                  _switchingClickJudgeTime = 0;
                  _circleImage.fillAmount = 1;
                  Debug.Log("アマウント1");
                }
                //円形ゲージに関する処理
                _switchingClickJudgeTime += Time.deltaTime;
                _circleImage.enabled = true;
                _circleImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(_switchingButtonList[k].transform.position.x, _switchingButtonList[k].transform.position.y, _switchingButtonList[k].transform.position.z);
                _circleImage.fillAmount -= (1.0f / _waitTime) * Time.deltaTime;
                break;//switchingボタンのうちどれか一つでもカーソルが乗っていると分かったらfor分を抜ける
              }
              else//カーソルがどのswitchingButtonにも乗っていないときに実行
              {
                if (!DetectLandmarkOnButton(_button, calibedRightWrist))//キャリブレーション系列のボタンがクリックされていないならば
                {
                  //if (_button == null) return;
                  _isSwitchingButtonClicked = false;
                  _isSwitchingButtonOn = false;
                  _switchingClickJudgeTime = 0;
                  _circleImage.fillAmount = 1;//悪さしている
                  Debug.Log("アマウント2");
                  Debug.Log(k + "どらえまん");

                  _circleImage.enabled = false;
                }
              }
            }

            //2023/10/9(月)追加
            //if (DetectLandmarkOnButtonList(_buttonList, calibedRightWrist))
            //{
            //  if (!_isButtonClicked)
            //  {
            //    //円形ゲージに関する処理
            //    _circleImage.enabled = true;
            //    Debug.Log("ボタンの上にカーソルが乗りました");
            //    _clickJudgeTime += Time.deltaTime;
            //    _circleImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(_button.transform.position.x, _button.transform.position.y, _button.transform.position.z);
            //    _circleImage.fillAmount -= (1.0f / _waitTime) * Time.deltaTime;
            //    Debug.Log("円形ゲージの量" + _circleImage.fillAmount + "経過時間" + _clickJudgeTime);
            //  }

            //  Debug.Log("クリック判定の時間" + _clickJudgeTime);

            //}
            //else
            //{
            //  _clickJudgeTime = 0;
            //  if (!_isSwitchingButtonOn)
            //  {
            //    _circleImage.fillAmount = 1;
            //    Debug.Log("アマウント4");

            //    _circleImage.enabled = false;
            //  }
            //}

            //if (_clickJudgeTime > _waitTime)
            //{
            //  _isButtonClicked = true;
            //  _button.GetComponent<Button>().onClick.Invoke();
            //  Debug.Log("ボタンがスクリプトから押されました");
            //  _clickJudgeTime = 0;

            //  if (!_isSwitchingButtonOn)
            //  {
            //    _circleImage.fillAmount = 1;
            //    Debug.Log("アマウント5");

            //    _circleImage.enabled = false;
            //  }
            //}

            //2023/10/10(火)追加 視点キャリブレーション時のタッチレスクリック処理
            if (_order == Order.EyeGazeUpCalib || _order == Order.EyeGazeDownCalib)
            {
              if (DetectLandmarkOnButtonList(_buttonList, calibedRightWrist) == EyeGazeCalibButtonType.Null)//どのボタンの上にもカーソルが乗っていないとき
              {
                Debug.Log("EyeGazeボタンリストの上にカーソルが乗っていません");
                _eyeGazeClickJudgeTime = 0;
                if (!_isSwitchingButtonOn)
                {
                  _circleImageInstanse.fillAmount = 1;
                  _circleImageInstanse.enabled = false;
                }
              }
              else
              {
                Debug.Log("EyeGaze視点キャリブレーション系列ボタンの検知");
                //2023/10/10(火)追加
                int buttonListClickedIndex = -1;

                if (!_isButtonClicked)
                {
                  //円形ゲージに関する処理

                  _circleImageInstanse.enabled = true;
                  Debug.Log("EyeGazeボタンの上にカーソルが乗りました");
                  _eyeGazeClickJudgeTime += Time.deltaTime;
                  buttonListClickedIndex = (int)DetectLandmarkOnButtonList(_buttonList, calibedRightWrist) - 1;//Enumの要素が持つ値から-1することでインデックスを取得する
                  Debug.Log(buttonListClickedIndex + "ばなな");

                  //2023/10/10(火)追加 生成位置をボタンリストの要素のもとへ
                  _circleImageInstanse.GetComponent<RectTransform>().anchoredPosition = new Vector3(_buttonList[buttonListClickedIndex].transform.position.x, _buttonList[buttonListClickedIndex].transform.position.y, _buttonList[buttonListClickedIndex].transform.position.z);
                  //GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                  //capsule.name = "Test";
                  //capsule.transform.position = _circleImageInstanse.transform.position;
                  if (DetectLandmarkOnButtonList(_buttonList, calibedRightWrist) == EyeGazeCalibButtonType.Up || DetectLandmarkOnButtonList(_buttonList, calibedRightWrist) == EyeGazeCalibButtonType.Down)
                  {
                    _circleImageInstanse.fillAmount -= (1.0f / _waitTime ) * Time.deltaTime;
                  }
                  else if (DetectLandmarkOnButtonList(_buttonList, calibedRightWrist) == EyeGazeCalibButtonType.Decision1 || DetectLandmarkOnButtonList(_buttonList, calibedRightWrist) == EyeGazeCalibButtonType.Decision2)
                  {
                    _circleImageInstanse.fillAmount -= (1.0f / _waitTime) * Time.deltaTime;
                  }
                  Debug.Log("EyeGaze円形ゲージの量" + _circleImageInstanse.fillAmount + "経過時間" + _eyeGazeClickJudgeTime);
                }

                Debug.Log("EyeGazeクリック判定の時間" + _eyeGazeClickJudgeTime);

                if (_eyeGazeClickJudgeTime > _waitTime)
                {
                  Debug.Log("EyeGazeクリックジャッジタイムが3秒を超えました");
                  _isButtonClicked = true;
                  _buttonList[buttonListClickedIndex].GetComponent<Button>().onClick.Invoke();
                  Debug.Log("EyeGazeボタンがスクリプトから押されました" + _buttonList[buttonListClickedIndex].name);

                  //矢印ボタンを押したら_isButtonClickedをfalseに変える必要性あり
                  if (DetectLandmarkOnButtonList(_buttonList, calibedRightWrist) == EyeGazeCalibButtonType.Up || DetectLandmarkOnButtonList(_buttonList, calibedRightWrist) == EyeGazeCalibButtonType.Down)
                  {
                    _isButtonClicked = false;
                  }
                  _eyeGazeClickJudgeTime = 0;

                  if (!_isSwitchingButtonOn)
                  {
                    _circleImageInstanse.fillAmount = 1;
                    Debug.Log("アマウント5");

                    _circleImageInstanse.enabled = false;
                  }
                }
              }
            }


            //2023/9/28(木)追加　スクワット姿勢のキャリブレーションボタンとゲーム開始ボタン用タッチレスクリック機能
            if (DetectLandmarkOnButton(_button, calibedRightWrist))
            {
              Debug.Log("キャリブレーション系列ボタンの検知");
              Debug.Log("初期姿勢" + _button.name);
              if (!_isButtonClicked)
              {
                //円形ゲージに関する処理
                _circleImage.enabled = true;
                Debug.Log("ボタンの上にカーソルが乗りました");
                _clickJudgeTime += Time.deltaTime;
                _circleImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(_button.transform.position.x, _button.transform.position.y, _button.transform.position.z);
                _circleImage.fillAmount -= (1.0f / _waitTime) * Time.deltaTime;
                
                Debug.Log("円形ゲージの量" + _circleImage.fillAmount + "経過時間" + _clickJudgeTime);
              }

              Debug.Log("クリック判定の時間" + _clickJudgeTime);

            }
            else
            {
              _clickJudgeTime = 0;
              if (!_isSwitchingButtonOn)
              {
                _circleImage.fillAmount = 1;
                Debug.Log("アマウント4");

                _circleImage.enabled = false;
              }
            }

            if (_clickJudgeTime > _waitTime)
            {
              _isButtonClicked = true;
              _button.GetComponent<Button>().onClick.Invoke();
              Debug.Log("ボタンがスクリプトから押されました");
              _clickJudgeTime = 0;

              if (!_isSwitchingButtonOn)
              {
                _circleImage.fillAmount = 1;
                Debug.Log("アマウント5");

                _circleImage.enabled = false;
              }
            }
            Debug.Log("もりもり");
            //2023/9/28(木)追加 ポインティングカーソルの表示
            if (!_isCameraOn)
            {
              Debug.Log("カーソル表示");
              _pointingCursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
              _pointingCursor.GetComponent<Renderer>().material.color = Color.yellow;
              _pointingCursor.GetComponent<Renderer>().material.SetFloat("_Metallic", 1.0f);
              _pointingCursor.GetComponent<Renderer>().material.SetFloat("_Glossiness", 0.5f);
              _pointingCursor.transform.localScale = new Vector3(5, 5, 5);
              _pointingCursor.name = "PointingCursor";
              _isCameraOn = true;
            }
            _pointingCursor.transform.position = calibedRightWrist;
            //_pointingCursor.transform.position = _calibedLeftWrist;

            SaveTaskParameter("CalibratedLandmarkPos", calibedRightWrist, _order, DetectLandmarkOnButton(_button, calibedRightWrist));//csvに書き込む



            //if (JudgeButtonClick())//毎フレームボタンクリックの判定を行う
            //{
            //  _button.GetComponent<Button>().onClick.Invoke();
            //  Debug.Log("ボタンがスクリプトから押されました");
            //}
          }


          //Debug.Log("時間リスト" + _timeList[i] + "オーダーリスト" + _timeList[i] + "ブールリスト" + _timeList[i]);

          Vector3 changedLandmarkPos = _landmarkListAnnotation[i].transform.position;
          Debug.Log(i + "番目のランドマークのキャリブレーション後の座標は" + changedLandmarkPos + "です");

          //2023/7/27(木)追加
          if (i == 0) SaveTaskData("SmartMirrorGame", changedLandmarkPos, _leftWrist, _rightWrist, _leftKnee, _rightKnee);

          //List<Vector3> convertedLandmarkList.Add(_landmarkListAnnotation[i].transform.position);
          Debug.Log("キャリブレーション行列の書かれたCSVの読み取り完了");
        }
      }
      else
      {
        Debug.Log("ミラーキャリブレーション行列が存在しないためCSVを開けません");
      }

      if (!isCreateCapsule)
      {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.position = new Vector3(1000, 1000, 1000);
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.parent = capsule.transform;
        cylinder.transform.localPosition = new Vector3(0, 0, 0);
        cylinder.name = _folderPath + "/" + "SmartMirrorGame.csv";
        isCreateCapsule = true;
        Debug.Log("カプセルを生成しました" + capsule.transform.position);
      }

      //2023/7/6(木)追加
      //Sphere(楕円)の大きさ：Vector3(43.291172, 55.6940918, 43.291172)
      if (sphere != null)//１フレーム前のSphereを消す
      {
        Destroy(sphere);
        Debug.Log("Sphereを消しました！！！");
      }

      sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);//顔のコライダー用のSphere
      Debug.Log("Sphereが生成されました！！！");

      //sphere.transform.position = _headPos;//Sphereの位置を顔の位置にする
      sphere.transform.position = _landmarkListAnnotation[0].transform.position;//Sphereの位置を顔の位置にする
      Debug.Log("Sphereの位置がセットされました！！！");

      sphere.transform.localScale = new Vector3(2 * faceRadi, 2 * faceRadi, 2 * faceRadi);
      Debug.Log("Sphereの大きさが変わりました！！！");

      sphere.GetComponent<Collider>().isTrigger = true;//コライダーのトリガーをオンにする
      Debug.Log("Sphereのトリガーがオンになりました！！！");

      Destroy(sphere.GetComponent<Renderer>().material);//Sphereの見た目を透明にする
      Debug.Log("①Sphereが透明になりました！！！");

      Destroy(sphere.GetComponent<MeshRenderer>());//Sphereの見た目を透明にする
      Debug.Log("②Sphereが透明になりました！！！");

      sphere.name = "HeadCollider";//名前を変えた
      Debug.Log("Sphereの名前が変わりました！！！");

      if (mask == BodyParts.All)
      {
        return;
      }

      if (!mask.HasFlag(BodyParts.Face))
      {
        // deactivate face landmarks
        for (var i = 0; i <= 10; i++)
        {
          _landmarkListAnnotation[i].SetActive(false);
        }
      }
      if (!mask.HasFlag(BodyParts.LeftArm))
      {
        // deactivate left elbow to hide left arm
        _landmarkListAnnotation[13].SetActive(false);
      }
      if (!mask.HasFlag(BodyParts.LeftHand))
      {
        // deactive left wrist, thumb, index and pinky to hide left hand
        _landmarkListAnnotation[15].SetActive(false);
        _landmarkListAnnotation[17].SetActive(false);
        _landmarkListAnnotation[19].SetActive(false);
        _landmarkListAnnotation[21].SetActive(false);
      }
      if (!mask.HasFlag(BodyParts.RightArm))
      {
        // deactivate right elbow to hide right arm
        _landmarkListAnnotation[14].SetActive(false);
      }
      if (!mask.HasFlag(BodyParts.RightHand))
      {
        // deactivate right wrist, thumb, index and pinky to hide right hand
        _landmarkListAnnotation[16].SetActive(false);
        _landmarkListAnnotation[18].SetActive(false);
        _landmarkListAnnotation[20].SetActive(false);
        _landmarkListAnnotation[22].SetActive(false);
      }
      if (!mask.HasFlag(BodyParts.LowerBody))
      {
        // deactivate lower body landmarks
        for (var i = 25; i <= 32; i++)
        {
          _landmarkListAnnotation[i].SetActive(false);
        }
      }

    }

    //2023/7/4(火)追加
    public string GetFolderPath()
    {
      DateTime now = DateTime.Now;
      string nowTime = now.Year.ToString() + "_" + now.Month.ToString() + "_" + now.Day.ToString() + "__" + now.Hour.ToString() + "_" + now.Minute.ToString() + "_" + now.Second.ToString();
      string folderPath = Application.persistentDataPath + "/" + "SmartMirror" + "/" + nowTime;
      Debug.Log(folderPath);
      return folderPath;
    }

    public void CreateDirectory(string path)
    {
      Directory.CreateDirectory(path);
    }

    public void SaveData(string path, string data, string fileName)
    {
      StreamWriter sw;
      FileInfo fi;

      fi = new FileInfo(path + "/" + fileName + ".csv");

      sw = fi.AppendText();
      sw.WriteLine(data);
      sw.Flush();
      sw.Close();
      //Debug.Log(path + "/" + fileName + ".csv" + data + "を保存");
    }


    //private void SaveTaskData(string fileName,Vector3 userPos)
    //{
    //  _time += Time.deltaTime;
    //  _timecount += Time.deltaTime;      

    //  //_folderPathは最初にセット済み
    //  string data = _time + "," + userPos.x + "," + userPos.y + "," + userPos.z;

    //  if (_timecount >= 0.1f)
    //  {
    //    SaveData(_folderPath, data, fileName);
    //    _timecount = 0;
    //  }
    //}

    private void SaveTaskData(string fileName, Vector3 headPos, Vector3 leftWrist, Vector3 rightWrist, Vector3 leftKnee, Vector3 rightKnee)
    {
      //Debug.Log(Time.time - _time + "です！！！");
      //_time = Time.time;
      //Debug.Log(_time);
      //_timecount += Time.deltaTime;

      _time = Time.time;

      //_folderPathは最初にセット済み
      string data = _time + "," + headPos.x + "," + headPos.y + "," + headPos.z + "," + leftWrist.x + "," + leftWrist.y + "," + leftWrist.z + "," + rightWrist.x + "," + rightWrist.y + "," + rightWrist.z + "," + leftKnee.x + "," + leftKnee.y + "," + leftKnee.z + "," + rightKnee.x + "," + rightKnee.y + "," + rightKnee.z;

      SaveData(_folderPath, data, fileName);
    }

    private void SaveTaskParameter(string fileName, Vector3 landmarkPos, Enum order, bool isLandmarkOnButton)
    {
      float time = Time.time;

      //_folderPathは最初にセット済み
      string data = time + "," + landmarkPos.x + "," + landmarkPos.y + "," + landmarkPos.z + "," + order + "," + isLandmarkOnButton;

      SaveData(_folderPath, data, fileName);
    }

    private bool DetectLandmarkOnButton(GameObject button, Vector3 landmarkPos)//右手首のランドマークがボタン上にあるかどうかを判定してブールで返してくれる関数
    {
      //2023/10/10(火)追加
      if (button == null) return false;

      bool isOn = false;
      float x1 = button.transform.GetChild(1).transform.position.x;
      float y1 = button.transform.GetChild(1).transform.position.y;
      float x3 = button.transform.GetChild(3).transform.position.x;
      float y3 = button.transform.GetChild(3).transform.position.y;
      float X = landmarkPos.x;
      float Y = landmarkPos.y;

      if (x1 <= X && X <= x3 && y3 <= Y && Y <= y1)
      {
        isOn = true;
      }

      return isOn;
    }

    //2023/10/9(月)追加 選択されているEyeGazeCalibButtonのTypeを返してくれる関数
    private EyeGazeCalibButtonType DetectLandmarkOnButtonList(List<GameObject> buttonList, Vector3 landmarkPos)//右手首のランドマークがボタン上にあるかどうかを判定してブールで返してくれる関数
    {
      float x1 = 0;
      float y1 = 0;
      float x3 = 0;
      float y3 = 0;
      float X = landmarkPos.x;
      float Y = landmarkPos.y;

      for (int i = 0; i < buttonList.Count; i++)
      {
        x1 = buttonList[i].transform.GetChild(1).transform.position.x;
        y1 = buttonList[i].transform.GetChild(1).transform.position.y;
        x3 = buttonList[i].transform.GetChild(3).transform.position.x;
        y3 = buttonList[i].transform.GetChild(3).transform.position.y;

        if (x1 <= X && X <= x3 && y3 <= Y && Y <= y1)
        {
          if (i == 0) return EyeGazeCalibButtonType.Up;
          if (i == 1) return EyeGazeCalibButtonType.Down;
          if (i == 2) return EyeGazeCalibButtonType.Decision1;//Decide1Button
          if (i == 3) return EyeGazeCalibButtonType.Decision2;//Decide2Button
        }
      }
      return EyeGazeCalibButtonType.Null;
    }

    //private bool JudgeButtonClick()//2023/9/23(土)追加
    //{
    //  bool isClick = false;
    //  _trueCount = 0;
    //  ReadCsv2(_folderPath + "/" + "CalibratedLandmarkPos.csv");//_timeList,_orderList,_boolListへの代入
    //  int index = -1;

    //  Debug.Log("オーダーリストの要素数" + _orderList.Count);
    //  for(int i =0;i<_orderList.Count;i++)
    //  {
    //    var orderItem = _orderList[i];
    //    Debug.Log("オーダーの中身" +  orderItem);
    //    Debug.Log("オーダーの中身2" + Enum.GetName(typeof(Order), _order));
    //    index++;
    //    Debug.Log("ブールの中身" + _boolList[index]);

    //    //2023/9/24(日)追加
    //    if (orderItem == Enum.GetName(typeof(Order), _order))
    //    {
    //      Debug.Log("オーダーの名前が一致");
    //      if(_boolList[index] == "False")
    //      {
    //        Debug.Log("ブールがFalse");
    //      }
    //    }

    //    if (orderItem == Enum.GetName(typeof(Order), _order) && _boolList[index] == "True")
    //    {
    //      Debug.Log(orderItem + index + "どらみ");
    //      Debug.Log(Enum.GetName(typeof(Order), _order) + "どらこ");
    //      if (_trueCount == 0)
    //      {
    //        _startTime = _timeList[index];
    //        _linesIndex = index;
    //      }
    //      _trueCount++;

    //      if (1 <= _trueCount) _endTime = _timeList[index];

    //      if (3 <= (_endTime - _startTime))
    //      {
    //        InitVariables(_trueCount, index);
    //        Debug.Log("ボタンがクリックされました");
    //        return isClick;
    //      }
    //    }
    //    else if (orderItem == Enum.GetName(typeof(Order), _order) && _boolList[index] != "True")
    //    {
    //      InitVariables(_trueCount, index);
    //      Debug.Log("変数を初期化します");
    //    }
    //    else
    //    {
    //      Debug.Log("_orderの値とcsvのorderに記載された値が一致しません");
    //    }
    //    continue;
    //  }


    //foreach (var orderItem in _orderList.Select((value, index) => new { value, index }))
    //{
    //  if (orderItem.value == Enum.GetName(typeof(Order), _order) && _boolList[orderItem.index] == "TRUE")
    //  {
    //    Debug.Log(orderItem.value + orderItem.index + "どらみ");
    //    Debug.Log(Enum.GetName(typeof(Order), _order) + "どらこ");
    //    if(trueCount == 0)
    //    {
    //      _startTime = _timeList[orderItem.index];
    //      _linesIndex = orderItem.index;
    //    }
    //    trueCount++;

    //    if (1 <= trueCount)_endTime = _timeList[orderItem.index];

    //    if(3 <= (_endTime - _startTime))
    //    {
    //      InitVariables(trueCount, orderItem.index);
    //      Debug.Log("ボタンがクリックされました");
    //      return isClick;
    //    }
    //  }
    //  else if(orderItem.value == Enum.GetName(typeof(Order), _order) && _boolList[orderItem.index] != "TRUE")
    //  {
    //    InitVariables(trueCount, orderItem.index);
    //  }
    //  else
    //  {
    //    Debug.Log("_orderの値とcsvのorderに記載された値が一致しません");
    //  }
    //  continue;
    //}



    //  return isClick;
    //}

    private void InitVariables(float trueCount, int index)
    {
      Debug.Log("イにっと");
      _trueCount = 0;//間違い候補
      _startTime = 0;
      _endTime = 0;
      _linesIndex = 0;

      //2023/9/24(日)追加　修正追加処理
      if (index == 0)
      {
        _timeList.RemoveAt(0);
        _orderList.RemoveAt(0);
        _boolList.RemoveAt(0);
        Debug.Log("リストの先頭を削除したよ");
        return;
      }

      for (int i = 0; i == index; i++)//FALSEが来た時点でそこまでインデックスのリストの要素をすべて削除する //間違い候補
      {
        _timeList.RemoveAt(0);
        _orderList.RemoveAt(0);
        _boolList.RemoveAt(0);
        Debug.Log("リストの先頭を削除したよ");
      }
    }

    private enum Order
    {
      MirrorCalib,
      EyeGazeUpCalib,
      EyeGazeDownCalib,
      TrainingHighCalib,
      TrainingLowCalib,
      GameOn,
    }

    //2023/10/10(火)追加　視点キャリブレーションにおけるボタンの種類（Null,Up,Down,Decision）
    private enum EyeGazeCalibButtonType
    {
      Null,
      Up,
      Down,
      Decision1,
      Decision2,
    }
  }
}

