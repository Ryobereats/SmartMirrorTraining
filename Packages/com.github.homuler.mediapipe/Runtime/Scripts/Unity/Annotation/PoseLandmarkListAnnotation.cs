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

        for(int i = 0; i < 8; i++)
        {
          floatLines[i] = float.Parse(_latestDatas[i]);
        }
        return floatLines;
      }
      catch (System.Exception e)
      {
        Debug.Log("CSV読み込み失敗: Path:" + filePath);
        System.Console.WriteLine(e.Message);
        for(int i = 0; i < 8; i++)
        {
          floatLines[i] = 0;
          Debug.Log("floatLinesの中身は" + floatLines[i]);
        }
        return floatLines;
      }
    }

    //2023/7/25(火)追加
    private float ConvertCoordinate(float[] lines, float x,float y,CoordinateType type)
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

      //2023/7/3(月)追加　多分_landmarkListAnnotation[i]がランドマークなはず！
      Debug.Log("鼻の位置は" + _landmarkListAnnotation[0].transform.position + "です！！！！");
      _headPos = _landmarkListAnnotation[0].transform.position;
      //Debug.Log(headPos + "です！！！！");

      //2023/7/17(月)追加
      _leftEar = _landmarkListAnnotation[7].transform.position;
      _rightEar = _landmarkListAnnotation[8].transform.position;
      float faceRadi = Vector3.Distance(_leftEar, _rightEar) /2;

      //2023/7/21(金)追加
      _leftWrist = _landmarkListAnnotation[15].transform.position;
      _rightWrist = _landmarkListAnnotation[16].transform.position;
      _leftKnee = _landmarkListAnnotation[25].transform.position;
      _rightKnee = _landmarkListAnnotation[26].transform.position;

      //2023/7/27(木)追加
      if(!GameObject.Find("MirrorCalibrationCompleted")) SaveTaskData("SmartMirrorGame", _headPos,_leftWrist,_rightWrist,_leftKnee,_rightKnee);

      //2023/7/25(火)&7/26(水)追加
      if (GameObject.Find("MirrorCalibrationCompleted"))
      {
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
          Vector3 changedLandmarkPos = _landmarkListAnnotation[i].transform.position;
          Debug.Log(i + "番目のランドマークのキャリブレーション後の座標は" + changedLandmarkPos + "です");

          //2023/7/27(木)追加
          if(i == 0)SaveTaskData("SmartMirrorGame", changedLandmarkPos, _leftWrist, _rightWrist, _leftKnee, _rightKnee);

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
      if(sphere != null)//１フレーム前のSphereを消す
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
    
    private void SaveTaskData(string fileName,Vector3 headPos,Vector3 leftWrist,Vector3 rightWrist,Vector3 leftKnee,Vector3 rightKnee)
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
  }
}
