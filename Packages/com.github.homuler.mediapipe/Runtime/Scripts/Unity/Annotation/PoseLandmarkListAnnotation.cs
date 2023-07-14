// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    public Vector3 headPos { get; private set; }
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

    public void Draw(IList<Landmark> target, Vector3 scale, bool visualizeZ = false)
    {
      if (ActivateFor(target))
      {
        _landmarkListAnnotation.Draw(target, scale, visualizeZ);
        // Draw explicitly because connection annotation's targets remain the same.
        _connectionListAnnotation.Redraw();
      }
    }

    public void Draw(LandmarkList target, Vector3 scale, bool visualizeZ = false)
    {
      Draw(target?.Landmark, scale, visualizeZ);
    }

    public void Draw(IList<NormalizedLandmark> target, BodyParts mask, bool visualizeZ = false)
    {
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

    private void ApplyMask(BodyParts mask)
    {
      //2023/7/3(月)追加　多分_landmarkListAnnotation[i]がランドマークなはず！
      //Debug.Log(_landmarkListAnnotation[0].transform.position + "です！！！！");
      headPos = _landmarkListAnnotation[0].transform.position;
      //Debug.Log(headPos + "です！！！！");

      SaveTaskData("SmartMirrorGame", headPos);

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

      sphere.transform.position = headPos;//Sphereの位置を顔の位置にする
      Debug.Log("Sphereの位置がセットされました！！！");

      sphere.transform.localScale = new Vector3(43.291172f, 55.6940918f, 43.291172f);
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
    
    private void SaveTaskData(string fileName,Vector3 userPos)
    {
      //Debug.Log(Time.time - _time + "です！！！");
      //_time = Time.time;
      //Debug.Log(_time);
      //_timecount += Time.deltaTime;

      _time = Time.time;

      //_folderPathは最初にセット済み
      string data = _time + "," + userPos.x + "," + userPos.y + "," + userPos.z;

        SaveData(_folderPath, data, fileName);
    }
  }
}
